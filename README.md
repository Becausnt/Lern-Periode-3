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
- [ ] Nachrichtenaustausch beginnen.

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
- [ ] Chat funktionalität hinzufügen
- [ ] Fähigkeit für mehrere Clients, und Client Namen hinzufügen
- [ ] Simple Server Commands hinzufügen
- [ ] bugfixes von  letztem mal

## Testfall
```
Client:
------
Connect: {IP:PORT}
Name: {Name}
[CLIENT LOG]: Connected to {IP:PORT} as {Name}

Message: {Message}
[{Name}]: {Message}
[SERVER]: {Name} has been banned.


Server-Side:
-----------
[SERVER-LOG]: xxx.xxx.xx.xx:{name} connected
name: {Message}
ban {name}
```

✍️ Heute am 23.1 habe ich... (50-100 Wörter)

☝️ Vergessen Sie nicht, bis zum 23.1 Ihren fixfertigen Code auf github hochzuladen, und in der Spalte Erfüllt? einzutragen, ob Ihr Code die Test-Fälle erfüllt

# 30.1.2024 Arbeitspakete
- [ ] Bugfixes
- [ ] Admin-role
- [ ] Password Auth
- [ ] Sauberer Code

✍️ Heute am 23.1 habe ich... (50-100 Wörter)

Reflexion
Formen Sie Ihre Zusammenfassungen in Hinblick auf Ihren VBV zu einem zusammenhängenden Text von 100 bis 200 Wörtern (wieder mit Angabe in Klammern).

Verfassen Sie zusätzlich einen kurzen Abschnitt, in welchem Sie über die Länge der Projekte reflektieren: Fanden Sie die 9-wöchtige LP2 oder die 4-wöchige LP3 angenehmer? Was bedeutet das für Ihre Planung der zukünftigen LP?
