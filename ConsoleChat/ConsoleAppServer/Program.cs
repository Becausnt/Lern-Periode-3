using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class ChatServer
{
    private static TcpListener _server;
    private static readonly ConcurrentDictionary<string, TcpClient> clients = new ConcurrentDictionary<string, TcpClient>();
    private static List<string> bannedNames = new List<string>();
    private static List<IPAddress> bannedIPs = new List<IPAddress>();
    private static List<TcpClient> adminList = new List<TcpClient>();
    private static string pwdHash;
    //「Client2」
    static void Main()
    {
        // set up Console
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.Title = "Simple TCP Chat Server";
        

        // set up Server
        int port = 0;
        bool serverStarted = false;
        do
        {
            try
            {
                Console.WriteLine("Enter port to run server on.(eg. 8888):");
                port = Convert.ToInt16(Console.ReadLine());
                _server = new TcpListener(IPAddress.Any, port);
                _server.Start();
                serverStarted = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER]: Failed to start Server | {ex}");
            }
        } while (!serverStarted);
        Console.WriteLine($"Server started on {GetLocalIPAddress()}:{port}");

        pwdHash = "0".ToUpper(); //Unachievable Sha512 hash
        bannedNames.Add("*");

        //Start Server handling in a seperate thread
        Thread serverThread = new Thread(Server);
        serverThread.Start();
        
        while (true)
        {
            string command = Console.ReadLine();
            string commandUpper = command.ToUpper();
            if (!string.IsNullOrEmpty(command))
            {
                // handle Commands
                if (commandUpper.StartsWith("/HELP"))
                {
                    Console.WriteLine("Server Commands:");
                    Console.WriteLine("/KICK {username}|*\t\tKicks a user from Server, announces kick, can rejoin");
                    Console.WriteLine("/SHADOWKICK {username}|*\tKicks a user from server, hides kick as error, can rejoin");
                    Console.WriteLine("/BAN {username}\t\t\tBans username");
                    Console.WriteLine("/UNBAN {username}|*\t\tUnbans username");
                    Console.WriteLine("/IPBAN {username}\t\tForbids IP from joining server, watch out not to ban yourself.");
                    Console.WriteLine("/IPUNBAN {username}|*\t\tAllows IP to join server");
                    Console.WriteLine("/RAW {message}\t\t\tSends raw message to clients, don't forget '[Server]' or username in your message.");
                    Console.WriteLine("/LIST\t\t\t\tLists all online clients");
                    Console.WriteLine("/LISTIP\t\t\t\tLists all online clients and their IP's");
                    Console.WriteLine("/OP {username}\t\t\tGives a client Admin priviledges without Auth.");
                    Console.WriteLine("/UNOP {username}|*\t\tRemoves a clients admin priviledges");
                    Console.WriteLine("/LOCK {password}\t\tLocks commands with Password, clears console.");
                    Console.WriteLine("/CLEAR\t\t\t\tEmpties console");
                    Console.WriteLine("/BANLIST\t\t\tLists all banned names and IP's");
                    Console.WriteLine($"/PWD\t\t\t\tChange admin Password, also activate admin.");
                }
                else if (commandUpper.StartsWith("/LISTIP"))
                {
                    string clientList = "";
                    foreach (KeyValuePair<string, TcpClient> kvp in clients)
                    {
                        clientList += $"\n\t「{kvp.Key}」: {GetIpAddressFromTcpClient(kvp.Value)}";
                    }
                    Console.WriteLine($"Online Clients:\n{clientList}");
                }
                else if (commandUpper.StartsWith("/LIST"))
                {
                    string clientList = "";
                    foreach (KeyValuePair<string, TcpClient> kvp in clients)
                    {
                        clientList += $"\n\t「{kvp.Key}」";
                    }
                    Console.WriteLine($"Online Clients:\n{clientList}");
                }
                else if (commandUpper.StartsWith("/CLEAR"))
                {
                    Console.Clear();
                }
                else if (commandUpper.StartsWith("/BANLIST"))
                {
                    string bannedList = "Banned IP's:";
                    foreach (IPAddress ip in bannedIPs)
                    {
                        bannedList += $"\n\t「{ip}」";
                    }
                    bannedList += "\nBanned Names:";
                    foreach (string name in bannedNames)
                    {
                        bannedList += $"\n\t「{name}」";
                    }
                    Console.WriteLine($"[/BANLIST]: \n{bannedList}");
                }
                else if (!string.IsNullOrEmpty(command.Substring(command.Split(" ")[0].Length))) // check if the argument isn't empty
                {
                    switch (commandUpper.Split(' ')[0])
                    {
                        case "/KICK":
                            // CHECK IF KICKING * OR USER
                            if (command.Substring(6) != "*")
                            {
                                try
                                {
                                    SendMessage(clients[command.Substring(6)], "\nYou have been Kicked.\n");
                                    clients[command.Substring(6)].Close();
                                    clients.TryRemove(command.Substring(6), out _);
                                    Broadcast($"[SERVER]: {command.Substring(6)} was kicked.", String.Empty);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[/KICK]: Failed to kick {command.Substring(6)} | {ex}");
                                }
                            }
                            else
                            {
                                try
                                {
                                    Broadcast("[SERVER]: Kicking all", String.Empty);
                                    foreach (TcpClient targetClient in clients.Values)
                                    {
                                        targetClient.Close();
                                        clients.TryRemove(command.Substring(6), out _);
                                    }
                                    Console.WriteLine($"[SERVER]: Kicked *");
                                }
                                catch (Exception ex) { Console.WriteLine($"[/KICK]: * Failed | {ex}"); }
                            }
                            break;
                        case "/SHADOWKICK":
                            if (command.Substring(12) != "*")
                            {
                                try
                                {
                                    SendMessage(clients[command.Substring(12)], "[SERVER]: System.StackOverflowException was unhandled An unhandled exception of type 'System.StackOverflowException' occurred in mscorlib.dll {Cannot evaluate expression because the current thread is in a stack overflow state.}"); // Random Error i found on stackoverflow
                                    clients[command.Substring(12)].Close();
                                    clients.TryRemove(command.Substring(12), out _);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[/KICK]: Failed to kick {command.Substring(6)} | {ex}");
                                }
                            } else
                            {
                                Broadcast("[SERVER]: System.StackOverflowException was unhandled An unhandled exception of type 'System.StackOverflowException' occurred in mscorlib.dll {Cannot evaluate expression because the current thread is in a stack overflow state.}", String.Empty); // Random Error i found on stackoverflow
                                foreach (TcpClient targetClient in clients.Values)
                                {
                                    targetClient.Close();
                                    clients.TryRemove(command.Substring(6), out _);
                                }
                            }
                            break;
                        case "/BAN":
                            try
                            {
                                string clientName = command.Substring(5);
                                bannedNames.Add(clientName);
                                clients[clientName].Close();
                                clients.TryRemove(clientName, out _);
                                Broadcast($"[SERVER]: {clientName} was banned", String.Empty);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[/BAN]: Failed to ban {command.Substring(5)} | {ex}");
                            }
                            break;
                        case "/UNBAN":
                            try
                            {
                                if (command.Substring(7) != "*")
                                {
                                    bannedNames.Remove(command.Substring(7));
                                    Console.WriteLine($"[/UNBAN]: Unbanned {command.Substring(7)}");
                                } else
                                {
                                    bannedNames.Clear();
                                    Console.WriteLine($"[/UNBAN]: Unbanned *");
                                }
                                if (!bannedNames.Contains("*")) { bannedNames.Add("*"); }
                            }
                            catch (Exception ex) { Console.WriteLine($"[/UNBAN]: Failed to unban | {ex}"); }
                            break;
                        case "/IPBAN":
                            try
                            {
                                IPAddress ip = IPAddress.Parse(GetIpAddressFromTcpClient(clients[command.Substring(7)]));
                                bannedIPs.Add(ip);
                                TcpClient client = GetTcpClientByIpAddress(clients, ip.ToString());
                                SendMessage(client, "[SERVER]: You have been banned.");
                                client.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[/IPBAN]: Failed to ban {command.Substring(7)} | {ex}");
                            }
                            break;
                        case "/IPUNBAN":
                            try
                            {
                                if (command.Substring(9) != "*")
                                {
                                    bannedIPs.Remove(IPAddress.Parse(command.Substring(9)));
                                    Console.WriteLine($"[/IPUNBAN]: Unbanned {command.Substring(9)}");
                                }
                                else
                                {
                                    bannedIPs.Clear();
                                    Console.WriteLine("[/IPUNBAN]: Unbanned *");
                                }
                            }
                            catch (Exception ex) { Console.WriteLine($"[/IPUNBAN]: Failed to unban {command.Substring(9)} | {ex}"); }
                            break;
                        case "/RAW":
                            try
                            {
                                Broadcast($"{command.Substring(5)}", String.Empty);
                            } catch (Exception ex) { Console.WriteLine($"[/RAW]: Failed to Broadcast | {ex}"); }
                            break;
                        case "/OP":
                            try
                            {
                                adminList.Add(clients[command.Substring(4)]);
                                Console.WriteLine($"[/OP]: Added {command.Substring(4)} to admins");
                            } catch { Console.WriteLine("[/OP]: Client not found"); }
                            break;
                        case "/UNOP":
                            try
                            {
                                if (command.Substring(6) != "*")
                                {
                                    adminList.Remove(clients[command.Substring(6)]);
                                    Console.WriteLine($"[/UNOP]: Removed {command.Substring(6)} from admins");
                                }
                                else
                                {
                                    adminList.Clear();
                                    Console.WriteLine($"[/UNOP]: Removed * from Admins");
                                }
                            } catch { Console.WriteLine("[/UNOP]: Client not found"); }
                            break;
                        case "/LOCK":
                            try
                            {
                                string lockPassword = GetSha512Hash(command.Substring(6));
                                string enteredPassword = "";
                                Console.Clear();
                                Console.Title = "[/LOCK]: Locked";
                                do
                                {
                                    Console.WriteLine("[/LOCK]: Enter Password:\n");
                                    enteredPassword = GetSha512Hash(Console.ReadLine());
                                } while (enteredPassword != lockPassword);
                                Console.Title = "Simple TCP Chat Server";
                                Console.Clear();
                            } catch { }
                            break;
                        case "/PWD":
                            try
                            {
                                pwdHash = GetSha512Hash(command.Substring(5));
                                Console.SetCursorPosition(0, Console.CursorTop - 1);
                                Console.WriteLine("[/PWD]: Successfully set new admin password");
                            }
                            catch { Console.WriteLine("[/PWD]: Failed to set new admin password."); }
                            break;
                        default:
                            Console.WriteLine($"[INFO]: Command not found: {commandUpper.Split(' ')[0]}");
                            break;     
                    }
                }
            }          
        }
    }

    // HANDLE SERVER: FUNC IN THREAD
    static void Server()
    {
        try
        {
            while (true)
            {
                TcpClient client = _server.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Server exception: {e.Message}");
        }
        finally
        {
            _server.Stop();
        }
    }

    //HANDLE CLIENT: FUNC IN THREAD
    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        string clientName = null;
        bool nameTaken = false; // no duplicate names allowed
        bool banned = false;

        // ADD THE CLIENT TO THE SERVER
        try
        {
            // Read the client's desired name from the stream
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            clientName = Encoding.Unicode.GetString(buffer, 0, bytesRead).Trim();

            // Check if the name is already taken
            if (clients.ContainsKey(clientName))
            {
                nameTaken = true;
                SendMessage(client, "Name already taken.");
                client.Close();
                return;
            }
            // check if the name is banned.
            if (bannedNames.Contains(clientName))
            {
                SendMessage(client, "[SERVER]: This name has been banned.");
                banned = true;
                return;
            }
            // Check if the IP is banned
            if (bannedIPs.Contains(IPAddress.Parse(GetIpAddressFromTcpClient(client))))
            {
                SendMessage(client, "[SERVER]: You have been banned");
                banned = true;
                return;
            }

            // Add the client with the chosen name
            if (!clients.TryAdd(clientName, client))
            {
                SendMessage(client, "Failed to join the chat.");
                client.Close();
                return;
            }
            SendMessage(client, "Joined the chat successfully.");
            Broadcast($"[SERVER]: {clientName} has joined the chat.", clientName);


            // HANDLE CLIENT
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.Unicode.GetString(buffer, 0, bytesRead);

                // HANDLE CLIENT COMMANDS
                if (message.StartsWith("/"))
                {
                    // AFTER AUTH OR /OP
                    if (adminList.Contains(client))
                    {
                        string command = message; // THE BELOW COMMANDS ARE COPIED FROM THE SERVER COMMANDS. (and i'm too lazy to change command to message everywhere)
                        switch (message.ToUpper().Split(' ')[0])
                        {
                            case "/KICK":
                                try
                                {
                                    SendMessage(clients[command.Substring(6)], "\nYou have been Kicked.\n");
                                    clients[command.Substring(6)].Close();
                                    clients.TryRemove(command.Substring(6), out _);
                                    Broadcast($"[SERVER]: {command.Substring(6)} was kicked.", String.Empty);
                                }
                                catch
                                {
                                    SendMessage(client, $"[/KICK]: Failed to kick {command.Substring(6)}");
                                }
                                break;
                            case "/SHADOWKICK":
                                try
                                {
                                    SendMessage(clients[command.Substring(12)], "A severe Exception has occured.");
                                    clients[command.Substring(12)].Close();
                                    clients.TryRemove(command.Substring(12), out _);
                                }
                                catch
                                {
                                    SendMessage(client, $"[/KICK]: Failed to kick {command.Substring(6)}");
                                }
                                break;
                            case "/BAN":
                                try
                                {
                                    string targetName = command.Substring(5);
                                    Broadcast($"[SERVER]: {targetName} was banned", String.Empty);
                                    clients[targetName].Close();
                                    clients.TryRemove(targetName, out _);
                                    bannedNames.Add(targetName);
                                }
                                catch
                                {
                                    SendMessage(client, $"[/BAN]: Failed to ban {command.Substring(5)}");
                                }
                                break;
                            case "/UNBAN":
                                if (!string.IsNullOrEmpty(command.Substring(6)))
                                {
                                    try
                                    {
                                        bannedNames.Remove(command.Substring(7));
                                    }
                                    catch { SendMessage(client, $"[/UNBAN]: Failed to unban {command.Substring(7)}"); }
                                }
                                if (!bannedNames.Contains("*")) { bannedNames.Add("*"); }
                                break;
                            case "/IPBAN":
                                try
                                {
                                    IPAddress ip = IPAddress.Parse(GetIpAddressFromTcpClient(clients[command.Substring(7)]));
                                    bannedIPs.Add(ip);
                                    TcpClient targetClient = GetTcpClientByIpAddress(clients, ip.ToString());
                                    SendMessage(targetClient, "[SERVER]: You have been banned.");
                                    targetClient.Close();
                                }
                                catch
                                {
                                    SendMessage(client, $"[/IPBAN]: Failed to ban {command.Substring(7)}");
                                }
                                break;
                            case "/IPUNBAN":
                                try
                                {
                                    bannedIPs.Remove(IPAddress.Parse(command.Substring(9)));
                                }
                                catch { SendMessage(client, $"[/IPUNBAN]: Failed to unban {command.Substring(9)}"); }
                                break;
                            case "/RAW":
                                try
                                {
                                    Broadcast($"{command.Substring(5)}", String.Empty);
                                }
                                catch { SendMessage(client, $"[/RAW]: Failed to Broadcast"); }
                                break;
                            case "/OP":
                                try
                                {
                                    adminList.Add(clients[command.Substring(4)]);
                                }
                                catch { Console.WriteLine("[/OP]: Client not found"); }
                                break;
                            case "/UNOP":
                                try
                                {
                                    adminList.Remove(clients[command.Substring(6)]);
                                    SendMessage(client, $"[/UNOP]: Removed {command.Substring(6)} from admins");
                                    Console.WriteLine($"[/UNOP]: Removed {command.Substring(6)} from admins");
                                }
                                catch { Console.WriteLine("[/UNOP]: Client not found"); }
                                break;
                            default:
                                SendMessage(client, $"[INFO]: Command not found: {command.ToUpper().Split(' ')[0]}");
                                break;
                        }
                    }
                    // NON ADMIN SERVER SIDE COMMANDS (has to be down here so the admin default statement from above isn't run.)
                    switch (message.ToUpper().Split(' ')[0])
                    {
                        case "/DM":
                            try
                            {
                                try
                                {
                                    string targetName = message.Substring(4).Split(' ')[0];
                                    string msg = message.Substring(4 + targetName.Length + 1);
                                    DirectMessage(msg, targetName, clientName);
                                }
                                catch (Exception ex)
                                {
                                    string targetName = message.Substring(2).Split(" ")[0];
                                    Console.WriteLine($"[SERVER LOG] DM from {clientName} to {targetName} failed.");
                                    SendMessage(client, $"[SERVER]: DM to {message.Substring(4 + message.Substring(4).Split(' ')[0].Length + 1)} failed. | {ex}");
                                }
                            }
                            catch { }  
                            break;

                        case "/LIST":
                            string clientListSend = "";
                            foreach (KeyValuePair<string, TcpClient> kvp in clients)
                            {
                                clientListSend += $"\n\t「{kvp.Key}」";
                            }
                            SendMessage(client, $"[SERVER] Currently Online:{clientListSend}");
                            break;

                        case "/AUTH":
                            if (!String.IsNullOrEmpty(message.Substring(5)))
                            {
                                if (message.Substring(6) == pwdHash)
                                {
                                    adminList.Add(client);
                                    SendMessage(client, "[AUTH]: Auth successful, added to Admins.");
                                    Console.WriteLine($"[AUTH]: Added {clientName} to Admins");
                                }
                                else
                                {
                                    SendMessage(client, "[AUTH]: Incorrect password.");
                                    Console.WriteLine($"[AUTH]: Failed Login attempt by {clientName}");
                                }
                            }
                            break;
                    }
                }
                // HANDLE NORMAL MESSAGES
                else
                {
                    Broadcast($"「{clientName}」: {message}", clientName);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[CLIENT]: ERROR BY {clientName}: {e.Message}");
        }
        // CLEAN UP
        finally
        {
            // BANNED ONLY GETS LOGGED NO DISCONNECT MESSAGE
            if (banned)
            {
                Console.WriteLine($"[LOG IN]: {GetIpAddressFromTcpClient(client)} tried to log in as {clientName}, but is banned.");
                client.Close();
            }
            // DECIDE ON DISCONNECT MESSAGE
            else
            {
                
                if (clientName != null && !nameTaken)
                {
                    clients.TryRemove(clientName, out _);
                    Broadcast($"[{clientName}] has left the chat.", clientName);
                }
                if (clientName == null && nameTaken)
                {
                    Console.WriteLine($"[LOG IN]: {GetIpAddressFromTcpClient(client)} tried to log in as {clientName}");
                }
                // if there is a failed login attempt as Admin
                else if (clientName == null)
                {
                    Console.WriteLine($"[LOG IN]: {GetIpAddressFromTcpClient(client)} tried to log, but forgot their username.");
                }
                client.Close();
            }
        }
    }

    // USEFUL FUNCTIONS
    static void SendMessage(TcpClient client, string message)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        NetworkStream stream = client.GetStream();
        stream.Write(buffer, 0, buffer.Length);
    }
    static void Broadcast(string message, string senderName)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        // log message to server
        Console.WriteLine(message);
        foreach (var entry in clients)
        {
            // Don't send the message back to the sender
            if (entry.Key != senderName)
            {
                TcpClient targetClient = entry.Value;
                NetworkStream targetStream = targetClient.GetStream();
                targetStream.Write(buffer, 0, buffer.Length);
            }
        }
    }
    static string GetIpAddressFromTcpClient(TcpClient tcpClient)
    {
        if (tcpClient?.Client?.RemoteEndPoint is IPEndPoint endPoint)
        {
            IPAddress ipAddress = endPoint.Address;
            return ipAddress.ToString();
        }
        return "Unknown IP Address";
    }
    static string GetSha512Hash(string input)
    {
        using (SHA512 sha512 = SHA512.Create())
        {
            byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

    }
    static TcpClient GetTcpClientByIpAddress(ConcurrentDictionary<string, TcpClient> clients, string ipAddressToFind)
    {
        foreach (var kvp in clients)
        {
            TcpClient tcpClient = kvp.Value;

            // Assuming the TcpClient is connected and has a valid IP endpoint
            if (tcpClient.Connected && ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString() == ipAddressToFind)
            {
                return tcpClient;
            }
        }

        return null; // TcpClient not found for the given IP address
    }
    static void DirectMessage(string message, string targetName, string senderName)
    {
        message = $"{senderName} -> You: " + message;
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        // log message to server
        Console.WriteLine($"{senderName} -> {targetName}: {message}");
        try
        {
            TcpClient targetClient = clients[targetName];
            NetworkStream targetStream = targetClient.GetStream();
            targetStream.Write(buffer, 0, buffer.Length);
            
        }catch {
            byte[] errorBuffer = Encoding.Unicode.GetBytes("[/DM]: Target not found.");
            clients[senderName].GetStream().Write(errorBuffer, 0, errorBuffer.Length); 
        }
    }
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

}
