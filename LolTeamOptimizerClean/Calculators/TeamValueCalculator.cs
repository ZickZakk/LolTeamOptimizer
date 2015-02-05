#region Using

using System.Collections.Generic;
using System.Linq;

using LolTeamOptimizerClean.Relations;

#endregion

namespace LolTeamOptimizerClean.Calculators
{
    public class TeamValueCalculator : BaseCalculator
    {
        public TeamValueCalculator(RelationsState relationsState)
            : base(relationsState)
        {
        }

        public int CalculateTeamValue(IList<int> champions, IList<int> enemyChampions)
        {
            if (champions.GroupBy(x => x).Any(x => x.Count() > 1))
            {
                return 0;
            }

            return champions.Sum(champion => this.CalculateNotWeaknesses(champion, enemyChampions) + this.CalculateStrenghts(champion, enemyChampions)) + this.CalculateSynergies(champions);
        }

        private int CalculateNotWeaknesses(int champ, IEnumerable<int> enemyChamps)
        {
            return enemyChamps.Count(enemyChamp => !this.RelationsState.Weaknesses[champ, enemyChamp]);
        }

        private int CalculateStrenghts(int champ, IEnumerable<int> enemyChamps)
        {
            return enemyChamps.Count(enemyChamp => this.RelationsState.Strengths[champ, enemyChamp]);
        }

        private int CalculateSynergies(IList<int> champs)
        {
            return (from champ in champs from otherChamp in champs.Where(id => id != champ) where this.RelationsState.Synergies[champ, otherChamp] select champ).Count();
        }
    }
}