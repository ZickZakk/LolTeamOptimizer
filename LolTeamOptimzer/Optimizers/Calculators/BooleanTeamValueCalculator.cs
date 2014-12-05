using System;
using System.Collections.Generic;

namespace LolTeamOptimizer.Optimizers.Calculators
{
    public class BooleanTeamValueCalculator : ITeamValueCalculator
    {
        public int CalculateTeamValue(IEnumerable<Champion> champs, IEnumerable<Champion> enemyChamps)
        {
            throw new NotImplementedException();
        }

        private int CalculateWeaknesses(IEnumerable<Champion> champs, IEnumerable<Champion> enemyChamps)
        {
            throw new NotImplementedException();
        }

        private int CalculateStrenghts(IEnumerable<Champion> champs, IEnumerable<Champion> enemyChamps)
        {
            throw new NotImplementedException();
        }

        private int CalculateSynergy(IList<Champion> champs)
        {
            throw new NotImplementedException();
        }
    }
}