namespace LolTeamOptimizerClean.Relations
{
    public class RelationsState
    {
        public RelationsState(bool[,] strengths, bool[,] synergies, bool[,] weaknesses, int championCount)
        {
            this.Strengths = strengths;
            this.Synergies = synergies;
            this.Weaknesses = weaknesses;
            this.ChampionCount = championCount;
        }

        public bool[,] Strengths { get; private set; }

        public bool[,] Weaknesses { get; private set; }

        public bool[,] Synergies { get; private set; }

        public int ChampionCount { get; private set; }
    }
}