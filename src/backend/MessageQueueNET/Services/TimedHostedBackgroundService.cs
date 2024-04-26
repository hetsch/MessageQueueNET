using MessageQueueNET.Services.Abstraction;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Services
{
    public class TimedHostedBackgroundService : BackgroundService
    {
        private readonly QueuesService _queues;
        private readonly IQueuesPersistService _persist;

        public TimedHostedBackgroundService(QueuesService queues,
                                            IQueuesPersistService persist)
        {
            _queues = queues;
            _persist = persist;
        }

        async protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWork();
                await Task.Delay(1000);
            }
        }

        async private Task DoWork()
        {
            try
            {
                foreach (var queue in _queues.Queues)
                {
                    if (queue.Properties.ConfirmationPeriodSeconds > 0)
                    {
                        await _queues.ReEnqueueUnconfirmedMessages(queue, _persist);
                    }

                    if (queue.Properties.LifetimeSeconds > 0 &&
                        (DateTime.UtcNow - queue.LastAccessUTC).TotalSeconds > queue.Properties.LifetimeSeconds)
                    {
                        var itemCount = queue.Where(item => item.IsValid(queue)).Count() +
                                        _queues.UnconfirmedMessagesCount(queue, null);
                        if (itemCount == 0)
                        {
                            if (_persist.RemoveQueue(queue.Name).Result)
                            {
                                _queues.RemoveQueue(queue.Name);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
