using System.Collections.Generic;
using System.Linq;

namespace LolTeamOptimizer.Optimizer
{
    public class TeamValueCalculator
    {
        public static int CalculateTeamValue(IEnumerable<Champion> champs, IEnumerable<Champion> enemyChamps)
        {
            var champions = champs as IList<Champion> ?? champs.ToList();

            // Wenn doppelte champs
            if (champions.GroupBy(n => n).Any(c => c.Count() > 1))
            {
                return int.MinValue;
            }

            var enemyChampions = enemyChamps as IList<Champion> ?? enemyChamps.ToList();
            return CalculateStrenghts(champions, enemyChampions) + CalculateSynergy(champions) - CalculateWeaknesses(champions, enemyChampions);
        }

        private static int CalculateWeaknesses(IEnumerable<Champion> champs, IEnumerable<Champion> enemyChamps)
        {
            var weaknesses = 0;

            foreach (var champ in champs)
            {
                foreach (var enemyChamp in enemyChamps)
                {
                    var relation = champ.IsWeakAgainst.FirstOrDefault(weak => weak.OtherChampion.Id == enemyChamp.Id);

                    if (relation != null)
                    {
                        weaknesses += relation.Rating;
                    }
                }
            }

            return weaknesses;
        }

        private static int CalculateStrenghts(IEnumerable<Champion> champs, IEnumerable<Champion> enemyChamps)
        {
            var strenghts = 0;

            foreach (var champ in champs)
            {
                foreach (var enemyChamp in enemyChamps)
                {
                    var relation = champ.IsStrongAgainst.FirstOrDefault(strong => strong.OtherChampion.Id == enemyChamp.Id);

                    if (relation != null)
                    {
                        strenghts += relation.Rating;
                    }
                }
            }

            return strenghts;
        }

        private static int CalculateSynergy(IList<Champion> champs)
        {
            var synergy = 0;

            for (var i = 0; i < champs.Count; i++)
            {
                for (var j = i + 1; j < champs.Count; j++)
                {
                    var relation = champs[i].GoesWellWith.FirstOrDefault(well => well.OtherChampion.Equals(champs[j]));

                    if (relation != null)
                    {
                        synergy += relation.Rating;
                    }
                }
            }

            return synergy;
        } 
    }
}