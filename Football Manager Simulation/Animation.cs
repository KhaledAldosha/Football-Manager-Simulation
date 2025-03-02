using System;
using System.Threading;

namespace GUI
{
    public static class Animation
    {
        /// Animates the ball’s travel from a start point to an endpoint.
        public static void SimulateBallTravel(int startX, int startY, int endX, int endY, int steps, int delay, Func<int, int, (int, int)> offsetFunc = null)
        {
            double deltaX = (endX - startX) / (double)steps;
            double deltaY = (endY - startY) / (double)steps;
            double ballX = startX, ballY = startY;
            for (int i = 0; i < steps; i++)
            {
                EraseBallAt((int)Math.Round(ballX), (int)Math.Round(ballY));
                ballX += deltaX;
                ballY += deltaY;
                int currentX = (int)Math.Round(ballX);
                int currentY = (int)Math.Round(ballY);
                if (offsetFunc != null)
                {
                    var offset = offsetFunc(i, steps);
                    currentX += offset.Item1;
                    currentY += offset.Item2;
                }
                DrawBallAt(currentX, currentY);
                Thread.Sleep(delay);
            }
            EraseBallAt((int)Math.Round(ballX), (int)Math.Round(ballY));
        }

        /// Animates a pass from one point to another.
        public static void AnimatePass(int sx, int sy, int ex, int ey, int gameSpeed, Random random, out bool stolen)
        {
            stolen = false;
            int steps = 20;
            int delay = (int)(30 / (gameSpeed / 100.0));  // Reduced delay from 50 to 30
            if (delay < 3)
                delay = 3;
            SimulateBallTravel(sx, sy, ex, ey, steps, delay, (i, total) => (0, (int)(Math.Sin(Math.PI * i / total))));
            // Reduce interception chance to 1%
            if (random.NextDouble() < 0.01)
                stolen = true;
        }

        /// Animates a shot from the shooter to the target.
        public static void AnimateShot(int shooterX, int shooterY, int targetX, int targetY, int gameSpeed)
        {
            int steps = 20;
            int delay = (int)(30 / (gameSpeed / 100.0));  // Reduced delay from 50 to 30
            if (delay < 3)
                delay = 3;
            SimulateBallTravel(shooterX, shooterY, targetX, targetY, steps, delay);
        }
        /// Erases the ball at the given coordinates.
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

        /// Draws the ball at the given coordinates.
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

        /// Checks if the given coordinates are within the pitch boundaries.
        private static bool IsPositionWithinPitch(int x, int y)
        {
            int pitchLeft = 2, pitchTop = 4, pitchRight = 100, pitchBottom = 30;
            return x >= pitchLeft && x <= pitchRight && y >= pitchTop && y <= pitchBottom;
        }

        /// Marks a cell as occupied in the given grid.
        /// The grid indices are computed relative to the provided offset.
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
