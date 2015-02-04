#region Using

using System.Collections.Generic;

#endregion

namespace LolTeamOptimizerClean.Common
{
    public class TeamValuePair
    {
        public IList<int> Champions { get; set; }

        public int Value { get; set; }
    }
}