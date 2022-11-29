using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Battleships
{
     public class GameManager
    {
        readonly MainWindow Window;
        BattleshipSoundPlayer SoundPlayer = new BattleshipSoundPlayer();
        Player Player { get; set; }
        Player Oponent { get; set; }
        public Player Turn { get; set; }
        Ship ShipHit { get; set; }

        public void ChangeTurn()
        {
            Turn = Turn.Equals(Player) ? Oponent : Player;
        }

        public void Shoot(Player target, int btnId)
        {
            SoundPlayer.PlayFireSound();
            Tile tile = target.Tiles[btnId];
            if (tile.IsEmpty())
            {
                tile.SetMiss();
                ChangeTurn();
            }
            else if (tile.IsOccupied())
            {
                tile.SetHit();
                if (IsTargetDestroyed(target, btnId))
                {
                    DestroyShip(target);
                    EndGame(target);
                }
            }
        }

        public void OnRestartGameClick(object sender, EventArgs e)
        {
            for(int i = 0; i < Window.player.Tiles.Count; i++)
            {
                if (!Window.player.Tiles[i].IsOccupied())
                    Window.player.Tiles[i].SetEmpty();

                Window.oponent.Tiles[i].SetEmpty();
            }

            foreach (Ship ship in Player.Ships)
            {
                ship.IsDestroyed = false;
                foreach (Tile tile in ship.Tiles)
                    tile.SetOccupied();
            }

            foreach (Button btn in Window.ButtonsPanel.Children)
                btn.IsEnabled = true;

            Window.shipChosen = "";
            Window.StartBtn.Content = "Start!";
            Window.StartBtn.Click -= OnRestartGameClick;
            Window.StartBtn.Click += OnStartGameClick;
        }

        public void EndGame(Player target)
        {
            bool allDestroyed = true;
            foreach (Ship ship in target.Ships)
            {
                if(!ship.IsDestroyed)
                    allDestroyed = false;
            }

            if (allDestroyed)
            {
                SoundPlayer.PlayBellSound();
                Window.gameStarted = false;
                Window.StartBtn.Content = "Play again!";
                Window.StartBtn.Click -= OnStartGameClick;
                Window.StartBtn.Click += OnRestartGameClick;
                Window.StartBtn.IsEnabled = true;
                Window.LogBox.Items.Insert(0, $"<{DateTime.Now.ToString("HH:mm:ss")}> {Turn.Name} won! GG!");
            }
        }

        public void OnStartGameClick(object sender, EventArgs e)
        {
            SoundPlayer.PlayBellSound();
            Window.LogBox.Items.Insert(0, $"<{DateTime.Now.ToString("HH:mm:ss")}> Starting new game! Good luck!");
            foreach (Button btn in Window.ButtonsPanel.Children)
                btn.IsEnabled = false;

            Window.gameStarted = true;
            //TODO: set oponents ships from backend
        }

        public bool IsTargetDestroyed(Player target, int btnId)
        {
            Tile tile = target.Tiles[btnId];
            foreach (Ship ship in target.Ships)
            {
                if (ship.Tiles.Contains(tile))
                {
                    ShipHit = ship;
                    foreach (Tile shipTile in ship.Tiles)
                    {
                        if (shipTile.IsOccupied())
                            return false;
                    }
                }
            }
            return true;
        }

        public void DestroyShip(Player target)
        {
            ShipHit.IsDestroyed = true;
            int firstShipTileId = ShipHit.Tiles[0].Coordinates.Y * 10 + ShipHit.Tiles[0].Coordinates.X;
            Window.CheckSurroundings(target, firstShipTileId, ShipHit.Tiles.Count, ShipHit.ShipDir, true, true);
        }

        public GameManager(Player player, Player oponent, MainWindow window)
        {
            Player = player;
            Oponent = oponent;
            Window = window;
            Turn = Player;
        }   
    }
}
