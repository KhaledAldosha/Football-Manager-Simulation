using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GUI
{
    public static class TeamFactory
    {
        // Prepares the team for match simulation by ensuring a formation is set
        // and players are assigned to formation positions.
        public static void PrepareTeam(Club club)
        {
            if (club.SelectedFormation == null)
            {
                // Assign a default 4-3-3 formation.
                club.SelectedFormation = new Formation("4-3-3", new List<FormationPosition>
                {
                    new FormationPosition("GK", 50, 20),
                    new FormationPosition("LB", 10, 40),
                    new FormationPosition("CB", 30, 40),
                    new FormationPosition("CB", 70, 40),
                    new FormationPosition("RB", 90, 40),
                    new FormationPosition("CM", 30, 60),
                    new FormationPosition("CAM", 50, 60),
                    new FormationPosition("CM", 70, 60),
                    new FormationPosition("RW", 90, 60),
                    new FormationPosition("ST", 50, 80),
                    new FormationPosition("LW", 10, 60)
                });
            }
            if (club.PositionAssignments == null || club.PositionAssignments.Count < club.SelectedFormation.Positions.Count)
            {
                club.PositionAssignments = AutoAssignPlayers(club);
            }
        }

        // Auto-assigns players to each formation position based on matching positions and rating.
        private static Dictionary<int, Player> AutoAssignPlayers(Club club)
        {
            var assignments = new Dictionary<int, Player>();
            var formation = club.SelectedFormation;
            for (int i = 0; i < formation.Positions.Count; i++)
            {
                string pos = formation.Positions[i].PositionName;
                var available = club.Players
                    .Where(p => p.Position.Equals(pos, StringComparison.OrdinalIgnoreCase)
                                && !assignments.Values.Contains(p))
                    .ToList();
                if (available.Count == 0)
                {
                    // If no player matches the position, choose any unassigned player.
                    available = club.Players.Where(p => !assignments.Values.Contains(p)).ToList();
                }
                // Pick the highest rated player.
                var sorted = available.OrderByDescending(p => p.Rating).ToList();
                if (sorted.Count > 0)
                {
                    assignments[i] = sorted[0];
                }
            }
            return assignments;
        }

        // Returns the starting eleven and bench from a club’s squad.
        public static (List<Player> starting, List<Player> bench) GetTeamFromClub(Club club)
        {
            PrepareTeam(club);
            var starting = club.PositionAssignments.Values.ToList();
            var bench = club.Players.Where(p => !starting.Contains(p)).ToList();
            return (starting, bench);
        }
    }
}
