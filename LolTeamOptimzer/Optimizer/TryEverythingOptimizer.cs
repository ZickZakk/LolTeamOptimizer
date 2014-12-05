#region Using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace LolTeamOptimizer.Optimizer
{
    public class TryEverythingOptimizer : ITeamOptimizer
    {
        public IEnumerable<Champion> CalculateOptimalePicks(PickingState state)
        {
            var database = new Database();
            var unavailableChampionIds = state.AlliedPicks.Union(state.Bans).Union(state.EnemyPicks).Select(champ => champ.Id);

            var availableChampionIds = database.Champions.Select(chmap => chmap.Id).Except(unavailableChampionIds).ToList();
            var availableChampions = availableChampionIds.Select(id => database.Champions.Find(id)).ToList();

            var bestTeamValue = int.MinValue;
            var bestTeam = new Champion[state.TeamSize];

            foreach (var champCombination in Combinations(availableChampions, 0, state.TeamSize - state.AlliedPicks.Count() - 1))
            {
                var teamValue = TeamValueCalculator.CalculateTeamValue(champCombination, state.EnemyPicks);

                if (teamValue > bestTeamValue)
                {
                    bestTeamValue = teamValue;
                    bestTeam = champCombination;
                }
            }

            return bestTeam;
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