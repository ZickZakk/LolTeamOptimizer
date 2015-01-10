#region Using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace LolTeamOptimizer.Optimizers.Calculators
{
    public class BooleanTeamValueCalculator : ITeamValueCalculator<int>
    {
        private readonly bool[,] strengths;

        private readonly bool[,] synergies;

        private readonly Dictionary<int, int> vsEnemyResults = new Dictionary<int, int>();

        private readonly bool[,] weaknesses;

        public BooleanTeamValueCalculator()
        {
            var database = new Database(CurrentDatabase.Name);
            var anz = database.Champions.Max(champ => champ.Id) + 1;

            this.synergies = new bool[anz, anz];
            this.weaknesses = new bool[anz, anz];
            this.strengths = new bool[anz, anz];

            foreach (var champion in database.Champions)
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

        public int CalculateTeamValue(IList<int> champs, IList<int> enemyChamps)
        {
            if (champs.GroupBy(x => x).Any(x => x.Count() > 1))
            {
                return 0;
            }

            var result = this.CalculateSynergy(champs);

            foreach (var champ in champs)
            {
                var vsEnemyResult = 0;
                if (!this.vsEnemyResults.TryGetValue(champ, out vsEnemyResult))
                {
                    vsEnemyResult += this.CalculateStrenghts(champ, enemyChamps);
                    vsEnemyResult += this.CalculateNotWeaknesses(champ, enemyChamps);

                    this.vsEnemyResults.Add(champ, vsEnemyResult);
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