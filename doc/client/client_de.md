# MessageQueueNET.Client

In der `MessageQueueNET.Client` .NET Standard Bibliothek (nuget) befindet sich die `QueueClient` Klasse, die für .NET Programm eine Abstraktion der REST Schnittstelle liefert.

https://www.nuget.org/packages/MessageQueueNET.Client

## Instanziierung 

Ein Client kann folgendermaßen instanziiert werden:

```csharp
var client = new QueueClient("https://clientId:clientSecret@my-server", "my-queue-1");
```

Verwendet die API Autorisierung kann, diese über die Url (Beispiel oben) oder auch über optionale Parameter übergeben werden.

## Methoden

Hier werden die Methoden aufgelistet. Die Funktionweise entspricht der für die REST API beschriebenen:

```csharp
Task<bool> EnqueueAsync(IEnumerable<string> messages)
```

```csharp
Task<MessagesResult> DequeueAsync(int count = 1, bool register = false)
```

```csharp
Task<bool> ConfirmDequeueAsync(Guid messageId)
```

```csharp
Task<MessagesResult> AllMessagesAsync(int max = 0, bool unconfirmedOnly = false)
```

```csharp
Task<QueueLengthResult> LengthAsync()
```

```csharp
Task<bool> RemoveAsync()
```

```csharp
Task<bool> RegisterAsync(int? lifetimeSeconds = null,
                         int? itemLifetimeSeconds = null,
                         bool? suspendEnqueue = null,
                         bool? suspendDequeue = null)
```

```csharp
Task<bool> PropertiesAsync()
```

```csharp
Task<IEnumerable<string>> QueueNamesAsync()
```

Beispiel:

```csharp
var client = new QueueClient("https://clientId:clientSecret@my-server", "my-queue-1");

await client.Enqueue(new [] { "Message1", "Messsage2" });

var queueNames = await client.QueueNames(); // => ["my-queue-1, ..."]

var length = await client.Length(); // => 2
var message = await client.Dequeue(); // => ["Message1"]
```

[Kommandozeilenwerkzeuge](../console/tools_de.md)