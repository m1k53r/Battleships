namespace Battleships.Common
{
    public enum Operation
    {
        Unknown,
        ReadWrite,
        Matchmaking,
        Message,
        Shoot,
        Hit,
        Miss,
        DestroyShip,
        ChangeTurn,
        EndGame,
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
