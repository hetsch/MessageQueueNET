using MessageQueueNET.Client;

namespace MessageQueueNET.StressTest;

internal class QueueClientThread
{
    private readonly string _queueName;
    private readonly CancellationTokenSource _cancellationToken;
    private readonly QueueClient _client;

    public QueueClientThread(string queueName, CancellationTokenSource cancellationToken)
    {
        _queueName = queueName;
        _cancellationToken = cancellationToken;
        _client = new QueueClient("https://localhost:5001", _queueName);
    }

    public void Start()
    {
        var thread = new Thread(Run);
        thread.Start();
    }

    private void Run()
    {
        Console.WriteLine($"Thread started: {_queueName}");

        while (!_cancellationToken.IsCancellationRequested)
        {
            long randomNumber = Random.Shared.NextInt64();

            try
            {
                if (randomNumber % 2 == 0)
                {
                    _client.DequeueAsync().Wait();
                }
                else
                {
                    _client.EnqueueAsync(new string[] { randomNumber.ToString() }).Wait();
                }
            } 
            catch(Exception ex)
            {
                Console.WriteLine($"{_queueName}: {ex.Message}");
            }
            
            Thread.Sleep(10);
        }

        Console.WriteLine($"Thread cancelled: {_queueName}");
    }
}
