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

    using System.Data.Entity.Core.Metadata.Edm;

    #endregion

    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            // GatherData();
            //GenerateRandomDatabase(25);
            //GenerateRandomDatabase(50);
            //GenerateRandomDatabase(75);
            //GenerateRandomDatabase(100);
            //GenerateRandomDatabase(200);
            //GenerateRandomDatabase(300);
            //GenerateRandomDatabase(400);
            //GenerateRandomDatabase(500);
            //GenerateRandomDatabase(750);

            // StatisticalAnalysis();

            TestTimes();

            // TestOptimization();

            Console.ReadKey();
        }

        private static void StatisticalAnalysis()
        {
            var database = new Database();

            var doubleRelation = Enumerable.Count(database.GoesWellWithSet, goesWellWith => database.GoesWellWithSet.Any(rel => rel.OtherChampion.Id == goesWellWith.Champion.Id && rel.Champion.Id == goesWellWith.OtherChampion.Id));

            double ergebnis = doubleRelation / Convert.ToDouble(database.GoesWellWithSet.Count());

            Console.WriteLine("Prozentzahl der doppelten Verbidnungen bei vs: {0}", ergebnis);
        }

        private static void TestTimes()
        {
            var random = new Random();

            GetRunningTime(random, 25);
            GetRunningTime(random, 50);
            GetRunningTime(random, 75);
            GetRunningTime(random, 100);
            GetRunningTime(random, 200);
            GetRunningTime(random, 300);
            GetRunningTime(random, 400);
            GetRunningTime(random, 500);
            //GetRunningTime(random, 750);
        }

        private static void GetRunningTime(Random random, int anzahlChampions)
        {
            const int TeamSize = 5;
            var database = new Database("Random_Database_" + anzahlChampions); 
            var stopWatch = new Stopwatch();

            Console.WriteLine("Teste {0} : ", anzahlChampions);

            var time = 0.0;

            var champions = database.Champions.Take(anzahlChampions).ToList();
            var optimizer = new BranchAndBoundCspOptimizer(champions);

            for (int j = 0; j < 10; j++)
            {
                var enemies = GetRandomEnemies(champions, random, TeamSize);
                var state = new PickingState(TeamSize) { EnemyPicks = enemies };

                for (int i = 0; i < 2; i++)
                {
                    stopWatch.Start();
                    optimizer.CalculateOptimalePicks(state);
                    stopWatch.Stop();

                    time += stopWatch.Elapsed.TotalMilliseconds;
                    stopWatch.Reset();
                }
            }

            Console.WriteLine("Ergebnis : {0}", time / 20.0);
        }

        private static IList<Champion> GetRandomEnemies(IList<Champion> champions, Random random, int teamSize)
        {
            var enemies = new List<Champion>(teamSize);

            var min = champions.Min(x => x.Id);
            var max = champions.Max(x => x.Id) + 1;

            for (int i = 0; i < teamSize; i++)
            {
                var nextId = random.Next(min, max);
                enemies.Add(champions.Single(champ => champ.Id == nextId));
            }

            return enemies;
        }

        private static void GenerateRandomDatabase(int anz)
        {
            const double SynergyDoubleProbality = 0.8;
            const double StrenghtDoubleProbality = 0.6;
            const double WeaknessDoubleProbality = 0.2;

            var database = InitializeDatabase(anz);

            GenerateCahmpions(anz, database);

            var min = database.Champions.Min(x => x.Id);
            var max = database.Champions.Max(x => x.Id) + 1;

            var random = new Random();
            var doubleRelationRandom = new Random();

            var relations = anz / 5;
            foreach (var champion in database.Champions.ToList())
            {
                for (int i = 0; i < relations; i++)
                {
                    var otherChamp = database.Champions.Find(random.Next(min, max));
                    database.GoesWellWithSet.Add(new GoesWellWith { Champion = champion, OtherChampion = otherChamp });
                    
                    if (doubleRelationRandom.NextDouble() <= SynergyDoubleProbality)
                    {
                        database.GoesWellWithSet.Add(new GoesWellWith { Champion = otherChamp, OtherChampion = champion });
                        i++;
                    }
                }

                for (int i = 0; i < relations; i++)
                {
                    var otherChamp = database.Champions.Find(random.Next(min, max));

                    database.IsStrongAgainstSet.Add(new IsStrongAgainst { Champion = champion, OtherChampion = otherChamp });

                    if (doubleRelationRandom.NextDouble() <= StrenghtDoubleProbality)
                    {
                        database.IsWeakAgainstSet.Add(new IsWeakAgainst { Champion = otherChamp, OtherChampion = champion });
                        i++;
                    }
                }

                for (int i = 0; i < relations; i++)
                {
                    var otherChamp = database.Champions.Find(random.Next(min, max));

                    database.IsWeakAgainstSet.Add(new IsWeakAgainst { Champion = champion, OtherChampion = otherChamp });

                    if (doubleRelationRandom.NextDouble() <= WeaknessDoubleProbality)
                    {
                        database.IsStrongAgainstSet.Add(new IsStrongAgainst { Champion = otherChamp, OtherChampion = champion });
                        i++;
                    }
                }

                database.SaveChanges();
            }

            Console.WriteLine("Fertig mit {0}!", anz);
        }

        private static void GenerateCahmpions(int anz, Database database)
        {
            for (int i = 0; i < anz; i++)
            {
                database.Champions.Add(new Champion { Name = i.ToString() });
            }

            database.SaveChanges();
        }

        private static Database InitializeDatabase(int anz)
        {
            var contextName = "name=Random_Database_" + anz;
            var database = new Database(contextName);
            database.Configuration.AutoDetectChangesEnabled = false;
            return database;
        }

        private static Database AddToContext<T>(Database context, string contextName, T entity, int count, int commitCount, bool recreateContext) where T : class
        {
            context.Set<T>().Add(entity);

            if (count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new Database(contextName);
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