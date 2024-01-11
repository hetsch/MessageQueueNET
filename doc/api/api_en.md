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

## REST Interface (Info)

### /Info

- **Description**: Retrieves information about the MessageQueue.NET API.
- **Method**: `GET`

#### Example

**Request**

```http
GET /Info
```

**Response**

```json
{
  "success": true,
  "version": {
    "major": 1,
    "minor": 0,
    "build": 0,
    "revision": 1,
    "majorRevision": 0,
    "minorRevision": 1
  }
}
```

## REST Interface (Queue)

The following functions are provided by the API. The unique name `{id}` of a queue always corresponds to the unique name of the queue.

The **MessageQueue.NET** API provides functionality to interact with message queues. 
In this context, an `id` is a unique identifier used to specify a queue. 
It can only consist of lowercase letters, digits, underscores (`_`), hyphens (`-`), and periods (`.`). 
For example, valid `id` values could be `queue1`, `test.queue`, or `my_queue-123`. 

The API also allows users to query multiple queues at once using an `idPattern`. This pattern can be a comma-separated list of queue identifiers, a wildcard expression, or a combination of both. For instance, `queue1,queue2` would target both `queue1` and `queue2`, while `test.*` would target all queues whose `id` starts with `test.`. A combined pattern like `queue1,test.*` would target `queue1` and all queues starting with `test.`.

### /Queue/dequeue/{idPattern}

Getting values from a waiting snake. The number of values can be specified via `count` (default: 1). If messages are picked up from a queue that does not exist,
an empty array `[]` is returned. This does not create a new queue with the name `{id}`. 
If you want to query a queue and at the same time create this queue even if it does not already exist, this is ensured by the parameter `register=true`.
The queue is therefore still empty, however, other processes can use '/queue/queuenames' (see below) to determine that this queue has already been queried and that thers is a process to handle the messages.

When values are queried using this method, they automatically disappear from the queue and are no longer available for other processes.

- **Method**: `GET`
- **Parameters**:
  - `idPattern`: String (path, required) - Pattern to match queue IDs.
  - `count`: Integer (query, optional, default=1) - Number of messages to dequeue.
  - `register`: Boolean (query, optional, default=false) - Indicates whether to register the queue if it doesn't exist.

#### Example

**Request**

```http
GET /Queue/dequeue/test.*
```

**Response**

```json
{
  "success": true,
  "messages": [
    {
      "queue": "test.queue1",
      "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
      "value": "Hello World",
      "creationDateUTC": "2023-11-03T15:01:02.123Z",
      "requireConfirmation": false
    }
  ]
}
```

### /Queue/confirmdequeue/{id}

If the property ``ConfirmationPeriodSeconds`` is defined for the queue (>0), a message must be confirmed within the
time span. Otherwise, the message is automatically added back to the queue.
This method confirms that the message has arrived and been successfully processed. With this method, the
message finally removed from the queue.

- **Method**: `GET`
- **Parameters**:
  - `id`: String (path, required) - ID of the queue.
  - `messageId`: UUID (query, required) - ID of the message to confirm.

#### Example

**Request**

```http
GET /Queue/confirmdequeue/test.queue1?messageId=d290f1ee-6c54-4b01-90e6-d701748f0851
```

**Response**

```json
{
  "success": true
}
```

### /Queue/enqueue/{id}

- **Description**: Enqueues messages into a specified queue.
- **Method**: `PUT`
- **Parameters**:
  - `id`: String (path, required) - ID of the queue.
- **Request Body**: Array of strings representing the messages.

#### Example

**Request**

```http
PUT /Queue/enqueue/test.queue1
Content-Type: application/json

["Hello World", "Test Message"]
```

**Response**

```json
{
  "success": true
}
```

### /Queue/all/{idPattern}

Returns all the values of a queue without affecting it.
The parameter ``max`` and ``unconfirmedOnly`` are optional.
``max``: the maximum number of results to be returned
``unconfirmedOnly``: only messages are returned that have been picked up but not yet by
Client have been confirmed. Only queues with ``confirmationPeriodSeconds>0`` can return values here.

- **Method**: `GET`
- **Parameters**:
  - `idPattern`: String (path, required) - Pattern to match queue IDs.
  - `max`: Integer (query, optional, default=0) - Maximum number of messages to retrieve.
  - `unconfirmedOnly`: Boolean (query, optional, default=false) - Indicates whether to retrieve only unconfirmed messages.

#### Example

**Request**

```http
GET /Queue/all/test.*
```

**Response**

```json
{
  "success": true,
  "messages": [
    {
      "queue": "test.queue1",
      "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
      "value": "Hello World",
      "creationDateUTC": "2023-11-03T15:01:02.123Z",
      "requireConfirmation": false
    }
  ]
}
```

### /Queue/length/{idPattern}

- **Description**: Retrieves the length of the specified queue(s).
- **Method**: `GET`
- **Parameters**:
  - `idPattern`: String (path, required) - Pattern to match queue IDs.

#### Example

**Request**

```http
GET /Queue/length/test.*
```

**Response**

```json
{
  "success": true,
  "queues": {
    "test.queue1": {
      "queueLength": 1,
      "unconfirmedItems": 0
    }
  }
}
```

### /Queue/remove/{idPattern}

- **Description**: Removes the specified queue(s) or messages within them.
- **Method**: `GET`
- **Parameters**:
  - `idPattern`: String (path, required) - Pattern to match queue IDs.
  - `removeType`: Integer (query, required) - Type of removal (0: Remove Queue, 1: Remove All Items, 2: Remove Unconfirmed Items).

#### Example

**Request**

```http
GET /Queue/remove/test.*?removeType=0
```

**Response**

```json
{
  "success": true
}
```

### /Queue/deletemessage/{id}

Delete a single message from an queue

- **Method**: `GET`
- **Parameters**:
  - `id`: String (path, required) - ID of the queue.
  - `messageId`: UUID (query, required) - ID of the message to confirm.

#### Example

**Request**

```http
GET /Queue/deletemessage/test?messageId=d290f1ee-6c54-4b01-90e6-d701748f0851
```

**Response**

```json
{
  "success": true
}
```

### /Queue/register/{idPattern}

Creates a queue with no values (if the queue does not already exist). Other processes can then see the queue using `/queue/queuenames`.
If the queue already exists, this method can be used to change the properties of the queue.

- **lifetimeSeconds <int>**: 
If the queue is empty for longer than the time specified here, it is automatically deleted. a value of ``0`` means that the queue is never automatically deleted

- **itemLifetimeSeconds <int>**: 
If a message is not picked up under the time span shown here, it is automatically deleted. A value of ``0`` means that messages are never automatically deleted.

- **confirmationPeriodSeconds <int>**: 
This value can be used to ensure that the successful processing of a message from the client
must be confirmed. Otherwise, this message is automatically added back to the queue.
A value ``0`` means that messages do not have to be confirmed and disappear completely from the queue after a ``dequeue``.

- **maxUnconfirmedItems <int>**:
Maximum number than can be dequeued and be in an unconfirmed status. If this number is reached, no ``dequeue`` is possible until items will get confirmed

- **maxUnconfirmedItemsStrategy <int>**:
The strategy, the maximum for unconfirmed items are calculated:
  * 0 ... **Absolute** maxUnconfirmedItems is the absolute value
  * 1 ... **Per Client** maxUnconfirmedItems is per client

- **suspendEnqueue <bool>**: 
If you set this werr to ``true``, no more values can be added to this queue until the value is set back to ``false``.

- **suspendDequeue <bool>**: 
If you set this value to ``true``, no messages can be retrieved from this queue. The messages are not deleted. If you set the value back to ``false``, the queue can be processed again.

- **Method**: `GET`
- **Parameters**:
  - `idPattern`: String (path, required) - Pattern to match queue IDs.
  - `lifetimeSeconds`, `itemLifetimeSeconds`, `confirmationPeriodSeconds`, `maxUnconfirmedItems`: Integer (query, optional) - Queue properties.
  - `suspendEnqueue`, `suspendDequeue`: Boolean (query, optional) - Suspend enqueue or dequeue operations.

#### Example

**Request**

```http
GET /Queue/register/test.*?lifetimeSeconds=3600
```

**Response**

```json
{
  "success": true,
  "queues": {
    "test.queue1": {
      "lastAccessUTC": "2023-11-03T15:01:02.123Z",
      "lastModifiedUTC": "2023-11-03T15:01:02.123Z",
      "creationDateUTC": "2023-11-03T15:01:02.123Z",
      "queueLength": 0,
      "unconfirmedItems": 0,
      "lifespanSeconds": 3600
    }
  }
}
```

### /Queue/queuenames

Returns the names (ids) of all queues


[Client Library](../client/client_en.md)