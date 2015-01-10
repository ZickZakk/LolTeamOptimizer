#region Using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace LolTeamOptimizer.Optimizers.Common
{
    public class IntTeamValuePair
    {
        private Database database = new Database(CurrentDatabase.Name);

        public IntTeamValuePair(IEnumerable<int> team, int calculateTeamValue)
        {
            this.Team = team;
            this.TeamValue = calculateTeamValue;
        }

        public IEnumerable<int> Team { get; set; }

        public int TeamValue { get; set; }

        public TeamValuePair ToTeamValuePair()
        {
            return new TeamValuePair(Team.Select(i => database.Champions.Find(i)), TeamValue);
        }
    }
}