using System.Net.Sockets;
using System.Text;

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

async void ReadAndWrite(){
    Console.WriteLine("Data: ");
    var input = Console.ReadLine();
    await stream.WriteAsync(Encoding.UTF8.GetBytes(input));

    Console.WriteLine("Sent data");
    Console.WriteLine("Trying to read data");

    var buffer = new byte[1024];
    int received = await stream.ReadAsync(buffer);

    var message = Encoding.UTF8.GetString(buffer, 0, received);
    Console.WriteLine($"{message}");
}

async void Matchmaking() { }
