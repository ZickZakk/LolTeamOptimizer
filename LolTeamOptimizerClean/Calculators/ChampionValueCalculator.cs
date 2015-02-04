#region Using

using System.Collections.Generic;
using System.Linq;

using LolTeamOptimizerClean.Relations;

#endregion

namespace LolTeamOptimizerClean.Calculators
{
    public class ChampionValueCalculator : BaseCalculator
    {
        public ChampionValueCalculator(RelationsState relationsState)
            : base(relationsState)
        {
        }

        public int CalculateNotWeaknesses(int champion, IEnumerable<int> enemyChampions)
        {
            return enemyChampions.Count(enemyChamp => !this.RelationsState.Weaknesses[champion, enemyChamp]);
        }

        public int CalculateStrenghts(int champion, IEnumerable<int> enemyChampions)
        {
            return enemyChampions.Count(enemyChamp => this.RelationsState.Strengths[champion, enemyChamp]);
        }

        public int CalculateSynergy(int champion, IEnumerable<int> teamMates)
        {
            var result = 0;
            foreach (var mate in teamMates)
            {
                if (this.RelationsState.Synergies[mate, champion])
                {
                    result++;
                }

                if (this.RelationsState.Synergies[champion, mate])
                {
                    result++;
                }
            }
            return result;
        }
    }
}