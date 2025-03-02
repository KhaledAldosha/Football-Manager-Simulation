using System;
using System.Drawing;
using System.Linq;

namespace GUI
{
    public class LeagueTableWindow : Window
    {
        private League _league;
        private int _managerClubIndex;

        public LeagueTableWindow(League league, int managerClubIndex, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _league = league;
            _managerClubIndex = managerClubIndex;
        }

        public override void Draw(bool active)
        {
            base.Draw(active);
            if (!Visible)
                return;

            // Sort clubs by Points (desc), then Goal Difference (desc), then Goals For (desc).
            var sortedClubs = _league.Clubs
                .OrderByDescending(c => c.Points)
                .ThenByDescending(c => c.GoalsFor - c.GoalsAgainst)
                .ThenByDescending(c => c.GoalsFor)
                .ToList();

            // Print header
            Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y + 2);
            Console.ForegroundColor = ConsoleColor.Yellow;
            // Using string interpolation with alignment specifiers for neat columns.
            // -22 means left-justify within 22 chars; 3 or 4 means right-justify within 3 or 4 chars, etc.
            Console.WriteLine($"{"Name",-22}{"W",3}{"D",3}{"L",3}{"GF",4}{"GA",4}{"Pts",4}");
            Console.ResetColor();

            // Print each club row
            for (int i = 0; i < sortedClubs.Count; i++)
            {
                Console.SetCursorPosition(_rectangle.X + 2, _rectangle.Y + 3 + i);

                // Highlight the manager’s club
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
