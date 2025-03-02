using System;
using System.Collections.Generic;
using System.Drawing;

namespace GUI
{
    // The ClubSelectionWindow displays a list of clubs for the user to choose from.
    // It extends the MenuWindow, using each club's name as a menu option.
    public class ClubSelectionWindow : MenuWindow
    {
        // Constructor takes a list of clubs, a title, a rectangle defining the window,
        // and a visibility flag. Each club's name is added as a menu option.
        public ClubSelectionWindow(List<Club> clubs, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible, new List<MenuOptionAction>())
        {
            // For each club in the list, add a new menu option with the club's name.
            foreach (var club in clubs)
            {
                _items.Add(new MenuOptionAction(club.Name, InterfaceAction.Nothing));
            }
        }
    }
}
