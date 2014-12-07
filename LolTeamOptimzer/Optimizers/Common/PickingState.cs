#region Using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace LolTeamOptimizer.Optimizers.Common
{
    public class PickingState
    {
        private readonly int teamSize;

        private IList<Champion> alliedPicks = new List<Champion>();

        private IList<Champion> bans = new List<Champion>();

        private IList<Champion> enemyPicks = new List<Champion>();

        public PickingState(int teamSize)
        {
            if (teamSize <= 1)
            {
                throw new ArgumentException("Teams must be at aleast size 2.");
            }

            this.teamSize = teamSize;
        }

        public int TeamSize
        {
            get
            {
                return this.teamSize;
            }
        }

        public IList<Champion> Bans
        {
            get
            {
                return this.bans;
            }

            set
            {
                if (value.Count() > this.teamSize)
                {
                    throw new ArgumentException("There can't be more than " + this.teamSize + " bans!");
                }

                this.bans = value;
            }
        }

        public IList<Champion> EnemyPicks
        {
            get
            {
                return this.enemyPicks;
            }

            set
            {
                if (value.Count() > this.teamSize)
                {
                    throw new ArgumentException("There can't be more than " + this.teamSize + " enemy picks!");
                }

                this.enemyPicks = value;
            }
        }

        public IList<Champion> AlliedPicks
        {
            get
            {
                return this.alliedPicks;
            }

            set
            {
                if (value.Count() > this.teamSize - 1)
                {
                    throw new ArgumentException("There can't be more than " + this.teamSize + " allied picks!");
                }

                this.alliedPicks = value;
            }
        }
    }
}