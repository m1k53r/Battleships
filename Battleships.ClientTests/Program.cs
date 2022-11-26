using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Battleships.Common;
using System.Buffers;

using var client = new TcpClient();
client.Connect("127.0.0.1", 8000);
NetworkStream stream = client.GetStream();

while (true)
{
    Console.WriteLine("[1] Send and receive data");
    Console.WriteLine("[2] Matchmaking");

    var input = Int32.Parse(Console.ReadLine());

    switch(input)
    {
        case 1:
            ReadAndWrite();
            break;
        case 2:
            Matchmaking();
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

    var deserialize = await Utilities.WaitForResponse(stream);
    Console.WriteLine($"{deserialize.data}");
}

async void Matchmaking() 
{
    await Utilities.SendRequest(stream, Operation.Matchmaking, "");
    var deserialize = await Utilities.WaitForResponse(stream);
    Console.WriteLine(deserialize.data);
    if (deserialize.data == "")
    {
        Console.WriteLine(deserialize.message);
        deserialize = await Utilities.WaitForResponse(stream);
        Console.WriteLine(deserialize.data);
    }
}
