# Commandline Tools

The software package provides command line tools that allow access to the queues. Precompiled tools require a .NET Core 5.0 runtime environment.

## MessageQueueNET.Cmd.exe

This tool can be used to pass individual commands to the queue (e.g. insert a new message)
Use it as follows:

```
Usage: MessageQueueNET.Cmd.exe serviceUrl -q queueName -c comand {-m message | -f messages-file}
       command: remove, enqueue, dequeue, length, queuenames, register, all
```

`serviceUrl`, `queneName` and `command` are required. If you use the API authorization, it can be done using url-format (`https://clientId:clientSecret@my-server/...`).

If messages are added to a queue, this can be done with the `-m` option. If multiple messages are passed at the same time,
you can pass a text file with the messages with the `-f` option. Each line in the text file corresponds to a message. Blank lines are ignored.

The above values can be specified as commands after the `-c` option. The meaning of each command was described in the REST API documentation.

Example

```
./MessageQueueNET.Cmd.exe http://my-queue-server -q my-queue-1 -c enqueue -m "Message 1"
```

### MessageQueueNET.Cmd Shell

The 'shell' command starts a simple shell. With the shell you can send commands to a MessageQueueNET instance.

```
./MessageQueueNET.Cmd.exe http://my-queue-server -c shell
```

Example:

```
MessageQueueNET Shell
Type help for help...
>>
>> queue1 register
>> queue1 properties
LastAccessUTC: 10.01.2022 13:59:03
Length: 0
LifetimeSeconds: 0
ItemLifetimeSeconds: 0
SuspendEnqueue: False
SuspendDequeue: False
>>
>> queuenames
queue1
>>
>> queue1 enqueue -m "message #1" -m "message #2"
>>
>> queue1 all
message #1
message #2
>>
>> exit
```