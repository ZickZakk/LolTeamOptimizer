﻿#region Using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace LolTeamOptimizer.Optimizers.Calculators
{
    public class SingleBooleanValueCalculator
    {
        private readonly bool[,] strengths;

        private readonly bool[,] synergies;

        private readonly bool[,] weaknesses;

        public SingleBooleanValueCalculator(IList<Champion> champs)
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

        public int CalculateSynergy(IList<int> champs)
        {
            var result = 0;
            foreach (var champ in champs.Take(champs.Count - 1))
            {
                if (this.synergies[champ, champs.Last()])
                {
                    result++;
                }

                if (this.synergies[champs.Last(), champ])
                {
                    result++;
                }
            }
            return result;
        }
    }
}