using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleships
{
    public class Ship
    {
        public string Name { get; }
        public int Length { get; set; }
        public List<Tile> Tiles { get; set; }
        public int ShipDir { get; } // 0 - up, 1 - right
        public bool IsDestroyed { get; set; }
        public Ship(int length, string name, int shipDir)
        {
            Length = length;
            Name = name;
            ShipDir = shipDir;
            Tiles = new List<Tile>();
        }
    }
}
