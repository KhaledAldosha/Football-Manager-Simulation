using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI
{
    public static class TeamUpdaterFixed
    {
        /// Finds a path using breadth-first search on a grid.
        /// The occupied grid is provided along with offsetX and offsetY that map absolute coordinates 
        /// to grid indices (i.e. grid cell = (absoluteX - offsetX, absoluteY - offsetY)).
        /// Returns a list of absolute coordinate pairs representing the path from start to goal.
        private static List<(int, int)> FindPathBFS(int startX, int startY, int goalX, int goalY, bool[,] occupied, int offsetX, int offsetY)
        {
            int rows = occupied.GetLength(0);
            int cols = occupied.GetLength(1);

            // Convert absolute coordinates to grid indices.
            int startGridX = startX - offsetX;
            int startGridY = startY - offsetY;
            int goalGridX = goalX - offsetX;
            int goalGridY = goalY - offsetY;

            // Validate starting and goal positions.
            if (startGridX < 0 || startGridX >= cols || startGridY < 0 || startGridY >= rows ||
                goalGridX < 0 || goalGridX >= cols || goalGridY < 0 || goalGridY >= rows)
            {
                return null;
            }

            var queue = new Queue<(int, int)>();
            var cameFrom = new Dictionary<(int, int), (int, int)>();
            bool[,] visited = new bool[rows, cols];

            queue.Enqueue((startGridX, startGridY));
            visited[startGridY, startGridX] = true;
            cameFrom[(startGridX, startGridY)] = (-1, -1);

            // 8 directional moves.
            int[] dx = { -1, 0, 1, 0, -1, -1, 1, 1 };
            int[] dy = { 0, -1, 0, 1, -1, 1, -1, 1 };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == (goalGridX, goalGridY))
                {
                    // Reconstruct path in grid coordinates.
                    var path = new List<(int, int)>();
                    var cur = current;
                    while (cur != (-1, -1))
                    {
                        path.Add(cur);
                        cur = cameFrom[cur];
                    }
                    path.Reverse();

                    // Convert grid coordinates back to absolute coordinates.
                    for (int i = 0; i < path.Count; i++)
                    {
                        var (gx, gy) = path[i];
                        path[i] = (gx + offsetX, gy + offsetY);
                    }
                    return path;
                }

                for (int dir = 0; dir < dx.Length; dir++)
                {
                    int nextX = current.Item1 + dx[dir];
                    int nextY = current.Item2 + dy[dir];

                    if (nextX < 0 || nextX >= cols || nextY < 0 || nextY >= rows)
                        continue;
                    if (visited[nextY, nextX])
                        continue;
                    // Skip occupied cells.
                    if (occupied[nextY, nextX])
                        continue;

                    visited[nextY, nextX] = true;
                    queue.Enqueue((nextX, nextY));
                    cameFrom[(nextX, nextY)] = current;
                }
            }
            return null; // No path found.
        }

        /// Returns the next step (absolute coordinates) along the BFS–computed path
        /// from the player's current position to the target.
        /// If no valid path is found or if already at the target, returns the player's current position.
        public static (int, int) CalculateNextStepBFS(Player player, int targetX, int targetY, bool[,] occupied, int offsetX, int offsetY)
        {
            var path = FindPathBFS(player.XPosition, player.YPosition, targetX, targetY, occupied, offsetX, offsetY);
            if (path != null && path.Count > 1)
                return path[1];
            return (player.XPosition, player.YPosition);
        }

        /// Updates each player's position.
        /// For non-goalkeepers (and players not holding the ball), the target is normally their home position.
        /// If the ball is nearby (within 10 units), the target is blended with the ball's position.
        /// If useBFS is true, BFS is used to determine the next step while avoiding occupied cells.
        /// offsetX and offsetY should be the same values used to create the occupied grid.
        public static void UpdateTeamPositionsGrid2(
            List<Player> team,
            bool[,] occupied,
            int ballX,
            int ballY,
            bool attackingRight,
            int offsetX,
            int offsetY,
            bool useBFS = true)
        {
            foreach (var p in team)
            {
                // Skip goalkeepers and the ball holder.
                if (p.Position.Contains("GK") || p.HasBall)
                    continue;

                int targetX = p.HomeX;
                int targetY = p.HomeY;

                int dx = ballX - p.XPosition;
                int dy = ballY - p.YPosition;
                double distanceToBall = Math.Sqrt(dx * dx + dy * dy);
                if (distanceToBall < 10)
                {
                    // Blend the target with the ball position.
                    targetX = (p.HomeX + ballX) / 2;
                    targetY = (p.HomeY + ballY) / 2;
                }

                (int nextX, int nextY) nextStep;
                if (useBFS)
                {
                    nextStep = CalculateNextStepBFS(p, targetX, targetY, occupied, offsetX, offsetY);
                    // If BFS doesn't yield progress, fall back to simple one-step movement.
                    if (nextStep == (p.XPosition, p.YPosition))
                    {
                        nextStep = SimpleNextStep(p, targetX, targetY, occupied, offsetX, offsetY);
                    }
                }
                else
                {
                    nextStep = SimpleNextStep(p, targetX, targetY, occupied, offsetX, offsetY);
                }

                p.XPosition = nextStep.Item1;
                p.YPosition = nextStep.Item2;
                MarkOccupied(occupied, nextStep.Item1, nextStep.Item2, offsetX, offsetY);
            }
        }
        /// A simple fallback for one-step movement (without pathfinding).
        private static (int, int) SimpleNextStep(Player p, int targetX, int targetY, bool[,] occupied, int offsetX, int offsetY)
        {
            int newX = p.XPosition;
            int newY = p.YPosition;
            if (p.XPosition < targetX && !IsOccupiedAbsolute(occupied, p.XPosition + 1, p.YPosition, offsetX, offsetY))
                newX++;
            else if (p.XPosition > targetX && !IsOccupiedAbsolute(occupied, p.XPosition - 1, p.YPosition, offsetX, offsetY))
                newX--;

            if (p.YPosition < targetY && !IsOccupiedAbsolute(occupied, newX, p.YPosition + 1, offsetX, offsetY))
                newY++;
            else if (p.YPosition > targetY && !IsOccupiedAbsolute(occupied, newX, p.YPosition - 1, offsetX, offsetY))
                newY--;

            return (newX, newY);
        }

        /// Checks if a cell (given in absolute coordinates) is occupied.
        /// The occupied grid uses indices relative to (offsetX, offsetY).
        private static bool IsOccupiedAbsolute(bool[,] occupied, int x, int y, int offsetX, int offsetY)
        {
            int gridX = x - offsetX;
            int gridY = y - offsetY;
            if (gridX < 0 || gridX >= occupied.GetLength(1) || gridY < 0 || gridY >= occupied.GetLength(0))
                return false;
            return occupied[gridY, gridX];
        }

        /// Marks a cell (in absolute coordinates) as occupied.
        private static void MarkOccupied(bool[,] occupied, int x, int y, int offsetX, int offsetY)
        {
            int gridX = x - offsetX;
            int gridY = y - offsetY;
            if (gridX < 0 || gridX >= occupied.GetLength(1) || gridY < 0 || gridY >= occupied.GetLength(0))
                return;
            occupied[gridY, gridX] = true;
        }

        public static bool GetAutomaticAttackingMode(List<Player> team, List<Player> opponents, bool isTeamA)
        {
            // Placeholder for future logic.
            return false;
        }
    }
}
