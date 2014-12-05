#region Using

using System.Collections.Generic;

#endregion

namespace LolTeamOptimizer.Optimizers.Calculators
{
    public interface ITeamValueCalculator
    {
        int CalculateTeamValue(IEnumerable<Champion> champs, IEnumerable<Champion> enemyChamps);
    }
}