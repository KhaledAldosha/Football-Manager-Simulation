using System;
using System.Drawing;

namespace GUI
{
    public class Window
    {
        protected Rectangle _rectangle;
        protected string _title;
        protected bool _visible;
        protected InterfaceAction _currentAction;

        public bool Visible { get => _visible; set => _visible = value; }
        public InterfaceAction CurrentAction => _currentAction;

        public Window(string title, Rectangle rectangle, bool visible)
        {
            _title = title;
            _rectangle = rectangle;
            _visible = visible;
            _currentAction = InterfaceAction.Nothing;
        }

        public virtual void Update() { }

        // Basic Draw: draws a border and title.
        public virtual void Draw(bool active)
        {
            if (!_visible)
                return;

            Console.ForegroundColor = active ? ConsoleColor.Blue : ConsoleColor.DarkGray;
            Console.SetCursorPosition(_rectangle.X, _rectangle.Y);
            Console.Write("+" + new string('-', _rectangle.Width - 2) + "+");

            for (int i = 1; i < _rectangle.Height - 1; i++)
            {
                Console.SetCursorPosition(_rectangle.X, _rectangle.Y + i);
                Console.Write("|" + new string(' ', _rectangle.Width - 2) + "|");
            }

            Console.SetCursorPosition(_rectangle.X, _rectangle.Y + _rectangle.Height - 1);
            Console.Write("+" + new string('-', _rectangle.Width - 2) + "+");

            // Draw the title inside the top border.
            Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y);
            Console.ForegroundColor = active ? ConsoleColor.White : ConsoleColor.Gray;
            Console.Write(_title);
            Console.ResetColor();
        }

        /// Draws text relative to the window’s content area (inside the border).
        protected void DrawText(int offsetX, int offsetY, string text, ConsoleColor fg, ConsoleColor bg)
        {
            Console.SetCursorPosition(_rectangle.X + 1 + offsetX, _rectangle.Y + 1 + offsetY);
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
            Console.Write(text);
            Console.ResetColor();
        }

        /// Clears the window’s content area (inside the border).
        protected void ClearContent()
        {
            for (int y = 1; y < _rectangle.Height - 1; y++)
            {
                Console.SetCursorPosition(_rectangle.X + 1, _rectangle.Y + y);
                Console.Write(new string(' ', _rectangle.Width - 2));
            }
        }
    }
}
