using MessageQueueNET.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.Core.Services.Abstraction;

public interface IBaseQueueProcessor
{
    bool CanProcessMessage(BaseQueueProcessorMessage jobProcessMessage);
    Task<QueueProcessorResult> Process(BaseQueueProcessorMessage jobProcessMessage);
}
