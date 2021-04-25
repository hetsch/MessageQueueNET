using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcService.Extensions
{
    static class StringExtensions
    {
        static public void Log(this string line)
        {
            Console.WriteLine($"{ DateTime.Now.ToShortDateString() } { DateTime.Now.ToLongTimeString() }: { line }");
        }
    }
}
