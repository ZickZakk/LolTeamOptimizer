#region Using

using System.Collections.Generic;

#endregion

namespace LolTeamOptimizerClean.Optimizers
{
    public interface IOptimizer
    {
        IList<int> FindBestTeam(IList<int> enemies);
    }
}