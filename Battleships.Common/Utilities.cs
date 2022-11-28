using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Battleships.Common
{
    public class Utilities
    {
        public static async Task<bool> SendRequest(NetworkStream stream, 
            Operation operation, string data)
        {
            try
            {
                Request request = new Request(operation, data);
                var serialize = JsonConvert.SerializeObject(request);
                await stream.WriteAsync(Encoding.UTF8.GetBytes(serialize));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public static async Task<bool> SendResponse(NetworkStream stream, 
            Status status, string data, string message = "")
        {
            try
            {
                Response response = new Response(status, data, message);
                var serialize = JsonConvert.SerializeObject(response);
                await stream.WriteAsync(Encoding.UTF8.GetBytes(serialize));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public static async Task<Response> WaitForResponse(NetworkStream stream)
        {
            try
            {
                var buffer = new byte[1024];
                int received = await stream.ReadAsync(buffer);

                var message = Encoding.UTF8.GetString(buffer, 0, received);

                return JsonConvert.DeserializeObject<Response>(message) 
                    ?? new Response(Status.Failure, "", "Unexpected error");
            }
            catch (Exception)
            {
                return new Response(Status.Failure, "", "Server error");
            }
        }
        public static async Task<Request> WaitForRequest(NetworkStream stream)
        {
            try
            {
                var buffer = new byte[1024];
                int received = await stream.ReadAsync(buffer);

                var message = Encoding.UTF8.GetString(buffer, 0, received);

                return JsonConvert.DeserializeObject<Request>(message) 
                    ?? new Request(Operation.Unknown, "");
            }
            catch (Exception)
            {
                return new Request(Operation.Unknown, "");
            }
        }
    }
}
