using System;

namespace GUI
{
    // The Drawing class handles low-level drawing operations for the simulation.
    // It manages a buffer of ConsoleCell items and renders them to the console.
    public static class Drawing
    {
        // Define the console buffer size for drawing.
        public static int ConsoleWidth = 150;
        public static int ConsoleHeight = 40;
        // Clears the given buffer by filling it with empty cells.
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
        // Draws the pitch onto the buffer.
        // Fills the pitch area with green and draws mid-line and goals.
        public static void DrawPitch(ConsoleCell[,] buffer, int pitchLeft, int pitchTop, int pitchRight, int pitchBottom)
        {
            // Fill pitch with green (representing grass)
            for (int y = pitchTop; y <= pitchBottom; y++)
            {
                for (int x = pitchLeft; x <= pitchRight; x++)
                {
                    buffer[y, x] = new ConsoleCell(' ', ConsoleColor.Green, ConsoleColor.Green);
                }
            }

            // Draw mid-line: vertical line in the middle of the pitch
            int midX = (pitchLeft + pitchRight) / 2;
            for (int y = pitchTop; y <= pitchBottom; y++)
            {
                buffer[y, midX] = new ConsoleCell('|', ConsoleColor.White, ConsoleColor.Green);
            }

            // Draw a simple center circle (or plus sign) at the middle of the pitch
            int centerY = (pitchTop + pitchBottom) / 2;
            for (int i = -3; i <= 3; i++)
            {
                int xx = midX + i;
                if (xx >= pitchLeft && xx <= pitchRight)
                {
                    buffer[centerY, xx] = new ConsoleCell('+', ConsoleColor.White, ConsoleColor.Green);
                }
            }

            // Draw goal areas (represented with 'G') at both ends
            // For left goal: two columns at pitchLeft and pitchLeft+1
            for (int y = centerY - 2; y <= centerY + 2; y++)
            {
                buffer[y, pitchLeft] = new ConsoleCell('G', ConsoleColor.Gray, ConsoleColor.DarkGray);
                if (pitchLeft + 1 <= pitchRight)
                    buffer[y, pitchLeft + 1] = new ConsoleCell('G', ConsoleColor.Gray, ConsoleColor.DarkGray);
            }
            // For right goal: two columns at pitchRight and pitchRight-1
            for (int y = centerY - 2; y <= centerY + 2; y++)
            {
                buffer[y, pitchRight] = new ConsoleCell('G', ConsoleColor.Gray, ConsoleColor.DarkGray);
                if (pitchRight - 1 >= pitchLeft)
                    buffer[y, pitchRight - 1] = new ConsoleCell('G', ConsoleColor.Gray, ConsoleColor.DarkGray);
            }
        }

        // Draws a single player (or other element) onto the buffer at (x, y)
        // with the specified character and colors.
        public static void DrawPlayer(ConsoleCell[,] buffer, int x, int y, char ch, ConsoleColor fg, ConsoleColor bg)
        {
            // Ensure the coordinates are within the buffer bounds.
            if (y >= 0 && y < buffer.GetLength(0) && x >= 0 && x < buffer.GetLength(1))
            {
                buffer[y, x] = new ConsoleCell(ch, fg, bg);
            }
        }

        // Places a string into the buffer starting at (x, y) with the specified colors.
        public static void PlaceString(ConsoleCell[,] buffer, int x, int y, string text, ConsoleColor fg, ConsoleColor bg)
        {
            if (y >= 0 && y < buffer.GetLength(0))
            {
                for (int i = 0; i < text.Length; i++)
                {
                    int col = x + i;
                    if (col >= 0 && col < buffer.GetLength(1))
                    {
                        buffer[y, col] = new ConsoleCell(text[i], fg, bg);
                    }
                }
            }
        }
        // Renders the entire buffer to the console.
        // Iterates over each cell, sets the colors, and writes the character.
        public static void RenderBufferToConsole(ConsoleCell[,] buffer)
        {
            Console.SetCursorPosition(0, 0);
            for (int row = 0; row < buffer.GetLength(0); row++)
            {
                for (int col = 0; col < buffer.GetLength(1); col++)
                {
                    var cell = buffer[row, col];
                    Console.ForegroundColor = cell.ForegroundColor;
                    Console.BackgroundColor = cell.BackgroundColor;
                    Console.Write(cell.Character);
                }
            }
            Console.ResetColor();
        }

        // Initiates a live match simulation by creating a Match object and starting it.
        public static void RunLiveMatchSimulation(Club userClub, Club opponentClub)
        {
            new Match(userClub, opponentClub).Start();
        }
    }
}
