namespace LolTeamOptimizer.Optimizers.Common
{
    public static class IntegerLimit
    {
        public static int LimitToRange(
        this int value, int inclusiveMinimum, int inclusiveMaximum)
        {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            if (value > inclusiveMaximum) { return inclusiveMaximum; }
            return value;
        } 
    }
}