using System;

namespace GUI
{
    // The MatchResultSimulator simulates the outcome of a match
    // using a Poisson distribution to generate goal counts.
    public static class MatchResultSimulator
    {
        // Generates a Poisson-distributed random number based on the given lambda.
        // This is used to simulate the number of goals.
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
        // Simulates a match between a home club and an away club.
        // Uses Poisson-distributed goal counts to determine the final score,
        // then records the result in both clubs.
        public static void SimulateMatch(Club home, Club away, Random random)
        {
            // These lambda values could be adjusted based on team strength
            double lambdaHome = 1.5;
            double lambdaAway = 1.0;
            int homeGoals = Poisson(lambdaHome, random);
            int awayGoals = Poisson(lambdaAway, random);

            // Update clubs with match result stats.
            home.RecordResult(homeGoals, awayGoals);
            away.RecordResult(awayGoals, homeGoals);

            // Optionally, output the match result to the console.
            Console.WriteLine($"{home.Name} {homeGoals} - {awayGoals} {away.Name}");
        }
    }
}
