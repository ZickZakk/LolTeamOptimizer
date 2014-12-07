#region Using

using LolTeamOptimizer.Optimizers.Calculators;
using LolTeamOptimizer.Optimizers.Common;
using LolTeamOptimizer.Optimizers.Interfaces;

#endregion

namespace LolTeamOptimizer.Optimizers.BaseClasses
{
    public abstract class BaseTeamOptimizer<T> : ITeamOptimizer
    {
        protected readonly ITeamValueCalculator<T> teamValueCalculator;

        protected BaseTeamOptimizer(ITeamValueCalculator<T> teamValueCalculator)
        {
            this.teamValueCalculator = teamValueCalculator;
        }

        public abstract TeamValuePair CalculateOptimalePicks(PickingState state);
    }
}