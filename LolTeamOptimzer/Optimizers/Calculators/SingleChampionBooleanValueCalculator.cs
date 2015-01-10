#region Using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace LolTeamOptimizer.Optimizers.Calculators
{
    public class SingleChampionBooleanValueCalculator
    {
        private readonly bool[,] strengths;

        private readonly bool[,] synergies;

        private readonly bool[,] weaknesses;

        public SingleChampionBooleanValueCalculator(IList<Champion> champs)
        {
            var anz = champs.Max(champ => champ.Id) + 1;

            this.synergies = new bool[anz, anz];
            this.weaknesses = new bool[anz, anz];
            this.strengths = new bool[anz, anz];

            foreach (var champion in champs)
            {
                foreach (var rel in champion.GoesWellWith)
                {
                    this.synergies[champion.Id, rel.OtherChampion.Id] = true;
                }

                foreach (var rel in champion.IsStrongAgainst)
                {
                    this.strengths[champion.Id, rel.OtherChampion.Id] = true;
                }

                foreach (var rel in champion.IsWeakAgainst)
                {
                    this.weaknesses[champion.Id, rel.OtherChampion.Id] = true;
                }
            }
        }

        public int CalculateNotWeaknesses(int champ, IList<int> enemyChamps)
        {
            return enemyChamps.Count(enemyChamp => !this.weaknesses[champ, enemyChamp]);
        }

        public int CalculateStrenghts(int champ, IList<int> enemyChamps)
        {
            return enemyChamps.Count(enemyChamp => this.strengths[champ, enemyChamp]);
        }

        public int CalculateSynergy(int champion, IList<int> teamMates)
        {
            var result = 0;
            foreach (var mate in teamMates)
            {
                if (this.synergies[mate, champion])
                {
                    result++;
                }

                if (this.synergies[champion, mate])
                {
                    result++;
                }
            }
            return result;
        }
    }
}