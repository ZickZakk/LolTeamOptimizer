#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LolTeamOptimizerClean.Relations;

#endregion

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

            this.team = this.CalculateTeam(this.AvailableChampions.Take(this.TeamSize).ToList());

            Action<List<int>, ConcurrentBag<SwitchOut>> loopAction;

            if (this.AvailableChampions.Count > 12500)
            {
                loopAction = (curentTeam, switchOuts) => Parallel.ForEach(this.AvailableChampions.Except(curentTeam), champ => this.FindPossibleSwitchOuts(champ, curentTeam, switchOuts));
            }
            else
            {
                loopAction = (curentTeam, switchOuts) =>
                    {
                        foreach (var champ in this.AvailableChampions.Except(curentTeam))
                        {
                            this.FindPossibleSwitchOuts(champ, curentTeam, switchOuts);
                        }
                    };
            }

            while (true)
            {
                var switchOuts = new ConcurrentBag<SwitchOut>();
                var curentTeam = this.team.Select(x => x.Champion).ToList();

                loopAction(curentTeam, switchOuts);

                if (switchOuts.Count == 0)
                {
                    break;
                }

                var bestSwitchOut = switchOuts.OrderByDescending(switchOut => switchOut.NewMate.Value).First();

                this.team.RemoveAt(bestSwitchOut.ReplaceId);
                this.team.Add(bestSwitchOut.NewMate);

                this.team = this.CalculateTeam(this.team.Select(pair => pair.Champion).ToList());
            }

            return this.team.Select(pair => pair.Champion).ToList();
        }

        private void FindPossibleSwitchOuts(int champ, List<int> curentTeam, ConcurrentBag<SwitchOut> switchOuts)
        {
            for (var mateId = 0; mateId < this.TeamSize; mateId++)
            {
                var mate = this.team[mateId];

                var newValue = this.VersusPoints[champ] + this.Calculator.CalculateSynergy(champ, curentTeam.Where(id => id != mate.Champion).ToList());

                if (newValue > mate.Value)
                {
                    switchOuts.Add(new SwitchOut { NewMate = new ChampionValuePair { Champion = champ, Value = newValue }, ReplaceId = mateId });
                }
            }
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

        private class ChampionValuePair
        {
            public int Champion { get; set; }

            public int Value { get; set; }
        }

        private class SwitchOut
        {
            public int ReplaceId { get; set; }

            public ChampionValuePair NewMate { get; set; }
        }
    }
}