using System;
using System.Collections.Generic;

namespace GUI
{
    public class Club
    {
        public string Name { get; }
        public double Balance { get; set; }
        public List<Player> Players { get; }
        public double TotalTransfersIn { get; set; }
        public double TotalTransfersOut { get; set; }
        public string SelectedTactic { get; set; }

        // League table properties:
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }

        // New properties needed by other parts of your application:
        public Formation? SelectedFormation { get; set; }
        public Dictionary<int, Player>? PositionAssignments { get; set; }

        public Club(string name, double balance)
        {
            Name = name;
            Balance = balance;
            Players = new List<Player>();
            TotalTransfersIn = 0;
            TotalTransfersOut = 0;
            SelectedTactic = "Tikitaka"; // default tactic
            Wins = 0;
            Draws = 0;
            Losses = 0;
            GoalsFor = 0;
            GoalsAgainst = 0;
            Points = 0;
            SelectedFormation = null;
            PositionAssignments = new Dictionary<int, Player>();
        }

        public void AddPlayer(Player player)
        {
            // If needed, remove from another club before adding.
            player.CurrentClub = this;
            Players.Add(player);
        }

        /// Records a match result by updating goals for/against and wins/draws/losses/points.
        public void RecordResult(int goalsFor, int goalsAgainst)
        {
            GoalsFor += goalsFor;
            GoalsAgainst += goalsAgainst;
            if (goalsFor > goalsAgainst)
            {
                Wins++;
                Points += 3;
            }
            else if (goalsFor == goalsAgainst)
            {
                Draws++;
                Points += 1;
            }
            else
            {
                Losses++;
            }
        }

        public virtual string GetLeagueInfoString()
        {
            // For display in the league table.
            return Name.PadRight(20, ' ');
        }

        public override string ToString() => Name;
    }
}
