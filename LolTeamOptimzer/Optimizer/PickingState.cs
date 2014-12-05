#region Using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace LolTeamOptimizer.Optimizer
{
    public class PickingState
    {
        private readonly int teamSize;

        private IEnumerable<Champion> alliedPicks = new List<Champion>();

        private IEnumerable<Champion> bans = new List<Champion>();

        private IEnumerable<Champion> enemyPicks = new List<Champion>();

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

        public IEnumerable<Champion> Bans
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

        public IEnumerable<Champion> EnemyPicks
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

        public IEnumerable<Champion> AlliedPicks
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