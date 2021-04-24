using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcService
{
    public class TaskQueue<T>
    {
        public delegate Task QueuedTask(T parameter);
        public delegate void TaskCompletedHandler(T parameter, Exception ex);

        public event TaskCompletedHandler TaskCompleted;

        private readonly CancelTracker _cancelTracker;

        private long ProcessId = 0;
        private long ProcessedTasks = 0;
        private int MaxParallelTasks = 50;
        private int MaxQueueLength = 100;
        private object locker = new object();

        public TaskQueue(int maxParallelTask)
            : this(maxParallelTask, int.MaxValue, null)
        {
            this.MaxParallelTasks = maxParallelTask;
        }

        public TaskQueue(int maxParallelTask, int maxQueueLength, CancelTracker cancelTracker)
        {
            this.MaxParallelTasks = maxParallelTask;
            this.MaxQueueLength = maxQueueLength;

            _cancelTracker = cancelTracker ?? new CancelTracker();
        }

        public int CurrentCapacity => MaxQueueLength - (int)(ProcessId - ProcessedTasks);

        private long NextProcessId()
        {
            lock (locker)
            {
                return ++ProcessId;
            }
        }

        private void IncreaseProcessedTasks()
        {
            lock (locker)
            {
                ProcessedTasks = ++ProcessedTasks;
            }
        }

        private bool IsReadyToRumble(long currentProcessId)
        {
            lock (locker)
            {
                return currentProcessId - ProcessedTasks <= MaxParallelTasks;
            }
        }

        public int RunningTask => (int)(ProcessId - ProcessedTasks);
        public bool HasRunningTasks => RunningTask > 0;

        async public Task<bool> AwaitRequest(TaskQueue<T>.QueuedTask method, T parameter)
        {
            try
            {
                if (_cancelTracker.IsCancelled)
                    return false;

                Console.WriteLine($"Queue task...");

                long currentProcessId = NextProcessId();

                while (true)
                {
                    if (_cancelTracker.IsCancelled)
                        return false;

                    if (IsReadyToRumble(currentProcessId))
                        break;

                    await Task.Delay(10);
                }

                await method?.Invoke(parameter);

                TaskCompleted?.Invoke(parameter, null);

                return true;
            }
            catch (Exception ex)
            {
                TaskCompleted?.Invoke(parameter, ex);

                return false;
            }
            finally
            {
                IncreaseProcessedTasks();
            }
        }
    }
}
