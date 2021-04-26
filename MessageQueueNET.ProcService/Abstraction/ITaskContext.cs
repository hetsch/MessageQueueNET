using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcService.Abstraction
{
    public interface ITaskContext
    {
        long TaskId { get; set; }
        DateTime StartTime { get; set; }
    }
}
