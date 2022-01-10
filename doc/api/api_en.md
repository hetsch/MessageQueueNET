# REST API

The Rest API is a .NET Core API Web application. The application runs in both *Microsoft IIS*, *Standalone* (dotnet MessageQueueNET.dll), or as a *Docker Container*.

The API provides methods for creating and querying queues. Any queues can be created with a unique name (any string). Each of these queues is handled by different processes.

## Configuration

The configuration is *JSON* formated in the *appsettings.json* or in the *_config/message-queue.json* file. It is recommended that the settings shown here are configured
inside the *_config/message-queue.json* file because it will not be overwritten during the update. To update, everything except the *_config* directory can be copied to an an existing installation.

### message-queue.json (appsettings.json)

```
{
  "Persist": {
    "Type": "filesystem",
    "RootPath": "C:\\temp\\message-queue\\persist"
  },
  "Authorization": {
    "Type": "Basic",
    "Clients": [
      {
        "Id": "client1",
        "Secret": "pas3w0rd1"
      },
      {
        "Id": "client2",
        "Secret": "pas3w0rd2"
      }
    ]
  }
}
```

The queues are managed by default `in memory`. This guarantees a high performance. If the application is terminated (reused/recycled), all queues are also deleted.
An alternative is the *persistence* of message queues. Only `filesystem` can be specified as `Type` here at the moment. The entries are additionally stored in the path specified under 'RootPath'.
If the application is terminated and restarted later, the application restores the last state. In the case of very long queues, the application's start-up process can be take a few more seconds.

The access to the message queue is open to all accesses via the *REST API*. If you want to restrict access to clients, they can be listed under `Authorization`.
Only `Basic` is currently offered as a type. All *Clients* have the same rights and full access to the queues.

Access is via `Basic Authentication` and can be specified via the *HTTP Header* (`Authencation: Basic base64(id:secret)`) or url format (`https://clientId:clientSecret@my-server`). 

## REST Interface

The following functions are provided by the API. The unique name `{id}` of a queue always corresponds to the unique name of the queue:

**[PUT] /queue/enqueue/{id}**

Adds new values to a queue. The values are passed as *JSON String Array*:

```
[
  "message1", "message2"
]
```

**[GET] /queue/dequeue/{id}** optional: ?count={**1**}&register={true/**false**}

Getting values from a waiting snake. The number of values can be specified via `count` (default: 1). If messages are picked up from a queue that does not exist,
an empty array `[]` is returned. This does not create a new queue with the name `{id}`. 
If you want to query a queue and at the same time create this queue even if it does not already exist, this is ensured by the parameter `register=true`.
The queue is therefore still empty, however, other processes can use '/queue/queuenames' (see below) to determine that this queue has already been queried and that thers is a process to handle the messages.

When values are queried using this method, they automatically disappear from the queue and are no longer available for other processes.

**[GET] /queue/all/{id}**

Returns all the values of a queue without affecting it.

**[GET] /queue/length/{id}**

Returns the number of messages in a queue without affecting it.

**[GET] /queue/remove/{id}**

Removes am message queue

**[GET] /queue/register/{id}**

Creates a queue with no values (if the queue does not already exist). Other processes can then see the queue using `/queue/queuenames`.
If the queue already exists, this method can be used to change the properties of the queue.

- **lifetimeSeconds <int>**: If the queue is empty for longer than the time specified here, it is automatically deleted. a value of ``0`` means that the queue is never automatically deleted
- **itemLifetimeSeconds <int>**: If a message is not picked up under the time span shown here, it is automatically deleted. A value of ``0`` means that messages are never automatically deleted.
- **suspendEnqueue <bool>**: If you set this werr to ``true``, no more values can be added to this queue until the value is set back to ``false``.
- **suspendDequeue <bool>**: If you set this value to ``true``, no messages can be retrieved from this queue. The messages are not deleted. If you set the value back to ``false``, the queue can be processed again.

**[GET] /queue/properties/{id}**

Lists the properties of a queue or returns them as JSON.

**[GET] /queue/queuenames**

Returns the names (ids) of all queues

[Client Library](../client/client_en.md)