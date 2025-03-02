using System;
using System.Drawing;
using System.Collections.Generic;

namespace GUI
{
    public class MenuWindow : Window
    {
        protected List<MenuOptionAction> _items;
        protected int _activeItem;

        public int ActiveItemIndex => _activeItem;

        public MenuWindow(string title, Rectangle rectangle, bool visible, List<MenuOptionAction> items)
            : base(title, rectangle, visible)
        {
            _items = items;
            _activeItem = 0;
        }

        public override void Update()
        {
            base.Update();
            if (!_visible) return;

            if (_items.Count == 0)
            {
                _currentAction = InterfaceAction.Nothing;
                return;
            }

            // Process up/down keys from the global input.
            if (UserInterface.Input.Key == ConsoleKey.DownArrow)
                _activeItem = (_activeItem + 1) % _items.Count;
            else if (UserInterface.Input.Key == ConsoleKey.UpArrow)
                _activeItem = (_activeItem - 1 + _items.Count) % _items.Count;

            _currentAction = _items[_activeItem].Action;
        }

        public override void Draw(bool active)
        {
            base.Draw(active);
            if (!_visible) return;

            for (int i = 0; i < _items.Count; i++)
            {
                Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y + 2 + i);
                Console.ForegroundColor = (active && i == _activeItem) ? ConsoleColor.Green : ConsoleColor.Gray;
                Console.Write($"{i + 1}. {_items[i].Text}");
            }
            Console.ResetColor();
        }
    }
}
