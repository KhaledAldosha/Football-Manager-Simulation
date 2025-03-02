using System;
using System.Collections.Generic;
using System.Drawing;

namespace GUI
{
    // The MatchesWindow provides the user interface for playing matches.
    // It shows upcoming fixtures and handles playing the match simulation.
    public class MatchesWindow : Window
    {
        private League _league;           // Reference to league data
        private Club _userTeam;           // The user's club
        private List<Fixture> _fixtures;  // List of fixtures for the user team
        private int _currentFixtureIndex; // Index of the next fixture to be played
        private int _matchCounter;        // How many matches have been played
        private Random _rnd = new Random();

        // Constructor sets up the matches window with the league, user's team, and window properties.
        public MatchesWindow(League league, Club userTeam, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _league = league;
            _userTeam = userTeam;
            GenerateFixtures();
            _currentFixtureIndex = 0;
            _matchCounter = 0;
        }
        // Generates fixtures for the user's team by pairing it with other clubs.
        // If odd number of clubs, adds a dummy club to allow pairing.
        private void GenerateFixtures()
        {
            List<Club> clubs = new List<Club>(_league.Clubs);
            bool hasDummy = false;
            if (clubs.Count % 2 != 0)
            {
                clubs.Add(new Club("Dummy", 0));
                hasDummy = true;
            }
            int n = clubs.Count;
            int rounds = n - 1;
            int matchesPerRound = n / 2;
            List<Fixture> singleRound = new List<Fixture>();
            List<Club> rotation = new List<Club>(clubs);
            for (int round = 0; round < rounds; round++)
            {
                for (int match = 0; match < matchesPerRound; match++)
                {
                    int homeIndex = match;
                    int awayIndex = n - 1 - match;
                    Club home = rotation[homeIndex];
                    Club away = rotation[awayIndex];
                    if (home.Name != "Dummy" && away.Name != "Dummy")
                    {
                        singleRound.Add(new Fixture(home, away));
                    }
                }
                Club last = rotation[n - 1];
                rotation.RemoveAt(n - 1);
                rotation.Insert(1, last);
            }
            List<Fixture> fullFixtures = new List<Fixture>();
            fullFixtures.AddRange(singleRound);
            foreach (var f in singleRound)
            {
                fullFixtures.Add(new Fixture(f.AwayTeam, f.HomeTeam));
            }
            if (hasDummy)
                clubs.RemoveAll(c => c.Name == "Dummy");
            // Filter fixtures to those involving the user's team.
            _fixtures = fullFixtures.FindAll(f => f.HomeTeam == _userTeam || f.AwayTeam == _userTeam);
            _currentFixtureIndex = 0;
        }
        // Update processes key input: if 'P' is pressed, plays the next match.
        // Also handles ESC to return to main menu.
        public override void Update()
        {
            var key = UserInterface.Input.Key;
            if (key == ConsoleKey.Escape)
            {
                _currentAction = InterfaceAction.ReturnToMainMenu;
                return;
            }
            if (key == ConsoleKey.P)
            {
                PlayMatch();
            }
        }
        // Draws the matches window UI, including next fixture info and match count.
        public override void Draw(bool active)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Play Match");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Press P to play the next match.");
            Console.WriteLine($"Matches Played: {_matchCounter} / {_fixtures.Count}");
            if (_currentFixtureIndex < _fixtures.Count)
            {
                Fixture next = _fixtures[_currentFixtureIndex];
                Console.WriteLine($"Next Fixture: {next.HomeTeam.Name} vs {next.AwayTeam.Name}");
            }
            else
            {
                Console.WriteLine("No more fixtures. Season Over.");
            }
            Console.WriteLine();
            Console.WriteLine("Press ESC to return to the main menu.");
        }

        // Plays the next fixture for the user's team.
        // Starts the live match simulation and then updates league state.
        private void PlayMatch()
        {
            if (_currentFixtureIndex >= _fixtures.Count)
            {
                Console.Clear();
                Console.WriteLine("No more matches to play.");
                Console.ReadKey(true);
                return;
            }

            Fixture fixture = _fixtures[_currentFixtureIndex];
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Playing match: {fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name}");
            Console.ResetColor();
            Console.WriteLine("Press any key to start live match simulation...");
            Console.ReadKey(true);

            // Determine the opponent relative to the user's team.
            Club opponent = (fixture.HomeTeam == _userTeam) ? fixture.AwayTeam : fixture.HomeTeam;
            new Match(_userTeam, opponent).Start();

            // Optionally simulate other matches for the match day.
            _league.SimulateRemainingMatchesForMatchDay();

            // AI clubs try to buy players after a match.
            _league.AIBuyPlayers();

            _matchCounter++;
            _currentFixtureIndex++;

            // Every 5 matches, AI also tries to sell players.
            if (_matchCounter % 5 == 0)
            {
                _league.AISellPlayers();
                RefreshTransferMarket();
            }
        }

        // Refreshes the transfer market with new players.
        private void RefreshTransferMarket()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Refreshing Transfer Market with new players...");
            Console.ResetColor();
            Console.WriteLine("Transfer Market refreshed.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }

    // Represents a single fixture between two clubs.
    public class Fixture
    {
        public Club HomeTeam { get; }
        public Club AwayTeam { get; }

        public Fixture(Club homeTeam, Club awayTeam)
        {
            HomeTeam = homeTeam;
            AwayTeam = awayTeam;
        }
    }
}
