using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcQueue
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
