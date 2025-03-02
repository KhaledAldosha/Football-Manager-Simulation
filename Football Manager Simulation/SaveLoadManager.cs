using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GUI
{
    public static class SaveLoadManager
    {
        // SaveGame writes the match counter, the user's club, and then each club's data.
        public static void SaveGame(int matchCounter, League league, string filePath = "savegame.txt")
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Save match counter.
                    writer.WriteLine(matchCounter);
                    // Save the user's club name.
                    writer.WriteLine("UserClub|" + (league.UserClub != null ? league.UserClub.Name : ""));

                    // For each club.
                    foreach (Club club in league.Clubs)
                    {
                        // Prepare starting eleven from PositionAssignments (sorted by key).
                        string startingEleven = "";
                        if (club.PositionAssignments != null && club.PositionAssignments.Count > 0)
                        {
                            var sortedAssignments = club.PositionAssignments.OrderBy(kv => kv.Key);
                            startingEleven = string.Join(",", sortedAssignments.Select(kv => kv.Value.Name));
                        }

                        // Format: Club|Name|Balance|Wins|Draws|Losses|GoalsFor|GoalsAgainst|Points|StartingEleven
                        string clubLine = $"Club|{club.Name}|{club.Balance}|{club.Wins}|{club.Draws}|{club.Losses}|{club.GoalsFor}|{club.GoalsAgainst}|{club.Points}|{startingEleven}";
                        writer.WriteLine(clubLine);

                        // Write each player's data.
                        // Format: Player|Name|Value|Rating|Age|Position|Potential|Wage|ContractLength|SquadStatus|TransferPrice|AvailableForTransfer
                        foreach (Player p in club.Players)
                        {
                            string playerLine = $"Player|{p.Name}|{p.Value}|{p.Rating}|{p.Age}|{p.Position}|{p.Potential}|{p.Wage}|{p.ContractLength}|{p.SquadStatus}|{p.TransferPrice}|{p.AvailableForTransfer}";
                            writer.WriteLine(playerLine);
                        }
                    }
                }
                Console.WriteLine("Game saved successfully.");
                // Display the full path so you know where the file is saved.
                Console.WriteLine("Save file location: " + Path.GetFullPath(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving game: " + ex.Message);
            }
        }

        // LoadGame restores the match counter, the user club, and each club's data.
        public static int LoadGame(League league, string filePath)
        {
            int matchCounter = 0;
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    // Read match counter.
                    string line = reader.ReadLine();
                    if (int.TryParse(line, out int counter))
                        matchCounter = counter;

                    // Read the user club line.
                    string userClubLine = reader.ReadLine();
                    string userClubName = "";
                    if (!string.IsNullOrEmpty(userClubLine) && userClubLine.StartsWith("UserClub|"))
                    {
                        string[] tokens = userClubLine.Split('|');
                        if (tokens.Length >= 2)
                            userClubName = tokens[1];
                    }

                    // Create a dictionary for quick club lookup.
                    Dictionary<string, Club> clubDict = league.Clubs.ToDictionary(c => c.Name, c => c);

                    // Read rest of the file.
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("Club|"))
                        {
                            // Club header format:
                            // Club|Name|Balance|Wins|Draws|Losses|GoalsFor|GoalsAgainst|Points|StartingEleven
                            string[] parts = line.Split('|');
                            if (parts.Length < 9)
                                continue;

                            string clubName = parts[1];
                            if (!clubDict.ContainsKey(clubName))
                                continue;

                            Club club = clubDict[clubName];

                            if (double.TryParse(parts[2], out double balance))
                                club.Balance = balance;
                            if (int.TryParse(parts[3], out int wins))
                                club.Wins = wins;
                            if (int.TryParse(parts[4], out int draws))
                                club.Draws = draws;
                            if (int.TryParse(parts[5], out int losses))
                                club.Losses = losses;
                            if (int.TryParse(parts[6], out int goalsFor))
                                club.GoalsFor = goalsFor;
                            if (int.TryParse(parts[7], out int goalsAgainst))
                                club.GoalsAgainst = goalsAgainst;
                            if (int.TryParse(parts[8], out int points))
                                club.Points = points;

                            string startingEleven = parts.Length >= 10 ? parts[9] : "";
                            // Clear current players and assignments.
                            club.Players.Clear();
                            club.PositionAssignments = new Dictionary<int, Player>();

                            // Read following Player lines for this club.
                            while ((line = reader.ReadLine()) != null && line.StartsWith("Player|"))
                            {
                                string[] pParts = line.Split('|');
                                if (pParts.Length < 12)
                                    continue;

                                string pName = pParts[1];
                                int pValue = int.TryParse(pParts[2], out int val) ? val : 0;
                                int pRating = int.TryParse(pParts[3], out int rat) ? rat : 0;
                                int pAge = int.TryParse(pParts[4], out int age) ? age : 0;
                                string pPosition = pParts[5];
                                int pPotential = int.TryParse(pParts[6], out int pot) ? pot : 0;
                                int pWage = int.TryParse(pParts[7], out int wage) ? wage : 0;
                                int pContractLength = int.TryParse(pParts[8], out int contract) ? contract : 0;
                                string pSquadStatus = pParts[9];
                                double pTransferPrice = double.TryParse(pParts[10], out double tp) ? tp : 0;
                                bool pAvailableForTransfer = bool.TryParse(pParts[11], out bool avail) ? avail : false;

                                // (Statistics are not saved in this example.)
                                Dictionary<string, int> stats = new Dictionary<string, int>();

                                Player player = new Player(pName, pValue, pRating, pAge, pPosition, pPotential, pWage, pContractLength, pSquadStatus, stats)
                                {
                                    TransferPrice = pTransferPrice,
                                    AvailableForTransfer = pAvailableForTransfer
                                };

                                club.AddPlayer(player);
                            }

                            // Reassign starting eleven based on saved names.
                            if (!string.IsNullOrWhiteSpace(startingEleven))
                            {
                                string[] startNames = startingEleven.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                int index = 0;
                                foreach (string name in startNames)
                                {
                                    Player found = club.Players.FirstOrDefault(p => p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
                                    if (found != null)
                                    {
                                        club.PositionAssignments[index] = found;
                                    }
                                    index++;
                                }
                            }
                        }
                    }

                    // After loading all clubs, set the user's club if saved.
                    if (!string.IsNullOrEmpty(userClubName) && clubDict.ContainsKey(userClubName))
                    {
                        league.UserClub = clubDict[userClubName];
                    }
                }
                Console.WriteLine("Game loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading game: " + ex.Message);
            }
            return matchCounter;
        }
    }
}
