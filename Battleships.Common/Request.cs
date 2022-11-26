namespace Battleships.Common
{
    public enum Operation
    {
        Unknown,
        ReadWrite,
        Matchmaking,
    }
    public class Request
    {
        public Operation operation { get; set; }
        public string data { get; set; }
        public Request(Operation operation, string data)
        {
            this.operation = operation;
            this.data = data;
        }
    }
}
