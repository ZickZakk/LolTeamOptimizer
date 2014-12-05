#region Using

using LolTeamOptimizer.Optimizers.Common;

#endregion

namespace LolTeamOptimizer.Optimizers.Interfaces
{
    public interface ITeamOptimizer
    {
        TeamValuePair CalculateOptimalePicks(PickingState state);
    }
}