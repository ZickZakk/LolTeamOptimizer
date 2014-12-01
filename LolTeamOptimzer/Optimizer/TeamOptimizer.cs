using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LolTeamOptimizer.Optimizer
{
    public class TeamOptimizer
    {
        public static IEnumerable<Champion> CalculateOptimalePicks(PickingState state)
        {
            var database = new Database();
            var unavailableChampionIds =
                state.AlliedPicks.Union(state.Bans).Union(state.EnemyPicks).Select(champ => champ.Id);

            var availableChampionIds = database.Champions.Select(chmap => chmap.Id).Except(unavailableChampionIds).ToList();
            var availableChampions = availableChampionIds.Select(id => database.Champions.Find(id)).ToList();

            int bestTeamValue = int.MinValue;
            var bestTeam = new Champion[5];

            foreach (var champCombination in Combinations(availableChampions, 0, 5 - state.AlliedPicks.Count() - 1))
            {
                var synergy = CalculateSynergy(champCombination.Union(state.AlliedPicks).ToList());

                var strength = CalculateStrenghts(champCombination.Union(state.AlliedPicks).ToList(), state.EnemyPicks);

                var weaknesses = CalculateWeaknesses(champCombination.Union(state.AlliedPicks).ToList(), state.EnemyPicks);

                var teamValue = synergy + strength - weaknesses;

                if (teamValue > bestTeamValue)
                {
                    bestTeamValue = teamValue;
                    bestTeam = champCombination;
                }
            }

            return bestTeam;
        }

        private static int CalculateWeaknesses(IEnumerable<Champion> champs, IEnumerable<Champion> enemyChamps)
        {
            var weaknesses = 0;

            foreach (var champ in champs)
            {
                foreach (var enemyChamp in enemyChamps)
                {
                    var relation = champ.IsWeakAgainst.FirstOrDefault(weak => weak.OtherChampion.Id == enemyChamp.Id);

                    if (relation != null)
                    {
                        weaknesses += relation.Rating;
                    }
                }
            }

            return weaknesses;
        }

        private static int CalculateStrenghts(IEnumerable<Champion> champs, IEnumerable<Champion> enemyChamps)
        {
            int strenghts = 0;

            foreach (var champ in champs)
            {
                foreach (var enemyChamp in enemyChamps)
                {
                    var relation = champ.IsStrongAgainst.FirstOrDefault(strong => strong.OtherChampion.Id == enemyChamp.Id);

                    if (relation != null)
                    {
                        strenghts += relation.Rating;
                    }
                }
            }

            return strenghts;
        }

        private static int CalculateSynergy(IList<Champion> champs)
        {
            int synergy = 0;

            for (int i = 0; i < champs.Count; i++)
            {
                for (int j = i + 1; j < champs.Count; j++)
                {
                    var relation = champs[i].GoesWellWith.FirstOrDefault(well => well.OtherChampion.Equals(champs[j]));

                    if (relation != null)
                    {
                        synergy += relation.Rating;
                    }
                }
            }

            return synergy;
        }

        private static IEnumerable<T[]> Combinations<T>(IList<T> argList, int argStart, int argIteration, List<int> argIndicies = null)
        {
            argIndicies = argIndicies ?? new List<int>();
            for (int i = argStart; i < argList.Count; i++)
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
                    for (int j = 0; j < argIndicies.Count; j++)
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