#region Using

using System.Collections.Generic;

#endregion

namespace LolTeamOptimizer.Optimizers.Calculators
{
    public interface ITeamValueCalculator
    {
        int CalculateTeamValue(IList<Champion> champs, IList<Champion> enemyChamps);
    }
}