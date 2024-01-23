using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class ChatClient
{
    private static TcpClient _client;
    private static NetworkStream _stream;

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        string server;
        int port;
        _client = new TcpClient();

        try
        {
            do {
                try
                {
                    Console.WriteLine("Enter Server IP:");
                    server = Console.ReadLine();
                    Console.WriteLine("Enter Server port:");
                    port = Convert.ToInt16(Console.ReadLine());
                    _client.Connect(server, port);
                    _stream = _client.GetStream();
                    Console.WriteLine("Connected to the chat server.");
                } catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENT]: Connection failed: {ex.Message}");
                }
            } while(!_client.Connected);
            // Ask the user to choose a name
            string name;
            do
            {
                Console.Write("Choose a name: ");
                name = Console.ReadLine().Trim();
            } while (String.IsNullOrEmpty(name));

            SendMessage(name);

            // Start a thread for receiving messages from the server
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            // Read messages from the console and send to the server
            while (true)
            {
                string messageToSend = Console.ReadLine();
                if (!string.IsNullOrEmpty(messageToSend))
                {
                    // don't print the password if the user is Authenticating 
                    if (!messageToSend.ToUpper().StartsWith("/AUTH"))
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.WriteLine($"「{name}」: {messageToSend}");
                    }
                    else
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.WriteLine("                              ");
                    }
                    if (messageToSend.StartsWith("/"))
                    {
                        switch (messageToSend.ToUpper().Split(' ')[0])
                        {
                            case "/HELP":
                                Console.WriteLine("COMMANDS:");
                                Console.WriteLine("/HELP:\t\t\tShows this.");
                                Console.WriteLine("/DM {username}:\t\tSend private message to someone.");
                                Console.WriteLine("/LIST:\t\t\tLists all online users.");
                                Console.WriteLine("/LEAVE:\t\t\tLeaves chat");
                                Console.WriteLine("/CAT:\t\t\tShows client-side ASCII cat.");
                                Console.WriteLine("/AUTH {password}:\tAuthenticate as admin");
                                Console.WriteLine("/CLEAR\t\t\tClears Console.");
                                break;
                            case "/DM":
                                SendMessage(messageToSend);
                                break;
                            case "/LIST":
                                // has it's own case in case i want to add something to it
                                SendMessage(messageToSend);
                                break;
                            case "/LEAVE":
                                _client.Close();
                                return;
                            case "/CAT":
                                Console.WriteLine("           __..--''``---....___   _..._    __\r\n /// //_.-'    .-/\";  `        ``<._  ``.''_ `. / // /\r\n///_.-' _..--.'_    \\                    `( ) ) // //\r\n/ (_..-' // (< _     ;_..__               ; `' / ///\r\n / // // //  `-._,_)' // / ``--...____..-' /// / //");
                                break;
                            case "/AUTH":
                                try
                                {
                                    string hash = GetSha512Hash(messageToSend.Substring(6));
                                    SendMessage($"/AUTH {hash}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("[CLIENT]: Failed to Authenticate");
                                }
                                break;
                            case "/CLEAR":
                                Console.Clear();
                                break;
                            default:
                                SendMessage(messageToSend);
                                break;
                        }
                    }
                    else
                    {
                        SendMessage(messageToSend);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Client exception: {e.Message}");
        }
        finally
        {
            _client.Close();
        }
    }

    static void SendMessage(string message)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        _stream.Write(buffer, 0, buffer.Length);

    }

    static void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // Server has disconnected
                    Console.WriteLine("Server has disconnected.");
                    break;
                }

                string receivedMessage = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                Console.WriteLine(receivedMessage);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Receiving exception: {e.Message}");
        }
    }
    static string GetSha512Hash(string input)
    {
        using (SHA512 sha512 = SHA512.Create())
        {
            byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

    }
}
