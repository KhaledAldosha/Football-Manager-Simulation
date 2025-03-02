using System;
using System.Drawing;
using System.Numerics;

namespace GUI
{
    // Represents a single transfer event of a player from one club to another.
    public class Transfer
    {
        public Player Player { get; set; }       // The player being transferred
        public Club FromClub { get; set; }       // Original club
        public Club ToClub { get; set; }         // Destination club
        public double Fee { get; set; }          // Transfer fee (in currency)
        public DateTime Date { get; set; }       // Date of the transfer

        // Constructor for a transfer record.
        public Transfer(Player player, Club fromClub, Club toClub, double fee, DateTime date)
        {
            Player = player;
            FromClub = fromClub;
            ToClub = toClub;
            Fee = fee;
            Date = date;
        }
    }
    // A user interface window that displays the transfer market
    // and allows the user to purchase players for their club.
    public class TransferMarketWindow : Window
    {
        private Club _userClub;                   // Reference to the user's club
        private League _league;                   // Reference to the league for data
        private List<Player> _transferListedPlayers; // List of players available for transfer
        private int _selectedPlayerIndex;         // Which player is highlighted in the UI

        // Constructor for the TransferMarketWindow, sets up references and default states.
        public TransferMarketWindow(
            Club userClub,
            League league,
            string title,
            Rectangle rectangle,
            bool visible)
            : base(title, rectangle, visible)
        {
            _userClub = userClub;
            _league = league;
            _transferListedPlayers = new List<Player>();
            _selectedPlayerIndex = 0;
        }
        // Update method handles user input for navigation and purchasing players.
        public override void Update()
        {
            // Capture the key from a global input object
            var key = UserInterface.Input.Key;

            // If user presses Escape, we want to return to main menu
            if (key == ConsoleKey.Escape)
            {
                _currentAction = InterfaceAction.ReturnToMainMenu;
                return;
            }

            // Up arrow => move selection up
            if (key == ConsoleKey.UpArrow)
            {
                if (_transferListedPlayers.Count > 0)
                {
                    _selectedPlayerIndex =
                        (_selectedPlayerIndex - 1 + _transferListedPlayers.Count)
                        % _transferListedPlayers.Count;
                }
            }
            // Down arrow => move selection down
            else if (key == ConsoleKey.DownArrow)
            {
                if (_transferListedPlayers.Count > 0)
                {
                    _selectedPlayerIndex =
                        (_selectedPlayerIndex + 1)
                        % _transferListedPlayers.Count;
                }
            }
            // Enter => attempt to buy selected player
            else if (key == ConsoleKey.Enter)
            {
                if (_transferListedPlayers.Count > 0)
                {
                    Player player = _transferListedPlayers[_selectedPlayerIndex];
                    // Prevent buying your own player
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
        // Draw method displays the list of transfer-listed players,
        // highlighting the selected player, and shows instructions.
        public override void Draw(bool active)
        {
            ClearWindowArea();
            base.Draw(active);

            // Display the user's balance at the top
            Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y + 1);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"Balance: £{_userClub.Balance / 1_000_000}M");
            Console.ResetColor();

            // Refresh the list of players on the transfer list
            RefreshTransferListedPlayers();

            int x = _rectangle.X + 2;
            int y = _rectangle.Y + 4;

            // Show heading
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Transfer Market:");
            Console.ResetColor();
            y++;

            // Loop through transfer-listed players
            for (int i = 0; i < _transferListedPlayers.Count &&
                 y < _rectangle.Y + _rectangle.Height - 4; i++)
            {
                Console.SetCursorPosition(x, y);
                Player p = _transferListedPlayers[i];

                // If the player belongs to the user
                if (p.CurrentClub == _userClub)
                {
                    // Highlight if selected
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
                    // If selected, use bright color
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

            // If no players are listed
            if (_transferListedPlayers.Count == 0)
            {
                Console.SetCursorPosition(x, y);
                Console.WriteLine("No players are currently listed for transfer.");
            }

            // Display instructions at bottom
            Console.SetCursorPosition(x, _rectangle.Y + _rectangle.Height - 2);
            Console.WriteLine("Use Up/Down arrows to navigate, Enter to buy, ESC to return.");
        }

        // Clears the window's content area, overwriting with spaces.
        private void ClearWindowArea()
        {
            for (int y = _rectangle.Y; y < _rectangle.Y + _rectangle.Height; y++)
            {
                Console.SetCursorPosition(_rectangle.X, y);
                Console.Write(new string(' ', _rectangle.Width));
            }
        }

        // Refreshes the list of transfer-listed players from the league data.
        private void RefreshTransferListedPlayers()
        {
            // Gather all players who are flagged as AvailableForTransfer
            _transferListedPlayers = _league.GetAllPlayers()
                .Where(p => p.AvailableForTransfer)
                .ToList();

            // If our index is out of range, reset to 0
            if (_selectedPlayerIndex >= _transferListedPlayers.Count)
                _selectedPlayerIndex = 0;
        }
        // Buys a player from the transfer list if the user has enough funds.
        private void BuyPlayer(Player player)
        {
            Console.Clear();
            Console.WriteLine($"Do you want to buy {player.Name} from {player.CurrentClub.Name} for £{player.TransferPrice}M? (Y/N)");
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Y)
            {
                double transferFee = player.TransferPrice * 1_000_000; // Convert M to actual
                if (_userClub.Balance >= transferFee)
                {
                    // Process the transfer
                    player.CurrentClub.Players.Remove(player);
                    _userClub.Players.Add(player);

                    // Update club balances
                    _userClub.Balance -= transferFee;
                    player.CurrentClub.Balance += transferFee;

                    // Update stats
                    _userClub.TotalTransfersIn += transferFee;
                    player.CurrentClub.TotalTransfersOut += transferFee;

                    // Mark the player's new club
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
