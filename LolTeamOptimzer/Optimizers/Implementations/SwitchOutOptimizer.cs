#region Using

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LolTeamOptimizer.Optimizers.BaseClasses;
using LolTeamOptimizer.Optimizers.Calculators;
using LolTeamOptimizer.Optimizers.Common;

#endregion

namespace LolTeamOptimizer.Optimizers.Implementations
{
    public class SwitchOutOptimizer : BaseTeamOptimizer<int>
    {
        private readonly SingleChampionBooleanValueCalculator calc;

        private readonly IList<Champion> championSet;

        private IList<int> availableChampions;

        private IList<ChampionValuePair> team;

        private IList<int> enemyChampions;

        private int teamSize;

        private ConcurrentDictionary<int, int> vsPoints = new ConcurrentDictionary<int, int>();

        public SwitchOutOptimizer(IList<Champion> championSet)
            : base(null)
        {
            this.championSet = championSet;
            this.calc = new SingleChampionBooleanValueCalculator(championSet);
        }

        public override TeamValuePair CalculateOptimalePicks(PickingState state)
        {
            this.teamSize = state.TeamSize;
            this.enemyChampions = state.EnemyPicks.Select(champ => champ.Id).ToList();
            this.vsPoints = new ConcurrentDictionary<int, int>();
            this.team = new List<ChampionValuePair>();

            this.InitiateAvailableChampions(state);

            this.team = this.CalculateTeam(availableChampions.Take(teamSize).ToList());

            while (true)
            {
                var betterValues = new ConcurrentBag<ChampionValuePair>();
                var curentTeam = team.Select(x => x.Champion).ToList();

                foreach (var champ in availableChampions.Except(curentTeam))
                {
                    for (int mateId = 0; mateId < teamSize; mateId++)
                    {
                        var mate = team[mateId];

                        var newValue = vsPoints[champ] + calc.CalculateSynergy(champ, curentTeam.Where(id => id != mate.Champion).ToList());

                        if (newValue > mate.Value)
                        {
                            betterValues.Add(new ChampionValuePair { Champion = champ, Value = newValue, ReplaceId = mateId });
                        }
                    }
                }

                if (betterValues.IsEmpty)
                {
                    break;
                }

                var bestNewMate = betterValues.OrderByDescending(pair => pair.Value).First();

                team.RemoveAt(bestNewMate.ReplaceId);
                team.Add(bestNewMate);

                team = CalculateTeam(team.Select(pair => pair.Champion).ToList());
            }

            var result = new IntTeamValuePair(this.team.Select(pair => pair.Champion), this.team.Sum(pair => pair.Value));

            return result.ToTeamValuePair();
        }

        private IList<ChampionValuePair> CalculateTeam(IList<int> champs)
        {
            var valuePairs = new List<ChampionValuePair>();
            foreach (var champ in champs)
            {
                var value = vsPoints[champ] + calc.CalculateSynergy(champ, champs.Where(c => c != champ).ToList());
                valuePairs.Add(new ChampionValuePair { Champion = champ, Value = value });
            }

            return valuePairs.OrderByDescending(pair => pair.Value).ToList();
        }

        private void InitiateAvailableChampions(PickingState state)
        {
            var unavailableChampions = state.AlliedPicks.Union(state.Bans).Union(state.EnemyPicks).Select(champ => champ.Id);

            this.availableChampions = this.championSet.Select(champ => champ.Id).Except(unavailableChampions).ToList();

            foreach (var availableChampion in this.availableChampions)
            {
                var value = this.calc.CalculateNotWeaknesses(availableChampion, this.enemyChampions) + this.calc.CalculateStrenghts(availableChampion, this.enemyChampions);

                this.vsPoints.TryAdd(availableChampion, value);
            }

            this.availableChampions = this.vsPoints.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
        }
    }
}