using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Battleships.Backend
{
    internal class Server
    {
        public static Dictionary<TcpClient, string> clients = new();
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8000);
        public void Start()
        {
            server.Start();
            Console.WriteLine(
                "Server has started on 127.0.0.1:8000.{0}Waiting for a connection…"
                , Environment.NewLine);

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                clients.Add(client, Guid.NewGuid().ToString());
                Console.WriteLine("Client connected");

                NetworkStream stream = client.GetStream();

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
                Console.WriteLine(client.Value);
            }
        }
        private async void HandleClientRequest(NetworkStream stream, TcpClient client)
        {
            try
            {
                while (client.Connected)
                {
                    PrintConnectedClients();

                    var buffer = new byte[1024];
                    int received = await stream.ReadAsync(buffer);

                    var incoming = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"{incoming}");

                    if (incoming == "hi")
                    {
                        var message = $"📅 {DateTime.Now} 🕛";
                        var dateTimeBytes = Encoding.UTF8.GetBytes(message);
                        await stream.WriteAsync(dateTimeBytes);

                        Console.WriteLine($"{message}");
                    }
                    else
                    {
                        await stream.WriteAsync(Encoding.UTF8.GetBytes("Unknown format"));
                    }
                }
            }
            catch (Exception) { }
            finally { RemoveDisconnectedClients(); }
        }
    }
}
