#region Using

using LolTeamOptimizer.Optimizers.Calculators;
using LolTeamOptimizer.Optimizers.Common;
using LolTeamOptimizer.Optimizers.Interfaces;

#endregion

namespace LolTeamOptimizer.Optimizers.BaseClasses
{
    public abstract class BaseTeamOptimizer : ITeamOptimizer
    {
        protected readonly ITeamValueCalculator teamValueCalculator;

        protected BaseTeamOptimizer(ITeamValueCalculator teamValueCalculator)
        {
            this.teamValueCalculator = teamValueCalculator;
        }

        public abstract TeamValuePair CalculateOptimalePicks(PickingState state);
    }
}