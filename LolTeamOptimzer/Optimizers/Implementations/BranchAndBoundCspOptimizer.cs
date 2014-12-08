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

        private SingleBooleanValueCalculator calc = new SingleBooleanValueCalculator(); 

        private TeamValuePair bestTeam;

        private int teamSize;

        private readonly Database database = new Database();

        private int durchläufe;

        public BranchAndBoundCspOptimizer()
            : base(null)
        {
        }

        public override TeamValuePair CalculateOptimalePicks(PickingState state)
        {
            bestTeam = new TeamValuePair(new List<Champion>(), int.MinValue);

            this.teamSize = state.TeamSize;
            this.enemyChampions = state.EnemyPicks.Select(champ => champ.Id).ToList();

            this.InitiateAvailableChampions(state);

            this.BranchAndBound(new List<int>(), 0, new List<int>(),  0);

            return bestTeam;
        }

        private void BranchAndBound(IList<int> currentTeam, int teamValue, IList<int> usedChampions, int tiefe)
        {
            if (tiefe == this.teamSize)
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

                var currentTeamValue = currentTeam.Select(x => vsPoints[x]).Sum() + calc.CalculateSynergy(currentTeam);

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

            this.availableChampions = database.Champions.Select(champ => champ.Id).Except(unavailableChampions).ToList();

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