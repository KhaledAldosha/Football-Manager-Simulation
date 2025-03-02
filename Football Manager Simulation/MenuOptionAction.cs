namespace GUI
{
    // An enumeration of interface actions. Each action represents a command
    // that can be triggered from a menu or UI element (e.g., opening a window,
    // toggling a player's transfer status, etc.).
    public enum InterfaceAction
    {
        Nothing,                    // No action
        OpenLeagueTableWindow,      // Open the league table window
        OpenPlayerWindow,           // Open the player information window
        OpenMatchesWindow,          // Open the match simulation window
        OpenSquadManagementWindow,  // Open the squad management window
        OpenTransferMarketWindow,   // Open the transfer market window
        TogglePlayerTransferStatus, // Toggle whether a player is transfer listed
        ReturnToMainMenu,           // Return to the main menu
        ExitGame                    // Exit the game
    }

    // MenuOptionAction represents a single option in a menu,
    // including the text to display and the associated action.
    public class MenuOptionAction
    {
        public readonly string Text;          // The text shown in the menu for this option
        public readonly InterfaceAction Action; // The action to perform when selected
        // Constructor that initializes a menu option with its text and action.
        public MenuOptionAction(string text, InterfaceAction action)
        {
            Text = text;
            Action = action;
        }
    }
}
