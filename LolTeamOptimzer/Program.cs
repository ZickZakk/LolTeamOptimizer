namespace LolTeamOptimizer
{
    #region using

    using System;
    using System.Linq;

    #endregion

    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Console.WriteLine("### LoL-Counter Database Sync ###");

            var dataBase = new Database();

            Console.WriteLine("### Clearing Database ###");

            dataBase.Database.ExecuteSqlCommand("DELETE FROM [IsStrongAgainstSet]");
            dataBase.Database.ExecuteSqlCommand("DELETE FROM [Champions]");

            Console.WriteLine("### Gathering Champions ###");

            var champions = HtmlService.GatherChapionNames()
                .ToList();

            foreach (var champion in champions)
            {
                dataBase.Champions.Add(new Champion
                                       {
                                           Name = champion
                                       });
            }

            dataBase.SaveChanges();

            Console.WriteLine("### Gathering IsStrongAgainst ###");

            foreach (var champion in dataBase.Champions)
            {
                var relations = HtmlService.GatherChampionRelations(champion.Name, "/strong");

                // Combine Champions, which appeard two times in the list and make IsStrong
                var isStrongRelations = relations.GroupBy(relation => relation.ChampionName)
                    .Select(group => new IsStrongAgainst
                                     {
                                         Champion = champion,
                                         OtherChampion = dataBase.Champions.Single(champ => champ.Name.Equals(group.Key)),
                                         Rating = Convert.ToInt32(group.Average(relation => relation.Value))
                                     }).ToList();

                // Add Relations
                dataBase.IsStrongAgainstSet.AddRange(isStrongRelations);

                relations = HtmlService.GatherChampionRelations(champion.Name, "/weak");

                // Combine Champions, which appeard two times in the list and make IsStrong
                var isWeakRelations = relations.GroupBy(relation => relation.ChampionName)
                    .Select(group => new IsWeakAgainst
                    {
                        Champion = champion,
                        OtherChampion = dataBase.Champions.Single(champ => champ.Name.Equals(group.Key)),
                        Rating = Convert.ToInt32(group.Average(relation => relation.Value))
                    }).ToList();

                // Add Relations
                dataBase.IsWeakAgainstSet.AddRange(isWeakRelations);

                relations = HtmlService.GatherChampionRelations(champion.Name, "/good");

                // Combine Champions, which appeard two times in the list and make IsStrong
                var goesWellRelations = relations.GroupBy(relation => relation.ChampionName)
                    .Select(group => new GoesWellWith
                    {
                        Champion = champion,
                        OtherChampion = dataBase.Champions.Single(champ => champ.Name.Equals(group.Key)),
                        Rating = Convert.ToInt32(group.Average(relation => relation.Value))
                    }).ToList();

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