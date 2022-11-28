using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Battleships
{
    public class Tile
    {
        public Coordinates Coordinates { get; set; }
        public TileType TileType { get; set; }
        public Button Button { get; set; }
        public void SetHit()
        {
            TileType = TileType.Hit;
            Button.Background = Brushes.Red;
        }
        public void SetMiss()
        {
            TileType = TileType.Miss;
            Button.Background = Brushes.Aqua;
        }
        public void SetPreview()
        {
            Button.Background = Brushes.Green;
        }
        public void SetOccupied()
        {
            TileType = TileType.Occupied;
            Button.Background = Brushes.Black;
        }
        public void SetEmpty()
        {
            TileType = TileType.Empty;
            Button.ClearValue(Button.BackgroundProperty);
        }
        public bool IsEmpty()
        {
            return TileType == TileType.Empty;
        }
        public bool IsOccupied()
        {
            return TileType == TileType.Occupied;
        }
        public Tile(Coordinates coordinates, TileType tileType)
        {
            Coordinates = coordinates;
            TileType = tileType;
            Button = new Button();
        }
    }
}
