using System;
using System.Drawing;
using System.Threading;

namespace GUI
{
    // The Program class is the entry point for the Football Manager Simulation.
    // It sets up the console properties, handles starting a new game or loading a game,
    // and then starts the main user interface loop.
    public class Program
    {
        public static void Main(string[] args)
        {
            // Set the console title.
            Console.Title = "Football Manager Simulation";

            // Attempt to set window and buffer size.
            try
            {
                Console.SetWindowSize(150, 40);
                Console.SetBufferSize(150, 40);
            }
            catch { /* Ignore errors if size cannot be set */ }

            Console.Clear();
            Console.WriteLine("Welcome to Football Manager Simulation!");
            Console.WriteLine("Press L to load a game or N to start a new game:");
            char choice = Char.ToUpper(Console.ReadKey(true).KeyChar);

            League league = new League();
            int matchCounter = 0;

            if (choice == 'L')
            {
                Console.Clear();
                Console.WriteLine("Enter the full path of your save file:");
                string filePath = Console.ReadLine();
                matchCounter = SaveLoadManager.LoadGame(league, filePath);
                // If the save file did not specify a user club, let the user choose one.
                if (league.UserClub == null)
                {
                    Console.WriteLine("Save file did not contain a user club. Please choose your club:");
                    league.UserClub = ChooseClub(league);
                }
            }
            else
            {
                league.UserClub = ChooseClub(league);
                Console.WriteLine("Starting a new game... Press any key to continue.");
                Console.ReadKey(true);
            }

            // Determine the user's club index.
            int userClubIndex = league.Clubs.IndexOf(league.UserClub);
            // Initialize the user interface with the league and the manager's club index.
            UserInterface ui = new UserInterface(league, userClubIndex);

            // Main game loop
            while (!ui.Closing)
            {
                ui.Update();
                ui.Draw();
                Thread.Sleep(50); // Adjust delay for smoother performance if needed.
            }
            Console.Clear();
            Console.WriteLine("Exiting game... Goodbye!");
        }
        // Helper method to choose a club if not provided by a save file.
        // Prompts the user to select a club from the list.
        private static Club ChooseClub(League league)
        {
            Console.Clear();
            Console.WriteLine("Select your club:");
            for (int i = 0; i < league.Clubs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {league.Clubs[i].Name}");
            }
            int selection = 0;
            while (true)
            {
                Console.Write("Enter the number of your club: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out selection) && selection >= 1 && selection <= league.Clubs.Count)
                {
                    break;
                }
                Console.WriteLine("Invalid selection. Please try again.");
            }
            return league.Clubs[selection - 1];
        }
    }
}
