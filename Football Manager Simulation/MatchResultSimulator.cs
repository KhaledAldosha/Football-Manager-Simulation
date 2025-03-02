using System;

namespace GUI
{
    public static class MatchResultSimulator
    {
        /// Generates a Poisson-distributed random number with the given lambda.
        public static int Poisson(double lambda, Random random)
        {
            double L = Math.Exp(-lambda);
            int k = 0;
            double p = 1.0;
            do
            {
                k++;
                p *= random.NextDouble();
            } while (p > L);
            return k - 1;
        }
        /// Simulates a match between a home club and an away club.
        /// The match result is recorded in both clubs.
        public static void SimulateMatch(Club home, Club away, Random random)
        {
            // Example lambdas – these could be modified to reflect club strength.
            double lambdaHome = 1.5;
            double lambdaAway = 1.0;
            int homeGoals = Poisson(lambdaHome, random);
            int awayGoals = Poisson(lambdaAway, random);

            // Update each club's stats.
            home.RecordResult(homeGoals, awayGoals);
            away.RecordResult(awayGoals, homeGoals);

            // Optionally, output the result to the console (for debugging).
            Console.WriteLine($"{home.Name} {homeGoals} - {awayGoals} {away.Name}");
        }
    }
}
