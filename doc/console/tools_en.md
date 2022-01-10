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

## MessageQueueNET.ProcService.exe

This tool monitors a queue and starts a process (e.g. shellscript) to which the received message is passed as arguments:

```
Usage: MessageQueueNET.ProcService.exe serviceUrl -q <queueName> -c <comand> {-p <max-parallel-tasks=1> -qsize <queuesize=0> -duration <seconds> | -stoptime <time>  }
```

The first parameter must always be the url to the REST API. 

The other options are:

* `-q`:
   the name (Id) of the queue that should be monitored

* `-c`: 
   the process that should be started as soon as an entry appears on the queue. A '.exe' or '.bat' file (Windows) or any other executable can be specified as a process.
   The message from the queue is passed to the process as arguments.

* `-p`: 
   If there are multiple messages in the queue, this parameter can be used to specify how many processes can run in parallel. If you do not specify this value, only one process is executed at a time.
   All other processes are queued and processed sequentially.

* `-qsize`: 
  The processes are executed one after the other, depending on the degree of parallelization. 
  The maximum length of this process queue can be specified here. Here you can determine how many values are collected from the *REST API Queue at a time.
  The REST API is queried once per second and then messages are processed.  If a process can run so quickly that multiple messages can be processed within a second, the length of the 
  process queue specified here should be greater than the number of parallel processes. Otherwise, if the process queue shoud have the same size as the maximum parallelization level (default) 

* `-duration`:
  The tool usually runs endlessly. This parameter can be used to specify the seconds until the monitoring is automatically terminated. After that, no new messages will be collected.
  The tool is still waiting on running processes to finalize. All processes in the process queue are *cancelled* and the corresponding messages are written back to the REST API queue. After that, the tool will finish running.

* `-stoptime`:
   As an alternative to the '-duration', you can also specify the exact time when the tool stops monitoring the queue. Possible inputs are: 

   * 09:00 or 14:30  (>= 00:00 and < 24:00)
   * 07:00 pm  (12-hour formatted)

Example:

```
./MessageQueueNET.ProcService.exe http://my-queue-server -q my-queue-1 -c c:\temp\dummy.bat -p 3 -stoptime "04:00 pm"
```