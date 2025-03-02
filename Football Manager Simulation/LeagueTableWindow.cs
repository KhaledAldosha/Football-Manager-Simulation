using System;
using System.Drawing;
using System.Linq;

namespace GUI
{
    // The LeagueTableWindow displays the current league standings.
    // It sorts clubs by points and goal difference and shows their basic stats.
    public class LeagueTableWindow : Window
    {
        private League _league;        // Reference to the league data
        private int _managerClubIndex;   // Index of the user's club in the league

        // Constructor that initializes the league table window with a league and manager club index.
        public LeagueTableWindow(League league, int managerClubIndex, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _league = league;
            _managerClubIndex = managerClubIndex;
        }
        // Draws the league table: first draws the window border, then the table header,
        // and finally each club's stats, highlighting the manager's club.
        public override void Draw(bool active)
        {
            base.Draw(active);
            if (!_visible)
                return;

            // Sort clubs by points, then goal difference, then goals for.
            var sortedClubs = _league.Clubs
                .OrderByDescending(c => c.Points)
                .ThenByDescending(c => c.GoalsFor - c.GoalsAgainst)
                .ThenByDescending(c => c.GoalsFor)
                .ToList();

            // Set header position and color
            Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y + 2);
            Console.ForegroundColor = ConsoleColor.Yellow;
            // Header format: Name, W, D, L, GF, GA, Pts
            Console.WriteLine($"{"Name",-22}{"W",3}{"D",3}{"L",3}{"GF",4}{"GA",4}{"Pts",4}");
            Console.ResetColor();

            // List each club with its stats
            for (int i = 0; i < sortedClubs.Count; i++)
            {
                Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y + 3 + i);
                // If this is the manager's club, highlight it
                if (sortedClubs[i] == _league.Clubs[_managerClubIndex])
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Gray;

                Console.WriteLine(
                    $"{sortedClubs[i].Name,-22}" +
                    $"{sortedClubs[i].Wins,3}" +
                    $"{sortedClubs[i].Draws,3}" +
                    $"{sortedClubs[i].Losses,3}" +
                    $"{sortedClubs[i].GoalsFor,4}" +
                    $"{sortedClubs[i].GoalsAgainst,4}" +
                    $"{sortedClubs[i].Points,4}"
                );
            }
            Console.ResetColor();
        }
    }
}
