using MessageQueueNET.Client.Extensions;
using MessageQueueNET.Client.Models;
using MessageQueueNET.Client.Services.Abstraction;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Client.Services;

public class MessageQueueClientService
{
    internal const string HttpClientName = "MessageQueueNETClient";

    private readonly HttpClient _httpClient;
    private readonly IMessageQueueApiVersionService _clientVersionService;
    private readonly ILogger<MessageQueueClientService> _logger;

    static private ConcurrentDictionary<string, Version> ApiVersions = new ConcurrentDictionary<string, Version>();

    public MessageQueueClientService(IMessageQueueApiVersionService clientVersionService,
                                     IHttpClientFactory httpClientFactory,
                                     ILogger<MessageQueueClientService> logger)
    {
        _clientVersionService = clientVersionService;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(HttpClientName);

        _logger.LogInformation("HttpClient create. Timeout {timeout}", _httpClient.Timeout);
    }

    async public Task<QueueClient> CreateClient(MessageQueueConnection connection,
                                                string queueNamePattern)
    {
        var client = new QueueClient(connection, queueNamePattern,
            httpClient: _httpClient);

        if (!ApiVersions.TryGetValue(connection.ApiUrl, out _))
        {
            var apiInfo = await client.ApiInfoAsync();
            var apiVersion = apiInfo.Version;

            if(apiVersion.Major == 2 && _clientVersionService.Version.Major == 3)
            {
                _logger.LogWarning("Client ({apiVersion}) and API ({clientVersion}) Major Version not match", apiVersion, _clientVersionService.Version);
            }
            else if (apiVersion.Major != _clientVersionService.Version.Major)
            {
                throw new Exception($"Client ({apiVersion}) and API ({_clientVersionService.Version}) Major Version not match");
            }

            ApiVersions[connection.ApiUrl] = apiVersion;
        }

        return client;
    }

    async public IAsyncEnumerable<QueuePropertiesResult> GetNextQueueProperties(
        MessageQueueConnection connection,
        string queueNamePattern,
        [EnumeratorCancellation] CancellationToken stoppingToken,
        bool? silentAccess = null)  // Action will not affectLastAccessTime (LifeTime)
    {
        int? hashCode = null;
        var client = await CreateClient(connection, queueNamePattern);

        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var minimumDelay = new MinimumDelay(1000))
            {
                var queueProperties = await client.PropertiesAsync(stoppingToken, hashCode, silentAccess);
                hashCode = queueProperties.HashCode;

                yield return queueProperties;
            }
        }
    }

    async public IAsyncEnumerable<MessagesResult> GetNextMessages(
        MessageQueueConnection connection,
        string queueNamePattern,
        [EnumeratorCancellation] CancellationToken stoppingToken,
        int constCount = 0)
    {
        int? hashCode = null;
        var client = await CreateClient(connection, queueNamePattern);

        while (!stoppingToken.IsCancellationRequested)
        {
            int max = constCount;
            if (max == 0)
            {
                var propertiesResult = await client.PropertiesAsync();

                max = propertiesResult.Queues?.Values
                            .Sum(p => p.DequeueMaxRecommendation()) ?? 0;

            }
            await using (var minimumDelay = new MinimumDelay(1000))
            {
                var messagesResult = await client.DequeueAsync(Math.Clamp(max, 1, 100),
                                                               cancelationToken: stoppingToken,
                                                               hashCode: hashCode);
                hashCode = messagesResult.HashCode;

                yield return messagesResult;
            }
        }
    }
}
