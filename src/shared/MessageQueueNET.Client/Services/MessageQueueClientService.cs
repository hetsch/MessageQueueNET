using MessageQueueNET.Client.Models;
using MessageQueueNET.Client.Services.Abstraction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    static private ConcurrentDictionary<string, Version> ApiVersions = new ConcurrentDictionary<string, Version>();

    public MessageQueueClientService(IMessageQueueApiVersionService clientVersionService,
                                     IHttpClientFactory httpClientFactory)
    {
        _clientVersionService = clientVersionService;
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
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

            if (apiVersion.Major != _clientVersionService.Version.Major)
            {
                throw new Exception($"Client ({apiVersion}) and API ({_clientVersionService}) Major Version not match");
            }

            ApiVersions[connection.ApiUrl] = apiVersion;
        }

        return client;
    }

    async public IAsyncEnumerable<QueuePropertiesResult> GetNextQueueProperties(
        MessageQueueConnection connection,
        string queueNamePattern,
        [EnumeratorCancellation] CancellationToken stoppingToken)
    {
        int? hashCode = null;
        var client = await CreateClient(connection, queueNamePattern);

        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var minimumDelay = new MinimumDelay(1000))
            {
                var queueProperties = await client.PropertiesAsync(stoppingToken, hashCode);
                hashCode = queueProperties.HashCode;

                yield return queueProperties;
            }
        }
    }

    async public IAsyncEnumerable<MessagesResult> GetNextMessages(
        MessageQueueConnection connection,
        string queueNamePattern,
        [EnumeratorCancellation] CancellationToken stoppingToken,
        int count)
    {
        int? hashCode = null;
        var client = await CreateClient(connection, queueNamePattern);

        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var minimumDelay = new MinimumDelay(1000))
            {
                var messagesResult = await client.DequeueAsync(count, 
                                                               cancelationToken: stoppingToken,
                                                               hashCode: hashCode);
                hashCode = messagesResult.HashCode;

                yield return messagesResult;

            }
        }
    }
}
