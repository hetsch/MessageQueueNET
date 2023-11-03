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
                await foreach (var messagesResult in _clientService.GetNextMessages(connection, _options.QueueNameFilter, stoppingToken, 0))
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
        Task.Factory.StartNew(async (state) =>
        {
            var processMessage = state as MessageResult;

            QueueProcessorResult? processorResult = null;
            BaseQueueProcessorMessage? baseProcessorMessage = null;
            IQueueProcessor? processor = null;

            try
            {
                if (String.IsNullOrEmpty(processMessage?.Value))
                {
                    throw new Exception("Empty message received");
                }

                baseProcessorMessage = JsonSerializer.Deserialize<BaseQueueProcessorMessage>(processMessage.Value);
                if (baseProcessorMessage is null)
                {
                    throw new Exception("Message is not valid");
                }

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var jobProcessors = scope.ServiceProvider.GetServices<IQueueProcessor>();

                    processor = jobProcessors.FirstOrDefault(j => baseProcessorMessage.Worker.Equals(j.WorkerId, StringComparison.OrdinalIgnoreCase));
                    if (processor is null)
                    {
                        throw new Exception($"no proper processor found with workerId '{baseProcessorMessage.Worker}'");
                    }

                    if (processor.TryGetGenericJobProcessorType(out var bodyType))
                    {
                        var genericJobMessage = processMessage.Value.DeserializeJobProcessingMessage(bodyType)!;
                        processorResult = await processor.CallProcessGeneric(genericJobMessage, stoppingToken);
                    }
                    else if (processor is INonGenericQueueProcessor nonGenericProcessor)
                    {
                        processorResult = await nonGenericProcessor.Process(baseProcessorMessage, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in BackgroundService - Run Task: {message}", ex.Message);
            }
            finally
            {
                if (processorResult != null
                    && baseProcessorMessage != null)
                {
                    if (string.IsNullOrEmpty(processorResult.ProcessId))
                    {
                        processorResult.ProcessId = baseProcessorMessage.ProcessId;
                    }
                }

                if (!String.IsNullOrEmpty(processMessage?.Queue)
                    && processMessage.ConfirmationRequired()
                    && (processorResult.ConfirmationRecommended() || processor.ConfirmationRecommended()))
                {
                    #region Confirm Message

                    try
                    {
                        var client = await _clientService.CreateClient(connection, processMessage.Queue);
                        await client.ConfirmDequeueAsync(processMessage.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception in BackgroundService - Confirm Message: {message}", ex.Message);
                    }

                    #endregion
                }

                if (!String.IsNullOrEmpty(baseProcessorMessage?.ResultQueue) && processorResult != null)
                {
                    #region Return Result to MessageQueue

                    try
                    {
                        var client = await _clientService.CreateClient(connection, baseProcessorMessage.ResultQueue);
                        await client.EnqueueAsync(new string[]
                        {
                            // cast to (object) => force serialize GenericQueueProcessorResult.Body!
                            JsonSerializer.Serialize((object)processorResult)
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
    