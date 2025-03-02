using System;
using System.Collections.Generic;

namespace GUI
{
    // Represents a football club with a name, balance, players, and league stats.
    public class Club
    {
        public string Name { get; }              // Club name
        public double Balance { get; set; }      // Financial balance
        public List<Player> Players { get; }     // The squad (list of players)
        public double TotalTransfersIn { get; set; }   // Money spent
        public double TotalTransfersOut { get; set; }  // Money earned from sales
        public string SelectedTactic { get; set; }      // Current tactic name

        // League table properties
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }

        // Formation/tactical properties
        public Formation? SelectedFormation { get; set; }
        public Dictionary<int, Player>? PositionAssignments { get; set; }
        // Constructor for a new club, initializes an empty players list
        // and default stats.
        public Club(string name, double balance)
        {
            Name = name;
            Balance = balance;
            Players = new List<Player>();
            TotalTransfersIn = 0;
            TotalTransfersOut = 0;
            SelectedTactic = "Tikitaka"; // default
            Wins = 0;
            Draws = 0;
            Losses = 0;
            GoalsFor = 0;
            GoalsAgainst = 0;
            Points = 0;
            SelectedFormation = null;
            PositionAssignments = new Dictionary<int, Player>();
        }
        // Adds a player to this club's squad, sets player's CurrentClub to this club.
        public void AddPlayer(Player player)
        {
            player.CurrentClub = this;
            Players.Add(player);
        }

        // Records a match result: updates goals for/against and W/D/L/Points.
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
        // Returns a string for displaying in the league table,
        // e.g. the club name padded to 20 chars.
        public virtual string GetLeagueInfoString()
        {
            return Name.PadRight(20, ' ');
        }

        // ToString returns the club's name.
        public override string ToString() => Name;
    }
}
