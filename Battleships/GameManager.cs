using Battleships.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Net.Sockets;
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
        Player Opponent { get; set; }
        public Player Turn { get; set; }
        Ship ShipHit { get; set; }
        TcpClient client;
        NetworkStream stream;
        string lobbyName;

        public void ChangeTurn()
        {
            Turn = Turn.Equals(Player) ? Opponent : Player;
        }

        public async void Shoot(int btnId)
        {
            SoundPlayer.PlayFireSound();
            await Utilities.SendRequest(stream, Operation.Shoot, btnId.ToString(), lobbyName);
        }

        public async void GetShot(Player target, int btnId)
        {
            Tile tile = target.Tiles[btnId];
            if (tile.IsEmpty())
            {
                tile.SetMiss();
                await Utilities.SendRequest(stream, Operation.Miss, btnId.ToString(), lobbyName);
                ChangeTurn();
                await Utilities.SendRequest(stream, Operation.ChangeTurn, "", lobbyName);
            }
            else if (tile.IsOccupied())
            {
                tile.SetHit();
                await Utilities.SendRequest(stream, Operation.Hit, btnId.ToString(), lobbyName);
                if (await IsTargetDestroyed(target, btnId))
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

        public async void EndGame(Player target)
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
                await Utilities.SendRequest(stream, Operation.EndGame, "", lobbyName);
            }
        }

        public async void OnStartGameClick(object sender, EventArgs e)
        {
            SoundPlayer.PlayBellSound();
            client = new TcpClient();
            client.Connect("127.0.0.1", 8000);
            stream = client.GetStream();
            BeginListen();

            // Create or join a game
            await Utilities.SendRequest(stream, Operation.Matchmaking, "");
        }

        public async void OnSendButtonClick(object sender, EventArgs e)
        {
            var message = Window.Input.Text;
            AddLogBoxItem(message);
            await Utilities.SendRequest(stream, Operation.Message, message, lobbyName);
            Window.Input.Clear();
        }

        public async Task<bool> IsTargetDestroyed(Player target, int btnId)
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
                    var coords = new List<Coordinates>();
                    ship.Tiles.ForEach(tile =>
                    {
                        coords.Add(tile.Coordinates);
                    });

                    var serializedShip = JsonConvert.SerializeObject(coords);
                    await Utilities.SendRequest(stream, Operation.DestroyShip, serializedShip, lobbyName);
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
        
        public void DestroyShipByCoords(List<Coordinates> coords)
        {
            var sortedCoords = coords.OrderBy(coord => coord.X).ToList();
            int shipDir = 1;
            // If all xs are equal, then dir should be horizontal
            if (coords.All(cord => cord.X == sortedCoords.First().X))
            {
                shipDir = 0;
            }
            int firstShipTileId = -1;
            Opponent.Tiles.ForEach(tile =>
            {
                if (tile.Coordinates.X == sortedCoords.First().X &&
                    tile.Coordinates.Y == sortedCoords.First().Y)
                {
                    firstShipTileId = tile.Coordinates.Y * 10 + tile.Coordinates.X;
                } 
            });

            if (firstShipTileId == -1)
            {
                Console.WriteLine("Id not found");
            }

            Window.CheckSurroundings(Opponent, firstShipTileId, coords.Count, shipDir, true, true);
        }

        void HandleResponse(Response response)
        {
            // sheesh
            Window.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                switch (response.status)
                {
                    case Status.Creation:
                        Console.WriteLine("Game has been created");
                        lobbyName = response.data;
                        OnGameCreation();
                        break;
                    case Status.Join:
                        Console.WriteLine("User joined");
                        lobbyName = response.data;
                        OnGameJoin();
                        break;
                    case Status.Message:
                        Console.WriteLine("New message");
                        OnReceiveMessage(response.data);
                        break;
                    case Status.Failure:
                        Console.WriteLine("Fatal error");
                        Console.WriteLine(response.message);
                        break;
                    case Status.Shot:
                        GetShot(Player, Convert.ToInt32(response.data));
                        break;
                    case Status.Miss:
                        Tile tile = Opponent.Tiles[Convert.ToInt32(response.data)];
                        tile.SetMiss();
                        break;
                    case Status.Hit:
                        tile = Opponent.Tiles[Convert.ToInt32(response.data)];
                        tile.SetHit();
                        break;
                    case Status.DestroyShip:
                        var coords = JsonConvert.DeserializeObject<List<Coordinates>>(response.data);
                        DestroyShipByCoords(coords);
                        break;
                    case Status.ChangeTurn:
                        ChangeTurn();
                        break;
                    case Status.EndGame:
                        EndGame(Opponent);
                        break;
                    default:
                        break;
                }
                Console.WriteLine(response.data);
            }));
        }

        public void AddLogBoxItem(string data)
        {
            Window.LogBox.Items.Insert(0, $"<{DateTime.Now.ToString("HH:mm:d")}> {data}");
        }

        void BeginListen()
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096]; 
            stream.BeginRead(buffer, 0, buffer.Length, EndListen, buffer);
        }

        void EndListen(IAsyncResult result)
        {
            var r = (byte[])(result.AsyncState ?? new byte[4096]);

            if (r != null)
            {
                var message = Encoding.UTF8.GetString(r);

                var response = JsonConvert.DeserializeObject<Response>(message);
                HandleResponse(response ?? new Response(Status.Failure, "", "Json convertion failure"));
            }

            BeginListen();
        }

        public void OnGameCreation()
        {
            AddLogBoxItem("Waiting for opponent");
        }

        public void OnGameJoin()
        {
            foreach (Button btn in Window.ButtonsPanel.Children)
                btn.IsEnabled = false;

            Window.gameStarted = true;
            AddLogBoxItem("Opponent found. Starting new game! Good luck!");
        }

        public void OnReceiveMessage(string data)
        {
            AddLogBoxItem(data);
        }

        public GameManager(Player player, Player oponent, MainWindow window)
        {
            Player = player;
            Opponent = oponent;
            Window = window;
            Turn = Opponent;
        }   
    }
}
