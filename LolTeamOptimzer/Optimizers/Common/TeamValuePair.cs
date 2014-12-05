#region Using

using System.Collections.Generic;

#endregion

namespace LolTeamOptimizer.Optimizers.Common
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