using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace GUI
{
    public class PlayersWindow : MenuWindow
    {
        private List<Player> _players;

        public PlayersWindow(List<Player> players, string title, Rectangle rectangle, bool visible, List<MenuOptionAction> items)
            : base(title, rectangle, visible, items)
        {
            _players = players;
            RefreshPlayerList();
        }

        private void RefreshPlayerList()
        {
            _items.Clear();
            if (_players.Count == 0)
            {
                _items.Add(new MenuOptionAction("No players available", InterfaceAction.Nothing));
            }
            else
            {
                foreach (var p in _players)
                    _items.Add(new MenuOptionAction(p.Info(), InterfaceAction.TogglePlayerTransferStatus));
            }
            if (_activeItem >= _items.Count)
                _activeItem = 0;
        }

        public override void Update()
        {
            // When the user presses T, prompt for transfer price and mark the player for transfer.
            if (UserInterface.Input.Key == ConsoleKey.T && _players.Count > 0 && _activeItem < _players.Count)
            {
                Player selected = _players[_activeItem];
                // Only prompt if the player is not already transfer-listed.
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

            base.Update();
            RefreshPlayerList();

            if (_players.Count > 0 && _activeItem < _players.Count)
                UserInterface.CurrentPlayer = _players[_activeItem];
            else
                UserInterface.CurrentPlayer = null;
        }
    }
}
