using System;
using System.Collections.Generic;
using System.Linq;

namespace LolTeamOptimizer.Optimizer
{
    public class PickingState
    {
        private IEnumerable<Champion> bans = new List<Champion>(); 

        public IEnumerable<Champion> Bans
        {
            get { return this.bans; }

            set
            {
                if (value.Count() > 6)
                {
                    throw new ArgumentException("There can't be more than six bans!");
                }

                this.bans = value;
            }
        }

        private IEnumerable<Champion> enemyPicks = new List<Champion>();

        public IEnumerable<Champion> EnemyPicks
        {
            get { return this.enemyPicks; }

            set
            {
                if (value.Count() > 5)
                {
                    throw new ArgumentException("There can't be more than five enemy picks!");
                }

                this.enemyPicks = value;
            }
        }

        private IEnumerable<Champion> alliedPicks = new List<Champion>();

        public IEnumerable<Champion> AlliedPicks
        {
            get { return this.alliedPicks; }

            set
            {
                if (value.Count() > 4)
                {
                    throw new ArgumentException("There can't be more than four allied picks!");
                }

                this.alliedPicks = value;
            }
        }
    }
}