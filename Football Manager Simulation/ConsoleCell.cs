using System;

namespace GUI
{
    public struct ConsoleCell
    {
        public char Character;
        public ConsoleColor ForegroundColor;
        public ConsoleColor BackgroundColor;

        public ConsoleCell(char character, ConsoleColor fg, ConsoleColor bg)
        {
            Character = character;
            ForegroundColor = fg;
            BackgroundColor = bg;
        }

        public static ConsoleCell Empty => new ConsoleCell(' ', ConsoleColor.Gray, ConsoleColor.Black);
    }
}
