using System;
using System.Threading.Tasks;

namespace MessageQueueNET.Client;

internal class MinimumDelay : IAsyncDisposable
{
    private readonly DateTime _start;
    private int _milliseconds;

    public MinimumDelay(int milliseconds)
    {
        _start = DateTime.Now;
        _milliseconds = milliseconds;

    }

    public async ValueTask DisposeAsync()
    {
        var waitMillisconds = _milliseconds - (int)(DateTime.Now - _start).TotalMilliseconds;

        if (waitMillisconds > 0)
        {
            await Task.Delay(waitMillisconds);
        }
    }
}
