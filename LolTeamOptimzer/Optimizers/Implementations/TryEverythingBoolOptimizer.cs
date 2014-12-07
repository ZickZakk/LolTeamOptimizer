﻿#region Using

using System.Collections.Generic;
using System.Linq;

using LolTeamOptimizer.Optimizers.BaseClasses;
using LolTeamOptimizer.Optimizers.Calculators;
using LolTeamOptimizer.Optimizers.Common;

#endregion

namespace LolTeamOptimizer.Optimizers.Implementations
{
    public class TryEverythingBoolOptimizer : BaseTeamOptimizer<int>
    {
        public TryEverythingBoolOptimizer()
            : base(new BooleanTeamValueCalculator())
        {
        }

        public override TeamValuePair CalculateOptimalePicks(PickingState state)
        {
            var database = new Database();
            var unavailableChampionIds = state.AlliedPicks.Union(state.Bans).Union(state.EnemyPicks).Select(champ => champ.Id);

            var availableChampionIds = database.Champions.Select(chmap => chmap.Id).Except(unavailableChampionIds).ToList();

            var enemyIds = state.EnemyPicks.Select(champ => champ.Id).ToList();

            var bestTeamValue = int.MinValue;
            var bestTeam = new Champion[state.TeamSize];

            foreach (var champCombination in Combinations(availableChampionIds, 0, state.TeamSize - state.AlliedPicks.Count() - 1))
            {
                var teamValue = this.teamValueCalculator.CalculateTeamValue(champCombination, enemyIds);

                if (teamValue > bestTeamValue)
                {
                    bestTeamValue = teamValue;
                    bestTeam = champCombination.Select(id => database.Champions.Find(id)).ToArray();
                }
            }

            return new TeamValuePair(bestTeam, bestTeamValue);
        }

        private static IEnumerable<T[]> Combinations<T>(IList<T> argList, int argStart, int argIteration, List<int> argIndicies = null)
        {
            argIndicies = argIndicies ?? new List<int>();
            for (var i = argStart; i < argList.Count; i++)
            {
                argIndicies.Add(i);
                if (argIteration > 0)
                {
                    foreach (var array in Combinations(argList, i + 1, argIteration - 1, argIndicies))
                    {
                        yield return array;
                    }
                }
                else
                {
                    var array = new T[argIndicies.Count];
                    for (var j = 0; j < argIndicies.Count; j++)
                    {
                        array[j] = argList[argIndicies[j]];
                    }

                    yield return array;
                }
                argIndicies.RemoveAt(argIndicies.Count - 1);
            }
        }
    }
}