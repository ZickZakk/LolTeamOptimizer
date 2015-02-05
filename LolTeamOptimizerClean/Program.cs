#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using LolTeamOptimizerClean.Calculators;
using LolTeamOptimizerClean.Optimizers;
using LolTeamOptimizerClean.Relations;

#endregion

namespace LolTeamOptimizerClean
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            const int TeamSize = 5;

            var championsCountsToTest = new List<int> { 25, 50, 75, 100, 125, 150, 200, 250, 300, 350, 400, 450, 500, 600, 700, 800, 900, 1000, 1250, 1500 };

            var random = new Random();
            var stopWatch = new Stopwatch();
            var fileName = DateTime.Now.ToString().Replace(" ", "_").Replace(":", ".") + ".csv";

            foreach (var championsCount in championsCountsToTest)
            {
                Console.WriteLine("#################");
                Console.WriteLine("Generate relations for {0} Champions", championsCount);

                Console.WriteLine("Teste {0} : ", championsCount);

                File.AppendAllText(fileName, championsCount + Environment.NewLine);
                File.AppendAllText(fileName, String.Join(";", "BnB", "SwO", "Abweichung") + Environment.NewLine);

                for (var i = 0; i < 2; i++)
                {
                    var relations = RelationsGenerator.GenerateRelations(championsCount);

                    var bnbOptimizer = new BranchAndBoundOptimizer(relations, TeamSize);
                    var soOptimizer = new SwitchOutOptimizer(relations, TeamSize);

                    var checkCalculator = new TeamValueCalculator(relations);

                    for (var j = 0; j < 10; j++)
                    {
                        var enemies = GetRandomEnemies(championsCount, random, TeamSize);

                        stopWatch.Start();
                        var resultbnb = bnbOptimizer.FindBestTeam(enemies);
                        stopWatch.Stop();

                        var timebnb = stopWatch.Elapsed.TotalMilliseconds;
                        stopWatch.Reset();

                        stopWatch.Start();
                        var resulso = soOptimizer.FindBestTeam(enemies);
                        stopWatch.Stop();

                        var timeso = stopWatch.Elapsed.TotalMilliseconds;
                        stopWatch.Reset();

                        var abweichung = checkCalculator.CalculateTeamValue(resultbnb, enemies) - checkCalculator.CalculateTeamValue(resulso, enemies);

                        File.AppendAllText(fileName, String.Join(";", timebnb, timeso, abweichung) + Environment.NewLine);
                    }
                }

                Console.WriteLine("Done!");
            }

            Console.ReadKey();
        }

        private static IList<int> GetRandomEnemies(int championCount, Random random, int teamSize)
        {
            var enemies = new List<int>(teamSize);

            for (var i = 0; i < teamSize; i++)
            {
                enemies.Add(random.Next(0, championCount));
            }

            return enemies;
        }
    }
}