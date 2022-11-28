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
        public Dictionary<ClientInfo, List<int>> players { get; set; }
        public bool turn { get; set; }
        public ClientInfo? winner;
        public Lobby(Dictionary<ClientInfo, List<int>> players)
        {
            this.players = players; //.OrderBy(x => new Random().Next()).ToList();
            this.turn = false;
            this.winner = null;
        }
    }
}
