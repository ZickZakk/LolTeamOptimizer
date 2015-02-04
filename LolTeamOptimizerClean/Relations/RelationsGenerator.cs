#region Using

using System;

#endregion

namespace LolTeamOptimizerClean.Relations
{
    public static class RelationsGenerator
    {
        public static RelationsState GenerateRelations(int championCount)
        {
            const double SynergyDoubleProbality = 0.8;
            const double StrenghtDoubleProbality = 0.6;
            const double WeaknessDoubleProbality = 0.2;

            var random = new Random();

            var relationsCount = championCount / 5;

            var synergies = new bool[championCount, championCount];
            var strengths = new bool[championCount, championCount];
            var weaknesses = new bool[championCount, championCount];

            for (var champion = 0; champion < championCount; champion++)
            {
                for (var relation = 0; relation < relationsCount; relation++)
                {
                    var otherChampion = random.Next(0, championCount);
                    synergies[champion, otherChampion] = true;

                    if (random.NextDouble() <= SynergyDoubleProbality)
                    {
                        synergies[otherChampion, champion] = true;
                        relation++;
                    }
                }

                for (var relation = 0; relation < relationsCount; relation++)
                {
                    var otherChampion = random.Next(0, championCount);
                    strengths[champion, otherChampion] = true;

                    if (random.NextDouble() <= StrenghtDoubleProbality)
                    {
                        weaknesses[otherChampion, champion] = true;
                        relation++;
                    }
                }

                for (var relation = 0; relation < relationsCount; relation++)
                {
                    var otherChampion = random.Next(0, championCount);
                    weaknesses[champion, otherChampion] = true;

                    if (random.NextDouble() <= WeaknessDoubleProbality)
                    {
                        strengths[otherChampion, champion] = true;
                        relation++;
                    }
                }
            }

            return new RelationsState(strengths, synergies, weaknesses, championCount);
        }
    }
}