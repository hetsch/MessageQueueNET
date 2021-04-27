# MessageQueueNET.Client

In der `MessageQueueNET.Client` .NET Standard Bibliothek (nuget) befindet sich die `QueueClient` Klasse, die für .NET Programm eine Abstraktion der REST Schnittstelle liefert.

## Instanziierung 

Ein Client kann folgendermaßen instanziiert werden:

```csharp
var client = new QueueClient("https://clientId:clientSecret@my-server", "my-queue-1");
```

Verwende die API Autorisierung kann diese über die Url (Beispiel oben) oder auch über optionale Parameter übergeben werden.

## Methoden

Hier werden die Methoden aufgelistet. Die Funktionweise entspricht der für die REST API beschriebenen:

```csharp
async public Task<bool> Enqueue(IEnumerable<string> messages)
```

```csharp
async public Task<IEnumerable<string>> Dequeue(int count = 1, bool register = false)
```

```csharp
async public Task<IEnumerable<string>> AllMessages()
```

```csharp
async public Task<int> Length()
```

```csharp
async public Task<bool> Remove()
```

```csharp
async public Task<bool> Register()
```

```csharp
async public Task<IEnumerable<string>> QueueNames()
```

Beispiel:

```csharp
var client = new QueueClient("https://clientId:clientSecret@my-server", "my-queue-1");

await client.Enqueue(new [] { "Message1", "Messsage2" });

var queueNames = await client.QueueNames(); // => ["my-queue-1, ..."]
var message = await client.Dequeue(); // => "Message1"
```