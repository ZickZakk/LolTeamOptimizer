#region Using

using System.Collections.Generic;

#endregion

namespace LolTeamOptimizer.Optimizers.Calculators
{
    public interface ITeamValueCalculator<T>
    {
        int CalculateTeamValue(IList<T> champs, IList<T> enemyChamps);
    }
}