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
        public Dictionary<ClientInfo, List<int>> players { get; set; }
        public bool turn { get; set; }
        public ClientInfo? winner;
        public Lobby(ClientInfo player, string data)
        {
            var ships = DeserializeShips(data);
            players = new Dictionary<ClientInfo, List<int>>();
            players.Add(player, ships); 
            turn = Convert.ToBoolean(new Random().Next() % 2); // pick who will start the game
            winner = null;
        }
        public void Join(ClientInfo player, string data)
        {
            var ships = DeserializeShips(data);
            players.Add(player, ships);
        }
        private List<int> DeserializeShips(string data)
        {
            return JsonConvert.DeserializeObject<List<int>>(data) ?? new List<int>();
        }
    }
}
