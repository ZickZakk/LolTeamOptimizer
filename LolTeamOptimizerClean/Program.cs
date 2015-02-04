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

            for (int championsCount = 50; championsCount <= 1000; championsCount += 50)
            {
                Console.WriteLine("#################");
                Console.WriteLine("Generate relations for {0} Champions", championsCount);

                var relations = RelationsGenerator.GenerateRelations(championsCount);

                var bnbOptimizer = new BranchAndBoundOptimizer(relations, TeamSize);
                var soOptimizer = new SwitchOutOptimizer(relations, TeamSize);

                var stopWatch = new Stopwatch();

                Console.WriteLine("Teste {0} : ", championsCount);

                var timebnb = 0.0;
                var timeso = 0.0;

                var abweichung = 0;

                var checkCalculator = new TeamValueCalculator(relations);

                var random = new Random();

                for (var j = 0; j < 10; j++)
                {
                    var enemies = GetRandomEnemies(championsCount, random, TeamSize);

                    for (var i = 0; i < 2; i++)
                    {
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