using System;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;

namespace GUI
{
    public enum MainMenuSection { LiveMatch, MatchSettings }
    public enum MatchSettingsSubMenu { None, GameSpeed, FormationChange, ModifyTactics, PlayerRoles, AttackingToggle }

    public class Match : Window
    {
        // ----- Simulation & UI Fields -----
        private int pitchLeft, pitchTop, pitchRight, pitchBottom;
        private int leftGoalBoxXMax, rightGoalBoxXMin;
        private MainMenuSection currentMainSection;
        private MatchSettingsSubMenu currentSubMenu;
        private bool isRunning, isPaused;
        private bool isFirstHalf = true;
        private int currentMinute, gameSpeed, teamAScore, teamBScore;
        private bool ballInTransit;
        private string injuryMessage;
        private int injuryMessageTimer;
        private string lastTeamToTouchBall;
        private Random randomGenerator;
        private List<string> tacticsList;
        private int currentTacticIndex;
        private string currentTactic;

        // Added field for clarity.
        private bool firstHalfStartingTeamA = true;

        // ----- Team Data -----
        private List<Player> teamAPlayers, teamBPlayers, benchTeamAPlayers, benchTeamBPlayers;

        // Now the match is played between two clubs.
        private Club userClub;
        private Club opponentClub;

        // ----- Constructor -----
        public Match(Club userClub, Club opponentClub)
            : base("Live Match", new Rectangle(0, 0, 150, 40), true)
        {
            Console.CursorVisible = false;
            this.userClub = userClub;
            this.opponentClub = opponentClub;
            InitializeSimulation();
        }

        // ----- Initialization -----
        private void InitializeSimulation()
        {
            currentMainSection = MainMenuSection.LiveMatch;
            currentSubMenu = MatchSettingsSubMenu.None;
            isRunning = true;
            isPaused = false;
            ballInTransit = false;
            currentMinute = 0;
            gameSpeed = 100;
            teamAScore = 0;
            teamBScore = 0;
            tacticsList = new List<string> { "Tiki-Taka", "Gegenpress", "Park The Bus" };
            currentTacticIndex = 0;
            currentTactic = tacticsList[currentTacticIndex];

            // Set pitch boundaries.
            pitchLeft = 2;
            pitchTop = 4;
            pitchRight = 100;
            pitchBottom = 30;
            leftGoalBoxXMax = pitchLeft + 1;
            rightGoalBoxXMin = pitchRight - 1;

            randomGenerator = new Random();
            InitializeTeams();

            // Use legacy formation assignment (for demo).
            AssignFormationPositions(teamAPlayers, true);
            AssignFormationPositions(teamBPlayers, false);

            // Start with Team A kickoff.
            firstHalfStartingTeamA = true;
            WaitForKickoffEvent();
            Player kickoffPlayer = SelectKickoffPlayer(teamAPlayers);
            if (kickoffPlayer != null)
                AssignBallToPlayer(kickoffPlayer);
        }

        private void InitializeTeams()
        {
            // Retrieve unified squads from TeamFactory.
            var userTeamData = TeamFactory.GetTeamFromClub(userClub);
            teamAPlayers = userTeamData.starting;
            benchTeamAPlayers = userTeamData.bench;

            var opponentTeamData = TeamFactory.GetTeamFromClub(opponentClub);
            teamBPlayers = opponentTeamData.starting;
            benchTeamBPlayers = opponentTeamData.bench;
        }

        /// <summary>
        /// Legacy method to assign formation positions using a fixed template.
        /// </summary>
        private void AssignFormationPositions(List<Player> team, bool isTeamA)
        {
            double[,] template = new double[11, 2]
            {
                {0.05, 0.5},   // GK (overridden)
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

            int count = team.Count < 11 ? team.Count : 11;
            for (int i = 0; i < count; i++)
            {
                int posX, posY;
                if (i == 0 && team[i].Position.Contains("GK"))
                {
                    if (isTeamA)
                    {
                        posX = pitchLeft + 1;
                        posY = (pitchTop + pitchBottom) / 2;
                    }
                    else
                    {
                        posX = pitchRight - 1;
                        posY = (pitchTop + pitchBottom) / 2;
                    }
                }
                else
                {
                    double relX = template[i, 0];
                    double relY = template[i, 1];
                    double newRelX = isTeamA ? relX * 0.5 : 0.5 + (relX * 0.5);
                    posX = pitchLeft + (int)(newRelX * (pitchRight - pitchLeft));
                    posY = pitchTop + (int)(relY * (pitchBottom - pitchTop));
                }
                team[i].XPosition = posX;
                team[i].YPosition = posY;
                team[i].HomeX = posX;
                team[i].HomeY = posY;
            }
        }

        // ----- Kickoff & Ball Assignment -----
        private void WaitForKickoffEvent()
        {
            Console.Clear();
            Console.SetCursorPosition(30, 15);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Kickoff! Press any key to start.");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        private Player SelectKickoffPlayer(List<Player> team)
        {
            // Find a striker.
            for (int i = 0; i < team.Count; i++)
            {
                if (team[i].Position.Contains("ST"))
                    return team[i];
            }
            // Else, first non-goalkeeper.
            for (int i = 0; i < team.Count; i++)
            {
                if (!team[i].IsGoalkeeper)
                    return team[i];
            }
            return team.Count > 0 ? team[0] : null;
        }

        private void AssignBallToPlayer(Player newHolder)
        {
            if (newHolder == null)
            {
                Console.WriteLine("[DEBUG] AssignBallToPlayer: newHolder is null.");
                return;
            }
            // Clear ball flag for all players.
            for (int i = 0; i < teamAPlayers.Count; i++)
                teamAPlayers[i].HasBall = false;
            for (int i = 0; i < teamBPlayers.Count; i++)
                teamBPlayers[i].HasBall = false;
            newHolder.HasBall = true;
            newHolder.IsRunningWithBall = false;
            if (IsPlayerInTeam(newHolder, teamAPlayers))
                lastTeamToTouchBall = "TeamA";
            else
                lastTeamToTouchBall = "TeamB";
        }

        // ----- Main Simulation Loop -----
        public void Start()
        {
            int simulationTick = 0;
            int center = (pitchLeft + pitchRight) / 2;
            while (isRunning)
            {
                if (!isPaused)
                {
                    simulationTick++;
                    currentMinute += (int)(gameSpeed / 100.0);
                    if (currentMinute >= 90)
                    {
                        isRunning = false;
                        continue;
                    }

                    UpdateDribbling(center);
                    UpdatePassing(simulationTick);
                    UpdateShooting(center);
                    CheckHalftime();
                    UpdatePlayerMovements(center);
                }
                HandleUserInput();
                Draw(true);
                Thread.Sleep(isPaused ? 100 : (1000 / (gameSpeed > 0 ? gameSpeed : 1)));
            }
            EndMatch();
        }

        // ----- Helper Methods for Simulation -----
        private void UpdateDribbling(int center)
        {
            Player ballHolder = GetBallHolder();
            if (ballHolder != null)
            {
                if (ballHolder.Position.Contains("GK"))
                {
                    // Instead of forcing GK exactly to home, clamp them to a small goal box.
                    if (IsPlayerInTeam(ballHolder, teamAPlayers))
                    {
                        int minX = pitchLeft;
                        int maxX = pitchLeft + 2;
                        int centerY = (pitchTop + pitchBottom) / 2;
                        int minY = centerY - 2;
                        int maxY = centerY + 2;
                        ballHolder.XPosition = Clamp(ballHolder.XPosition, minX, maxX);
                        ballHolder.YPosition = Clamp(ballHolder.YPosition, minY, maxY);
                    }
                    else if (IsPlayerInTeam(ballHolder, teamBPlayers))
                    {
                        int minX = pitchRight - 2;
                        int maxX = pitchRight;
                        int centerY = (pitchTop + pitchBottom) / 2;
                        int minY = centerY - 2;
                        int maxY = centerY + 2;
                        ballHolder.XPosition = Clamp(ballHolder.XPosition, minX, maxX);
                        ballHolder.YPosition = Clamp(ballHolder.YPosition, minY, maxY);
                    }
                }
                else
                {
                    // For non-goalkeepers, move horizontally depending on possession.
                    if (IsPlayerInTeam(ballHolder, teamAPlayers))
                    {
                        int step = ballHolder.XPosition > center ? 3 : 2;
                        if (isFirstHalf)
                            ballHolder.XPosition = Math.Min(ballHolder.XPosition + step, pitchRight);
                        else
                            ballHolder.XPosition = Math.Max(ballHolder.XPosition - step, pitchLeft);
                    }
                    else if (IsPlayerInTeam(ballHolder, teamBPlayers))
                    {
                        int step = ballHolder.XPosition < center ? 3 : 2;
                        if (isFirstHalf)
                            ballHolder.XPosition = Math.Max(ballHolder.XPosition - step, pitchLeft);
                        else
                            ballHolder.XPosition = Math.Min(ballHolder.XPosition + step, pitchRight);
                    }
                }
            }
        }

        private void UpdatePassing(int simulationTick)
        {
            if (!ballInTransit && simulationTick % 5 == 0)
            {
                Player ballHolder = GetBallHolder();
                if (ballHolder != null)
                {
                    List<Player> team = IsPlayerInTeam(ballHolder, teamAPlayers) ? teamAPlayers : teamBPlayers;
                    if (team.Count > 1)
                    {
                        // Choose a receiver simply.
                        int index = randomGenerator.Next(team.Count);
                        Player receiver = team[index];
                        if (receiver == ballHolder)
                            receiver = team[(index + 1) % team.Count];

                        bool stolen;
                        Animation.AnimatePass(ballHolder.XPosition, ballHolder.YPosition,
                            receiver.XPosition, receiver.YPosition,
                            gameSpeed, randomGenerator, out stolen);
                        if (!stolen)
                            AssignBallToPlayer(receiver);
                    }
                }
            }
        }

        private void UpdateShooting(int center)
        {
            if (!ballInTransit && currentMinute % 10 == 0)
            {
                Player ballHolder = GetBallHolder();
                if (ballHolder != null && !ballHolder.Position.Contains("GK"))
                {
                    if (IsPlayerInTeam(ballHolder, teamAPlayers))
                    {
                        bool inOppHalf = ballHolder.XPosition > center;
                        double shotChance = inOppHalf ? 0.5 : 0.3;
                        if (randomGenerator.NextDouble() < shotChance)
                        {
                            Player defendingGK = GetGoalkeeper(teamBPlayers);
                            double saveChance = 0.4;
                            if (defendingGK != null && randomGenerator.NextDouble() < saveChance)
                            {
                                Console.WriteLine(defendingGK.Name + " saves the shot!");
                                AssignBallToPlayer(defendingGK);
                            }
                            else
                            {
                                Animation.AnimateShot(ballHolder.XPosition, ballHolder.YPosition,
                                    pitchRight, (pitchTop + pitchBottom) / 2 + randomGenerator.Next(-2, 3),
                                    gameSpeed);
                                teamAScore++;
                                WaitForKickoffEvent();
                                Player kp = SelectKickoffPlayer(teamBPlayers);
                                if (kp != null)
                                    AssignBallToPlayer(kp);
                            }
                        }
                    }
                    else if (IsPlayerInTeam(ballHolder, teamBPlayers))
                    {
                        bool inOppHalf = ballHolder.XPosition < center;
                        double shotChance = inOppHalf ? 0.5 : 0.3;
                        if (randomGenerator.NextDouble() < shotChance)
                        {
                            Player defendingGK = GetGoalkeeper(teamAPlayers);
                            double saveChance = 0.4;
                            if (defendingGK != null && randomGenerator.NextDouble() < saveChance)
                            {
                                Console.WriteLine(defendingGK.Name + " saves the shot!");
                                AssignBallToPlayer(defendingGK);
                            }
                            else
                            {
                                Animation.AnimateShot(ballHolder.XPosition, ballHolder.YPosition,
                                    pitchLeft, (pitchTop + pitchBottom) / 2 + randomGenerator.Next(-2, 3),
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

        private void CheckHalftime()
        {
            if (isFirstHalf && currentMinute >= 45)
            {
                isFirstHalf = false;
                WaitForKickoffEvent();
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

        private void UpdatePlayerMovements(int center)
        {
            // For simplicity, move non-ball-holders one step toward their home positions.
            UpdateTeamMovement(teamAPlayers, true, center, 10, true);
            UpdateTeamMovement(teamBPlayers, true, center, 10, false);
        }

        private void UpdateTeamMovement(List<Player> team, bool inPossession, int center, int offset, bool isTeamA)
        {
            for (int i = 0; i < team.Count; i++)
            {
                Player p = team[i];
                if (p.HasBall || p.Position.Contains("GK"))
                    continue;
                int targetX = p.HomeX;
                if (inPossession)
                {
                    if (isTeamA)
                    {
                        int desired = p.HomeX + offset;
                        desired = isFirstHalf ? Math.Min(desired, center - 1) : Math.Max(desired, center + 1);
                        if (p.XPosition < desired)
                            targetX = desired;
                    }
                    else
                    {
                        int desired = p.HomeX - offset;
                        desired = isFirstHalf ? Math.Max(desired, center + 1) : Math.Min(desired, center - 1);
                        if (p.XPosition > desired)
                            targetX = desired;
                    }
                }
                else
                {
                    targetX = p.HomeX;
                }
                int targetY = p.HomeY;
                if (p.XPosition < targetX)
                    p.XPosition++;
                else if (p.XPosition > targetX)
                    p.XPosition--;
                if (p.YPosition < targetY)
                    p.YPosition++;
                else if (p.YPosition > targetY)
                    p.YPosition--;
            }
        }

        // ----- Utility Methods -----
        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private Player GetBallHolder()
        {
            foreach (Player p in teamAPlayers)
            {
                if (p.HasBall)
                    return p;
            }
            foreach (Player p in teamBPlayers)
            {
                if (p.HasBall)
                    return p;
            }
            return null;
        }

        private bool IsPlayerInTeam(Player player, List<Player> team)
        {
            foreach (Player p in team)
            {
                if (p == player)
                    return true;
            }
            return false;
        }

        private bool HasBall(List<Player> team)
        {
            foreach (Player p in team)
            {
                if (p.HasBall)
                    return true;
            }
            return false;
        }

        private Player GetGoalkeeper(List<Player> team)
        {
            foreach (Player p in team)
            {
                if (p.Position.Contains("GK"))
                    return p;
            }
            return null;
        }

        private void HandleUserInput()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Spacebar)
                    isPaused = !isPaused;
            }
        }

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

        // ----- UI Drawing -----
        public override void Draw(bool active)
        {
            ConsoleCell[,] buffer = new ConsoleCell[Drawing.ConsoleHeight, Drawing.ConsoleWidth];
            Drawing.ClearBuffer(buffer);
            Drawing.DrawPitch(buffer, pitchLeft, pitchTop, pitchRight, pitchBottom);
            foreach (Player p in teamAPlayers)
            {
                ConsoleColor col = p.HasBall ? ConsoleColor.Magenta : ConsoleColor.Blue;
                Drawing.DrawPlayer(buffer, p.XPosition, p.YPosition, 'P', col, col);
            }
            foreach (Player p in teamBPlayers)
            {
                ConsoleColor col = p.HasBall ? ConsoleColor.Magenta : ConsoleColor.Red;
                Drawing.DrawPlayer(buffer, p.XPosition, p.YPosition, 'P', col, col);
            }
            Drawing.PlaceString(buffer, 2, 1, $"Time: {currentMinute}'   Score: {teamAScore}-{teamBScore}   Tactic: {currentTactic}", ConsoleColor.Yellow, ConsoleColor.Black);
            Drawing.RenderBufferToConsole(buffer);
        }
    }
}
