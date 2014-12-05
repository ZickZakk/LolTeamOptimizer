using System.Collections.Generic;

namespace LolTeamOptimizer.Optimizer
{
    public interface ITeamOptimizer
    {
        IEnumerable<Champion> CalculateOptimalePicks(PickingState state);
    }
}