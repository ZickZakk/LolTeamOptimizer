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
            //GenerateRandomDatabase();

            TestTimes();

            // TestOptimization();

            Console.ReadKey();
        }

        private static void TestTimes()
        {
            var database = new Database("Random_Database");

            var random = new Random();

            GetRunningTime(database, random, 25);
            GetRunningTime(database, random, 50);
            GetRunningTime(database, random, 100);
            GetRunningTime(database, random, 150);
            GetRunningTime(database, random, 250);
            GetRunningTime(database, random, 500);
            GetRunningTime(database, random, 1000);
        }

        private static void GetRunningTime(Database database, Random random, int anzahlChampions)
        {
            var stopWatch = new Stopwatch();

            Console.WriteLine("Teste {0} : ", anzahlChampions);

            var time = 0.0;

            var champions = database.Champions.Take(anzahlChampions).ToList();
            for (int j = 0; j < 5; j++)
            {
                var enemies = GetRandomEnemies(champions, random);
                var state = new PickingState(5) { EnemyPicks = enemies };

                for (int i = 0; i < 2; i++)
                {
                    stopWatch.Start();
                    var optimizer = new BranchAndBoundCspOptimizer(champions);
                    optimizer.CalculateOptimalePicks(state);
                    stopWatch.Stop();

                    time += stopWatch.Elapsed.TotalMilliseconds;
                    stopWatch.Reset();
                }
            }

            Console.WriteLine("Ergebnis : {0}", time / 10.0);
        }

        private static IList<Champion> GetRandomEnemies(IList<Champion> champions, Random random)
        {
            var enemies = new List<Champion>(5);

            var min = champions.Min(x => x.Id);
            var max = champions.Max(x => x.Id) + 1;

            for (int i = 0; i < 5; i++)
            {
                var nextId = random.Next(min, max);
                enemies.Add(champions.Single(champ => champ.Id == nextId));
            }

            return enemies;
        }

        private static void GenerateRandomDatabase()
        {
            var database = new Database("Random_Database");
            database.Configuration.AutoDetectChangesEnabled = false;

            // Generate Champs

            for (int i = 0; i < 1000; i++)
            {
                database = AddToContext(database, new Champion { Name = i.ToString() }, i, 100, true);
            }
            database.SaveChanges();

            var min = database.Champions.Min(x => x.Id);
            var max = database.Champions.Max(x => x.Id) + 1;

            var random = new Random();
            var count = 0;
            foreach (var champion in database.Champions.ToList())
            {
                for (int i = 0; i < 200; i++)
                {
                    var otherChamp = database.Champions.Find(random.Next(min, max));
                    count++;
                    database = AddToContext(database, new GoesWellWith { Champion = champion, OtherChampion = otherChamp }, count, 100, true);
                }

                for (int i = 0; i < 200; i++)
                {
                    var otherChamp = database.Champions.Find(random.Next(min, max));

                    count++;
                    database = AddToContext(database, new IsStrongAgainst { Champion = champion, OtherChampion = otherChamp }, count, 100, true);
                }

                for (int i = 0; i < 200; i++)
                {
                    var otherChamp = database.Champions.Find(random.Next(min, max));

                    count++;
                    database = AddToContext(database, new IsWeakAgainst { Champion = champion, OtherChampion = otherChamp }, count, 100, true);
                }

                Console.WriteLine(count / 600.0);
            }

            database.SaveChanges();

            Console.WriteLine("Fertig!");
        }

        private static Database AddToContext<T>(Database context, T entity, int count, int commitCount, bool recreateContext) where T : class
        {
            context.Set<T>().Add(entity);

            if (count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new Database("Random_Database");
                    context.Configuration.AutoDetectChangesEnabled = false;
                }
            }

            return context;
        }

        private static void TestOptimization()
        {
            var dataBase = new Database();

            var thresh = dataBase.Champions.Single(champ => champ.Name == "thresh");
            var lucian = dataBase.Champions.Single(champ => champ.Name == "lucian");
            var maokai = dataBase.Champions.Single(champ => champ.Name == "maokai");
            var jarvan = dataBase.Champions.Single(champ => champ.Name == "jarvan-iv");
            var orianna = dataBase.Champions.Single(champ => champ.Name == "orianna");

            var enemies = new List<Champion> { thresh, maokai, lucian, jarvan, orianna };

            var state = new PickingState(enemies.Count) { EnemyPicks = enemies };

            ITeamOptimizer optimizer = new BranchAndBoundCspOptimizer(dataBase.Champions.ToList());
            var watch = new Stopwatch();
            watch.Start();
            var result = optimizer.CalculateOptimalePicks(state);
            watch.Stop();

            var line = string.Join(", ", result.Team.Select(champ => champ.Name));

            Console.WriteLine("Best Team ({0}): " + line, result.TeamValue);
            Console.WriteLine("Time: {0}", watch.Elapsed.TotalMilliseconds);
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
        }

        #endregion
    }
}