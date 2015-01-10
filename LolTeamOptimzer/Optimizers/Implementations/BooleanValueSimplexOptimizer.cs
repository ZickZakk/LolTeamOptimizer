#region Using

using System;
using System.Collections.Generic;
using System.Linq;

using LolTeamOptimizer.Optimizers.BaseClasses;
using LolTeamOptimizer.Optimizers.Calculators;
using LolTeamOptimizer.Optimizers.Common;

#endregion

namespace LolTeamOptimizer.Optimizers.Implementations
{
    public class BooleanValueSimplexOptimizer : BaseTeamOptimizer<int>
    {
        private readonly Database database = new Database(CurrentDatabase.Name);

        private readonly SingleBooleanValueCalculator calc;

        private readonly IList<Champion> championSet;

        private IList<int> availableChampions;

        private IList<int> enemyChampions;

        private int teamSize;

        private Dictionary<int, int> vsPoints = new Dictionary<int, int>();

        public BooleanValueSimplexOptimizer(IList<Champion> championSet)
            : base(new BooleanTeamValueCalculator())
        {
            this.championSet = championSet;
            this.calc = new SingleBooleanValueCalculator(championSet);
        }

        public override TeamValuePair CalculateOptimalePicks(PickingState state)
        {
            // Simplex Parameter Initialisierung
            var anzahlDimensionen = state.TeamSize;

            const int Alpha = 1;
            const int Beta = 2;
            const double Gamma = 0.5;

            const int dauerBisAbbruch = 10;
            const int dauerOhneAenderung = 10;
            
            // Tuple = Team + Wert
            var simplexPoints = new List<IntTeamValuePair>(anzahlDimensionen + 1);

            this.enemyChampions = state.EnemyPicks.Select(champ => champ.Id).ToList();
            this.vsPoints = new Dictionary<int, int>();

            this.InitiateAvailableChampions(state);

            // Grenzen des Definitionsbereichs
            var minChampion = availableChampions.Min();
            var maxChampion = availableChampions.Max();

            // Startpunkte festlegen
            var schrittweite = 1;
            for (var i = 0; i < anzahlDimensionen + 1; i++)
            {
                // Wechsle den letzten Champ in dem Team aus
                var team = availableChampions.Skip(schrittweite * i).Take(state.TeamSize).ToList();

                simplexPoints.Add(new IntTeamValuePair(team, this.teamValueCalculator.CalculateTeamValue(team, enemyChampions)));
            }

            var durchläufeSeitVerbesserung = 0;
            var festgefahren = 0;
            while (true)
            {
                durchläufeSeitVerbesserung ++;

                // Teams nach Wert sortieren
                simplexPoints = simplexPoints.OrderByDescending(tuple => tuple.TeamValue).ToList();

                // Lange keine pos. Änderung
                if (durchläufeSeitVerbesserung > dauerOhneAenderung)
                {
                    festgefahren++;
                    if (festgefahren > dauerBisAbbruch)
                    {
                        break;
                    }

                    var abstand = new int[state.TeamSize];
                    // Abstand von bestem zu schlechtestem team bestimmen
                    for (var i = 0; i < state.TeamSize; i++)
                    {
                        abstand[i] = Math.Abs(simplexPoints.First().Team.ElementAt(i) - simplexPoints.Last().Team.ElementAt(i));
                    }

                    // Simplex gleichmäßig um bestes Team verteilen
                    for (var i = 1; i < simplexPoints.Count - 1; i++)
                    {
                        // Koordinaten wie bei bestem team
                        var teamKoordinaten = simplexPoints.First().Team.ToList();

                        // Abstand von bestem zu schlechtestem team bestimmen
                        for (var j = 0; j < state.TeamSize; j++)
                        {
                            var factor = i == j ? -1 : 1;
                            teamKoordinaten[j] = Math.Abs((factor * abstand[j] + teamKoordinaten[j])).LimitToRange(minChampion, maxChampion);
                        }

                        simplexPoints.RemoveAt(i);
                        var team = teamKoordinaten;
                        simplexPoints.Add(new IntTeamValuePair(team, this.teamValueCalculator.CalculateTeamValue(team, enemyChampions)));
                    }

                    durchläufeSeitVerbesserung = 0;
                }

                // Mittelpunkt der besten Teams finden
                var mittelpunkt = new int[state.TeamSize];
                for (var i = 0; i < state.TeamSize; i++)
                {
                    mittelpunkt[i] = Math.Abs(Convert.ToInt32(simplexPoints.Take(simplexPoints.Count - 1).Average(tuple => tuple.Team.ElementAt(i)))).LimitToRange(minChampion, maxChampion);
                }

                // Neues Team bestimmen
                var neuesTeam = new int[state.TeamSize];
                for (var i = 0; i < state.TeamSize; i++)
                {
                    neuesTeam[i] = Math.Abs(Convert.ToInt32(mittelpunkt[i] + Alpha * (mittelpunkt[i] - simplexPoints.Last().Team.ElementAt(i)))).LimitToRange(minChampion, maxChampion);
                }

                // Berechne Wertungen zum vergleich
                var mittelPunktWert = this.teamValueCalculator.CalculateTeamValue(mittelpunkt.ToList(), enemyChampions);
                var neuesTeamWert = this.teamValueCalculator.CalculateTeamValue(neuesTeam, enemyChampions);

                // Neuer Punkt ist Verbesserung zum Mittelpunkt, aber keine Gesamtverbesserung
                if (simplexPoints.First().TeamValue >= neuesTeamWert && neuesTeamWert > mittelPunktWert)
                {
                    simplexPoints.RemoveAt(simplexPoints.Count - 1);
                    simplexPoints.Add(new IntTeamValuePair(neuesTeam, neuesTeamWert));
                    continue;
                }

                // Neuer Punkt ist Verbesserung --> Streckung
                if (simplexPoints.First().TeamValue < neuesTeamWert)
                {
                    // Doppelter Schritt noch besser bestimmen
                    var doppeltNeuesTeam = new int[state.TeamSize];
                    for (var i = 0; i < state.TeamSize; i++)
                    {
                        doppeltNeuesTeam[i] = Math.Abs(Convert.ToInt32(mittelpunkt[i] + Beta * (neuesTeam[i] - mittelpunkt[i]))).LimitToRange(minChampion, maxChampion);
                    }
                    
                    // Wert bestimmen
                    var doppeltNeuesTeamWert = this.teamValueCalculator.CalculateTeamValue(doppeltNeuesTeam, enemyChampions);

                    if (doppeltNeuesTeamWert > neuesTeamWert)
                    {
                        neuesTeam = doppeltNeuesTeam;
                        neuesTeamWert = doppeltNeuesTeamWert;
                    }

                    simplexPoints.RemoveAt(simplexPoints.Count - 1);
                    simplexPoints.Add(new IntTeamValuePair(neuesTeam, neuesTeamWert));
                    durchläufeSeitVerbesserung = 0;
                    continue;
                }

                // Neuer Punkt ist evtl. keine Verbesserung --> Stauchung
                if (simplexPoints.ElementAt(anzahlDimensionen - 1).TeamValue > neuesTeamWert)
                {
                    // Stauchung bestimmt
                    var doppeltNeuesTeam = new int[state.TeamSize];
                    for (var i = 0; i < state.TeamSize; i++)
                    {
                        // Gar keine Verbesserung ?
                        if (neuesTeamWert < simplexPoints.Last().TeamValue)
                        {
                            doppeltNeuesTeam[i] = Math.Abs(Convert.ToInt32(mittelpunkt[i] + Gamma * (simplexPoints.Last().Team.ElementAt(i) - mittelpunkt[i]))).LimitToRange(minChampion, maxChampion);
                        }
                        else
                        {
                            doppeltNeuesTeam[i] = Math.Abs(Convert.ToInt32(mittelpunkt[i] + Gamma * (neuesTeam[i] - mittelpunkt[i]))).LimitToRange(minChampion, maxChampion);
                        }
                    }
                    // Wert bestimmen
                    var doppeltNeuesTeamWert = this.teamValueCalculator.CalculateTeamValue(doppeltNeuesTeam, enemyChampions);

                    // Ist gestauchter Wert jetzt besser?
                    if (doppeltNeuesTeamWert > Math.Min(neuesTeamWert, simplexPoints.Last().TeamValue))
                    {
                        simplexPoints.RemoveAt(simplexPoints.Count - 1);
                        simplexPoints.Add(new IntTeamValuePair(doppeltNeuesTeam, doppeltNeuesTeamWert));
                        continue;
                    }

                    // Simplex zu aktuell bestem team hin stauchen
                    for (var i = 1; i < simplexPoints.Count; i++)
                    {
                        var teamKoordinaten = simplexPoints[i].Team.ToList();

                        for (var j = 0; j < state.TeamSize; j++)
                        {
                            teamKoordinaten[j] = Math.Abs(Convert.ToInt32(Gamma * (simplexPoints.First().Team.ElementAt(j) + teamKoordinaten[j]))).LimitToRange(minChampion, maxChampion);
                        }

                        simplexPoints.RemoveAt(i);
                        var team = teamKoordinaten.ToList();
                        simplexPoints.Add(new IntTeamValuePair(team, this.teamValueCalculator.CalculateTeamValue(team, enemyChampions)));
                    }
                }
            }

            return simplexPoints.First().ToTeamValuePair();
        }

        private void InitiateAvailableChampions(PickingState state)
        {
            var unavailableChampions = state.AlliedPicks.Union(state.Bans).Union(state.EnemyPicks).Select(champ => champ.Id);

            this.availableChampions = this.championSet.Select(champ => champ.Id).Except(unavailableChampions).ToList();

            foreach (var availableChampion in this.availableChampions)
            {
                var value = this.calc.CalculateNotWeaknesses(availableChampion, this.enemyChampions) + this.calc.CalculateStrenghts(availableChampion, this.enemyChampions);

                this.vsPoints.Add(availableChampion, value);
            }

            this.availableChampions = this.vsPoints.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
        }
    }
}