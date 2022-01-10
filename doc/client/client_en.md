# MessageQueueNET.Client

The `MessageQueueNET.Client` .NET Standard Library (nuget) contains the `QueueClient` class, which provides an abstraction of the REST interface using any .NET language.

## Instantiation

A client can be created as follows:

```csharp
var client = new QueueClient("https://clientId:clientSecret@my-server", "my-queue-1");
```

If API authorization is used, the `ClientId` and `ClientSecret` can be passed in url-format (example above) or as optional parameters.

## Methods

Here all methods are listed. The function is the same as described for the REST API:

```csharp
Task<bool> EnqueueAsync(IEnumerable<string> messages)
```

```csharp
Task<IEnumerable<string>> DequeueAsync(int count = 1, bool register = false)
```

```csharp
Task<IEnumerable<string>> AllMessagesAsync()
```

```csharp
Task<int> LengthAsync()
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

Example:

```csharp
var client = new QueueClient("https://clientId:clientSecret@my-server", "my-queue-1");

await client.Enqueue(new [] { "Message1", "Messsage2" });

var queueNames = await client.QueueNames(); // => ["my-queue-1, ..."]

var length = await client.Length(); // => 2
var message = await client.Dequeue(); // => ["Message1"]
```

[Commandline Tools](../console/tools_en.md)
