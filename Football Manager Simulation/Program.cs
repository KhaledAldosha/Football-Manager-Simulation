using System;
using System.Drawing;
using System.Threading;

namespace GUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Football Manager Simulation";
            try
            {
                Console.SetWindowSize(150, 40);
                Console.SetBufferSize(150, 40);
            }
            catch { }

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
                // If the save file contained a user club, resume the game normally.
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

            int userClubIndex = league.Clubs.IndexOf(league.UserClub);
            UserInterface ui = new UserInterface(league, userClubIndex);

            // Main loop—no clearing per frame.
            while (!ui.Closing)
            {
                ui.Update();
                ui.Draw();
                Thread.Sleep(50); // Adjust delay as needed for smooth performance.
            }
            Console.Clear();
            Console.WriteLine("Exiting game... Goodbye!");
        }

        // This method is only used if the save file doesn't include a user club.
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
