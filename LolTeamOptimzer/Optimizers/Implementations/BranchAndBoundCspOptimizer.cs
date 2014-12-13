using System.Collections.Generic;
using System.Linq;

using LolTeamOptimizer.Optimizers.BaseClasses;
using LolTeamOptimizer.Optimizers.Calculators;
using LolTeamOptimizer.Optimizers.Common;

namespace LolTeamOptimizer.Optimizers.Implementations
{
    public class BranchAndBoundCspOptimizer : BaseTeamOptimizer<int>
    {
        private IList<int> availableChampions;

        private IList<int> enemyChampions;

        private Dictionary<int,int> vsPoints = new Dictionary<int, int>();

        private SingleBooleanValueCalculator calc; 

        private TeamValuePair bestTeam;

        private int teamSize;

        private readonly IList<Champion> championSet;

        public BranchAndBoundCspOptimizer(IList<Champion> championSet)
            : base(null)
        {
            this.championSet = championSet;
            this.calc = new SingleBooleanValueCalculator(championSet);
        }

        public override TeamValuePair CalculateOptimalePicks(PickingState state)
        {
            bestTeam = new TeamValuePair(new List<Champion>(), int.MinValue);

            this.teamSize = state.TeamSize;
            this.enemyChampions = state.EnemyPicks.Select(champ => champ.Id).ToList();
            this.vsPoints = new Dictionary<int, int>();

            this.InitiateAvailableChampions(state);

            this.BranchAndBound(new List<int>(), 0, 0, new List<int>(),  0);

            return bestTeam;
        }

        private void BranchAndBound(IList<int> currentTeam, int synergies, int strenghts, IList<int> usedChampions, int tiefe)
        {
            if (tiefe == this.teamSize)
            {
                // bestTeam.Team = currentTeam.Select(id => database.Champions.Find(id)).ToList();
                bestTeam.TeamValue = synergies + strenghts;
                return;
            }

            var alternativeChamps = this.BerechneMengeDerAlternativen(currentTeam.ToList(), usedChampions.ToList());
            var currentTeamSize = tiefe + 1;

            foreach (var alternativerChamp in alternativeChamps)
            {
                currentTeam.Add(alternativerChamp);
                usedChampions.Add(alternativerChamp);

                // Calc synergy with new champ
                var currentTeamSynergy = synergies + calc.CalculateSynergy(currentTeam);

                // Calc possible Synergies
                var possibleAdditionalTeamSynergy = this.teamSize * (this.teamSize - 1) - currentTeamSize * (currentTeamSize - 1);

                // Calc vsPoints with new champ
                var currentTeamStrengths = strenghts + vsPoints[alternativerChamp];

                // Add possible Strenghts & NotWeaknesses
                var possibleAdditionalTeamStrength = alternativeChamps.SkipWhile(id => id != alternativerChamp).Except(currentTeam).Take(teamSize - currentTeamSize).Select(id => vsPoints[id]).Sum();
                
                var currentTeamValue = currentTeamSynergy + possibleAdditionalTeamSynergy + currentTeamStrengths
                                       + possibleAdditionalTeamStrength;

                if (currentTeamValue > bestTeam.TeamValue)
                {
                    this.BranchAndBound(currentTeam.ToList(), currentTeamSynergy, currentTeamStrengths, usedChampions.ToList(), currentTeamSize);
                }

                currentTeam.Remove(alternativerChamp);
                usedChampions.Remove(alternativerChamp);
            }
        }

        private IList<int> BerechneMengeDerAlternativen(IList<int> champions, IList<int> usedChampions)
        {
            var unavailableChampions = champions.Union(usedChampions);

            return availableChampions.Except(unavailableChampions).ToList();
        }

        private void InitiateAvailableChampions(PickingState state)
        {
            var unavailableChampions = state.AlliedPicks.Union(state.Bans).Union(state.EnemyPicks).Select(champ => champ.Id);

            this.availableChampions = this.championSet.Select(champ => champ.Id).Except(unavailableChampions).ToList();

            foreach (var availableChampion in availableChampions)
            {
                var value = calc.CalculateNotWeaknesses(availableChampion, enemyChampions)
                            + calc.CalculateStrenghts(availableChampion, enemyChampions);

                this.vsPoints.Add(availableChampion, value);
            }

            this.availableChampions = vsPoints.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
        }
    }
}