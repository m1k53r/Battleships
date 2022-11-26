namespace Battleships.Common
{
    public enum Status
    {
        Success,
        Failure,
    }
    public class Response
    {
        public Status status { get; set; }
        public string data { get; set; }
        public string message { get; set; }
        public Response(Status status, string data, string message = "")
        {
            this.status = status;
            this.data = data;
            this.message = message;
        }
    }
}
