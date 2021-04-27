# Kommandozeilen Tools

Das Softwarepaket stellt noch Kommandozeilen Tools zur Verfügung, über die auf die Warteschlangen zugegriffen werden kann. Vorkompilierte Tool setzen eine .NET Core 5.0 Laufzeitumgebung voraus.

## MessageQueueNET.Cmd.exe

Mit diesem Tool können einzelnen Befehle an die Warteschlange übergeben werden (zB: Füge eine neue Nachricht ein)
Der Aufruf lautet folgendermaßen:

```
Usage: MessageQueueNET.Cmd.exe serviceUrl -q queueName -c comand {-m message | -f messages-file}
       command: remove, enqueue, dequeue, length, queuenames, register, all
```

`serviceUrl`, `queneName` und `command` müssen immer angeben werden. Verwende die API Autorisierung, kann diese über die Url angeben werden (`https://clientId:clientSecret@my-server/...`).

Sollten Nachrichten einer Warteschleife hinzugefügt werden, kann dies nach der Option `-m` angeführt werden. Sollte mehrere Nachrichten gleichzeit übergeben werden,
kann mit der Option `-f` ein Textfile mit den Nachrichten übergeben werden. Jede Zeile im Textfile entspricht dabei einer Nachricht. Leerzeilen werden ignoriert.

Als Kommandos können die oben angeführte Werte hinter der Option `-c` angegeben werden. Die bedeutung der einzelnen Kommandos wurden bei der REST API Beschrieben.

Beispiel

```
./MessageQueueNET.exe http://my-queue-server -q my-queue-1 -c enqueue -m "Message 1"
```

## MessageQueueNET.ProcService.exe

Dieses Tool überwacht eine Warteschlange und startet einem Prozess (zB Shellscript) dem es die erhalte Nachricht als Parameter übergibt:

```
Usage: MessageQueueNET.ProcService.exe serviceUrl -q <queueName> -c <comand> {-p <max-parallel-tasks=1> -qsize <queuesize=0> -duration <seconds> | -stoptime <time>  }
```

Als erster Parameter muss immer die Url zur REST API angegeben werden. 

Wei weiteren Optionen lauten:

* `-q`:
   der Name (Id) der Warteschlange die überwacht werden sollte

* `-c`: 
   der Prozess, der gestartet werden sollte, sobald ein Eintrag in der Warteschleife auftaucht. Als Prozess kann hier wieder ein eine `.exe` oder `.bat` Datei (Windows) oder jede andere ausführbare Datei angegeben werden.
   Die Nachricht aus der Warteschlange wird als Argument an den Prozess übergeben

* `-p`: 
   Befinden sich mehrere Nachrichten in der Wartschlage, kann über diesen Parameter angegeben werden, wie viele Prozesse parallel ausgeführt werden dürfen. Gibt man diesen Wert nicht an, wird immer nur ein Prozess gleichzeitig ausgeführt.
   Alle weiteren Prozessen wirden in einer Warteschleife gestellt und sequentiell abgearbeitet.

* `-qsize`: 
   Die Prozesse werden je nach Parallelisierungsgrad hintereinader ausgeführt. Die maximale Länge dieser Prozesswarteschlange kann hier angegeben werden. So bestimmt werden, wie viel Werte maximal von der *REST API Queue* abgeholt werden
   darf. Die REST API wird 1 mal pro Sekunde abgefragt und danach Nachrichten abgearbeitet. Ist der Prozess so schnell ausgeführt werden, dass mehrerer Nachrichten innerhalb einer Sekunde bearbeiten lassen. Sollte die Länge der 
   hier angegeben Prozesswarteschlange größer als die anzahl der parallellen Prozesse sein. Ansonsten reicht es, wenn die die Prozesswarteschleife gleich groß ist, wie die maximale Parallelisierungsgrad (default) 

* `-duration`:
   Das Tool läuft in der Regel endlos. Mit diesem Parameter können die Sekunden bis zum automatischen Beenden der Überwachung angegeben werden. Nach werden keine neuen Werte mehr abgeholt. Das Tool wartet noch
   auf die bereits laufenden Prozesse. Alle Prozesse in der Prozesswarteschleife werden *gecancelled* und die entsprechenden Nachrichten wieder zurück in die REST API Queue geschrieben. Danach endet der Ausführung des Tools.

* `-stoptime`:
   Alternativ zur `-duration` kann auch der Genaue Zeitpunkt angegeben werden, da den das Tool die Überwachung der Warteschlange beendet. Mögliche Eingaben sind: 

   * 09:00 or 14:30  (>= 00:00 and < 24:00)
   * 07:00 pm  (12-hour formatted)

