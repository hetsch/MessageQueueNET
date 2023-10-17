using MessageQueueNET.ProcService.Abstraction;
using MessageQueueNET.ProcService.Extensions;
using System;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcService
{
    public class TaskQueue<T>
       where T : ITaskContext
    {
        public delegate Task QueuedTask(T parameter);

        public delegate void TaskCompletedHandler(T parameter);
        public delegate void TaskCanceledHandler(T parameter);
        public delegate void TaskCrashedHandler(T parameter, Exception ex);

        public event TaskCompletedHandler? TaskCompleted;
        public event TaskCanceledHandler? TaskCanceled;
        public event TaskCrashedHandler? TaskCrashed;

        private readonly CancelTracker _cancelTracker;

        private long TaskId = 0;
        private long ProcessedTasks = 0;
        private int MaxParallelTasks = 50;
        private int MaxQueueLength = 100;
        private object locker = new object();

        public TaskQueue(int maxParallelTask)
            : this(maxParallelTask, int.MaxValue, new CancelTracker())
        {
            this.MaxParallelTasks = maxParallelTask;
        }

        public TaskQueue(int maxParallelTask, int maxQueueLength, CancelTracker cancelTracker)
        {
            this.MaxParallelTasks = maxParallelTask;
            this.MaxQueueLength = maxQueueLength;

            _cancelTracker = cancelTracker;
        }

        public int CurrentCapacity => MaxQueueLength - (int)(TaskId - ProcessedTasks);

        private long NextProcessId()
        {
            lock (locker)
            {
                return ++TaskId;
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

        public int RunningTask => (int)(TaskId - ProcessedTasks);
        public bool HasRunningTasks => RunningTask > 0;

        async public Task<bool> AwaitRequest(TaskQueue<T>.QueuedTask method, T parameter)
        {
            try
            {
                long currentProcessId = parameter.TaskId = NextProcessId();

                $"Queuing...".Log(parameter);

                while (true)
                {
                    if (_cancelTracker.IsCancelled)
                    {
                        TaskCanceled?.Invoke(parameter);
                        return false;
                    }

                    if (IsReadyToRumble(currentProcessId))
                        break;

                    await Task.Delay(10);
                }

                parameter.StartTime = DateTime.Now;

                if (method != null)
                {
                    await method.Invoke(parameter);
                }

                TaskCompleted?.Invoke(parameter);

                return true;
            }
            catch (Exception ex)
            {
                TaskCrashed?.Invoke(parameter, ex);

                return false;
            }
            finally
            {
                IncreaseProcessedTasks();
            }
        }
    }
}
