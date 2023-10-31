using MessageQueueNET.Client.Extensions;
using MessageQueueNET.Client.Models;
using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Client.Services;
internal class QueueWatcherBackgroundService : BackgroundService
{
    private readonly ILogger<QueueWatcherBackgroundService> _logger;
    private readonly MessageQueueClientService _clientService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly QueueWatcherBackgroundServiceOptions _options;

    public QueueWatcherBackgroundService(ILogger<QueueWatcherBackgroundService> logger,
                                         MessageQueueClientService clientService,
                                         IServiceScopeFactory serviceScopeFactory,
                                         IOptions<QueueWatcherBackgroundServiceOptions> options)
    {
        _logger = logger;
        _clientService = clientService;
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
    }

    async protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (String.IsNullOrEmpty(_options.MessageQueueApiUrl))
        {
            throw new Exception("No MessageQueueApiUrl defined");
        }

        var connection = new MessageQueueConnection(_options.MessageQueueApiUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var messagesResult in _clientService.GetNextMessages(connection, _options.QueueNameFilter, stoppingToken, 100))
                {
                    if (messagesResult.Messages?.Any() != true)
                    {
                        continue;
                    }

                    foreach (var messageResult in messagesResult.Messages)
                    {
                        RunTaskAndForget(connection, messageResult, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in BackgroundService: {message}", ex.Message);
            }
        }
    }

    private void RunTaskAndForget(MessageQueueConnection connection,
                                  MessageResult messageResult,
                                  CancellationToken stoppingToken)
    {
        var task = Task.Factory.StartNew(async (state) =>
        {
            QueueProcessorResult? jobResult = null;
            BaseQueueProcessorMessage? baseJobMessage = null;

            try
            {
                var messageResult = state as MessageResult;

                if (String.IsNullOrEmpty(messageResult?.Value))
                {
                    throw new Exception("Empty message received");
                }

                baseJobMessage = JsonSerializer.Deserialize<BaseQueueProcessorMessage>(messageResult.Value);
                if (baseJobMessage is null)
                {
                    throw new Exception("Message is not valid");
                }

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var jobProcessors = scope.ServiceProvider.GetServices<IBaseQueueProcessor>();

                    var processor = jobProcessors.FirstOrDefault(j => j.CanProcessMessage(baseJobMessage));
                    if (processor is null)
                    {
                        throw new Exception("no proper processor fouond");
                    }

                    if (processor.TryGetGenericJobProcessorType(out var bodyType))
                    {
                        var genericJobMessage = messageResult.Value.DeserializeJobProcessingMessage(bodyType)!;
                        jobResult = await processor.CallProcessGeneric(genericJobMessage);
                    }
                    else
                    {
                        jobResult = await processor.Process(baseJobMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in BackgroundService - Run Task: {message}", ex.Message);
            }
            finally
            {
                if (messageResult.ConfirmationRequired()
                    && jobResult.ConfirmationRecommended())
                {
                    #region Confirm Message

                    try
                    {
                        var client = await _clientService.CreateClient(connection, messageResult.Queue);
                        await client.ConfirmDequeueAsync(messageResult.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception in BackgroundService - Confirm Message: {message}", ex.Message);
                    }

                    #endregion
                }

                if (!String.IsNullOrEmpty(baseJobMessage?.ResultQueue))
                {
                    #region Return Result to MessageQueue

                    try
                    {
                        var client = await _clientService.CreateClient(connection, baseJobMessage.ResultQueue);
                        await client.EnqueueAsync(new string[]
                        {
                            JsonSerializer.Serialize(jobResult)
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception in BackgroundService - Return Result to MessageQueue: {message}", ex.Message);
                    }

                    #endregion
                }
            }
        }, messageResult, stoppingToken);
    }
}
