using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI
{
    // Static class responsible for updating team/player positions,
    // including BFS pathfinding logic to avoid occupied cells.
    public static class TeamUpdaterFixed
    {
        // Finds a path using a Breadth-First Search (BFS) on a 2D grid.
        // The grid is represented by a 2D boolean array called 'occupied'.
        // 'occupied[y, x]' is true if that cell is blocked.
        // offsetX and offsetY define how to translate absolute coordinates into grid indices.
        private static List<(int, int)> FindPathBFS(
            int startX,
            int startY,
            int goalX,
            int goalY,
            bool[,] occupied,
            int offsetX,
            int offsetY)
        {
            // Number of rows in the 'occupied' array
            int rows = occupied.GetLength(0);
            // Number of columns in the 'occupied' array
            int cols = occupied.GetLength(1);

            // Convert absolute (world) coordinates to grid-based indices
            int startGridX = startX - offsetX;
            int startGridY = startY - offsetY;
            int goalGridX = goalX - offsetX;
            int goalGridY = goalY - offsetY;

            // Validate that our start/goal are within the grid bounds
            if (startGridX < 0 || startGridX >= cols ||
                startGridY < 0 || startGridY >= rows ||
                goalGridX < 0 || goalGridX >= cols ||
                goalGridY < 0 || goalGridY >= rows)
            {
                return null; // Out of bounds => no path
            }

            // A queue for BFS frontier (stores cells to explore)
            var queue = new Queue<(int, int)>();
            // Dictionary that tracks where each cell came from (for path reconstruction)
            var cameFrom = new Dictionary<(int, int), (int, int)>();
            // 2D visited array to mark visited cells
            bool[,] visited = new bool[rows, cols];

            // Enqueue the start cell
            queue.Enqueue((startGridX, startGridY));
            visited[startGridY, startGridX] = true;

            // Mark that the start cell has no 'cameFrom' (use (-1,-1) as a sentinel)
            cameFrom[(startGridX, startGridY)] = (-1, -1);

            // We can move in 8 directions (including diagonals)
            int[] dx = { -1, 0, 1, 0, -1, -1, 1, 1 };
            int[] dy = { 0, -1, 0, 1, -1, 1, -1, 1 };

            // Standard BFS loop
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // If we've reached our goal, reconstruct the path
                if (current == (goalGridX, goalGridY))
                {
                    // Build a list of grid cells from the goal back to the start
                    var path = new List<(int, int)>();
                    var cur = current;

                    // Move backwards through 'cameFrom' until we reach the sentinel
                    while (cur != (-1, -1))
                    {
                        path.Add(cur);
                        cur = cameFrom[cur];
                    }

                    // Reverse it so it's from start => goal
                    path.Reverse();

                    // Convert grid coords back to absolute coords
                    for (int i = 0; i < path.Count; i++)
                    {
                        var (gx, gy) = path[i];
                        path[i] = (gx + offsetX, gy + offsetY);
                    }
                    return path;
                }

                // Explore neighbors in 8 directions
                for (int dir = 0; dir < dx.Length; dir++)
                {
                    int nextX = current.Item1 + dx[dir];
                    int nextY = current.Item2 + dy[dir];

                    // Check boundaries
                    if (nextX < 0 || nextX >= cols || nextY < 0 || nextY >= rows)
                        continue;

                    // Skip if already visited
                    if (visited[nextY, nextX])
                        continue;

                    // Skip if occupied
                    if (occupied[nextY, nextX])
                        continue;

                    // Mark visited and enqueue
                    visited[nextY, nextX] = true;
                    queue.Enqueue((nextX, nextY));
                    // Track where we came from
                    cameFrom[(nextX, nextY)] = current;
                }
            }

            // If we exhaust BFS without finding goal, no path exists
            return null;
        }
        // Returns the next step along a BFS-computed path from the player's current position to a target.
        // If no valid path is found or if already at the target, returns current position.
        public static (int, int) CalculateNextStepBFS(
            Player player,
            int targetX,
            int targetY,
            bool[,] occupied,
            int offsetX,
            int offsetY)
        {
            // Attempt to find a path
            var path = FindPathBFS(player.XPosition, player.YPosition, targetX, targetY, occupied, offsetX, offsetY);
            // If path exists and has more than 1 node, return the second node (the first is current position)
            if (path != null && path.Count > 1)
                return path[1];

            // Otherwise remain in place
            return (player.XPosition, player.YPosition);
        }
        // Updates each player's position for a given team.
        // For non-GK players, we typically move them towards their home positions,
        // or blend with the ball's position if it's near.
        // If BFS is chosen, we do a BFS step; otherwise a simple fallback movement.
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
                // Skip GKs and ball holders
                if (p.Position.Contains("GK") || p.HasBall)
                    continue;

                // Default target is their home position
                int targetX = p.HomeX;
                int targetY = p.HomeY;

                // If ball is within 10 units, move partially toward the ball
                int dx = ballX - p.XPosition;
                int dy = ballY - p.YPosition;
                double distanceToBall = Math.Sqrt(dx * dx + dy * dy);
                if (distanceToBall < 10)
                {
                    // Blend home pos with ball pos
                    targetX = (p.HomeX + ballX) / 2;
                    targetY = (p.HomeY + ballY) / 2;
                }

                (int nextX, int nextY) nextStep;
                if (useBFS)
                {
                    // BFS-based next step
                    nextStep = CalculateNextStepBFS(p, targetX, targetY, occupied, offsetX, offsetY);
                    // If BFS doesn't move us, fallback to simple movement
                    if (nextStep == (p.XPosition, p.YPosition))
                    {
                        nextStep = SimpleNextStep(p, targetX, targetY, occupied, offsetX, offsetY);
                    }
                }
                else
                {
                    // Direct, naive approach
                    nextStep = SimpleNextStep(p, targetX, targetY, occupied, offsetX, offsetY);
                }

                // Update player position
                p.XPosition = nextStep.Item1;
                p.YPosition = nextStep.Item2;

                // Mark new position as occupied so no overlap
                MarkOccupied(occupied, nextStep.Item1, nextStep.Item2, offsetX, offsetY);
            }
        }
        // Simple fallback method for moving one step toward target.
        // Checks for occupancy to avoid collisions but doesn't do BFS.
        private static (int, int) SimpleNextStep(
            Player p,
            int targetX,
            int targetY,
            bool[,] occupied,
            int offsetX,
            int offsetY)
        {
            int newX = p.XPosition;
            int newY = p.YPosition;

            // Move horizontally if not blocked
            if (p.XPosition < targetX && !IsOccupiedAbsolute(occupied, p.XPosition + 1, p.YPosition, offsetX, offsetY))
                newX++;
            else if (p.XPosition > targetX && !IsOccupiedAbsolute(occupied, p.XPosition - 1, p.YPosition, offsetX, offsetY))
                newX--;

            // Move vertically if not blocked
            if (p.YPosition < targetY && !IsOccupiedAbsolute(occupied, newX, p.YPosition + 1, offsetX, offsetY))
                newY++;
            else if (p.YPosition > targetY && !IsOccupiedAbsolute(occupied, newX, p.YPosition - 1, offsetX, offsetY))
                newY--;

            return (newX, newY);
        }
        // Checks if a given absolute coordinate (x,y) is occupied,
        // by translating it into the 'occupied' grid with offset.
        private static bool IsOccupiedAbsolute(
            bool[,] occupied,
            int x,
            int y,
            int offsetX,
            int offsetY)
        {
            int gridX = x - offsetX;
            int gridY = y - offsetY;

            // If out of range, treat as not occupied
            if (gridX < 0 || gridX >= occupied.GetLength(1) ||
                gridY < 0 || gridY >= occupied.GetLength(0))
                return false;

            return occupied[gridY, gridX];
        }
        // Marks a cell (in absolute coords) as occupied in the 'occupied' grid.
        private static void MarkOccupied(
            bool[,] occupied,
            int x,
            int y,
            int offsetX,
            int offsetY)
        {
            int gridX = x - offsetX;
            int gridY = y - offsetY;

            // If valid grid coords, set to true
            if (gridX >= 0 && gridX < occupied.GetLength(1) &&
                gridY >= 0 && gridY < occupied.GetLength(0))
            {
                occupied[gridY, gridX] = true;
            }
        }
        // Placeholder function that can be used to determine if the team should be in 'automatic attacking mode'.
        // Currently returns false.
        public static bool GetAutomaticAttackingMode(
            List<Player> team,
            List<Player> opponents,
            bool isTeamA)
        {
            // Placeholder logic, always returns false
            return false;
        }
    }
}
