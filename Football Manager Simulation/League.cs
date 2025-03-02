using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI
{
    public class League
    {
        private List<Club> _clubs;

        // New property to hold the user's club.
        public Club UserClub { get; set; }

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

            Random random = new Random();
            string[] firstNames = new string[]
            {
                "James", "Oliver", "Benjamin", "Lucas", "Mason",
                "Ethan", "Liam", "Alexander", "Noah", "Daniel",
                "Samuel", "William", "Henry", "Jacob", "Michael",
                "Jack", "Elijah", "Sebastian", "Charlie", "Oscar"
            };
            string[] lastNames = new string[]
            {
                "Smith", "Jones", "Williams", "Taylor", "Brown",
                "Davies", "Evans", "Wilson", "Thomas", "Roberts",
                "Johnson", "Lewis", "Walker", "Robinson", "Wright",
                "Thompson", "White", "Hughes", "Edwards", "Green"
            };
            string[] positions = new string[] { "GK", "CB", "LB", "RB", "CDM", "CM", "CAM", "ST", "LW", "RW" };

            // Generate 16 players per club.
            foreach (var club in _clubs)
            {
                for (int i = 0; i < 16; i++)
                {
                    string fullName = firstNames[random.Next(firstNames.Length)] + " " +
                                      lastNames[random.Next(lastNames.Length)];
                    int rating = random.Next(75, 91);
                    int value = rating * 100000;
                    int age = random.Next(18, 36);
                    string pos = positions[random.Next(positions.Length)];
                    int potential = random.Next(rating, Math.Min(101, rating + 10));
                    int wage = random.Next(50, 201); // in thousands per week
                    int contractLength = random.Next(1, 6);
                    string squadStatus = "First Team Member";
                    var statistics = new Dictionary<string, int>
                    {
                        { "Goals", random.Next(0, 11) },
                        { "Assists", random.Next(0, 11) },
                        { "Appearances", random.Next(0, 39) }
                    };

                    Player newPlayer = new Player(fullName, value, rating, age, pos, potential, wage, contractLength, squadStatus, statistics);
                    newPlayer.AvailableForTransfer = random.NextDouble() < 0.3;
                    if (newPlayer.AvailableForTransfer)
                    {
                        // Set transfer price based on rating (in millions).
                        newPlayer.TransferPrice = Math.Round(rating * 0.2, 1);
                    }
                    club.AddPlayer(newPlayer);
                }
            }
        }
        /// Returns all players from all clubs.
        public List<Player> GetAllPlayers()
        {
            return _clubs.SelectMany(c => c.Players).ToList();
        }

        public List<Club> Clubs => _clubs;
        /// Simulates a match day by randomly pairing clubs (adding a dummy if needed)
        /// and simulating matches using the existing MatchResultSimulator.
        public void SimulateRemainingMatchesForMatchDay()
        {
            Random random = new Random();
            List<Club> clubsCopy = new List<Club>(Clubs);
            if (clubsCopy.Count % 2 != 0)
            {
                clubsCopy.Add(new Club("Dummy", 0));
            }
            clubsCopy = clubsCopy.OrderBy(c => random.Next()).ToList();
            for (int i = 0; i < clubsCopy.Count; i += 2)
            {
                if (clubsCopy[i].Name == "Dummy" || clubsCopy[i + 1].Name == "Dummy")
                    continue;
                MatchResultSimulator.SimulateMatch(clubsCopy[i], clubsCopy[i + 1], random);
            }
        }
        /// Called after every match. AI clubs (excluding the user's club)
        /// try to buy a player from the transfer market.
        public void AIBuyPlayers()
        {
            Random rand = new Random();
            // Get transfer-listed players.
            List<Player> transferListed = GetAllPlayers()
                .Where(p => p.AvailableForTransfer && p.CurrentClub != null)
                .ToList();

            foreach (var club in _clubs)
            {
                // Skip the user's club and clubs without a formation.
                if (club == UserClub || club.SelectedFormation == null)
                    continue;

                // 25% chance to attempt a purchase.
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
        /// Called every 5 matches. AI clubs (excluding the user's club)
        /// randomly list a player for sale.
        public void AISellPlayers()
        {
            Random rand = new Random();
            foreach (var club in _clubs)
            {
                if (club == UserClub || club.SelectedFormation == null)
                    continue;

                // 30% chance to list a player.
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
        /// Determines a needed position by choosing the one with the lowest average rating in the club.
        /// If no players exist in a position, that position is immediately needed.
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
