using MessageQueueNET.Services.Abstraction;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Services
{
    public class TimedHostedBackgroundService : IHostedService, IDisposable
    {
        private Timer _timer;
        private int counter = 0;
        private bool _working = false;

        private readonly QueuesService _queues;
        private readonly IQueuesPersistService _persist;

        public TimedHostedBackgroundService(QueuesService queues, IQueuesPersistService persist)
        {
            _queues = queues;
            _persist = persist;
        }

        #region IDisposable

        public void Dispose()
        {
            _timer?.Dispose();
        }

        #endregion

        #region IHostedService

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        #endregion

        private void DoWork(object state)
        {
            if (_working)
            {
                return;
            }

            try
            {
                _working = true;

                foreach (var queue in _queues.Queues)
                {
                    if (queue.Properties.ConfirmProcessingSeconds > 0)
                    {
                        _queues.ReEnqueueUnconfirmedMessages(queue);
                    }

                    if (queue.Properties.LifetimeSeconds > 0 &&
                        (DateTime.UtcNow - queue.LastAccessUTC).TotalSeconds > queue.Properties.LifetimeSeconds)
                    {
                        var itemCount = queue.Where(item => item.IsValid(queue)).Count();
                        if (itemCount == 0)
                        {
                            if (_persist.RemoveQueue(queue.Name).Result)
                            {
                                _queues.RemoveQueue(queue.Name);
                            }
                        }
                    }
                }

                counter++;
                if (counter >= 86400)
                {
                    counter = 0;
                }
            }
            finally
            {
                _working = false;
            }
        }
    }
}
