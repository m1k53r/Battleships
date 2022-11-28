using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Battleships.Common;
using System.Buffers;
using System.ComponentModel.DataAnnotations;

using var client = new TcpClient();
client.Connect("127.0.0.1", 8000);
NetworkStream stream = client.GetStream();
BeginListen();

string lobbyName = "";
List<int> ships = new List<int>() { 0, 1, 2 };

while (true)
{
    Console.WriteLine("[1] Send and receive data");
    Console.WriteLine("[2] Matchmaking");
    Console.WriteLine("[3] Send message");

    var input = Int32.Parse(Console.ReadLine());

    switch(input)
    {
        case 1:
            ReadAndWrite();
            break;
        case 2:
            Matchmaking();
            break;
        case 3:
            SendMessage();
            break;
        default:
            Console.WriteLine("oops");
            break;
    }
}

async void ReadAndWrite()
{
    Console.WriteLine("Data: ");
    var input = Console.ReadLine();
    await Utilities.SendRequest(stream, Operation.ReadWrite, input);

    Console.WriteLine("Sent data");
    Console.WriteLine("Trying to read data");
}

async void Matchmaking() 
{
    var serializedShips = JsonConvert.SerializeObject(new { ships = ships });
    await Utilities.SendRequest(stream, Operation.Matchmaking, serializedShips);
}

async void SendMessage()
{
    Console.WriteLine("Data: ");
    var input = Console.ReadLine();
    await Utilities.SendRequest(stream, Operation.Message, $"{lobbyName}:{input}");
}

void BeginListen()
{
    NetworkStream stream = client.GetStream();
    byte[] buffer = new byte[4096]; 
    stream.BeginRead(buffer, 0, buffer.Length, EndListen, buffer);
}

void EndListen(IAsyncResult result)
{
    var buffer = new byte[4096];

    var r = (byte[])(result.AsyncState ?? new byte[4096]);

    if (r != null)
    {
        var message = Encoding.UTF8.GetString(r);

        var response = JsonConvert.DeserializeObject<Response>(message);
        HandleResponse(response ?? new Response(Status.Failure, "", "Json convertion failure"));
    }

    BeginListen();
}

void HandleResponse(Response response)
{
    switch (response.status)
    {
        case Status.Creation:
            Console.WriteLine("Game has been created");
            lobbyName = response.data;
            break;
        case Status.Join:
            Console.WriteLine("User joined");
            lobbyName = response.data;
            break;
        case Status.Message:
            Console.WriteLine("New message");
            break;
        case Status.Failure:
            Console.WriteLine("Fatal error");
            Console.WriteLine(response.message);
            break;
        default:
            break;
    }
    Console.WriteLine(response.data);
}
