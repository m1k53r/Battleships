﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Battleships.Common;

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
                            HandleMatchmaking(stream, client);
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
        private async void HandleMatchmaking(NetworkStream stream, ClientInfo client)
        {
            if (client.isPlaying)
            {
                await Utilities.SendResponse(stream, Status.Failure, "", 
                    "Client is already playing!");
                return;
            }

            if (waitingClient is null || !waitingClient.tcpClient.Connected)
            {
                waitingClient = client;
                await Utilities.SendResponse(stream, Status.Success, "", 
                    "Waiting for opponent...");
                return;
            }

            var waitingClientStream = waitingClient.tcpClient.GetStream();

            var lobbyName = Path.GetRandomFileName();
            while (lobbies.ContainsKey(lobbyName))
            {
                lobbyName = Path.GetRandomFileName();
            }

            lobbies.Add(lobbyName, new Lobby(new Dictionary<ClientInfo, List<int>>() {
                { client, new List<int>() },
                { waitingClient, new List<int>() },
            }));

            await Utilities.SendResponse(stream, Status.Success, lobbyName);
            await Utilities.SendResponse(waitingClientStream, Status.Success, lobbyName);

            waitingClient.isPlaying = true;
            client.isPlaying = true;

            waitingClient = null;
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
    }
}
