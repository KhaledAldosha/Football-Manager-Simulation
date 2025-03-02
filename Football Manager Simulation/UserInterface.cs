using System;
using System.Collections.Generic;
using System.Drawing;

namespace GUI
{
    // Main user interface manager that controls which window is active
    // and processes global user input (e.g. saving the game).
    public class UserInterface
    {
        // Global input key
        public static ConsoleKeyInfo Input;
        // Which window index is currently active
        public static int ActiveWindowIndex = 0;
        // Reference to the current player, if any
        public static Player? CurrentPlayer;

        private League _league;         // Reference to the league data
        private List<Window> _windows;  // List of windows in the UI
        public bool Closing;            // Whether the UI is closing (game exit)

        // Constructor sets up references and creates the windows in the interface.
        // managerClubIndex is which club is the user's club.
        public UserInterface(League league, int managerClubIndex)
        {
            _league = league;
            Closing = false;
            CurrentPlayer = null;
            _windows = new List<Window>();

            // Define main menu options
            var menuItems = new List<MenuOptionAction>()
            {
                new MenuOptionAction("League Table", InterfaceAction.OpenLeagueTableWindow),
                new MenuOptionAction("Players", InterfaceAction.OpenPlayerWindow),
                new MenuOptionAction("Play Match", InterfaceAction.OpenMatchesWindow),
                new MenuOptionAction("Squad Management", InterfaceAction.OpenSquadManagementWindow),
                new MenuOptionAction("Transfer Market", InterfaceAction.OpenTransferMarketWindow),
                new MenuOptionAction("Exit", InterfaceAction.ExitGame)
            };

            // Create the various windows used by the UI
            _windows.Add(new MenuWindow("Main Menu", new Rectangle(0, 0, 40, 15), true, menuItems));
            _windows.Add(new LeagueTableWindow(_league, managerClubIndex, "League Table", new Rectangle(0, 0, 60, 20), false));
            _windows.Add(new PlayersWindow(_league.Clubs[managerClubIndex].Players, "Players", new Rectangle(0, 0, 60, 20), false, new List<MenuOptionAction>()));
            _windows.Add(new MatchesWindow(_league, _league.Clubs[managerClubIndex], "Play Match", new Rectangle(0, 0, 80, 25), false));
            _windows.Add(new SquadManagementWindow(_league.Clubs[managerClubIndex], "Squad Management", new Rectangle(0, 0, 80, 25), false));
            _windows.Add(new TransferMarketWindow(_league.Clubs[managerClubIndex], _league, "Transfer Market", new Rectangle(0, 0, 80, 25), false));
        }
        // Main update loop: checks for global input (like saving) or ESC to return to main menu,
        // and then updates the active window.
        public void Update()
        {
            // If there's a key available
            if (Console.KeyAvailable)
            {
                // Read it
                ConsoleKeyInfo globalKey = Console.ReadKey(true);

                // If the user pressed 'S', we attempt to save the game
                if (globalKey.Key == ConsoleKey.S)
                {
                    // For example, we pass '0' as matchCounter
                    SaveLoadManager.SaveGame(0, _league);
                    // Show a quick message
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Game saved successfully. Press any key to continue...");
                    Console.ReadKey(true);
                    Input = new ConsoleKeyInfo();
                    return;
                }

                // Otherwise store it as the current input
                Input = globalKey;
            }

            // If ESC is pressed, return to main menu
            if (Input.Key == ConsoleKey.Escape)
            {
                ActiveWindowIndex = 0;
                HideAllBut(0);
                Console.Clear();
                Input = new ConsoleKeyInfo();
                return;
            }

            // If Enter is pressed, do the current window's action
            if (Input.Key == ConsoleKey.Enter)
            {
                DoInterfaceAction(_windows[ActiveWindowIndex].CurrentAction);
            }

            // Update the active window
            _windows[ActiveWindowIndex].Update();

            // Clear input so it doesn't repeat
            Input = new ConsoleKeyInfo();
        }
        // Draw method draws only the active window.
        public void Draw()
        {
            _windows[ActiveWindowIndex].Draw(true);
        }
        // Takes an interface action (like open a window or exit the game)
        // and performs the appropriate UI change.
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
                    // Toggle the transfer status of the current player (if any)
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
        // Hides all windows except the one at 'index'.
        private void HideAllBut(int index)
        {
            for (int i = 0; i < _windows.Count; i++)
                _windows[i].Visible = (i == index);
        }
        // Switches active window to the given index,
        // hides all others, and clears the console.
        private void SwitchToWindow(int index)
        {
            ActiveWindowIndex = index;
            HideAllBut(index);
            Console.Clear();
        }
    }
}
