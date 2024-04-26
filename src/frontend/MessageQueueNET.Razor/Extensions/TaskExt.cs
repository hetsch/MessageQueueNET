namespace MessageQueueNET.Razor.Extensions;

static internal class TaskExt
{
    async static public Task WaitUntil(Func<bool> cancellation, int milliseconds = 100)
    {
        while (cancellation() == false)
        {
            await Task.Delay(milliseconds);
        }
    }

    async static public Task WaitForCancellation(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(int.MaxValue, cancellationToken);
        }
    }
}
