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

        private TeamValuePair bestTeam;

        private int teamSize;

        private readonly Database database = new Database();

        private int durchläufe;

        public BranchAndBoundCspOptimizer()
            : base(new BooleanTeamValueCalculator())
        {
        }

        public override TeamValuePair CalculateOptimalePicks(PickingState state)
        {
            this.InitiateAvailableChampions(state);

            bestTeam = new TeamValuePair(new List<Champion>(), int.MinValue);

            this.teamSize = state.TeamSize;
            this.enemyChampions = state.EnemyPicks.Select(champ => champ.Id).ToList();

            this.BranchAndBound(new List<int>(), 0, new List<int>(),  0);

            return bestTeam;
        }

        private void BranchAndBound(IList<int> currentTeam, int teamValue, IList<int> usedChampions, int tiefe)
        {
            if (tiefe == this.teamSize )
            {
                bestTeam.Team = currentTeam.Select(id => database.Champions.Find(id)).ToList();
                bestTeam.TeamValue = teamValue;
                return;
            }

            var alternativeChamps = this.BerechneMengeDerAlternativen(currentTeam.ToList(), usedChampions.ToList());
            var currentTeamSize = tiefe + 1;

            foreach (var alternativerChamp in alternativeChamps)
            {
                currentTeam.Add(alternativerChamp);
                usedChampions.Add(alternativerChamp);

                var currentTeamValue = this.teamValueCalculator.CalculateTeamValue(currentTeam.ToList(), this.enemyChampions.ToList());

                // Add possible Synergies
                currentTeamValue += this.teamSize * (this.teamSize - 1) - currentTeamSize * (currentTeamSize - 1);

                // Add possible Strenghts & NotWeaknesses
                currentTeamValue += this.enemyChampions.Count * (this.teamSize - currentTeamSize) * 2;

                if (currentTeamValue > bestTeam.TeamValue)
                {
                    this.BranchAndBound(currentTeam.ToList(), currentTeamValue, usedChampions.ToList(), currentTeamSize);
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

            this.availableChampions = database.Champions.Select(chmap => chmap.Id).Except(unavailableChampions).ToList();
        }
    }
}