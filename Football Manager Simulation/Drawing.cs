using System;
using System.Text;

namespace GUI
{
    public static class Drawing
    {
        public static int ConsoleWidth = 150;
        public static int ConsoleHeight = 40;

        // Fills the entire off-screen buffer with empty cells.
        public static void ClearBuffer(ConsoleCell[,] buffer)
        {
            for (int row = 0; row < buffer.GetLength(0); row++)
            {
                for (int col = 0; col < buffer.GetLength(1); col++)
                {
                    buffer[row, col] = ConsoleCell.Empty;
                }
            }
        }

        // Draws the pitch into the off-screen buffer.
        public static void DrawPitch(ConsoleCell[,] buffer, int pitchLeft, int pitchTop, int pitchRight, int pitchBottom)
        {
            for (int y = pitchTop; y <= pitchBottom; y++)
            {
                for (int x = pitchLeft; x <= pitchRight; x++)
                {
                    buffer[y, x] = new ConsoleCell(' ', ConsoleColor.Green, ConsoleColor.Green);
                }
            }
            int midX = (pitchLeft + pitchRight) / 2;
            for (int y = pitchTop; y <= pitchBottom; y++)
            {
                buffer[y, midX] = new ConsoleCell('|', ConsoleColor.White, ConsoleColor.Green);
            }
        }

        // Draws a player into the buffer.
        public static void DrawPlayer(ConsoleCell[,] buffer, int x, int y, char ch, ConsoleColor fg, ConsoleColor bg)
        {
            if (y >= 0 && y < buffer.GetLength(0) && x >= 0 && x < buffer.GetLength(1))
            {
                buffer[y, x] = new ConsoleCell(ch, fg, bg);
            }
        }

        // Places a string onto the buffer.
        public static void PlaceString(ConsoleCell[,] buffer, int x, int y, string text, ConsoleColor fg, ConsoleColor bg)
        {
            if (y >= 0 && y < buffer.GetLength(0))
            {
                for (int i = 0; i < text.Length; i++)
                {
                    int pos = x + i;
                    if (pos >= 0 && pos < buffer.GetLength(1))
                    {
                        buffer[y, pos] = new ConsoleCell(text[i], fg, bg);
                    }
                }
            }
        }

        /// <summary>
        /// Renders the off-screen buffer to the console in one write operation.
        /// This minimizes flicker.
        /// </summary>
        public static void RenderBufferToConsole(ConsoleCell[,] buffer)
        {
            // Reset the cursor position to top left.
            Console.SetCursorPosition(0, 0);
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < buffer.GetLength(0); row++)
            {
                for (int col = 0; col < buffer.GetLength(1); col++)
                {
                    sb.Append(buffer[row, col].Character);
                }
                sb.AppendLine();
            }
            Console.Write(sb.ToString());
        }
    }
}
