using Newtonsoft.Json;
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
        public List<ClientInfo> players { get; set; }
        public bool turn { get; set; }
        public ClientInfo? winner;
        public Lobby(ClientInfo player)
        {
            players = new List<ClientInfo>();
            players.Add(player); 
            turn = Convert.ToBoolean(new Random().Next() % 2); // pick who will start the game
            winner = null;
        }
        public void Join(ClientInfo player)
        {
            players.Add(player);
        }
    }
}
