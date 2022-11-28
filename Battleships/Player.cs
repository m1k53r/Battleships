using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleships
{
    public class Player
    {
        public string Name { get; set; }
        public List<Ship> Ships { get; set; }
        public List<Tile> Tiles { get; set; }
        public Player(string name)
        {
            Name = name;
            Tiles = new List<Tile>();
            Ships = new List<Ship>();
        }
    }
}
