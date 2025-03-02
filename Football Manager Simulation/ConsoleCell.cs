using System;

namespace GUI
{
    // Represents a single cell of the console's drawing buffer.
    // Contains the character to be drawn, and its foreground and background colors.
    public struct ConsoleCell
    {
        // The character displayed in this cell
        public char Character;
        // The color of the text
        public ConsoleColor ForegroundColor;
        // The background color of the cell
        public ConsoleColor BackgroundColor;
        // Constructor to create a ConsoleCell with specified character and colors.
        public ConsoleCell(char character, ConsoleColor fg, ConsoleColor bg)
        {
            Character = character;
            ForegroundColor = fg;
            BackgroundColor = bg;
        }
        // Returns an empty cell with default gray foreground and black background.
        public static ConsoleCell Empty => new ConsoleCell(' ', ConsoleColor.Gray, ConsoleColor.Black);
    }
}
