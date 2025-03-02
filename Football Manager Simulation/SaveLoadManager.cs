using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GUI
{
    // Handles saving and loading of the game state to/from a file.
    // This includes clubs, players, match counter, etc.
    public static class SaveLoadManager
    {
        // Writes out the match counter, user club name, and then each club's data
        // (plus players) to a text file.
        public static void SaveGame(int matchCounter, League league, string filePath = "savegame.txt")
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // First line: match counter
                    writer.WriteLine(matchCounter);

                    // Second line: user club name
                    string userClubName = (league.UserClub != null) ? league.UserClub.Name : "";
                    writer.WriteLine("UserClub|" + userClubName);

                    // Then for each club, we write a line with the club data
                    foreach (Club club in league.Clubs)
                    {
                        // Prepare the 'startingEleven' string from PositionAssignments
                        string startingEleven = "";
                        if (club.PositionAssignments != null && club.PositionAssignments.Count > 0)
                        {
                            var sortedAssignments = club.PositionAssignments.OrderBy(kv => kv.Key);
                            startingEleven = string.Join(",", sortedAssignments.Select(kv => kv.Value.Name));
                        }

                        // Club line format:
                        // Club|Name|Balance|Wins|Draws|Losses|GoalsFor|GoalsAgainst|Points|StartingEleven
                        string clubLine = $"Club|{club.Name}|{club.Balance}|{club.Wins}|{club.Draws}|{club.Losses}|{club.GoalsFor}|{club.GoalsAgainst}|{club.Points}|{startingEleven}";
                        writer.WriteLine(clubLine);

                        // Then for each player in that club, we write a Player line
                        // Format: Player|Name|Value|Rating|Age|Position|Potential|Wage|ContractLength|SquadStatus|TransferPrice|AvailableForTransfer
                        foreach (Player p in club.Players)
                        {
                            string playerLine = $"Player|{p.Name}|{p.Value}|{p.Rating}|{p.Age}|{p.Position}|{p.Potential}|{p.Wage}|{p.ContractLength}|{p.SquadStatus}|{p.TransferPrice}|{p.AvailableForTransfer}";
                            writer.WriteLine(playerLine);
                        }
                    }
                }
                Console.WriteLine("Game saved successfully.");
                Console.WriteLine("Save file location: " + Path.GetFullPath(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving game: " + ex.Message);
            }
        }

        // Reads the match counter, user club name, then each club and its players
        // from a text file. Returns the match counter as an int.
        public static int LoadGame(League league, string filePath)
        {
            int matchCounter = 0;
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    // First line => match counter
                    string line = reader.ReadLine();
                    if (int.TryParse(line, out int counter))
                        matchCounter = counter;

                    // Second line => user club name
                    string userClubLine = reader.ReadLine();
                    string userClubName = "";
                    if (!string.IsNullOrEmpty(userClubLine) && userClubLine.StartsWith("UserClub|"))
                    {
                        string[] tokens = userClubLine.Split('|');
                        if (tokens.Length >= 2)
                            userClubName = tokens[1];
                    }

                    // Build a dictionary for quick club lookup
                    Dictionary<string, Club> clubDict = league.Clubs.ToDictionary(c => c.Name, c => c);

                    // Now read each subsequent line
                    while ((line = reader.ReadLine()) != null)
                    {
                        // If it's a club line
                        if (line.StartsWith("Club|"))
                        {
                            // Format: Club|Name|Balance|Wins|Draws|Losses|GoalsFor|GoalsAgainst|Points|StartingEleven
                            string[] parts = line.Split('|');
                            if (parts.Length < 9)
                                continue;

                            string clubName = parts[1];
                            if (!clubDict.ContainsKey(clubName))
                                continue; // If the club isn't in the league

                            Club club = clubDict[clubName];

                            // Parse each piece of data
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

                            // Possibly read the 'startingEleven' string
                            string startingEleven = parts.Length >= 10 ? parts[9] : "";
                            // Clear existing players and assignments
                            club.Players.Clear();
                            club.PositionAssignments = new Dictionary<int, Player>();

                            // After reading this 'Club|' line, we read subsequent 'Player|' lines
                            // until we reach a new 'Club|' or end of file
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

                                // We skip statistics in this example
                                Dictionary<string, int> stats = new Dictionary<string, int>();

                                // Create a new Player object
                                Player player = new Player(
                                    pName,
                                    pValue,
                                    pRating,
                                    pAge,
                                    pPosition,
                                    pPotential,
                                    pWage,
                                    pContractLength,
                                    pSquadStatus,
                                    stats)
                                {
                                    TransferPrice = pTransferPrice,
                                    AvailableForTransfer = pAvailableForTransfer
                                };

                                // Add to the club
                                club.AddPlayer(player);
                            }

                            // Reconstruct startingEleven if present
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

                            // If 'line' isn't a 'Player|' line anymore, it might be the next 'Club|' or null
                            // We continue the loop from there
                        }
                    }

                    // After reading all clubs, set userClub if we have a match
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
