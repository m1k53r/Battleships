using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Battleships.Backend
{
    internal class Lobby
    {
        public string name { get; set; }
        public List<TcpClient> players { get; set; }
        public bool turn { get; set; }
        public TcpClient? winner;
        public Lobby(string name, List<TcpClient> players)
        {
            this.name = name;
            this.players = players.OrderBy(x => new Random().Next()).ToList();
            this.turn = false;
            this.winner = null;
        }
    }
}
