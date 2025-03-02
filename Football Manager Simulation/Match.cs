using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Drawing;

namespace GUI
{
    // The Match class simulates a live football match.
    // It inherits from a Window class (presumably for console GUI purposes).
    public class Match : Window
    {
        // Field declarations for pitch boundaries, time, score, game state, etc.
        private int pitchLeft, pitchTop, pitchRight, pitchBottom;
        private int currentMinute, teamAScore, teamBScore;
        private bool isRunning, isPaused, ballInTransit, isFirstHalf;
        private bool firstHalfStartingTeamA;
        private Club clubA, clubB;
        private List<Player> teamAPlayers, teamBPlayers;
        private Random randomGenerator;
        private int gameSpeed = 100;
        private double simulationTime = 0.0;
        private double timePerTick = 0.1;
        private int gkBoxWidth = 15; // Not actively used since GK is pinned by HomeX/HomeY

        // Constructor for a match with two clubs.
        public Match(Club clubA, Club clubB)
            : base("Live Match", new Rectangle(0, 0, 150, 40), true)
        {
            // Hide the cursor for a better simulation view.
            Console.CursorVisible = false;
            this.clubA = clubA;
            this.clubB = clubB;
            teamAPlayers = clubA.Players;
            teamBPlayers = clubB.Players;
            // Initialize simulation parameters and positions.
            InitializeSimulation();
        }

        // Overloaded constructor where only one club is provided.
        // The opponent club is generated with default parameters.
        public Match(Club clubA)
            : this(clubA, new Club("Opponent", 100000000))
        {
        }

        // Set up initial simulation settings and assign initial positions.
        private void InitializeSimulation()
        {
            isRunning = true;
            isPaused = false;
            ballInTransit = false;
            currentMinute = 0;
            teamAScore = 0;
            teamBScore = 0;
            isFirstHalf = true;
            firstHalfStartingTeamA = true;

            // Define the pitch boundaries.
            pitchLeft = 2;
            pitchTop = 4;
            pitchRight = 100;
            pitchBottom = 30;

            // Create a new random generator instance for simulation randomness.
            randomGenerator = new Random();

            // Assign formation positions for both teams.
            AssignFormationPositions(teamAPlayers, true);
            AssignFormationPositions(teamBPlayers, false);

            // Wait for user to press a key to start kickoff.
            WaitForKickoffEvent();

            // Select a kickoff player (preferably a forward) from team A and assign the ball.
            Player kickoffPlayer = SelectKickoffPlayer(teamAPlayers);
            if (kickoffPlayer != null)
                AssignBallToPlayer(kickoffPlayer);
        }

        // Displays a message and waits for any key press to start/resume the match.
        private void WaitForKickoffEvent()
        {
            // Reset cursor position and display the kickoff prompt.
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Kickoff! Press any key to start...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        // Assigns positions to the players based on a formation template.
        // The goalkeeper (assumed at index 0 if their Position contains "GK") is pinned to the goal line.
        // Outfield players are positioned based on a predefined formation.
        private void AssignFormationPositions(List<Player> team, bool isTeamA)
        {
            double[,] formationTemplate = new double[11, 2]
            {
                {0.05, 0.5},  // GK; ignored for outfielders.
                {0.20, 0.2},
                {0.20, 0.8},
                {0.40, 0.3},
                {0.40, 0.7},
                {0.55, 0.5},
                {0.70, 0.3},
                {0.70, 0.7},
                {0.85, 0.4},
                {0.90, 0.3},
                {0.90, 0.7}
            };

            int count = Math.Min(team.Count, 11);
            int centerY = (pitchTop + pitchBottom) / 2;

            // Loop through each player and assign positions.
            for (int i = 0; i < count; i++)
            {
                Player p = team[i];

                // If the player is the goalkeeper (assumed to be at index 0)...
                if (i == 0 && p.Position.Contains("GK"))
                {
                    if (isTeamA)
                    {
                        p.XPosition = pitchLeft;   // For team A, GK is pinned to the left goal line.
                        p.YPosition = centerY;
                    }
                    else
                    {
                        p.XPosition = pitchRight;  // For team B, GK is pinned to the right goal line.
                        p.YPosition = centerY;
                    }
                }
                else
                {
                    // For outfield players, use the formation template to determine positions.
                    double rx = formationTemplate[i, 0];
                    double ry = formationTemplate[i, 1];
                    double newRx = isTeamA ? rx * 0.5 : 0.5 + rx * 0.5;
                    int x = pitchLeft + (int)(newRx * (pitchRight - pitchLeft));
                    int y = pitchTop + (int)(ry * (pitchBottom - pitchTop));
                    // Ensure the positions are within the pitch boundaries.
                    x = Clamp(x, pitchLeft, pitchRight);
                    y = Clamp(y, pitchTop, pitchBottom);
                    p.XPosition = x;
                    p.YPosition = y;
                }
                // Set home positions for later reference.
                p.HomeX = p.XPosition;
                p.HomeY = p.YPosition;
            }
        }

        // Helper method to ensure a value is within a specified range.
        private int Clamp(int val, int min, int max)
        {
            return Math.Max(min, Math.Min(val, max));
        }

        // Selects a player for kickoff from a team.
        // Prefers players with a striker ("ST") position, otherwise chooses any outfield player.
        private Player SelectKickoffPlayer(List<Player> team)
        {
            var strikers = team.Where(p => p.Position.Contains("ST")).ToList();
            if (!strikers.Any())
                strikers = team.Where(p => !p.Position.Contains("GK")).ToList();
            return strikers.Any() ? strikers[randomGenerator.Next(strikers.Count)] : team.FirstOrDefault();
        }

        // Clears the ball possession from all players and assigns it to the specified player.
        private void AssignBallToPlayer(Player p)
        {
            if (p == null)
                return;
            foreach (var pl in teamAPlayers)
                pl.HasBall = false;
            foreach (var pl in teamBPlayers)
                pl.HasBall = false;
            p.HasBall = true;
        }

        // Main simulation loop. Updates game events until the match ends.
        public void Start()
        {
            while (isRunning)
            {
                if (!isPaused)
                {
                    // Update simulation time and match minute.
                    simulationTime += timePerTick;
                    currentMinute = (int)Math.Round(simulationTime);
                    // End match after 90 minutes.
                    if (simulationTime >= 90.0)
                    {
                        isRunning = false;
                        break;
                    }

                    // Update game dynamics.
                    UpdateDribbling();
                    UpdatePassing();
                    AttemptTackle();    // Check if any player can successfully tackle.
                    UpdateShooting();
                    CheckHalftime();
                    UpdateNonBallPlayers();
                }

                // Handle user input such as pausing the game.
                HandleUserInput();
                // Render the current state of the match.
                Draw(true);
                // Pause execution briefly based on game speed or pause status.
                Thread.Sleep(isPaused ? 100 : (1000 / (gameSpeed > 0 ? gameSpeed : 1)));
            }

            // Record the match result for both clubs.
            clubA.RecordResult(teamAScore, teamBScore);
            clubB.RecordResult(teamBScore, teamAScore);
            EndMatch();
        }

        // Allows an opposing outfield player to attempt a tackle if close enough to the ball holder.
        // A successful tackle (30% chance) results in a change of possession.
        private void AttemptTackle()
        {
            Player ballHolder = teamAPlayers.Concat(teamBPlayers).FirstOrDefault(p => p.HasBall);
            if (ballHolder == null)
                return;

            // Determine which team is the opponent.
            List<Player> opponents = teamAPlayers.Contains(ballHolder) ? teamBPlayers : teamAPlayers;

            foreach (var opponent in opponents)
            {
                // Skip goalkeepers when attempting a tackle.
                if (opponent.Position.Contains("GK"))
                    continue;

                // Calculate distance to the ball holder.
                double dist = Distance(opponent.XPosition, opponent.YPosition, ballHolder.XPosition, ballHolder.YPosition);
                if (dist < 3) // If within 3 units, consider as "close".
                {
                    // 30% chance to successfully tackle.
                    if (randomGenerator.NextDouble() < 0.3)
                    {
                        Console.WriteLine($"{opponent.Name} tackles and takes the ball!");
                        AssignBallToPlayer(opponent);
                        break;
                    }
                }
            }
        }

        // Simulates dribbling by updating the ball holder's position.
        private void UpdateDribbling()
        {
            int center = (pitchLeft + pitchRight) / 2;
            Player ballHolder = teamAPlayers.Concat(teamBPlayers).FirstOrDefault(p => p.HasBall);
            if (ballHolder == null)
                return;

            // If the ball holder is a goalkeeper, they reset to their home position.
            if (ballHolder.Position.Contains("GK"))
            {
                ballHolder.XPosition = ballHolder.HomeX;
                ballHolder.YPosition = ballHolder.HomeY;
                return;
            }

            // Outfield players move minimally laterally.
            if (teamAPlayers.Contains(ballHolder))
            {
                int step = (ballHolder.XPosition > center) ? 2 : 1;
                ballHolder.XPosition = isFirstHalf
                    ? Clamp(ballHolder.XPosition + step, pitchLeft, pitchRight)
                    : Clamp(ballHolder.XPosition - step, pitchLeft, pitchRight);
            }
            else
            {
                int step = (ballHolder.XPosition < center) ? 2 : 1;
                ballHolder.XPosition = isFirstHalf
                    ? Clamp(ballHolder.XPosition - step, pitchLeft, pitchRight)
                    : Clamp(ballHolder.XPosition + step, pitchLeft, pitchRight);
            }
        }

        // Simulates passing by attempting to pass the ball every set number of ticks.
        private void UpdatePassing()
        {
            int tickCount = (int)(simulationTime / timePerTick);
            if (!ballInTransit && tickCount % 10 == 0)
            {
                Player ballHolder = teamAPlayers.Concat(teamBPlayers).FirstOrDefault(p => p.HasBall);
                if (ballHolder != null)
                {
                    var team = teamAPlayers.Contains(ballHolder) ? teamAPlayers : teamBPlayers;
                    if (team.Count > 1)
                    {
                        // Select a random receiver from the same team (excluding the ball holder).
                        var potentialReceivers = team.Where(p => p != ballHolder).ToList();
                        Player receiver = potentialReceivers[randomGenerator.Next(potentialReceivers.Count)];
                        bool stolen;
                        // Animate the passing action.
                        Animation.AnimatePass(ballHolder.XPosition, ballHolder.YPosition,
                            receiver.XPosition, receiver.YPosition,
                            gameSpeed, randomGenerator, out stolen);
                        if (!stolen)
                            AssignBallToPlayer(receiver);
                    }
                }
            }
        }

        // Simulates shooting by attempting a shot every set number of ticks.
        // Determines shot chances and outcomes, including saves by the goalkeeper.
        private void UpdateShooting()
        {
            int tickCount = (int)(simulationTime / timePerTick);
            if (!ballInTransit && tickCount % 20 == 0)
            {
                Player ballHolder = teamAPlayers.Concat(teamBPlayers).FirstOrDefault(p => p.HasBall);
                if (ballHolder != null && !ballHolder.Position.Contains("GK"))
                {
                    int center = (pitchLeft + pitchRight) / 2;
                    if (teamAPlayers.Contains(ballHolder))
                    {
                        bool inOppHalf = (ballHolder.XPosition > center);
                        double shotChance = inOppHalf ? 0.5 : 0.3;
                        if (randomGenerator.NextDouble() < shotChance)
                        {
                            // Attempt to shoot on target against team B's goalkeeper.
                            Player gk = teamBPlayers.FirstOrDefault(p => p.Position.Contains("GK"));
                            double saveChance = 0.4;
                            if (gk != null && randomGenerator.NextDouble() < saveChance)
                            {
                                Console.WriteLine($"{gk.Name} saves the shot!");
                                // Reset goalkeeper position after save.
                                gk.XPosition = gk.HomeX;
                                gk.YPosition = gk.HomeY;
                                AssignBallToPlayer(gk);
                            }
                            else
                            {
                                // Animate the shot and score a goal.
                                Animation.AnimateShot(ballHolder.XPosition, ballHolder.YPosition,
                                    pitchRight,
                                    Clamp((pitchTop + pitchBottom) / 2 + randomGenerator.Next(-2, 3),
                                          pitchTop, pitchBottom),
                                    gameSpeed);
                                teamAScore++;
                                // Restart from kickoff after a goal.
                                WaitForKickoffEvent();
                                Player kp = SelectKickoffPlayer(teamBPlayers);
                                if (kp != null)
                                    AssignBallToPlayer(kp);
                            }
                        }
                    }
                    else
                    {
                        bool inOppHalf = (ballHolder.XPosition < center);
                        double shotChance = inOppHalf ? 0.5 : 0.3;
                        if (randomGenerator.NextDouble() < shotChance)
                        {
                            // Attempt to shoot on target against team A's goalkeeper.
                            Player gk = teamAPlayers.FirstOrDefault(p => p.Position.Contains("GK"));
                            double saveChance = 0.4;
                            if (gk != null && randomGenerator.NextDouble() < saveChance)
                            {
                                Console.WriteLine($"{gk.Name} saves the shot!");
                                gk.XPosition = gk.HomeX;
                                gk.YPosition = gk.HomeY;
                                AssignBallToPlayer(gk);
                            }
                            else
                            {
                                Animation.AnimateShot(ballHolder.XPosition, ballHolder.YPosition,
                                    pitchLeft,
                                    Clamp((pitchTop + pitchBottom) / 2 + randomGenerator.Next(-2, 3),
                                          pitchTop, pitchBottom),
                                    gameSpeed);
                                teamBScore++;
                                WaitForKickoffEvent();
                                Player kp = SelectKickoffPlayer(teamAPlayers);
                                if (kp != null)
                                    AssignBallToPlayer(kp);
                            }
                        }
                    }
                }
            }
        }

        // Checks if it's halftime, and if so, resets the game state for the second half.
        private void CheckHalftime()
        {
            if (isFirstHalf && simulationTime >= 45.0)
            {
                isFirstHalf = false;
                // Wait for kickoff event to resume the game.
                WaitForKickoffEvent();
                // Swap possession for the kickoff at the start of the second half.
                if (firstHalfStartingTeamA)
                {
                    Player kp = SelectKickoffPlayer(teamBPlayers);
                    if (kp != null)
                        AssignBallToPlayer(kp);
                }
                else
                {
                    Player kp = SelectKickoffPlayer(teamAPlayers);
                    if (kp != null)
                        AssignBallToPlayer(kp);
                }
            }
        }

        // --- Breaking UpdateNonBallPlayers into two subroutines ---

        // Builds a grid representing the pitch and marks the occupied cells by players.
        private bool[,] BuildOccupiedGrid()
        {
            bool[,] occupied = new bool[pitchBottom - pitchTop + 1, pitchRight - pitchLeft + 1];
            foreach (var pl in teamAPlayers.Concat(teamBPlayers))
            {
                int gx = pl.XPosition - pitchLeft;
                int gy = pl.YPosition - pitchTop;
                if (gx >= 0 && gx < occupied.GetLength(1) && gy >= 0 && gy < occupied.GetLength(0))
                    occupied[gy, gx] = true;
            }
            return occupied;
        }

        // Updates the positions of players without the ball.
        // They move toward their home positions or adjust based on the ball's location.
        private void UpdateTeamPlayers(List<Player> team, bool teamHasPossession)
        {
            bool[,] occupied = BuildOccupiedGrid();
            int center = (pitchLeft + pitchRight) / 2;
            int attackOffset = 10;

            foreach (var p in team)
            {
                // Skip players who have the ball.
                if (p.HasBall)
                    continue;
                // Reset goalkeepers to their home positions.
                if (p.Position.Contains("GK"))
                {
                    p.XPosition = p.HomeX;
                    p.YPosition = p.HomeY;
                    continue;
                }

                int targetX = p.HomeX;
                if (teamHasPossession)
                {
                    // Adjust position to simulate attacking movement.
                    int desired = p.HomeX + (team == teamAPlayers ? attackOffset : -attackOffset);
                    desired = team == teamAPlayers
                        ? (isFirstHalf ? Math.Min(desired, center - 1) : Math.Max(desired, center + 1))
                        : (isFirstHalf ? Math.Max(desired, center + 1) : Math.Min(desired, center - 1));
                    targetX = (p.XPosition < desired) ? desired : p.XPosition;

                    // If near the ball holder, further adjust the position.
                    Player ballHolder = team.FirstOrDefault(pl => pl.HasBall);
                    if (ballHolder != null)
                    {
                        double dist = Distance(p.XPosition, p.YPosition, ballHolder.XPosition, ballHolder.YPosition);
                        if (dist < 5)
                        {
                            targetX = team == teamAPlayers
                                ? (isFirstHalf ? Math.Min(targetX + 5, center - 1) : Math.Max(targetX - 5, center + 1))
                                : (isFirstHalf ? Math.Max(targetX - 5, center + 1) : Math.Min(targetX + 5, center - 1));
                        }
                    }
                }
                int targetY = p.HomeY;
                // Calculate the next step for the player using a BFS algorithm.
                var next = TeamUpdaterFixed.CalculateNextStepBFS(p, targetX, targetY, occupied, pitchLeft, pitchTop);
                p.XPosition = Clamp(next.Item1, pitchLeft, pitchRight);
                p.YPosition = Clamp(next.Item2, pitchTop, pitchBottom);
            }
        }

        // Updates positions of all players who do not have the ball.
        private void UpdateNonBallPlayers()
        {
            bool teamAInPossession = teamAPlayers.Any(p => p.HasBall);
            bool teamBInPossession = teamBPlayers.Any(p => p.HasBall);
            UpdateTeamPlayers(teamAPlayers, teamAInPossession);
            UpdateTeamPlayers(teamBPlayers, teamBInPossession);
        }

        // Utility method to calculate the Euclidean distance between two points.
        private double Distance(int x1, int y1, int x2, int y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Checks and handles user input. Currently, it allows toggling pause state using the Spacebar.
        private void HandleUserInput()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Spacebar)
                    isPaused = !isPaused;
            }
        }

        // Ends the match and displays the final score.
        private void EndMatch()
        {
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Match Ended!");
            Console.WriteLine($"Final Score: {teamAScore} - {teamBScore}");
            Console.ResetColor();
        }

        // Draws the current state of the match onto the console.
        // This includes drawing the pitch, players, scores, and game time.
        public override void Draw(bool active)
        {
            Console.SetCursorPosition(0, 0);
            // Create a drawing buffer for the console.
            ConsoleCell[,] buffer = new ConsoleCell[Drawing.ConsoleHeight, Drawing.ConsoleWidth];
            Drawing.ClearBuffer(buffer);
            Drawing.DrawPitch(buffer, pitchLeft, pitchTop, pitchRight, pitchBottom);

            // Draw Team A players.
            foreach (var p in teamAPlayers)
            {
                ConsoleColor color = p.Position.Contains("GK")
                    ? ConsoleColor.Black
                    : p.HasBall ? ConsoleColor.Magenta : ConsoleColor.Blue;
                Drawing.DrawPlayer(buffer, p.XPosition, p.YPosition, 'P', color, color);
            }

            // Draw Team B players.
            foreach (var p in teamBPlayers)
            {
                ConsoleColor color = p.Position.Contains("GK")
                    ? ConsoleColor.Black
                    : p.HasBall ? ConsoleColor.Magenta : ConsoleColor.Red;
                Drawing.DrawPlayer(buffer, p.XPosition, p.YPosition, 'P', color, color);
            }

            // Display current match time and score.
            Drawing.PlaceString(buffer, 2, 1,
                $"Time: {currentMinute}'   Score: {teamAScore}-{teamBScore}",
                ConsoleColor.Yellow, ConsoleColor.Black);

            // Render the complete buffer to the console.
            Drawing.RenderBufferToConsole(buffer);
        }
    }
}
