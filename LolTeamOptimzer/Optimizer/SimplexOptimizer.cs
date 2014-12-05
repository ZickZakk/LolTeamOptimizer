﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace LolTeamOptimizer.Optimizer
{
    public class SimplexOptimizer : ITeamOptimizer
    {
        private Database database = new Database();

        public IEnumerable<Champion> CalculateOptimalePicks(PickingState state)
        {
            // Simplex Parameter Initialisierung
            int anzahlDimensionen = state.TeamSize;

            const int Alpha = 1;
            const int Beta = 1;
            const double Gamma = 0.5;

            int dauerOhneAenderung = anzahlDimensionen * 2;
            int dauerBisAbbruch = 1000;

            // Tuple = Team + Wert
            var simplexPoints = new List<TeamValuePair>(anzahlDimensionen + 1);

            // Champmenge bestimmen
            var unavailableChampionIds = state.AlliedPicks.Union(state.Bans).Union(state.EnemyPicks).Select(champ => champ.Id);

            var availableChampionIds = database.Champions.Select(chmap => chmap.Id).Except(unavailableChampionIds).ToList();
            var availableChampions = availableChampionIds.Select(id => database.Champions.Find(id)).ToList();
            
            // Mappings werden benötigt, damit durchgängiger Definitionsbereich
            var championRepresentation = 0;
            var integerChampionMapping = availableChampions.ToDictionary(champion => championRepresentation++);
            var championIntegerMapping = integerChampionMapping.Select(pair => pair.Key).ToDictionary(integer => integerChampionMapping[integer]);

            // Startpunkte festlegen
            int schrittweite = availableChampions.Count / (anzahlDimensionen + 1);
            for (int i = 0; i < anzahlDimensionen + 1; i++)
            {
                // Wechsle den letzten Champ in dem Team aus
                var team = availableChampions.Skip(schrittweite * i).Take(state.TeamSize).ToList();

                simplexPoints.Add(new TeamValuePair(team, TeamValueCalculator.CalculateTeamValue(team, state.EnemyPicks)));
            }

            int durchläufe = 0;
            int durchläufeSeitVerbesserung  = 0;
            int festgefahren = 0;
            while (true)
            {
                durchläufe++;
                durchläufeSeitVerbesserung ++;

                // Teams nach Wert sortieren
                simplexPoints = simplexPoints.OrderByDescending(tuple => tuple.TeamValue).ToList();

                // Lange keine pos. Änderung
                if (durchläufeSeitVerbesserung  > dauerOhneAenderung)
                {
                    festgefahren++;
                    if (festgefahren > dauerBisAbbruch)
                    {
                        break;
                    }

                    var abstand = new int[state.TeamSize];
                    // Abstand von bestem zu schlechtestem team bestimmen
                    for (int i = 0; i < state.TeamSize; i++)
                    {
                        abstand[i] = Convert.ToInt32(Math.Abs(championIntegerMapping[simplexPoints.First().Team.ElementAt(i)] - championIntegerMapping[simplexPoints.Last().Team.ElementAt(i)]));
                    }

                    // Simplex gleichmäßig um bestes Team verteilen
                    for (int i = 1; i < simplexPoints.Count - 1; i++)
                    {
                        // Koordinaten wie bei bestem team
                        var teamKoordinaten = simplexPoints.First().Team.Select(champ => championIntegerMapping[champ]).ToArray();

                        // Abstand von bestem zu schlechtestem team bestimmen
                        for (int j = 0; j < state.TeamSize; j++)
                        {
                            var factor = i == j ? -1 : 1;
                            teamKoordinaten[j] = Math.Abs((factor * abstand[j] + teamKoordinaten[j]) % availableChampions.Count);
                        }

                        simplexPoints.RemoveAt(i);
                        var team = teamKoordinaten.Select(integer => integerChampionMapping[integer]).ToList();
                        simplexPoints.Add(new TeamValuePair(team, TeamValueCalculator.CalculateTeamValue(team, state.EnemyPicks)));
                    }

                    durchläufeSeitVerbesserung = 0;
                }

                // Mittelpunkt der besten Teams finden
                var mittelpunktKoordinaten = new int[state.TeamSize];
                for (int i = 0; i < state.TeamSize; i++)
                {
                    mittelpunktKoordinaten[i] = Math.Abs(Convert.ToInt32(simplexPoints.Take(simplexPoints.Count - 1).Average(tuple => championIntegerMapping[tuple.Team.ElementAt(i)])) % availableChampions.Count);
                }

                // Neues Team bestimmen
                var neuesTeamKoordinaten = new int[state.TeamSize];
                for (int i = 0; i < state.TeamSize; i++)
                {
                    neuesTeamKoordinaten[i] = Math.Abs(Convert.ToInt32(mittelpunktKoordinaten[i] + Alpha * (mittelpunktKoordinaten[i] - championIntegerMapping[simplexPoints.Last().Team.ElementAt(i)])) % availableChampions.Count);
                }

                // Champs bestimmen
                var mittelpunktTeam = mittelpunktKoordinaten.Select(integer => integerChampionMapping[integer]);
                var neuesTeam = neuesTeamKoordinaten.Select(integer => integerChampionMapping[integer]).ToList();

                // Berechne Wertungen zum vergleich
                var mittelPunktWert = TeamValueCalculator.CalculateTeamValue(mittelpunktTeam, state.EnemyPicks);
                var neuesTeamWert = TeamValueCalculator.CalculateTeamValue(neuesTeam, state.EnemyPicks);

                // Neuer Punkt ist Verbesserung zum Mittelpunkt, aber keine Gesamtverbesserung
                if (simplexPoints.First().TeamValue >= neuesTeamWert && neuesTeamWert >= mittelPunktWert)
                {
                    simplexPoints.RemoveAt(simplexPoints.Count - 1);
                    simplexPoints.Add(new TeamValuePair(neuesTeam, neuesTeamWert));
                    durchläufeSeitVerbesserung = 0; 
                    continue;
                }

                // Neuer Punkt ist Verbesserung --> Streckung
                if (simplexPoints.First().TeamValue < neuesTeamWert)
                {
                    // Doppelter Schritt noch besser bestimmen
                    var doppeltNeuesTeamKoordinaten = new int[state.TeamSize];
                    for (int i = 0; i < state.TeamSize; i++)
                    {
                        doppeltNeuesTeamKoordinaten[i] = Math.Abs(Convert.ToInt32(mittelpunktKoordinaten[i] + Beta * (neuesTeamKoordinaten[i] - mittelpunktKoordinaten[i])) % availableChampions.Count);
                    }

                    // Champs bestimmen
                    var doppeltNeuesTeam = doppeltNeuesTeamKoordinaten.Select(integer => integerChampionMapping[integer]).ToList();

                    // Wert bestimmen
                    var doppeltNeuesTeamWert = TeamValueCalculator.CalculateTeamValue(doppeltNeuesTeam, state.EnemyPicks);

                    if (doppeltNeuesTeamWert > neuesTeamWert)
                    {
                        neuesTeam = doppeltNeuesTeam;
                        neuesTeamWert = doppeltNeuesTeamWert;
                    }

                    simplexPoints.RemoveAt(simplexPoints.Count - 1);
                    simplexPoints.Add(new TeamValuePair(neuesTeam, neuesTeamWert));
                    durchläufeSeitVerbesserung = 0; 
                    continue;
                }

                // Neuer Punkt ist evtl. keine Verbesserung --> Stauchung
                if (simplexPoints.ElementAt(anzahlDimensionen - 1).TeamValue < neuesTeamWert)
                {
                    // Stauchung bestimmt
                    var doppeltNeuesTeamKoordinaten = new int[state.TeamSize];
                    for (int i = 0; i < state.TeamSize; i++)
                    {
                        // Gar keine Verbesserung ?
                        if (neuesTeamWert < simplexPoints.Last().TeamValue)
                        {
                            doppeltNeuesTeamKoordinaten[i] = Math.Abs(Convert.ToInt32(mittelpunktKoordinaten[i] + Gamma * (championIntegerMapping[simplexPoints.Last().Team.ElementAt(i)] - mittelpunktKoordinaten[i])) % availableChampions.Count);
                        }
                        else
                        {
                            doppeltNeuesTeamKoordinaten[i] = Math.Abs(Convert.ToInt32(mittelpunktKoordinaten[i] + Gamma * (neuesTeamKoordinaten[i] - mittelpunktKoordinaten[i])) % availableChampions.Count);
                        }
                    }

                    // Champs bestimmen
                    var doppeltNeuesTeam = doppeltNeuesTeamKoordinaten.Select(integer => integerChampionMapping[integer]).ToList();

                    // Wert bestimmen
                    var doppeltNeuesTeamWert = TeamValueCalculator.CalculateTeamValue(doppeltNeuesTeam, state.EnemyPicks);

                    // Ist gestauchter Wert jetzt besser?
                    if (doppeltNeuesTeamWert > Math.Min(neuesTeamWert, simplexPoints.Last().TeamValue))
                    {
                        simplexPoints.RemoveAt(simplexPoints.Count - 1);
                        simplexPoints.Add(new TeamValuePair(doppeltNeuesTeam, doppeltNeuesTeamWert));
                        continue;
                    }

                    // Simplex zu aktuell bestem team hin stauchen
                    for (int i = 1; i < simplexPoints.Count; i++)
                    {
                        var teamKoordinaten = simplexPoints[i].Team.Select(champ => championIntegerMapping[champ]).ToArray();

                        for (int j = 0; j < state.TeamSize; j++)
                        {
                            teamKoordinaten[j] = Math.Abs(Convert.ToInt32(Gamma * (championIntegerMapping[simplexPoints.First().Team.ElementAt(j)] + teamKoordinaten[j])) % availableChampions.Count);
                        }

                        simplexPoints.RemoveAt(i);
                        var team = teamKoordinaten.Select(integer => integerChampionMapping[integer]).ToList();
                        simplexPoints.Add(new TeamValuePair(team, TeamValueCalculator.CalculateTeamValue(team, state.EnemyPicks)));
                    }
                }

            }

            return simplexPoints.First().Team;
        }
    }
}