# MessageQueueNET

Eine *Message Queue API* komplett in .NET Core entwickelt.

Message Queues stellen ein asynchrones Kommunikationsprotokoll bereit, wobei Sender und Empfänger von Nachrichten nicht gleichzeitig mit der Nachrichtenwarteschlange interagieren.
Nachrichten in der Wartschlange werden nur so lange gespeichert, bis der Empfänger diese abruft. Nach dem Abrufen wird eine Nachricht wieder gelöscht.
Jede Nachricht wird außerdem nur von einem einzelnen Empfänger abgeholt und verarbeitet.

*MessageQueueNET* stellt für dieses Protokoll folgende Werkzeug bereit:

* Web API: die eigentlich *Messsage Queue*
* Client Library: Eine .NET Standard Bibliothek zur interaktion mit der API
* Kommandozeilenwerkzeuge: Zugriff auf die *Message Queue* über die Kommandozeile

[Web API](api/api_de.md)