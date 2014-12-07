#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using LolTeamOptimizer.Optimizers.Common;
using LolTeamOptimizer.Optimizers.Implementations;
using LolTeamOptimizer.Optimizers.Interfaces;

#endregion

namespace LolTeamOptimizer
{

    #region using

    #endregion

    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            // GatherData();

            TestOptimization();
        }

        private static void TestOptimization()
        {
            var dataBase = new Database();

            var thresh = dataBase.Champions.Single(champ => champ.Name == "thresh");
            var lucian = dataBase.Champions.Single(champ => champ.Name == "lucian");
            var maokai = dataBase.Champions.Single(champ => champ.Name == "maokai");
            var jarvan = dataBase.Champions.Single(champ => champ.Name == "jarvan-iv");
            var orianna = dataBase.Champions.Single(champ => champ.Name == "orianna");

            var enemies = new List<Champion> { thresh, maokai, lucian, jarvan/*, orianna*/ };

            var state = new PickingState(enemies.Count) { EnemyPicks = enemies };

            ITeamOptimizer optimizer = new BranchAndBoundCspOptimizer();
            var watch = new Stopwatch();
            watch.Start();
            var result = optimizer.CalculateOptimalePicks(state);
            watch.Stop();

            var line = string.Join(", ", result.Team.Select(champ => champ.Name));

            Console.WriteLine("Best Team ({0}): " + line, result.TeamValue);
            Console.WriteLine("Time: {0}", watch.Elapsed.TotalMilliseconds);
            Console.ReadKey();
        }

        private static void GatherData()
        {
            Console.WriteLine("### LoL-Counter Database Sync ###");

            var dataBase = new Database();

            Console.WriteLine("### Clearing Database ###");

            dataBase.Database.ExecuteSqlCommand("DELETE FROM [IsStrongAgainstSet]");
            dataBase.Database.ExecuteSqlCommand("DELETE FROM [IsWeakAgainstSet]");
            dataBase.Database.ExecuteSqlCommand("DELETE FROM [GoesWellWithSet]");
            dataBase.Database.ExecuteSqlCommand("DELETE FROM [Champions]");

            Console.WriteLine("### Gathering Champions ###");

            var champions = HtmlService.GatherChapionNames().ToList();

            foreach (var champion in champions)
            {
                dataBase.Champions.Add(new Champion { Name = champion });
            }

            dataBase.SaveChanges();

            Console.WriteLine("### Gathering IsStrongAgainst ###");

            foreach (var champion in dataBase.Champions)
            {
                var relations = HtmlService.GatherChampionRelations(champion.Name, "/strong");

                // Combine Champions, which appeard two times in the list and make IsStrong
                var isStrongRelations = relations.GroupBy(relation => relation.ChampionName).Select(group => new IsStrongAgainst { Champion = champion, OtherChampion = dataBase.Champions.Single(champ => champ.Name.Equals(@group.Key)), Rating = Convert.ToInt32(@group.Average(relation => relation.Value)) }).ToList();

                // Add Relations
                dataBase.IsStrongAgainstSet.AddRange(isStrongRelations);

                relations = HtmlService.GatherChampionRelations(champion.Name, "/weak");

                // Combine Champions, which appeard two times in the list and make IsStrong
                var isWeakRelations = relations.GroupBy(relation => relation.ChampionName).Select(group => new IsWeakAgainst { Champion = champion, OtherChampion = dataBase.Champions.Single(champ => champ.Name.Equals(@group.Key)), Rating = Convert.ToInt32(@group.Average(relation => relation.Value)) }).ToList();

                // Add Relations
                dataBase.IsWeakAgainstSet.AddRange(isWeakRelations);

                relations = HtmlService.GatherChampionRelations(champion.Name, "/good");

                // Combine Champions, which appeard two times in the list and make IsStrong
                var goesWellRelations = relations.GroupBy(relation => relation.ChampionName).Select(group => new GoesWellWith { Champion = champion, OtherChampion = dataBase.Champions.Single(champ => champ.Name.Equals(@group.Key)), Rating = Convert.ToInt32(@group.Average(relation => relation.Value)) }).ToList();

                // Add Relations
                dataBase.GoesWellWithSet.AddRange(goesWellRelations);
            }

            Console.WriteLine("### Saving... ###");
            dataBase.SaveChanges();

            Console.WriteLine("### Done! ###");
            Console.ReadKey();
        }

        #endregion
    }
}