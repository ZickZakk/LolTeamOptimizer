#region Using

using LolTeamOptimizerClean.Relations;

#endregion

namespace LolTeamOptimizerClean.Calculators
{
    public abstract class BaseCalculator
    {
        protected BaseCalculator(RelationsState relationsState)
        {
            this.RelationsState = relationsState;
        }

        protected RelationsState RelationsState { get; private set; }
    }
}