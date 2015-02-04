using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using LolTeamOptimizerClean.Common;
using LolTeamOptimizerClean.Relations;

namespace LolTeamOptimizerClean.Optimizers
{
    public class SwitchOutOptimizer : BaseOptimizer
    {
        private IList<ChampionValuePair> team;

        public SwitchOutOptimizer(RelationsState relationsState, int teamSize)
            : base(relationsState, teamSize)
        {
        }

        public override IList<int> FindBestTeam(IList<int> enemies)
        {
            this.team = new List<ChampionValuePair>();

            this.InitiateAvailableChampions(enemies);

            this.team = this.CalculateTeam(this.AvailableChampions.Take(TeamSize).ToList());

            while (true)
            {
                var switchOuts = new List<SwitchOut>();
                var curentTeam = team.Select(x => x.Champion).ToList();

                foreach (var champ in this.AvailableChampions.Except(curentTeam))
                {
                    for (int mateId = 0; mateId < TeamSize; mateId++)
                    {
                        var mate = team[mateId];

                        var newValue = this.VersusPoints[champ] + this.Calculator.CalculateSynergy(champ, curentTeam.Where(id => id != mate.Champion).ToList());

                        if (newValue > mate.Value)
                        {
                            switchOuts.Add(new SwitchOut { NewMate = new ChampionValuePair {Champion = champ, Value = newValue}, ReplaceId = mateId });
                        }
                    }
                }

                if (switchOuts.Count == 0)
                {
                    break;
                }

                var bestSwitchOut = switchOuts.OrderByDescending(switchOut => switchOut.NewMate.Value).First();

                team.RemoveAt(bestSwitchOut.ReplaceId);
                team.Add(bestSwitchOut.NewMate);

                team = CalculateTeam(team.Select(pair => pair.Champion).ToList());
            }

            return team.Select(pair => pair.Champion).ToList();
        }

        private IList<ChampionValuePair> CalculateTeam(IList<int> champs)
        {
            var valuePairs = new List<ChampionValuePair>();
            foreach (var champ in champs)
            {
                var value = this.VersusPoints[champ] + this.Calculator.CalculateSynergy(champ, champs.Where(c => c != champ).ToList());
                valuePairs.Add(new ChampionValuePair { Champion = champ, Value = value });
            }

            return valuePairs.OrderByDescending(pair => pair.Value).ToList();
        }

        private class SwitchOut
        {
            public int ReplaceId { get; set; }

            public ChampionValuePair NewMate { get; set; }
        }

        private class ChampionValuePair
        {
            public int Champion { get; set; }

            public int Value { get; set; }
        }
    }
}