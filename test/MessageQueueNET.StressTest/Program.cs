using MessageQueueNET.StressTest;

CancellationTokenSource cancellationToken = new CancellationTokenSource();

for (int i = 1; i <= 10; i++)
{
   var queueThread = new QueueClientThread($"test.queue{i}", cancellationToken);
    queueThread.Start();
}

Console.WriteLine("Hit any key to stop...");
Console.ReadLine();
cancellationToken.Cancel();
