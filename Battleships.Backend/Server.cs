using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Battleships.Common;
using System.Reflection.Metadata.Ecma335;

namespace Battleships.Backend
{
    internal class Server
    {
        public static Dictionary<TcpClient, ClientInfo> clients = new();
        public static ClientInfo? waitingClient = null;
        public static Dictionary<string, Lobby> lobbies = new();
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8000);
        public void Start()
        {
            server.Start();
            Console.WriteLine(
                "Server has started on 127.0.0.1:8000.{0}Waiting for a connection…"
                , Environment.NewLine);

            while (true)
            {
                TcpClient tcpClient = server.AcceptTcpClient();
                var client = new ClientInfo(Guid.NewGuid().ToString(), "user", false, tcpClient);
                clients.Add(tcpClient, client);
                Console.WriteLine("Client connected");
                PrintConnectedClients();

                NetworkStream stream = tcpClient.GetStream();

                new Thread(() => HandleClientRequest(stream, client)).Start();
            }
        }
        private void RemoveDisconnectedClients()
        {
            List<TcpClient> clientsToRemove = new();
            foreach (var client in clients.Keys)
            {
                if(!client.Connected)
                {
                    clientsToRemove.Add(client);
                }
            }
            clientsToRemove.ForEach(x => clients.Remove(x));
        }
        private void PrintConnectedClients()
        {
            Console.WriteLine("Connected clients:");
            foreach (var client in clients)
            {
                Console.WriteLine(client.Value.uuid);
            }
        }
        private async void HandleClientRequest(NetworkStream stream, ClientInfo client)
        {
            try
            {
                while (client.tcpClient.Connected)
                {
                    RemoveDisconnectedClients(); 
                    var incoming = await Utilities.WaitForRequest(stream);
                    Console.WriteLine(incoming.data);

                    switch (incoming.operation)
                    {
                        case Operation.ReadWrite:
                            HandleReadWrite(stream);
                            break;
                        case Operation.Matchmaking:
                            HandleMatchmaking(stream, client, incoming.data);
                            break;
                        case Operation.Message:
                            HandleMessage(stream, client, incoming.data);
                            break;
                        default:
                            HandleUnknown(stream);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                HandleException(stream);
            }
        }
        private async void HandleReadWrite(NetworkStream stream)
        {
            var message = $"📅 {DateTime.Now} 🕛";
            await Utilities.SendResponse(stream, Status.Success, message);
        }
        private async void HandleMatchmaking(NetworkStream stream, ClientInfo client, string data)
        {
            RemoveDeadLobbies();
            if (client.isPlaying)
            {
                await Utilities.SendResponse(stream, Status.Failure, "", 
                    "Client is already playing!");
                return;
            }

            var lobbyName = FindOpenLobbies();
            if (lobbyName == null)
            {
                await Utilities.SendResponse(stream, Status.Creation, CreateLobby(client, data), 
                    "Waiting for opponent...");
                return;
            }

            var lobby = lobbies[lobbyName];
            lobby.Join(client, data);

            lobby.players.Keys.ToList().ForEach(async x =>
            {
                x.isPlaying = true;
                await Utilities.SendResponse(x.tcpClient.GetStream(), Status.Join, lobbyName);
                Console.WriteLine(lobby.players[x][1]);
            });
        }
        private async void HandleMessage(NetworkStream stream, ClientInfo client, string data)
        {
            if (!client.isPlaying || lobbies.Count == 0)
            {
                await Utilities.SendResponse(stream, Status.Failure, "Error", 
                    "There is no one to send message to");
                return;
            }

            Console.WriteLine(data);
            if (!data.Contains(":"))
            {
                await Utilities.SendResponse(stream, Status.Failure, "Error", 
                    "Wrong data format");
                return;
            }

            var message = data.Split(":");

            var opponent = lobbies[message.First()].players.Where(x => x.Key != client).First();
            await Utilities.SendResponse(opponent.Key.tcpClient.GetStream(), Status.Message, message.Last());
        }
        private async void HandleUnknown(NetworkStream stream)
        {
            await Utilities.SendResponse(stream, Status.Failure,
                "Error", "Input was not in a correct format");
        }
        private async void HandleException(NetworkStream stream)
        {
            await Utilities.SendResponse(stream, Status.Failure,
                "Error", "Unexpected error");
        }
        private string CreateLobby(ClientInfo host, string data)
        {
            var lobbyName = Path.GetRandomFileName();
            while (lobbies.ContainsKey(lobbyName))
            {
                lobbyName = Path.GetRandomFileName().Replace(".", "");
            }

            lobbies.Add(lobbyName, new Lobby(host, data));

            return lobbyName;
        }
        private string? FindOpenLobbies()
        {

            foreach (var lobby in lobbies)
            {
                if (lobby.Value.players.Count == 1)
                {
                    return lobby.Key;
                }
            }
            return null;
        }
        private void RemoveDeadLobbies()
        {
            List<string> lobbyNames = new();
            foreach (var lobby in lobbies)
            {
                var players = lobby.Value.players;
                if (players.Count == 0 || 
                    players.Any(x => !x.Key.tcpClient.Connected))
                {
                    lobbyNames.Add(lobby.Key); 
                }
            }
            foreach (var lobbyName in lobbyNames)
            {
                lobbies.Remove(lobbyName);
            }
        }
    }
}
