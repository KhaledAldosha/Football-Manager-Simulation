using System;
using System.Collections.Generic;
using System.Drawing;

namespace GUI
{
    public class ClubSelectionWindow : MenuWindow
    {
        public ClubSelectionWindow(List<Club> clubs, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible, new List<MenuOptionAction>())
        {
            foreach (var club in clubs)
            {
                _items.Add(new MenuOptionAction(club.Name, InterfaceAction.Nothing));
            }
        }
    }
}
