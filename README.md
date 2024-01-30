# Lern-Periode 3
## V. Hug

## 9.1.2024 bis 30.1.2024 (☃️ Sportferien)
## Grob-Planung
Im Moment bin ich mit meinen Noten zufrieden. Ich habe bis jetzt keine ungenügenden Informatiknoten geschrieben. 
Ich denke das Modul 319 war besonders wichtig, da es die Grundlage zum Programmieren legt. Mein Verbesserungsvorschlag am Ende der LP2 war folgender: 
> Mehr Pausen machen, auch wenn es wirkt als ob man dadurch Zeit verliert ist man danach wieder schneller. Bei einem Fehler vielleicht mehr als einmal schauen ob ich ihn selbst lösen kann, ich denke falls man den Fehler
> dann findet lernt man mehr daraus als wenn man es googelt(allerdings auch nicht Stunden an einem Fehler verbringen)
Ich denke, ein simpler Konsolen-Chatroom, oder auch mit GUI falls ich Zeit dazu habe, wäre ein geeignetes Projekt, da ich damit mal noch etwas praktisches über das Internet lerne.

## ✍️ Heute habe ich... (50-100 Wörter)
Ich habe mir heute viele Youtube-Videos mit etwa 200 Aufrufen und 20 Likes angeschaut. Manche waren sehr aufschlussreich, andere weniger. Ich möchte eine Chat-App mit Sockets programmieren, da ich auch verstehen möchte, was wie funktioniert. In manche Videos wurden vorgebaute Frameworks verwendet, diese möchte ich aber eben nicht verwenden, da sie meiner Meinung nach alles nur unnötig verkomplizieren. Ich kenne den etwaigen Aufbau eines Servers und denke es ist machbar. Ich habe auch schon angefangen, einem Tutorial zum Datenaustausch mit Sockets zu folgen und mein Plan ist es, diesen Datenaustausch so auszubauen, das daraus ein Server entsteht, zu dem mehrere Clients eine Verbindung aufbauen und miteinander Chatten können.

## 9.1.2024 Arbeitspakete
- [x] Lernen, wie simple Chats funktionieren.
- [x] Lernen, wie man in C# einen Chat macht.
- [x] Den Server und Client planen (evt. PAP)


## 16.1.2024 Arbeitspakete
- [x] Wissen, dass ich mir in der letzten Woche angeeignet habe Repetieren.
- [x] Sehr simplen Server Programmieren, noch kein Chat aber Verbindungsfähig
- [x] Sehr simplen Client Programmieren, noch kein Chat aber Verbindungsfähig
- [x] Nachrichtenaustausch beginnen.

## Testfall
```
Client:
------
Connection Successfull

Server-Side
------
[{DateTime.Now}] [CONNECTION]: Client has connected with the username: {Username}"
```

✍️ Heute am 16.1 habe ich... (50-100 Wörter)
Heute habe ich an dem Tutorial weitergearbeitet, und habe einen Teil verstanden und einen Teil nicht verstanden. Je nach dem frage ich dann noch ob mir das jemand erklären kann. Das Programm funktioniert, es hat ein GUI und einen Server, der Server akzeptiert die Verbindung und Outputtet eine Log Nachricht, schliesst sich danach aber. Die Log Nachricht kann ich beliebig gestalten. Bis jetzt läuft alles nur auf dem Localhost aber ich denke das kommt dann später im Tutorial auch noch. Wenn ich mit dem Tutorial durch bin, möchte ich es auch noch selbstständig erweitern.


## Server
Client handling
```C#
        static List<Client> _users;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();


            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);

                /* Broadcast connection to everyone. */
            }


        }
```
Client objekt
```C#
class Client
{
    public string Username { get; set; }
    public Guid UID { get; set; }
    public TcpClient ClientSocket { get; set; }

    public Client(TcpClient client)
    {
        ClientSocket = client;
        UID = Guid.NewGuid();

        Console.WriteLine($"[{DateTime.Now}] [CONNECTION]: Client has connected with the username: {Username}");
    }
}
```
## Client
Klasse die ich so halb verstehe
```C#
namespace ChatApp.MVVM.Core
{
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
```
Verbindung zum Server aufbauen
```C#
        public RelayCommand ConnectToServerCommand {  get; set; }

        private Server _server;
        public MainViewModel()
        {
            _server = new Server();
            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer());
        }
```
Funktion für den Verbindungsaufbau
```C#
    class Server
    {
        TcpClient _client;
        public Server()
            {
                _client = new TcpClient();
            }
        public void ConnectToServer()
        {
            if (!_client.Connected)
            {
                _client.Connect("127.0.0.1", 7891);
            }
        }
    }
```

Der Testfall ist erfüllt.

# 23.1.2024 Arbeitspakete
- [x] Chat funktionalität hinzufügen
- [x] Fähigkeit für mehrere Clients, und Client Namen hinzufügen
- [x] Simple Server Commands hinzufügen
- [x] bugfixes von  letztem mal

## Testfall
```
Enter Server IP:
127.0.0.1
Enter Server port:
8888
Connected to the chat server.
Choose a name: Bruno
Joined the chat successfully.
「Bruno」: Hallo
「Bruno」: /Help
COMMANDS:
/HELP:                  Shows this.
/DM {username}:         Send private message to someone.
/LIST:                  Lists all online users.
/LEAVE:                 Leaves chat
/CAT:                   Shows client-side ASCII cat.
/AUTH {password}:       Authenticate as admin
/CLEAR                  Clears Console.

You have been Kicked.

Server has disconnected.

Server-Side:
-----------
Enter port to run server on.(eg. 8888):
8888
Server started on 192.168.56.1:8888
[SERVER]: Bruno has joined the chat.
「Bruno」: Hallo
/Help
Server Commands:
/KICK {username}|*              Kicks a user from Server, announces kick, can rejoin
/SHADOWKICK {username}|*        Kicks a user from server, hides kick as error, can rejoin
/BAN {username}                 Bans username
/UNBAN {username}|*             Unbans username
/IPBAN {username}               Forbids IP from joining server, watch out not to ban yourself.
/IPUNBAN {username}|*           Allows IP to join server
/RAW {message}                  Sends raw message to clients, don't forget '[Server]' or username in your message.
/LIST                           Lists all online clients
/LISTIP                         Lists all online clients and their IP's
/OP {username}                  Gives a client Admin priviledges without Auth.
/UNOP {username}|*              Removes a clients admin priviledges
/LOCK {password}                Locks commands with Password, clears console.
/CLEAR                          Empties console
/BANLIST                        Lists all banned names and IP's
/PWD                            Change admin Password, also activate admin.
/kick Bruno
[SERVER]: Bruno was kicked.
[CLIENT]: ERROR BY Bruno: Unable to read data from the transport connection: An established connection was aborted by the software in your host machine..
[Bruno] has left the chat.
```
Erfüllt: Ja


✍️ Heute am 23.1 habe ich... (50-100 Wörter)
Heute habe ich sehr viele Befehle hinzugefügt, welche sie oben sehen können, alle diese Befehle sind voll funktionsfähig und einsatzbereit. Zudem kann der client sich als Admin anmelden. Dies tut er mit `/AUTH Passwort`. Das Password wird dann als SHA512-Hash an den Server gesendet, wo es mit dem SHA512-Hash des Passwortes des Servers abgeglichen wird, welches nur in der Server-Konsole mit `/PWD Passwort` geändert werden kann. Dies ist sicherer als Klartext, jeddoch immer noch anfällig für Pass-the-Hash attacken. Die Clients können miteinander Nachrichten austauschen und haben auch Befehle, welche alle auch oben aufgelistet sind. Der Server Funktioniert als Log und Administrator gleichzeitig, er hat die Rechte alle Nachrichten, auch Admin-Promotionen und DM's einzusehen. Falls der Server nur passiv genutzt werden soll kann man ihn jedoch mit `/LOCK Passwort` sperren, was alle Befehle deaktiviert. Zudem habe ich mich vom Tutorial gelöst und jetzt nur noch code den ich selbst geschrieben habe. Ich habe eine Sehr simple Console-Chat Grundstruktur mit einem Tutorial gemacht und damit selbst noch sehr viel selbst ausgebaut. Vom den code-snippets die ich am 16.1 als Beispiele aufgeführt habe, habe ich nur Erfahrung mitgenommen und Ihn nicht genutzt.


# 30.1.2024 Arbeitspakete
- [x] Bugfixes
- [x] Sauberer Code

✍️ Heute am 23.1 habe ich... (50-100 Wörter)
Ich habe heute eine Reflexion geschrieben, einen Bug in meinem Programm gefixt, eine kritische Sicherheitslücke geschlossen und eine lange Zeit nach der aktuellsten Version meines Projektes aus der LP2 gesucht.

## Reflexion
Ich finde ich habe in dieser Lernperiode gut und effizient gearbeitet. Ich habe meine Projekt rechtzeitig abgeschlossen und mich an meine früheren VBV's gehalten und damit meine Arbeitseffizienz erhöht. Als weiteren VBV an mich selbst habe ich folgendes: Manchmal während dem Programmieren, inzwischen weniger als am Anfang, wenn ich über ein Problem nachdenke schweife ich mit den Gedanken ab und starre Löcher in meinen Monitor. Dies dauert aber meist nur kurze Zeit an, ist also nciht so ein grosses Problem.
Von der Länge her, fand ich bisher alle LP's angenehm, da wir am Anfang(also in der LP-1) auch länger gebraucht haben um etwas zu Programmieren war es gut, das dies auch eine Längere LP war. Nun schaffen wir (wir = meine IMS-Klasse) es aber schon kompliziertere Projekte als in der LP-1 in kürzerer Zeit auf die Beine zu stellen. So lange die LP's also nicht zu schnell kürzer werden, finde ich es sogar gut das sie kürzer werden, da dadurch auch wirklich die Effizienz der Schüler gefordert wird.
