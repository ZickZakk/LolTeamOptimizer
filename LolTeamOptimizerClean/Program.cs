#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;

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

            foreach (var championsCount in championsCountsToTest)
            {
                Console.WriteLine("#################");
                Console.WriteLine("Generate relations for {0} Champions", championsCount);

                Console.WriteLine("Teste {0} : ", championsCount);

                var timebnb = 0.0;
                var timeso = 0.0;
                var abweichung = 0;

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

                        timebnb += stopWatch.Elapsed.TotalMilliseconds;
                        stopWatch.Reset();

                        stopWatch.Start();
                        var resulso = soOptimizer.FindBestTeam(enemies);
                        stopWatch.Stop();

                        timeso += stopWatch.Elapsed.TotalMilliseconds;
                        stopWatch.Reset();

                        abweichung += checkCalculator.CalculateTeamValue(resultbnb, enemies) - checkCalculator.CalculateTeamValue(resulso, enemies);

                    }
                }

                Console.WriteLine("Result for {0} Champions: B&B: {1}", championsCount, timebnb / 20.0);
                Console.WriteLine("Result for {0} Champions: SWO: {1} | Abweichung: {2}", championsCount, timeso / 20.0, abweichung / 20.0);
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