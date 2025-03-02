namespace GUI
{
    public enum InterfaceAction
    {
        Nothing,
        OpenLeagueTableWindow,
        OpenPlayerWindow,
        OpenMatchesWindow,
        OpenSquadManagementWindow,
        OpenTransferMarketWindow,
        TogglePlayerTransferStatus,
        ReturnToMainMenu,
        ExitGame
    }

    public class MenuOptionAction
    {
        public readonly string Text;
        public readonly InterfaceAction Action;

        public MenuOptionAction(string text, InterfaceAction action)
        {
            Text = text;
            Action = action;
        }
    }
}
