using System;
using System.Collections.Generic;

namespace GUI
{
    // Represents a player with various attributes such as name, rating, position, and statistics.
    // This class also contains properties used during match simulation (positions, ball possession, etc.).
    public class Player
    {
        // Private fields for the player's attributes.
        private string _name;
        private int _value;
        private int _rating;
        private bool _availableForTransfer;
        private Club? _currentClub;
        private int _age;
        private string _position; // The player's position (e.g., "GK", "ST", etc.)
        private int _potential;
        private int _wage;
        private int _contractLength;
        private string _squadStatus;
        private Dictionary<string, int> _statistics;

        // Public read-only properties and public getters/setters where needed.
        public string Name => _name;
        public int Value { get => _value; set => _value = value; }
        public int Rating => _rating;
        public bool AvailableForTransfer { get => _availableForTransfer; set => _availableForTransfer = value; }
        public Club? CurrentClub { get => _currentClub; set => _currentClub = value; }
        public int Age => _age;
        public string Position => _position; // Exposed as read-only property.
        public int Potential => _potential;
        public int Wage => _wage;
        public int ContractLength { get => _contractLength; set => _contractLength = value; }
        public string SquadStatus { get => _squadStatus; set => _squadStatus = value; }
        public Dictionary<string, int> Statistics => _statistics;

        // Simulation-related properties:
        public double TransferPrice { get; set; }
        public string TacticalRole { get; set; }
        public DateTime LastTackleAttempt { get; set; } = DateTime.MinValue;
        public double SkillLevel => (double)_rating / 100.0;

        // In-match properties (updated during a match simulation):
        public int XPosition { get; set; }
        public int YPosition { get; set; }
        public bool HasBall { get; set; }
        public bool IsGoalkeeper { get; set; } // Flag indicating if the player is a goalkeeper.
        public bool IsKickoffPlayer { get; set; }
        public bool IsRunningWithBall { get; set; }
        public bool IsInjured { get; set; }
        public double Fitness { get; set; }

        // Home (formation) position: where the player is expected to be during match start.
        public int HomeX { get; set; }
        public int HomeY { get; set; }

        // Constructor to initialize a new player with the given attributes.
        public Player(string name, int value, int rating, int age, string position, int potential, int wage, int contractLength, string squadStatus, Dictionary<string, int> statistics)
        {
            _name = name;
            _value = value;
            _rating = rating;
            _availableForTransfer = false;
            _currentClub = null;
            _age = age;
            _position = position;
            _potential = potential;
            _wage = wage;
            _contractLength = contractLength;
            _squadStatus = squadStatus;
            _statistics = statistics;

            // Initialize simulation properties.
            XPosition = 0;
            YPosition = 0;
            HasBall = false;
            IsGoalkeeper = position.Contains("GK");
            IsKickoffPlayer = false;
            IsRunningWithBall = false;
            IsInjured = false;
            Fitness = 100.0;
            TacticalRole = "";
            TransferPrice = 0;
            HomeX = 0;
            HomeY = 0;
        }
        // Allows updating the player's position.
        // This is useful for converting a player from a goalkeeper to an outfield role, etc.
        public void SetPosition(string newPos)
        {
            _position = newPos;
            // Update the IsGoalkeeper flag based on the new position.
            IsGoalkeeper = newPos.Contains("GK");
        }
        // Returns a string with key player information for display.
        public virtual string Info()
        {
            return $"{_name} (£{_value}) Rating: {_rating} Transfer: {_availableForTransfer}";
        }
        // Toggles the player's availability for transfer.
        public void ToggleTransferStatus()
        {
            _availableForTransfer = !_availableForTransfer;
        }
    }
}
