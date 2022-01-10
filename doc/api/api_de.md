# REST API

Die Rest API ist eine .NET Core API Web Anwendung. Die Anwendung läuft sowohl im *Microsoft IIS*, *Standalone* (dotnet MessageQueueNET.dll), oder als *Docker Container*.

Die API stellt Methoden zur Verfügung, mit denen Queues (Warteschlangen) erstellt und abgefragt werden können. Es können beliebige Warteschlangen angelegt werden, die durch
einen eindeutigen Namen (beliebige Zeichenkette) unterschieden werden. Jede diese Warteschlangen von unterschiedlichen Prozessen abgearbeitet werden.

## Konfiguration

Die Konfiguration erfolgt im *JSON* Format in der *appsettings.json* oder in der *_config/message-queue.json* Datei. Empfohlen wird, die hier gezeigten Einstellungen in der
*_config/message-queue.json* Datei durchzuführen, da diese beim Updates nicht überschrieben wird. Beim einem Update kann alles außer dem *_config* Verzeichnis über eine
bestehende Installtation kopieren werden.

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

Die Warteschlangen werden per default `in Memory` verwaltet. Das garantiert eine hohe Performance. Wird die Applikation beendet (wiederverwendet/recycled) sind alle Warteschlagen ebenfalls gelöscht.
Ein Ausweg ist die *Persistierung* von Warteschlagen. Als `Type` kann hier derzeit nur `filesystem` angegeben werden. Die einzelnen Einträge werden damit zusätzlich in dem unter `RootPath` angegeben Pfad gespeichert.
Wird die Anwendung beendet und später wieder neue gestartet, baut die Applikation den letzten Zustand wieder her. Bei sehr langen Warteschlagen, kann der Startvorgang der Applikation dadurch einige Sekunden 
in Anspruch nehmen.

Der Zurgriff auf die Wartschlange ist über die *REST API* für alle Zugriffe offen. Möchte man die Zugriffe auf Clients beschränken, können diese unter `Authorization` angeführt werden. 
Als Type wird derzeit nur `Basic` angeboten. Es können beliebige *Clients* angeführt werden. Alle *Clients* haben die selben Rechte und vollen Zugriff auf die Warteschlangen.

Der Zugriff erfolgt über `Basic Authentication` und kann über die entsprechende *Header Variable* (`Authencation: Basic base64(id:secret)`) oder über die Url (`https://clientId:clientSecret@my-server`) angegeben werden. 

## REST Schnittstelle

Folgende Funktionen werden über die API angeboten. `{id}` entspricht dabei immer dem eindeutigen Namen einer Warteschlange:

**[PUT] /queue/enqueue/{id}**

Fügt einer Warteschlange neue Werte hinzu. Die Werte werden als *JSON String Array* übergeben:

```
[
  "message1", "message2"
]
```

**[GET] /queue/dequeue/{id}** optional: ?count={**1**}&register={true/**false**}

Holt sich Werte aus einer Wartschlange. Die Anzahl der Werte kann über `count` angegeben werden (default: 1). Werden Nachrichten von einer Warteschlange abgeholt, die nicht existiert,
wird ein leeres Array `[]` zurück gegeben. Dadurch wird keine neue Warteschlange mit der angebeben *Id* angelegt. Möchte man eine Warteschlange abfragen und gleichzeitig 
diese Warteschlange auch erstellen, wenn diese noch nicht vorhanden ist, wird das über den Parameter `register=true` gewährleistet. Die Warteschlange ist damit zwar noch immer leer,
anderer Prozesse können aber über `/queue/queuenames` (siehe unten) ermitteln, dass diese Warteschlange bereits abgefragt wurde und ein Prozess für das abarbeiten bereit steht.

Werden über diese Methode Werte abgefragt, verschwinden diese automatisch aus der Warteschlange und stehen für anderer Prozesse nicht mehr zur Verfügung.

**[GET] /queue/all/{id}**

Gibt alle Werte einer Warteschlange zurück ohne diese zu beeinflussen.

**[GET] /queue/length/{id}**

Gibt die Anzahl der Nachrichten in einer Warteschlange zurück, ohne diese zu beeinflussen.

**[GET] /queue/remove/{id}**

Löscht eine Warteschlange

**[GET] /queue/register/{id}**

Erstellt eine Warteschlange ohne Werte (falls die Wartschlange noch nicht vorhanden ist). Anderere Prozesse können die Warteschlange danach über `/queue/queuenames` sehen.
Existiert die Wartschlange bereits, hat diese Methode keinen Effekt.

dabei können zusätzliche Parameter übergeben werden: 

- **lifetimeSeconds <int>**: Ist die Liste länger als die hier angegeben Zeitspanne leer, wird sich automatisch gelöscht. Ein Wert ``0`` bedeutet, dass die Liste niemals automatisch gelöscht wird 
- **itemLifetimeSeconds <int>**: Wird eine Nachricht nicht unter der hier angezeigten Zeitspann abgeholt, wird sie automatisch gelöscht. Ein Wert ``0`` bedeutet, dass Nachrichten nie automatisch gelöscht werden.
- **suspendEnqueue <bool>**: Setzt man diesen Werr auf ``true`` können dieser Liste eine Werte mehr hinzugefügt werden, bis der Wert wieder auf ``false`` gesetzt wird.
- **suspendDequeue <bool>**: Setzt man diesen Wert auf ``true`` können aus dieser Liste keine Nachrichten abgeholt werden. Die Nachrichten werden dabei nicht gelöscht. Setzt man Wert wieder auf ``false`` kann die Warteschlange wieder abgearbeitet werden.

**[GET] /queue/properties/{id}**

Listet die Eigenschaften einer Warteschlange auf bzw. gibt diese als JSON zurück.

**[GET] /queue/queuenames**

Gibt die Namen (Ids) aller Warteschlangen zurück

[Client Library](../client/client_de.md)