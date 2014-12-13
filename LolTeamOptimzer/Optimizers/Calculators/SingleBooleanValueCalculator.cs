namespace LolTeamOptimizer.Optimizers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;

    public class SingleBooleanValueCalculator
    {
        private readonly bool[,] synergies;

        private readonly bool[,] weaknesses;

        private readonly bool[,] strengths;

        public SingleBooleanValueCalculator(IList<Champion> champs)
        {
            var anz = champs.Max(champ => champ.Id) + 1;

            synergies = new bool[anz, anz];
            weaknesses = new bool[anz, anz];
            strengths = new bool[anz, anz];

            foreach (var champion in champs)
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
                if (synergies[champ, champs.Last()])
                {
                    result++;
                }

                if (synergies[champs.Last(), champ])
                {
                    result++;
                }
            }
            return result;
        }
    }
}