namespace MessageQueueNET.ProcService
{
    public class CancelTracker
    {
        public CancelTracker()
        {
            this.IsCancelled = false;
        }

        public bool IsCancelled { get; private set; }

        public void Cancel() => IsCancelled = true;
    }
}
