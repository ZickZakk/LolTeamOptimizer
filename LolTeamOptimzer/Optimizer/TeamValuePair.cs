using System.Collections.Generic;

namespace LolTeamOptimizer.Optimizer
{
    public class TeamValuePair
    {
        public TeamValuePair(IEnumerable<Champion> team, int calculateTeamValue)
        {
            this.Team = team;
            this.TeamValue = calculateTeamValue;
        }

        public IEnumerable<Champion> Team { get; set; }

        public int TeamValue { get; set; }
    }
}