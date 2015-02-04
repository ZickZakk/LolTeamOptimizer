#region Using

using System.Collections.Generic;
using System.Linq;

using LolTeamOptimizerClean.Common;
using LolTeamOptimizerClean.Relations;

#endregion

namespace LolTeamOptimizerClean.Optimizers
{
    public class BranchAndBoundOptimizer : BaseOptimizer
    {
        private TeamValuePair bestTeam;

        public BranchAndBoundOptimizer(RelationsState relationsState, int teamSize)
            : base(relationsState, teamSize)
        {
        }

        public override IList<int> FindBestTeam(IList<int> enemies)
        {
            this.bestTeam = new TeamValuePair();
            this.InitiateAvailableChampions(enemies);

            this.BranchAndBound(new List<int>(), 0, 0, new List<int>(), 0);

            return this.bestTeam.Champions;
        }

        private void BranchAndBound(IList<int> currentTeam, int synergies, int strenghts, IList<int> usedChampions, int tiefe)
        {
            if (tiefe == TeamSize)
            {
                this.bestTeam.Champions = currentTeam;
                this.bestTeam.Value = synergies + strenghts;
                return;
            }

            var alternativeChamps = this.BerechneMengeDerAlternativen(currentTeam, usedChampions).ToList();
            var currentTeamSize = tiefe + 1;

            foreach (var alternativerChamp in alternativeChamps)
            {
                // Calc synergy with new champ
                var currentTeamSynergy = synergies + this.Calculator.CalculateSynergy(alternativerChamp, currentTeam);

                // Calc possible Synergies
                var possibleAdditionalTeamSynergy = TeamSize * (TeamSize - 1) - currentTeamSize * (currentTeamSize - 1);

                // Calc versusPoints with new champ
                var currentTeamStrengths = strenghts + this.VersusPoints[alternativerChamp];

                // Add possible Strenghts & NotWeaknesses
                var possibleAdditionalTeamStrength = alternativeChamps.SkipWhile(id => id != alternativerChamp).Except(currentTeam).Take(TeamSize - currentTeamSize).Select(id => this.VersusPoints[id]).Sum();

                var currentTeamValue = currentTeamSynergy + possibleAdditionalTeamSynergy + currentTeamStrengths + possibleAdditionalTeamStrength;

                if (currentTeamValue > this.bestTeam.Value)
                {
                    currentTeam.Add(alternativerChamp);
                    usedChampions.Add(alternativerChamp);

                    this.BranchAndBound(currentTeam.ToList(), currentTeamSynergy, currentTeamStrengths, usedChampions.ToList(), currentTeamSize);

                    usedChampions.Remove(alternativerChamp);
                    currentTeam.Remove(alternativerChamp);
                }
            }
        }

        private IEnumerable<int> BerechneMengeDerAlternativen(IEnumerable<int> champions, IEnumerable<int> usedChampions)
        {
            var unavailableChampions = champions.Union(usedChampions);

            return this.AvailableChampions.Except(unavailableChampions);
        }
    }
}