using System;
using System.Collections.Generic;
using System.Drawing;

namespace GUI
{
    public class UserInterface
    {
        public static ConsoleKeyInfo Input;
        public static int ActiveWindowIndex = 0;
        public static Player? CurrentPlayer;

        private League _league;
        private List<Window> _windows;
        public bool Closing;

        public UserInterface(League league, int managerClubIndex)
        {
            _league = league;
            Closing = false;
            CurrentPlayer = null;
            _windows = new List<Window>();

            // Define main menu options.
            var menuItems = new List<MenuOptionAction>()
            {
                new MenuOptionAction("League Table", InterfaceAction.OpenLeagueTableWindow),
                new MenuOptionAction("Players", InterfaceAction.OpenPlayerWindow),
                new MenuOptionAction("Play Match", InterfaceAction.OpenMatchesWindow),
                new MenuOptionAction("Squad Management", InterfaceAction.OpenSquadManagementWindow),
                new MenuOptionAction("Transfer Market", InterfaceAction.OpenTransferMarketWindow),
                new MenuOptionAction("Exit", InterfaceAction.ExitGame)
            };

            // Create and add windows.
            _windows.Add(new MenuWindow("Main Menu", new Rectangle(0, 0, 40, 15), true, menuItems));
            _windows.Add(new LeagueTableWindow(_league, managerClubIndex, "League Table", new Rectangle(0, 0, 60, 20), false));
            _windows.Add(new PlayersWindow(_league.Clubs[managerClubIndex].Players, "Players", new Rectangle(0, 0, 60, 20), false, new List<MenuOptionAction>()));
            _windows.Add(new MatchesWindow(_league, _league.Clubs[managerClubIndex], "Play Match", new Rectangle(0, 0, 80, 25), false));
            _windows.Add(new SquadManagementWindow(_league.Clubs[managerClubIndex], "Squad Management", new Rectangle(0, 0, 80, 25), false));
            _windows.Add(new TransferMarketWindow(_league.Clubs[managerClubIndex], _league, "Transfer Market", new Rectangle(0, 0, 80, 25), false));
        }

        public void Update()
        {
            // Process input if available.
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo globalKey = Console.ReadKey(true);
                // Global save key check.
                if (globalKey.Key == ConsoleKey.S)
                {
                    SaveLoadManager.SaveGame(0, _league); // Adjust matchCounter as needed.
                    // Display a temporary message.
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Game saved successfully. Press any key to continue...");
                    Console.ReadKey(true);
                    Input = new ConsoleKeyInfo();
                    return;
                }
                Input = globalKey;
            }

            // Check for Escape key to return to main menu.
            if (Input.Key == ConsoleKey.Escape)
            {
                ActiveWindowIndex = 0;
                HideAllBut(0);
                Console.Clear(); // Clear previous window content.
                Input = new ConsoleKeyInfo();
                return;
            }

            // Process Enter key for current window action.
            if (Input.Key == ConsoleKey.Enter)
            {
                DoInterfaceAction(_windows[ActiveWindowIndex].CurrentAction);
            }

            // Update current active window.
            _windows[ActiveWindowIndex].Update();

            // Clear input after processing to avoid repeated actions.
            Input = new ConsoleKeyInfo();
        }

        public void Draw()
        {
            // Draw the active window.
            _windows[ActiveWindowIndex].Draw(true);
        }

        private void DoInterfaceAction(InterfaceAction action)
        {
            switch (action)
            {
                case InterfaceAction.OpenLeagueTableWindow:
                    SwitchToWindow(1);
                    break;
                case InterfaceAction.OpenPlayerWindow:
                    SwitchToWindow(2);
                    break;
                case InterfaceAction.OpenMatchesWindow:
                    SwitchToWindow(3);
                    break;
                case InterfaceAction.OpenSquadManagementWindow:
                    SwitchToWindow(4);
                    break;
                case InterfaceAction.OpenTransferMarketWindow:
                    SwitchToWindow(5);
                    break;
                case InterfaceAction.TogglePlayerTransferStatus:
                    CurrentPlayer?.ToggleTransferStatus();
                    break;
                case InterfaceAction.ReturnToMainMenu:
                    SwitchToWindow(0);
                    break;
                case InterfaceAction.ExitGame:
                    Closing = true;
                    break;
                default:
                    break;
            }
        }

        private void HideAllBut(int index)
        {
            for (int i = 0; i < _windows.Count; i++)
                _windows[i].Visible = (i == index);
        }

        private void SwitchToWindow(int index)
        {
            ActiveWindowIndex = index;
            HideAllBut(index);
            Console.Clear(); // Clear console when switching windows.
        }
    }
}
