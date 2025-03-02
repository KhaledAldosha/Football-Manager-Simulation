using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI
{
    // The League class holds information about the entire league,
    // including all clubs, and provides methods to simulate match days,
    // process AI transfers, and gather overall player information.
    // It also generates a roster for each club.
    public class League
    {
        // List of all clubs in the league.
        private List<Club> _clubs;
        // Reference to the user's club.
        public Club UserClub { get; set; }
        // Constructor creates a new league with a preset list of clubs.
        // It also generates a roster of players for each club.
        public League()
        {
            _clubs = new List<Club>
            {
                new Club("Manchester City", 100000000),
                new Club("Liverpool", 100000000),
                new Club("Manchester United", 100000000),
                new Club("Arsenal", 100000000),
                new Club("Chelsea", 100000000),
                new Club("Tottenham Hotspur", 100000000),
                new Club("Newcastle United", 100000000),
                new Club("Aston Villa", 100000000),
                new Club("Brighton & Hove Albion", 100000000),
                new Club("West Ham United", 100000000),
                new Club("Brentford", 100000000),
                new Club("Fulham", 100000000),
                new Club("Crystal Palace", 100000000),
                new Club("Everton", 100000000),
                new Club("Wolverhampton Wanderers", 100000000),
                new Club("Leicester City", 100000000)
            };

            // Generate players for each club.
            GeneratePlayers();
        }
        // Generates a roster of players for each club.
        // In this example, 16 players per club are created with random attributes.
        // The first player generated for each club is forced to be a goalkeeper ("GK").
        private void GeneratePlayers()
        {
            Random random = new Random();

            // Arrays for random generation of names and positions.
            string[] firstNames = { "James", "Oliver", "Benjamin", "Lucas", "Mason", "Ethan", "Liam", "Alexander", "Noah", "Daniel" };
            string[] lastNames = { "Smith", "Jones", "Williams", "Taylor", "Brown", "Davies", "Evans", "Wilson", "Thomas", "Roberts" };
            // Position list – index 0 is GK; others are outfield positions.
            string[] positions = { "GK", "CB", "LB", "RB", "CDM", "CM", "CAM", "ST", "LW", "RW" };

            // Iterate through each club.
            foreach (var club in _clubs)
            {
                // Create 16 players per club.
                for (int i = 0; i < 16; i++)
                {
                    // Generate a random full name.
                    string name = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";

                    // Random rating between 70 and 90.
                    int rating = random.Next(70, 91);

                    // Value based on rating.
                    int value = rating * 100000;

                    // Random age between 18 and 35.
                    int age = random.Next(18, 36);

                    // Force the first player to be a goalkeeper.
                    string pos = (i == 0) ? "GK" : positions[random.Next(1, positions.Length)];

                    // Potential between current rating and a maximum of 100.
                    int potential = random.Next(rating, Math.Min(101, rating + 10));

                    // Wage in thousands per week.
                    int wage = random.Next(50, 201);

                    // Contract length in years.
                    int contractLength = random.Next(1, 6);

                    // Basic statistics dictionary.
                    var stats = new Dictionary<string, int>
                    {
                        { "Goals", random.Next(0, 11) },
                        { "Assists", random.Next(0, 11) },
                        { "Appearances", random.Next(0, 39) }
                    };

                    // Create a new player with the generated attributes.
                    Player newPlayer = new Player(name, value, rating, age, pos, potential, wage, contractLength, "First Team Member", stats);

                    // Add the player to the club.
                    club.AddPlayer(newPlayer);
                }
            }
        }
        // Returns a list of all players in the league (from all clubs).
        // Useful for global statistics or the transfer market.
        public List<Player> GetAllPlayers()
        {
            return _clubs.SelectMany(c => c.Players).ToList();
        }
        // Gets the list of all clubs in the league.
        public List<Club> Clubs => _clubs;
        // Returns exactly 11 players for match simulation:
        // 1 goalkeeper and 10 outfield players.
        // It selects the highest-rated GK (or a dummy if none) and
        // then the 10 best outfield players. If there aren’t enough, dummy players are added.
        public List<Player> SelectStartingEleven(Club club)
        {
            List<Player> starting = new List<Player>();

            // Select the goalkeeper from club.Players whose position contains "GK"
            var gks = club.Players.Where(p => p.Position.Contains("GK")).ToList();
            if (gks.Any())
                starting.Add(gks.OrderByDescending(p => p.Rating).First());
            else
                starting.Add(new Player("Dummy GK", 0, 70, 25, "GK", 70, 0, 1, "Dummy", new Dictionary<string, int>()));

            // Select 10 best outfield players (players whose Position does not contain "GK")
            var outfield = club.Players.Where(p => !p.Position.Contains("GK"))
                                       .OrderByDescending(p => p.Rating)
                                       .Take(10)
                                       .ToList();
            starting.AddRange(outfield);

            // If fewer than 11 players, add dummy outfield players.
            while (starting.Count < 11)
            {
                starting.Add(new Player("Dummy OF", 0, 70, 25, "ST", 70, 0, 1, "Dummy", new Dictionary<string, int>()));
            }
            return starting;
        }
        // Simulates the remaining matches for the current match day.
        // Randomly pairs clubs and simulates matches using MatchResultSimulator.
        public void SimulateRemainingMatchesForMatchDay()
        {
            Random random = new Random();
            List<Club> clubsCopy = new List<Club>(Clubs);
            // If odd number of clubs, add a dummy club.
            if (clubsCopy.Count % 2 != 0)
            {
                clubsCopy.Add(new Club("Dummy", 0));
            }
            // Shuffle the clubs randomly.
            clubsCopy = clubsCopy.OrderBy(c => random.Next()).ToList();
            for (int i = 0; i < clubsCopy.Count; i += 2)
            {
                if (clubsCopy[i].Name == "Dummy" || clubsCopy[i + 1].Name == "Dummy")
                    continue;
                // Simulate the match between the two clubs.
                MatchResultSimulator.SimulateMatch(clubsCopy[i], clubsCopy[i + 1], random);
            }
        }
        // AI-controlled clubs (excluding the user's club) attempt to buy players
        // from the transfer market. Each club has a 25% chance to attempt a purchase.
        public void AIBuyPlayers()
        {
            Random rand = new Random();
            // Get a list of players flagged as available for transfer.
            List<Player> transferListed = GetAllPlayers()
                .Where(p => p.AvailableForTransfer && p.CurrentClub != null)
                .ToList();

            foreach (var club in _clubs)
            {
                // Skip the user's club and clubs without a formation.
                if (club == UserClub || club.SelectedFormation == null)
                    continue;

                if (rand.NextDouble() > 0.25)
                    continue;

                string neededPosition = DetermineNeededPosition(club);
                var matchingPlayers = transferListed
                    .Where(p => p.Position.Equals(neededPosition, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingPlayers.Count == 0)
                    continue;

                Player target = matchingPlayers[rand.Next(matchingPlayers.Count)];
                double transferFee = target.TransferPrice * 1_000_000;
                if (club.Balance >= transferFee)
                {
                    Club fromClub = target.CurrentClub!;
                    fromClub.Players.Remove(target);
                    club.Players.Add(target);

                    club.Balance -= transferFee;
                    fromClub.Balance += transferFee;

                    club.TotalTransfersIn += transferFee;
                    fromClub.TotalTransfersOut += transferFee;

                    target.CurrentClub = club;
                    target.AvailableForTransfer = false;
                    target.TransferPrice = 0;

                    Console.WriteLine($"[AI BUY] {club.Name} bought {target.Name} for £{transferFee} from {fromClub.Name}");
                }
            }
        }
        // AI-controlled clubs, every 5 matches, attempt to list a player for sale.
        // Each club (excluding the user's) has a 30% chance to list a player.
        public void AISellPlayers()
        {
            Random rand = new Random();
            foreach (var club in _clubs)
            {
                if (club == UserClub || club.SelectedFormation == null)
                    continue;

                if (rand.NextDouble() > 0.30)
                    continue;

                var safeToSell = club.Players.Where(p => !p.AvailableForTransfer).ToList();
                if (safeToSell.Count == 0)
                    continue;

                Player toSell = safeToSell[rand.Next(safeToSell.Count)];
                toSell.AvailableForTransfer = true;
                toSell.TransferPrice = Math.Round(toSell.Rating * 0.2, 1);
                Console.WriteLine($"[AI SELL] {club.Name} listed {toSell.Name} for £{toSell.TransferPrice}M");
            }
        }
        // Determines which position a club needs to strengthen based on the average rating
        // of players in each position. If a position has no players, that position is immediately needed.
        private string DetermineNeededPosition(Club club)
        {
            if (club.SelectedFormation == null)
                return "ST";

            var positions = club.SelectedFormation.Positions.Select(fp => fp.PositionName).Distinct();
            string neededPos = "ST";
            double minRating = double.MaxValue;

            foreach (var pos in positions)
            {
                var playersInPos = club.Players.Where(p => p.Position.Equals(pos, StringComparison.OrdinalIgnoreCase));
                if (!playersInPos.Any())
                    return pos;
                double avg = playersInPos.Average(p => p.Rating);
                if (avg < minRating)
                {
                    minRating = avg;
                    neededPos = pos;
                }
            }
            return neededPos;
        }
    }
}
