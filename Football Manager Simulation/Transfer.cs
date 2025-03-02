using System;
using System.Drawing;
using System.Numerics;

namespace GUI
{
    public class Transfer
    {
        public Player Player { get; set; }
        public Club FromClub { get; set; }
        public Club ToClub { get; set; }
        public double Fee { get; set; }
        public DateTime Date { get; set; }

        public Transfer(Player player, Club fromClub, Club toClub, double fee, DateTime date)
        {
            Player = player;
            FromClub = fromClub;
            ToClub = toClub;
            Fee = fee;
            Date = date;
        }
    }

    public class TransferMarketWindow : Window
    {
        private Club _userClub;
        private League _league;
        private List<Player> _transferListedPlayers;
        private int _selectedPlayerIndex;

        public TransferMarketWindow(Club userClub, League league, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _userClub = userClub;
            _league = league;
            _transferListedPlayers = new List<Player>();
            _selectedPlayerIndex = 0;
        }

        public override void Update()
        {
            var key = UserInterface.Input.Key;

            if (key == ConsoleKey.Escape)
            {
                _currentAction = InterfaceAction.ReturnToMainMenu;
                return;
            }

            if (key == ConsoleKey.UpArrow)
            {
                if (_transferListedPlayers.Count > 0)
                    _selectedPlayerIndex = (_selectedPlayerIndex - 1 + _transferListedPlayers.Count) % _transferListedPlayers.Count;
            }
            else if (key == ConsoleKey.DownArrow)
            {
                if (_transferListedPlayers.Count > 0)
                    _selectedPlayerIndex = (_selectedPlayerIndex + 1) % _transferListedPlayers.Count;
            }
            else if (key == ConsoleKey.Enter)
            {
                if (_transferListedPlayers.Count > 0)
                {
                    Player player = _transferListedPlayers[_selectedPlayerIndex];
                    // Prevent purchasing your own player.
                    if (player.CurrentClub == _userClub)
                    {
                        Console.Clear();
                        Console.WriteLine("You cannot purchase your own player!");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey(true);
                    }
                    else
                    {
                        BuyPlayer(player);
                    }
                }
            }
        }

        public override void Draw(bool active)
        {
            ClearWindowArea();
            base.Draw(active);

            // Display the user's balance at the top.
            Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y + 1);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"Balance: £{_userClub.Balance / 1_000_000}M");
            Console.ResetColor();

            RefreshTransferListedPlayers();

            int x = _rectangle.X + 2;
            int y = _rectangle.Y + 4;

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Transfer Market:");
            Console.ResetColor();
            y++;

            for (int i = 0; i < _transferListedPlayers.Count && y < _rectangle.Y + _rectangle.Height - 4; i++)
            {
                Console.SetCursorPosition(x, y);
                Player p = _transferListedPlayers[i];

                if (p.CurrentClub == _userClub)
                {
                    if (i == _selectedPlayerIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"> {p.Name} ({p.Position}) - £{p.TransferPrice}M [Your Player]");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"  {p.Name} ({p.Position}) - £{p.TransferPrice}M [Your Player]");
                    }
                }
                else
                {
                    if (i == _selectedPlayerIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"> {p.Name} ({p.Position}) - £{p.TransferPrice}M");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"  {p.Name} ({p.Position}) - £{p.TransferPrice}M");
                    }
                }
                Console.ResetColor();
                y++;
            }

            if (_transferListedPlayers.Count == 0)
            {
                Console.SetCursorPosition(x, y);
                Console.WriteLine("No players are currently listed for transfer.");
            }

            // Display instructions at the bottom.
            Console.SetCursorPosition(x, _rectangle.Y + _rectangle.Height - 2);
            Console.WriteLine("Use Up/Down arrows to navigate, Enter to buy, ESC to return.");
        }

        private void ClearWindowArea()
        {
            for (int y = _rectangle.Y; y < _rectangle.Y + _rectangle.Height; y++)
            {
                Console.SetCursorPosition(_rectangle.X, y);
                Console.Write(new string(' ', _rectangle.Width));
            }
        }

        private void RefreshTransferListedPlayers()
        {
            // Gather all players that are marked for transfer.
            _transferListedPlayers = _league.GetAllPlayers()
                .Where(p => p.AvailableForTransfer)
                .ToList();

            if (_selectedPlayerIndex >= _transferListedPlayers.Count)
                _selectedPlayerIndex = 0;
        }

        private void BuyPlayer(Player player)
        {
            Console.Clear();
            Console.WriteLine($"Do you want to buy {player.Name} from {player.CurrentClub.Name} for £{player.TransferPrice}M? (Y/N)");
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Y)
            {
                double transferFee = player.TransferPrice * 1_000_000; // Convert to actual fee.

                if (_userClub.Balance >= transferFee)
                {
                    // Process the transfer.
                    player.CurrentClub.Players.Remove(player);
                    _userClub.Players.Add(player);

                    // Update club balances.
                    _userClub.Balance -= transferFee;
                    player.CurrentClub.Balance += transferFee;

                    // Update transfer totals.
                    _userClub.TotalTransfersIn += transferFee;
                    player.CurrentClub.TotalTransfersOut += transferFee;

                    // Change player's club and remove from market.
                    player.CurrentClub = _userClub;
                    player.AvailableForTransfer = false;
                    player.TransferPrice = 0;

                    Console.WriteLine($"{player.Name} has joined {_userClub.Name}.");
                }
                else
                {
                    Console.WriteLine("Not enough funds to complete the transfer.");
                }
            }
            else
            {
                Console.WriteLine("Transfer canceled.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
