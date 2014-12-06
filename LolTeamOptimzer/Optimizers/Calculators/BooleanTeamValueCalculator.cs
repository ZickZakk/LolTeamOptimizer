using System;
using System.Collections.Generic;
using System.Linq;

namespace LolTeamOptimizer.Optimizers.Calculators
{
    public class BooleanTeamValueCalculator : ITeamValueCalculator
    {
        public int CalculateTeamValue(IList<Champion> champs, IList<Champion> enemyChamps)
        {
            var weaknesses = this.CalculateWeaknesses(champs, enemyChamps);
            var strengths = this.CalculateStrenghts(champs, enemyChamps);
            var synergies = this.CalculateSynergy(champs);

            return strengths + synergies - weaknesses;
        }

        private int CalculateWeaknesses(IEnumerable<Champion> champs, IList<Champion> enemyChamps)
        {
            return (from champion in champs from enemyChamp in enemyChamps where champion.IsWeakAgainst.Any(relation => relation.OtherChampion.Id == enemyChamp.Id) select champion).Count();
        }

        private int CalculateStrenghts(IEnumerable<Champion> champs, IList<Champion> enemyChamps)
        {
            return (from champion in champs from enemyChamp in enemyChamps where champion.IsStrongAgainst.Any(relation => relation.OtherChampion.Id == enemyChamp.Id) select champion).Count();
        }

        private int CalculateSynergy(IList<Champion> champs)
        {
            return (from champion in champs from otherChamp in champs where champion.IsWeakAgainst.Any(relation => relation.OtherChampion.Id == otherChamp.Id) select champion).Count();
        }
    }
}