using System;
using System.Drawing;
using System.Collections.Generic;

namespace GUI
{
    // A window that displays a list of menu options (like a vertical menu).
    // The user can navigate up/down and select an action.
    public class MenuWindow : Window
    {
        protected List<MenuOptionAction> _items; // The list of menu items
        protected int _activeItem;               // Which item is selected

        public int ActiveItemIndex => _activeItem;

        // Constructor for a MenuWindow. 
        // 'items' are the list of menu actions we can choose from.
        public MenuWindow(
            string title,
            Rectangle rectangle,
            bool visible,
            List<MenuOptionAction> items)
            : base(title, rectangle, visible)
        {
            _items = items;
            _activeItem = 0;
        }

        // Update handles up/down arrow input to change the active item
        // and sets the _currentAction to the action of the active item.
        public override void Update()
        {
            base.Update();
            if (!_visible) return;

            // If there are no items, do nothing
            if (_items.Count == 0)
            {
                _currentAction = InterfaceAction.Nothing;
                return;
            }

            // Check global input
            if (UserInterface.Input.Key == ConsoleKey.DownArrow)
            {
                _activeItem = (_activeItem + 1) % _items.Count;
            }
            else if (UserInterface.Input.Key == ConsoleKey.UpArrow)
            {
                _activeItem = (_activeItem - 1 + _items.Count) % _items.Count;
            }

            // Set the current action to the active item's action
            _currentAction = _items[_activeItem].Action;
        }
        // Draw shows each menu item, highlighting the active one.
        public override void Draw(bool active)
        {
            base.Draw(active);
            if (!_visible) return;

            for (int i = 0; i < _items.Count; i++)
            {
                Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y + 2 + i);
                // If this is the active item and window is active, highlight it
                Console.ForegroundColor = (active && i == _activeItem) ? ConsoleColor.Green : ConsoleColor.Gray;
                Console.Write($"{i + 1}. {_items[i].Text}");
            }
            Console.ResetColor();
        }
    }
}
