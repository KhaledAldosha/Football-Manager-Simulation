using System;
using System.Threading;

namespace GUI
{
    // The Animation class provides methods for simulating ball travel,
    // passes, and shots with simple delays and console output effects.
    public static class Animation
    {
        // Simulates the ball traveling from a starting point to an endpoint in a given number of steps.
        // Optionally, an offset function can be applied to modify the ball's path.
        // Each step the ball is drawn and then erased after a delay.
        public static void SimulateBallTravel(int startX, int startY, int endX, int endY, int steps, int delay, Func<int, int, (int, int)> offsetFunc = null)
        {
            double deltaX = (endX - startX) / (double)steps;
            double deltaY = (endY - startY) / (double)steps;
            double ballX = startX, ballY = startY;

            for (int i = 0; i < steps; i++)
            {
                // Erase previous ball position.
                EraseBallAt((int)Math.Round(ballX), (int)Math.Round(ballY));
                // Move ball along the calculated delta.
                ballX += deltaX;
                ballY += deltaY;
                int currentX = (int)Math.Round(ballX);
                int currentY = (int)Math.Round(ballY);

                // If an offset function is provided, modify the current coordinates.
                if (offsetFunc != null)
                {
                    var offset = offsetFunc(i, steps);
                    currentX += offset.Item1;
                    currentY += offset.Item2;
                }

                // Draw the ball at the new position.
                DrawBallAt(currentX, currentY);
                Thread.Sleep(delay); // Pause between steps.
            }

            // Erase the ball at the final position.
            EraseBallAt((int)Math.Round(ballX), (int)Math.Round(ballY));
        }
        // Animates a pass from the starting coordinates to the ending coordinates.
        // The delay is adjusted based on game speed, and there is a small chance the pass is intercepted.
        public static void AnimatePass(int sx, int sy, int ex, int ey, int gameSpeed, Random random, out bool stolen)
        {
            stolen = false;
            int steps = 20;
            int delay = (int)(30 / (gameSpeed / 100.0));
            if (delay < 3)
                delay = 3;

            // Apply a sine offset to simulate a curved pass
            SimulateBallTravel(sx, sy, ex, ey, steps, delay, (i, total) => (0, (int)(Math.Sin(Math.PI * i / total))));

            // 1% chance of interception
            if (random.NextDouble() < 0.01)
                stolen = true;
        }
        // Animates a shot from the shooter to the target.
        // Uses a similar simulation as AnimatePass but without an offset.
        public static void AnimateShot(int shooterX, int shooterY, int targetX, int targetY, int gameSpeed)
        {
            int steps = 20;
            int delay = (int)(30 / (gameSpeed / 100.0));
            if (delay < 3)
                delay = 3;
            SimulateBallTravel(shooterX, shooterY, targetX, targetY, steps, delay);
        }

        // Erases the ball at the specified coordinates.
        // Sets the console colors to green (pitch color) to blend in.
        public static void EraseBallAt(int x, int y)
        {
            if (IsPositionWithinPitch(x, y))
            {
                Console.SetCursorPosition(x, y);
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" ");
                Console.ResetColor();
            }
        }
        // Draws the ball (represented by an 'o') at the specified coordinates.
        // Uses black background and yellow foreground to highlight the ball.
        public static void DrawBallAt(int x, int y)
        {
            if (IsPositionWithinPitch(x, y))
            {
                Console.SetCursorPosition(x, y);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("o");
                Console.ResetColor();
            }
        }
        // Checks if the given absolute coordinates (x, y) fall within the pitch boundaries.
        private static bool IsPositionWithinPitch(int x, int y)
        {
            int pitchLeft = 2, pitchTop = 4, pitchRight = 100, pitchBottom = 30;
            return x >= pitchLeft && x <= pitchRight && y >= pitchTop && y <= pitchBottom;
        }

        // Marks a cell as occupied in the provided grid.
        // The grid's indices are adjusted based on offsetX and offsetY.
        public static void MarkOccupied(bool[,] occupied, int x, int y, int offsetX, int offsetY)
        {
            int gridX = x - offsetX;
            int gridY = y - offsetY;
            if (gridX < 0 || gridX >= occupied.GetLength(1) || gridY < 0 || gridY >= occupied.GetLength(0))
                return;
            occupied[gridY, gridX] = true;
        }
    }
}
