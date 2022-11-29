using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Battleships
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameManager gm { get; set; }
        public Player player { get; set; }
        public Player oponent { get; set; }
        public string shipChosen = "";
        public bool gameStarted = false;
        int shipLength;
        int dir = 1; // 0 - up, 1 - right

        public void InitializeGrid(Player player, Grid grid)
        {
            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                for (int j = 0; j < 10; j++)
                {
                    if (i == 1)
                        grid.ColumnDefinitions.Add(new ColumnDefinition());

                    Coordinates cords = new Coordinates(j, i);
                    Tile tile = new Tile(cords, TileType.Empty);
                    tile.Button.Name = $"{player.Name}_{count}";
                    tile.Button.Click += OnTileClick;
                    tile.Button.MouseEnter += OnTileHover;
                    tile.Button.MouseWheel += OnTileScroll;
                    tile.Button.MouseWheel += OnTileHover;
                    tile.Button.MouseLeave += OnTileLeave;
                    player.Tiles.Add(tile);
                    Grid.SetColumn(player.Tiles[count].Button, j);
                    Grid.SetRow(player.Tiles[count].Button, i);
                    grid.Children.Add(player.Tiles[count].Button);
                    count++;
                }
            }
        }

        //######
        //#SHIP# - # are checked here
        //######
        //return true if you can place a ship there
        public bool CheckSurroundings(Player player, int btnId, int tilesCount, int shipDir, bool checkMore = true, bool markEmpty = false)
        {
            for (int i = 0; i < tilesCount; i++)
            {
                if(shipDir == 0)
                {
                    if (btnId % 10 != 0) //tile left
                    {
                        if (player.Tiles[btnId - i * 10 - 1].IsOccupied())
                            return false;
                        else if(markEmpty)
                            player.Tiles[btnId - i * 10 - 1].SetMiss();
                    }
                    if (btnId % 10 != 9) //tile right
                    {
                        if (player.Tiles[btnId - i * 10 + 1].IsOccupied())
                            return false;
                        else if(markEmpty)
                            player.Tiles[btnId - i * 10 + 1].SetMiss();
                    }

                    if (checkMore)
                    {
                        if (i == 0)
                            if (btnId < 90) //bottom wall check
                                if (player.Tiles[btnId + 10].IsOccupied() || !CheckSurroundings(player, btnId + 10, 1, shipDir, false, markEmpty))
                                    return false;
                        if (i == tilesCount - 1)
                            if (btnId - i * 10 > 9) //top wall check
                                if (player.Tiles[btnId - i * 10].IsOccupied() || !CheckSurroundings(player, btnId - (i + 1) * 10, 1, shipDir, false, markEmpty))
                                    return false;
                    }
                }
                else
                {
                    if (btnId > 9) //tile above
                    {
                        if (player.Tiles[btnId + i - 10].IsOccupied())
                            return false;
                        else if(markEmpty)
                            player.Tiles[btnId + i - 10].SetMiss();
                    } 
                    if (btnId < 90) //tile below
                    {
                        if (player.Tiles[btnId + i + 10].IsOccupied())
                            return false;
                        else if (markEmpty)
                            player.Tiles[btnId + i + 10].SetMiss();
                    }

                    if (checkMore)
                    {
                        if (i == 0)
                            if (btnId % 10 != 0) //left wall check
                                if (player.Tiles[btnId - 1].IsOccupied() || !CheckSurroundings(player, btnId - 1, 1, shipDir, false, markEmpty))
                                    return false;
                        if (i == tilesCount - 1)
                            if ((btnId + i) % 10 != 9 && btnId + i + 1 < 100) //right wall check
                                if ((player.Tiles[btnId + i + 1].IsOccupied() && player.Tiles[btnId + i].Coordinates.X < 9) || !CheckSurroundings(player, btnId + i + 1, 1, shipDir, false, markEmpty))
                                    return false;
                    }
                }
            }
            if(markEmpty && !checkMore)
                player.Tiles[btnId].SetMiss();
            return true;
        }

        public bool CheckIfShipFits(int btnId, int shipLength, int shipDir)
        {
            for (int i = 0; i < shipLength; i++)
            {
                //if tile is outside of map boundaries or is anyhow occupied, or neighbour tiles are occupied
                if (dir == 0)
                {
                    if (btnId - i * 10 < 0 || !player.Tiles[btnId - i * 10].IsEmpty() || player.Tiles[btnId].Coordinates.Y - i < 0)
                        return false;
                }
                else
                {
                    if (btnId + i >= 100 || !player.Tiles[btnId + i].IsEmpty() || player.Tiles[btnId].Coordinates.X + i >= 10)
                        return false;
                }
            }
            if (!CheckSurroundings(player, btnId, shipLength, shipDir))
                return false;
            return true;
        }

        public bool CheckIfShipsSet()
        {
            foreach (Button btn in ButtonsPanel.Children)
                if (btn.Tag.Equals("unset"))
                    return false;
            return true;
        }

        public void OnTileScroll(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int btnId = int.Parse(btn.Name.Split('_')[2]);

            int prevDir = dir;
            dir++;
            if (dir == 2)
                dir = 0;
            for (int i = 0; i < shipLength; i++)
            {
                if(prevDir == 0)
                {
                    if (btnId - i * 10 >= 0 && player.Tiles[btnId - i * 10].IsEmpty())
                        player.Tiles[btnId - i * 10].SetEmpty();
                }
                else
                {
                    if (btnId + i < 100 && player.Tiles[btnId + i].IsEmpty())
                        player.Tiles[btnId + i].SetEmpty();
                }
            }
        }

        public void OnChoiceButtonClick(object sender, EventArgs e)
        {
            if (shipChosen != "")
                return;

            Button btn = (Button)sender;
            switch (btn.Name.Split('_')[0])
            {
                case "Big":
                    shipLength = 4; break;
                case "Medium":
                    shipLength = 3; break;
                case "Small":
                    shipLength = 1; break;
            }
            btn.Opacity = 0.50;

            shipChosen = btn.Name;
            foreach (Ship ship in player.Ships)
            {
                if (ship.Name == shipChosen)
                {
                    btn.Tag = "unset";
                    foreach (Tile tile in ship.Tiles)
                        tile.SetEmpty();
                    
                    player.Ships.Remove(ship);
                    break;
                }
            }
            StartBtn.IsEnabled = false;
        }

        public void OnTileClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int btnId = int.Parse(btn.Name.Split('_')[2]);
            
            //left board, ship placing, where tile isnt occupied, and ship fits
            if (btn.Name.Split('_')[1] == "1" && shipChosen != "" && player.Tiles[btnId].IsEmpty() && CheckIfShipFits(btnId, shipLength, dir))
            {
                Ship ship = new Ship(shipLength, shipChosen, dir);
                for(int i = 0; i < shipLength; i++)
                {
                    Tile tile;
                    if(dir == 0)
                        tile = player.Tiles[btnId - i * 10];
                    else
                        tile = player.Tiles[btnId + i];

                    tile.SetOccupied();
                    ship.Tiles.Add(tile);
                }

                Button shipBtn = (Button)this.FindName(shipChosen);
                shipBtn.Tag = "set";
                player.Ships.Add(ship);
                shipChosen = "";
                if (CheckIfShipsSet())
                    StartBtn.IsEnabled = true;
            }else if (btn.Name.Split('_')[1] == "1" && gm.Turn.Equals(oponent) && gameStarted)
            {
                gm.Shoot(btnId);
            }
            else if(btn.Name.Split('_')[1] == "2" && gm.Turn.Equals(player) && gameStarted)
            {
                gm.Shoot(btnId);
            }
        }

        public void OnTileHover(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            //ship placing, left board
            if (shipChosen != "" && btn.Name.Split('_')[1] == "1")
            {
                int btnId = int.Parse(btn.Name.Split('_')[2]);

                for (int i = 0; i < shipLength; i++)
                {
                    if(dir == 0)
                    {
                        if (btnId - i * 10 >= 0 && player.Tiles[btnId - i * 10].IsEmpty() && player.Tiles[btnId].Coordinates.Y - i >= 0)
                            player.Tiles[btnId - i * 10].SetPreview();
                    }
                    else
                    {
                        if (btnId + i < 100 && player.Tiles[btnId + i].IsEmpty() && player.Tiles[btnId].Coordinates.X + i < 10)
                            player.Tiles[btnId + i].SetPreview();
                    }
                }
            }
        }

        public void OnTileLeave(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            //ship placing, left board
            if (shipChosen != "" && btn.Name.Split('_')[1] == "1")
            {
                int btnId = int.Parse(btn.Name.Split('_')[2]);

                for (int i = 0; i < shipLength; i++)
                {
                    if(dir == 0)
                    {
                        if (btnId - i * 10 >= 0 && player.Tiles[btnId - i * 10].IsEmpty())
                            player.Tiles[btnId - i * 10].SetEmpty();
                    } 
                    else
                    {
                        if (btnId + i < 100 && player.Tiles[btnId + i].IsEmpty())
                            player.Tiles[btnId + i].SetEmpty();
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            player = new Player("Player_1");
            oponent = new Player("Player_2");

            gm = new(player, oponent, this);

            InitializeGrid(player, PlayerGrid);
            InitializeGrid(oponent, OponentGrid);
            StartBtn.Click += gm.OnStartGameClick;
            SendBtn.Click += gm.OnSendButtonClick;
        }
    }
}
