using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Drawing;

namespace GUI
{
    // Represents a live match simulation window/class.
    // Handles team A vs. team B logic, updating positions,
    // scoring, halftime checks, etc.
    public class Match : Window
    {
        // Boundaries of the pitch
        private int pitchLeft, pitchTop, pitchRight, pitchBottom;

        // Current match minute, and each team's score
        private int currentMinute, teamAScore, teamBScore;

        // Flags to indicate match state
        private bool isRunning, isPaused, ballInTransit, isFirstHalf;
        private bool firstHalfStartingTeamA;

        // References to clubs and their player lists
        private Club clubA, clubB;
        private List<Player> teamAPlayers, teamBPlayers;

        // Random generator for chance-based events
        private Random randomGenerator;

        // Game speed factor
        private int gameSpeed = 100;

        // Track how much match time has passed, and how much time per tick
        private double simulationTime = 0.0;
        private double timePerTick = 0.1;

        // Constructor requires two clubs to simulate a match.
        // Also sets up the window area.
        public Match(Club clubA, Club clubB)
            : base("Live Match", new Rectangle(0, 0, 150, 40), true)
        {
            Console.CursorVisible = false;
            this.clubA = clubA;
            this.clubB = clubB;

            // Extract player lists from each club
            teamAPlayers = clubA.Players;
            teamBPlayers = clubB.Players;

            InitializeSimulation();
        }
        // Overloaded constructor if only one club is provided.
        // Creates a dummy opponent.
        public Match(Club clubA)
            : this(clubA, new Club("Opponent", 100000000))
        {
        }
        // Initializes all match variables, sets pitch boundaries, random, etc.
        // Assigns formation positions, and waits for user to start.
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

            // Hard-coded pitch boundaries
            pitchLeft = 2;
            pitchTop = 4;
            pitchRight = 100;
            pitchBottom = 30;

            randomGenerator = new Random();

            // Assign formation positions for each team's players
            AssignFormationPositions(teamAPlayers, true);
            AssignFormationPositions(teamBPlayers, false);

            WaitForKickoffEvent();

            // Choose a kickoff player from Team A
            Player kickoffPlayer = SelectKickoffPlayer(teamAPlayers);
            if (kickoffPlayer != null) AssignBallToPlayer(kickoffPlayer);
        }

        // Waits for user input before starting the match.
        private void WaitForKickoffEvent()
        {
            Console.Clear();
            Console.SetCursorPosition(30, 15);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Kickoff! Press any key to start...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        // Assigns formation positions for up to 11 players using a simple template,
        // with the GK pinned to the net.
        private void AssignFormationPositions(List<Player> team, bool isTeamA)
        {
            // A sample formation for up to 11 players
            double[,] formationTemplate = new double[11, 2]
            {
                {0.05, 0.5},
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
            for (int i = 0; i < count; i++)
            {
                int x, y;
                // If the i-th player is GK, place them at the net
                if (i == 0 && team[i].Position.Contains("GK"))
                {
                    if (isTeamA)
                    {
                        x = pitchLeft + 1;
                        y = (pitchTop + pitchBottom) / 2;
                    }
                    else
                    {
                        x = pitchRight - 1;
                        y = (pitchTop + pitchBottom) / 2;
                    }
                }
                else
                {
                    // Use the formation template for outfield
                    double rx = formationTemplate[i, 0];
                    double ry = formationTemplate[i, 1];
                    double newRx = isTeamA ? rx * 0.5 : 0.5 + rx * 0.5;
                    x = pitchLeft + (int)(newRx * (pitchRight - pitchLeft));
                    y = pitchTop + (int)(ry * (pitchBottom - pitchTop));
                }
                x = Clamp(x, pitchLeft, pitchRight);
                y = Clamp(y, pitchTop, pitchBottom);

                // Store them
                team[i].XPosition = x;
                team[i].YPosition = y;
                team[i].HomeX = x;
                team[i].HomeY = y;
            }
        }

        private int Clamp(int val, int min, int max)
        {
            return Math.Max(min, Math.Min(val, max));
        }
        // Select a kickoff player from the team's forwards or non-GK players.
        private Player SelectKickoffPlayer(List<Player> team)
        {
            var strikers = team.Where(p => p.Name.Contains("ST")).ToList();
            if (!strikers.Any())
                strikers = team.Where(p => !p.IsGoalkeeper).ToList();
            return strikers.Any() ? strikers[randomGenerator.Next(strikers.Count)] : team.FirstOrDefault();
        }
        // Assign the ball to a given player, removing it from all others.
        private void AssignBallToPlayer(Player p)
        {
            if (p == null) return;
            foreach (var pl in teamAPlayers) pl.HasBall = false;
            foreach (var pl in teamBPlayers) pl.HasBall = false;
            p.HasBall = true;
        }
        // Main match loop: runs until isRunning is false (time >= 90 or user exit).
        // Updates dribbling, passing, shooting, etc.
        public void Start()
        {
            while (isRunning)
            {
                if (!isPaused)
                {
                    // Advance simulation time
                    simulationTime += timePerTick;
                    currentMinute = (int)Math.Round(simulationTime);

                    // End at 90 minutes
                    if (simulationTime >= 90.0)
                    {
                        isRunning = false;
                        break;
                    }

                    UpdateDribbling();
                    UpdatePassing();
                    UpdateShooting();
                    CheckHalftime();
                    UpdateNonBallPlayers();
                }

                HandleUserInput();
                Draw(true);

                // Sleep for a short time based on game speed
                Thread.Sleep(isPaused ? 100 : (1000 / (gameSpeed > 0 ? gameSpeed : 1)));
            }

            // After match, record result
            clubA.RecordResult(teamAScore, teamBScore);
            clubB.RecordResult(teamBScore, teamAScore);
            EndMatch();
        }

 
        // Basic dribbling logic: if a player has the ball and isn't GK,
        // they move slightly toward or away from center, depending on half.
        // GK stays in net.
        private void UpdateDribbling()
        {
            int center = (pitchLeft + pitchRight) / 2;
            Player ballHolder = teamAPlayers.Concat(teamBPlayers).FirstOrDefault(p => p.HasBall);
            if (ballHolder != null)
            {
                if (ballHolder.Position.Contains("GK"))
                {
                    // GK pinned
                    ballHolder.XPosition = ballHolder.HomeX;
                    ballHolder.YPosition = ballHolder.HomeY;
                }
                else
                {
                    if (teamAPlayers.Contains(ballHolder))
                    {
                        int step = ballHolder.XPosition > center ? 2 : 1;
                        ballHolder.XPosition = isFirstHalf
                            ? Clamp(ballHolder.XPosition + step, pitchLeft, pitchRight)
                            : Clamp(ballHolder.XPosition - step, pitchLeft, pitchRight);
                    }
                    else
                    {
                        int step = ballHolder.XPosition < center ? 2 : 1;
                        ballHolder.XPosition = isFirstHalf
                            ? Clamp(ballHolder.XPosition - step, pitchLeft, pitchRight)
                            : Clamp(ballHolder.XPosition + step, pitchLeft, pitchRight);
                    }
                }
            }
        }
        // Trigger passing every 10 ticks. The ball holder may pass to a random teammate.
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
                        var potentialReceivers = team.Where(p => p != ballHolder).ToList();
                        Player receiver = potentialReceivers[randomGenerator.Next(potentialReceivers.Count)];
                        bool stolen;
                        Animation.AnimatePass(
                            ballHolder.XPosition, ballHolder.YPosition,
                            receiver.XPosition, receiver.YPosition,
                            gameSpeed, randomGenerator, out stolen);
                        if (!stolen)
                            AssignBallToPlayer(receiver);
                    }
                }
            }
        }
        // Trigger shooting every 20 ticks. The ball holder may attempt a shot if in 'opponent's half'.
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
                            Player gk = teamBPlayers.FirstOrDefault(p => p.Position.Contains("GK"));
                            double saveChance = 0.4;
                            if (gk != null && randomGenerator.NextDouble() < saveChance)
                            {
                                Console.WriteLine($"{gk.Name} saves the shot!");
                                AssignBallToPlayer(gk);
                            }
                            else
                            {
                                Animation.AnimateShot(
                                    ballHolder.XPosition, ballHolder.YPosition,
                                    pitchRight,
                                    Clamp((pitchTop + pitchBottom)/2 + randomGenerator.Next(-2, 3),
                                          pitchTop, pitchBottom),
                                    gameSpeed);
                                teamAScore++;

                                WaitForKickoffEvent();
                                Player kp = SelectKickoffPlayer(teamBPlayers);
                                if (kp != null) AssignBallToPlayer(kp);
                            }
                        }
                    }
                    else
                    {
                        bool inOppHalf = (ballHolder.XPosition < center);
                        double shotChance = inOppHalf ? 0.5 : 0.3;
                        if (randomGenerator.NextDouble() < shotChance)
                        {
                            Player gk = teamAPlayers.FirstOrDefault(p => p.Position.Contains("GK"));
                            double saveChance = 0.4;
                            if (gk != null && randomGenerator.NextDouble() < saveChance)
                            {
                                Console.WriteLine($"{gk.Name} saves the shot!");
                                AssignBallToPlayer(gk);
                            }
                            else
                            {
                                Animation.AnimateShot(
                                    ballHolder.XPosition, ballHolder.YPosition,
                                    pitchLeft,
                                    Clamp((pitchTop + pitchBottom)/2 + randomGenerator.Next(-2, 3),
                                          pitchTop, pitchBottom),
                                    gameSpeed);
                                teamBScore++;

                                WaitForKickoffEvent();
                                Player kp = SelectKickoffPlayer(teamAPlayers);
                                if (kp != null) AssignBallToPlayer(kp);
                            }
                        }
                    }
                }
            }
        }
        // Check if we've reached halftime (45'), and if so, switch sides for kickoff.
        private void CheckHalftime()
        {
            if (isFirstHalf && simulationTime >= 45.0)
            {
                isFirstHalf = false;
                WaitForKickoffEvent();
                if (firstHalfStartingTeamA)
                {
                    Player kp = SelectKickoffPlayer(teamBPlayers);
                    if (kp != null) AssignBallToPlayer(kp);
                }
                else
                {
                    Player kp = SelectKickoffPlayer(teamAPlayers);
                    if (kp != null) AssignBallToPlayer(kp);
                }
            }
        }
        // Moves all non-ball holders back to their home or a forward position
        // using BFS to avoid collisions.
        private void UpdateNonBallPlayers()
        {
            bool teamAInPossession = teamAPlayers.Any(p => p.HasBall);
            bool teamBInPossession = teamBPlayers.Any(p => p.HasBall);
            int center = (pitchLeft + pitchRight) / 2;
            int attackOffset = 10;

            // Build occupancy grid
            bool[,] occupied = new bool[pitchBottom - pitchTop + 1, pitchRight - pitchLeft + 1];
            foreach (var pl in teamAPlayers.Concat(teamBPlayers))
            {
                int gx = pl.XPosition - pitchLeft;
                int gy = pl.YPosition - pitchTop;
                if (gx >= 0 && gx < occupied.GetLength(1) &&
                    gy >= 0 && gy < occupied.GetLength(0))
                {
                    occupied[gy, gx] = true;
                }
            }

            // Team A updates
            foreach (var p in teamAPlayers)
            {
                if (p.HasBall) continue;
                if (p.Position.Contains("GK"))
                {
                    // GK pinned
                    p.XPosition = p.HomeX;
                    p.YPosition = p.HomeY;
                    continue;
                }
                int targetX = p.HomeX;
                if (teamAInPossession)
                {
                    // Move forward if in possession
                    int desired = p.HomeX + attackOffset;
                    desired = isFirstHalf ? Math.Min(desired, center - 1)
                                          : Math.Max(desired, center + 1);
                    targetX = (p.XPosition < desired) ? desired : p.XPosition;

                    // If near the ball, move slightly further
                    Player ballHolder = teamAPlayers.FirstOrDefault(pl => pl.HasBall);
                    if (ballHolder != null)
                    {
                        double dist = Distance(p.XPosition, p.YPosition, ballHolder.XPosition, ballHolder.YPosition);
                        if (dist < 5)
                            targetX = isFirstHalf ? Math.Min(targetX + 5, center - 1)
                                                  : Math.Max(targetX - 5, center + 1);
                    }
                }
                int targetY = p.HomeY;
                // BFS next step
                var next = TeamUpdaterFixed.CalculateNextStepBFS(p, targetX, targetY, occupied, pitchLeft, pitchTop);
                p.XPosition = Clamp(next.Item1, pitchLeft, pitchRight);
                p.YPosition = Clamp(next.Item2, pitchTop, pitchBottom);
            }

            // Team B updates
            foreach (var p in teamBPlayers)
            {
                if (p.HasBall) continue;
                if (p.Position.Contains("GK"))
                {
                    // GK pinned
                    p.XPosition = p.HomeX;
                    p.YPosition = p.HomeY;
                    continue;
                }
                int targetX = p.HomeX;
                if (teamBInPossession)
                {
                    // Move forward if in possession (for B, forward is decreasing X if isFirstHalf, etc.)
                    int desired = p.HomeX - attackOffset;
                    desired = isFirstHalf ? Math.Max(desired, center + 1)
                                          : Math.Min(desired, center - 1);
                    targetX = (p.XPosition > desired) ? desired : p.XPosition;

                    // If near the ball, move further
                    Player ballHolder = teamBPlayers.FirstOrDefault(pl => pl.HasBall);
                    if (ballHolder != null)
                    {
                        double dist = Distance(p.XPosition, p.YPosition, ballHolder.XPosition, ballHolder.YPosition);
                        if (dist < 5)
                            targetX = isFirstHalf ? Math.Max(targetX - 5, center + 1)
                                                  : Math.Min(targetX + 5, center - 1);
                    }
                }
                int targetY = p.HomeY;
                var next = TeamUpdaterFixed.CalculateNextStepBFS(p, targetX, targetY, occupied, pitchLeft, pitchTop);
                p.XPosition = Clamp(next.Item1, pitchLeft, pitchRight);
                p.YPosition = Clamp(next.Item2, pitchTop, pitchBottom);
            }
        }
        // Distance helper for BFS logic and tackle checks.
        private double Distance(int x1, int y1, int x2, int y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        // Handles user input for pausing or other global keys.
        private void HandleUserInput()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Spacebar)
                    isPaused = !isPaused;
            }
        }
        // Called when the match ends (time >= 90).
        // Prints final score.
        private void EndMatch()
        {
            Console.Clear();
            Console.SetCursorPosition(50, 20);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Match Ended!");
            Console.WriteLine($"Final Score: {teamAScore} - {teamBScore}");
            Console.ResetColor();
            Console.SetCursorPosition(0, 0);
        }

        // Draw method: uses a console buffer to draw the pitch and players,
        // then renders to the console.
        public override void Draw(bool active)
        {
            // Create a buffer matching the console size
            ConsoleCell[,] buffer = new ConsoleCell[Drawing.ConsoleHeight, Drawing.ConsoleWidth];
            // Clear it
            Drawing.ClearBuffer(buffer);
            // Draw pitch background
            Drawing.DrawPitch(buffer, pitchLeft, pitchTop, pitchRight, pitchBottom);

            // Draw Team A players
            foreach (var p in teamAPlayers)
            {
                // If the player has the ball, color them magenta
                ConsoleColor color = p.HasBall ? ConsoleColor.Magenta : ConsoleColor.Blue;
                Drawing.DrawPlayer(buffer, p.XPosition, p.YPosition, 'P', color, color);
            }
            // Draw Team B players
            foreach (var p in teamBPlayers)
            {
                ConsoleColor color = p.HasBall ? ConsoleColor.Magenta : ConsoleColor.Red;
                Drawing.DrawPlayer(buffer, p.XPosition, p.YPosition, 'P', color, color);
            }

            // Place scoreboard text
            Drawing.PlaceString(buffer, 2, 1,
                $"Time: {currentMinute}'   Score: {teamAScore}-{teamBScore}",
                ConsoleColor.Yellow, ConsoleColor.Black);

            // Render buffer
            Drawing.RenderBufferToConsole(buffer);
        }
    }
}
