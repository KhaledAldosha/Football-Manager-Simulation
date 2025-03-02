using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace GUI
{
    // A window that displays a list of players and allows toggling
    // their transfer status or setting their price.
    public class PlayersWindow : MenuWindow
    {
        private List<Player> _players; // Reference to the list of players displayed

        // Constructor for the players window.
        // Inherits from MenuWindow so we can navigate through player entries as menu items.
        public PlayersWindow(
            List<Player> players,
            string title,
            Rectangle rectangle,
            bool visible,
            List<MenuOptionAction> items)
            : base(title, rectangle, visible, items)
        {
            _players = players;
            RefreshPlayerList(); // Build the initial list of menu items from the players
        }
        // Rebuilds the _items list based on the current _players collection.
        private void RefreshPlayerList()
        {
            _items.Clear();
            // If there are no players, show a single "No players" item
            if (_players.Count == 0)
            {
                _items.Add(new MenuOptionAction("No players available", InterfaceAction.Nothing));
            }
            else
            {
                // For each player, add a menu option showing their info
                foreach (var p in _players)
                    _items.Add(new MenuOptionAction(p.Info(), InterfaceAction.TogglePlayerTransferStatus));
            }

            // Ensure _activeItem is within bounds
            if (_activeItem >= _items.Count)
                _activeItem = 0;
        }
        // Update method handles toggling transfer status (via T key),
        // then calls base.Update() for up/down navigation.
        public override void Update()
        {
            // If user presses 'T', prompt for a new transfer price for the selected player
            if (UserInterface.Input.Key == ConsoleKey.T &&
                _players.Count > 0 &&
                _activeItem < _players.Count)
            {
                Player selected = _players[_activeItem];
                // Only prompt if player isn't already on the transfer list
                if (!selected.AvailableForTransfer)
                {
                    Console.Clear();
                    Console.WriteLine($"Enter transfer price in millions for {selected.Name}:");
                    string input = Console.ReadLine();
                    if (double.TryParse(input, out double price))
                    {
                        selected.AvailableForTransfer = true;
                        selected.TransferPrice = price;
                    }
                    else
                    {
                        Console.WriteLine("Invalid price. Press any key to continue...");
                        Console.ReadKey(true);
                    }
                }
            }

            // Call the base update to handle menu navigation (up/down) and action setting
            base.Update();

            // Refresh the list in case something changed (like toggling status)
            RefreshPlayerList();

            // If we have a valid selected item, set the global CurrentPlayer
            if (_players.Count > 0 && _activeItem < _players.Count)
                UserInterface.CurrentPlayer = _players[_activeItem];
            else
                UserInterface.CurrentPlayer = null;
        }
    }
}
