using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcService
{
    class ProccessContext
    {
        public ProccessContext()
        {
            ProcId = Guid.NewGuid().ToString();
        }

        public string Command { get; set; }
        public string Arguments { get; set; }
        public string ProcId { get; set; }

        public int ExitCode { get; set; }
        public string Output { get; set; }
    }
}
