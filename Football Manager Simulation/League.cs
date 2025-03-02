using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI
{
    // The League class holds information about the entire league,
    // including all clubs, and provides methods to simulate matches and
    // perform league-wide operations (such as transfers and match day simulations).
    public class League
    {
        private List<Club> _clubs;  // List of all clubs in the league
        // Holds a reference to the user-controlled club.
        public Club UserClub { get; set; }
        // Constructor initializes a default league with several clubs.
        // Each club is created with a given starting balance.
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

            // Additional code to generate players for each club can be here...
        }
        // Returns a list of all players in the league from all clubs.
        // Useful for global searches, transfers, and statistics.
        public List<Player> GetAllPlayers()
        {
            return _clubs.SelectMany(c => c.Players).ToList();
        }
        // Gets the list of clubs in the league.
        public List<Club> Clubs => _clubs;

        // Simulates the remaining matches for a given match day.
        // Randomly pairs clubs and simulates matches.
        public void SimulateRemainingMatchesForMatchDay()
        {
            Random random = new Random();
            List<Club> clubsCopy = new List<Club>(Clubs);
            // If odd number of clubs, add a dummy to make pairs.
            if (clubsCopy.Count % 2 != 0)
            {
                clubsCopy.Add(new Club("Dummy", 0));
            }
            // Shuffle clubs randomly.
            clubsCopy = clubsCopy.OrderBy(c => random.Next()).ToList();
            // Simulate matches pairwise.
            for (int i = 0; i < clubsCopy.Count; i += 2)
            {
                // Skip dummy clubs.
                if (clubsCopy[i].Name == "Dummy" || clubsCopy[i + 1].Name == "Dummy")
                    continue;
                MatchResultSimulator.SimulateMatch(clubsCopy[i], clubsCopy[i + 1], random);
            }
        }
        // Called after every match day; AI-controlled clubs try to buy players.
        // (This method can be expanded for more complex AI behavior.)
        public void AIBuyPlayers()
        {
            Random rand = new Random();
            // Get all players listed for transfer.
            List<Player> transferListed = GetAllPlayers()
                .Where(p => p.AvailableForTransfer && p.CurrentClub != null)
                .ToList();

            foreach (var club in _clubs)
            {
                // Skip the user's club and clubs without a formation.
                if (club == UserClub || club.SelectedFormation == null)
                    continue;

                // 25% chance for an AI club to attempt a purchase.
                if (rand.NextDouble() > 0.25)
                    continue;

                // Determine which position is needed.
                string neededPosition = DetermineNeededPosition(club);
                var matchingPlayers = transferListed
                    .Where(p => p.Position.Equals(neededPosition, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingPlayers.Count == 0)
                    continue;

                // Randomly pick one player from the matching players.
                Player target = matchingPlayers[rand.Next(matchingPlayers.Count)];
                double transferFee = target.TransferPrice * 1_000_000;
                if (club.Balance >= transferFee)
                {
                    // Process the transfer: remove from current club, add to buying club.
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

        // Called after every 5 matches; AI-controlled clubs randomly list a player for sale.
        public void AISellPlayers()
        {
            Random rand = new Random();
            foreach (var club in _clubs)
            {
                if (club == UserClub || club.SelectedFormation == null)
                    continue;

                // 30% chance to list a player for transfer.
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

        // Determines the position that needs strengthening in the club,
        // based on the average rating of players in each position.
        private string DetermineNeededPosition(Club club)
        {
            if (club.SelectedFormation == null)
                return "ST";

            // Get all distinct positions from the formation.
            var positions = club.SelectedFormation.Positions.Select(fp => fp.PositionName).Distinct();
            string neededPos = "ST";
            double minRating = double.MaxValue;

            foreach (var pos in positions)
            {
                // Get players in this position.
                var playersInPos = club.Players.Where(p => p.Position.Equals(pos, StringComparison.OrdinalIgnoreCase));
                if (!playersInPos.Any())
                    return pos; // Immediately need a player if none exist.

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
