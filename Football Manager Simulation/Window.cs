using System;
using System.Drawing;

namespace GUI
{
    // Base class for all UI windows.
    // Handles basic rectangle and visibility,
    // plus a simple border drawing.
    public class Window
    {
        protected Rectangle _rectangle;     // Where the window is placed on console
        protected string _title;           // Window title
        protected bool _visible;           // Whether the window is currently shown
        protected InterfaceAction _currentAction; // The action associated with the window

        public bool Visible { get => _visible; set => _visible = value; }
        public InterfaceAction CurrentAction => _currentAction;
        // Constructor sets up title, rectangle, and visibility.
        public Window(string title, Rectangle rectangle, bool visible)
        {
            _title = title;
            _rectangle = rectangle;
            _visible = visible;
            _currentAction = InterfaceAction.Nothing;
        }
        // Virtual Update method: does nothing by default.
        // Subclasses override to implement logic.
        public virtual void Update() { }

        // Draws the window's border and title if it's visible.
        // 'active' indicates if it's the topmost / active window.
        public virtual void Draw(bool active)
        {
            if (!_visible)
                return;

            // Choose color for border
            Console.ForegroundColor = active ? ConsoleColor.Blue : ConsoleColor.DarkGray;

            // Draw top border
            Console.SetCursorPosition(_rectangle.X, _rectangle.Y);
            Console.Write("+" + new string('-', _rectangle.Width - 2) + "+");

            // Draw side borders
            for (int i = 1; i < _rectangle.Height - 1; i++)
            {
                Console.SetCursorPosition(_rectangle.X, _rectangle.Y + i);
                Console.Write("|" + new string(' ', _rectangle.Width - 2) + "|");
            }

            // Draw bottom border
            Console.SetCursorPosition(_rectangle.X, _rectangle.Y + _rectangle.Height - 1);
            Console.Write("+" + new string('-', _rectangle.Width - 2) + "+");

            // Draw the title
            Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y);
            Console.ForegroundColor = active ? ConsoleColor.White : ConsoleColor.Gray;
            Console.Write(_title);
            Console.ResetColor();
        }
        // Helper method to draw text inside the window’s content area.
        // offsetX, offsetY are relative to the top-left inside the border.
        protected void DrawText(int offsetX, int offsetY, string text, ConsoleColor fg, ConsoleColor bg)
        {
            Console.SetCursorPosition(_rectangle.X + 1 + offsetX, _rectangle.Y + 1 + offsetY);
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
            Console.Write(text);
            Console.ResetColor();
        }
        // Clears the window’s content area (inside the border) by overwriting with spaces.
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
