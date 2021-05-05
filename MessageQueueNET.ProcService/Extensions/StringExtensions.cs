using MessageQueueNET.ProcService.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcService.Extensions
{
    static class StringExtensions
    {
        static object _writeLock = new object();

        static public void Log(this string line, ITaskContext context = null, string logFile = "")
        { 
            if (context != null)
            {
                line = $"{ DateTime.Now.ToShortDateString() } { DateTime.Now.ToLongTimeString() }: Task { context.TaskId } - { line }";
            } 
            else
            {
                line = $"{ DateTime.Now.ToShortDateString() } { DateTime.Now.ToLongTimeString() }: { line }";
            }

            Console.WriteLine(line);

            logFile = String.IsNullOrEmpty(logFile) ? context?.LogFile : logFile;
            if(!String.IsNullOrEmpty(logFile))
            {
                lock (_writeLock)
                {
                    File.AppendAllLines(logFile, new string[] { line });
                }
            }
        }
    }
}
