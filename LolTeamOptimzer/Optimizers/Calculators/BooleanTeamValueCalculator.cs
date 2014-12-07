using System;
using System.Collections.Generic;
using System.Linq;

namespace LolTeamOptimizer.Optimizers.Calculators
{
    public class BooleanTeamValueCalculator : ITeamValueCalculator<int>
    {
        private readonly bool[,] synergies;

        private readonly bool[,] weaknesses;

        private readonly bool[,] strengths;

        private Dictionary<int, int> vsEnemyResults = new Dictionary<int, int>(); 

        public BooleanTeamValueCalculator()
        {
            var database = new Database();
            var anz = database.Champions.Max(champ => champ.Id) + 1;

            synergies = new bool[anz, anz];
            weaknesses = new bool[anz, anz];
            strengths = new bool[anz, anz];

            foreach (var champion in database.Champions)
            {
                foreach (var rel in champion.GoesWellWith)
                {
                    synergies[champion.Id, rel.OtherChampion.Id] = true;
                }

                foreach (var rel in champion.IsStrongAgainst)
                {
                    strengths[champion.Id, rel.OtherChampion.Id] = true;
                }

                foreach (var rel in champion.IsWeakAgainst)
                {
                    weaknesses[champion.Id, rel.OtherChampion.Id] = true;
                }
            }
        }

        public int CalculateTeamValue(IList<int> champs, IList<int> enemyChamps)
        {
            var result = this.CalculateSynergy(champs);

            foreach (var champ in champs)
            {
                var vsEnemyResult = 0;
                if (!vsEnemyResults.TryGetValue(champ, out vsEnemyResult))
                {
                    vsEnemyResult += this.CalculateStrenghts(champ, enemyChamps);
                    vsEnemyResult += this.CalculateNotWeaknesses(champ, enemyChamps);

                    vsEnemyResults.Add(champ, vsEnemyResult);
                }

                result += vsEnemyResult;
            }

            return result;
        }

        private int CalculateNotWeaknesses(int champ, IList<int> enemyChamps)
        {
            return enemyChamps.Count(enemyChamp => !this.weaknesses[champ, enemyChamp]);
        }

        private int CalculateStrenghts(int champ, IList<int> enemyChamps)
        {
            return enemyChamps.Count(enemyChamp => this.strengths[champ, enemyChamp]);
        }

        private int CalculateSynergy(IList<int> champs)
        {
            return (from champ in champs from otherChamp in champs.Where(id => id != champ) where this.synergies[champ, otherChamp] select champ).Count();
        }
    }
}