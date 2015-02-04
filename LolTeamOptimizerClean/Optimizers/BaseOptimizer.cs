#region Using

using System.Collections.Generic;
using System.Linq;

using LolTeamOptimizerClean.Calculators;
using LolTeamOptimizerClean.Relations;

#endregion

namespace LolTeamOptimizerClean.Optimizers
{
    public abstract class BaseOptimizer : IOptimizer
    {
        protected readonly ChampionValueCalculator Calculator;

        protected readonly int TeamSize;

        private readonly int championCount;

        protected IList<int> AvailableChampions;

        protected Dictionary<int, int> VersusPoints;

        protected BaseOptimizer(RelationsState relationsState, int teamSize)
        {
            this.Calculator = new ChampionValueCalculator(relationsState);
            this.championCount = relationsState.ChampionCount;
            this.TeamSize = teamSize;
        }

        public abstract IList<int> FindBestTeam(IList<int> enemies);

        protected void InitiateAvailableChampions(IList<int> enemies)
        {
            this.AvailableChampions = Enumerable.Range(0, this.championCount).Except(enemies).ToList();

            this.CalculateVersusPoints(enemies);

            this.AvailableChampions = this.VersusPoints.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
        }

        private void CalculateVersusPoints(IList<int> enemies)
        {
            this.VersusPoints = new Dictionary<int, int>();

            foreach (var champion in this.AvailableChampions)
            {
                var value = this.Calculator.CalculateNotWeaknesses(champion, enemies) + this.Calculator.CalculateStrenghts(champion, enemies);

                this.VersusPoints.Add(champion, value);
            }
        }
    }
}