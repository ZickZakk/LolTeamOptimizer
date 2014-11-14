namespace LolTeamOptimizer
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text.RegularExpressions;

    #endregion

    internal static class HtmlService
    {
        #region Public Methods and Operators

        public static IEnumerable<string> GatherChapions()
        {
            var client = new WebClient();
            string downloadString = client.DownloadString("http://www.lolcounter.com/champions");

            var regex = new Regex(@"find='\w+'");
            var matches = regex.Matches(downloadString);

            foreach (var match in matches)
            {
                yield return match.ToString()
                    .Remove(0, 6)
                    .Replace("'", "");
            }
        }

        public static IEnumerable<ChampionRelation> GatherStrongAgainst(string champion)
        {
            var client = new WebClient();
            string downloadString = client.DownloadString(String.Concat("http://www.lolcounter.com/champions/", champion, "/strong"));

            // Name freispliten
            var regex = new Regex("<div class=\"name\">");
            var matches = regex.Split(downloadString);

            var name = matches[1].Split('<')[0];

            // UpVotes freispliten
            regex = new Regex("<img src=\"/resources/img/up.png\">");
            matches = regex.Split(matches[1]);

            var upVotes = int.Parse(matches[1].Split('<')[0].Replace(",", ""));

            // DownVotes freisplitten
            regex = new Regex("<img src=\"/resources/img/down.png\">");
            matches = regex.Split(matches[1]);

            var downVotes = int.Parse(matches[1].Split('<')[0].Replace(",", ""));

            yield return new ChampionRelation
                         {
                             ChampionName = name,
                             Value = upVotes - downVotes
                         };
        }

        #endregion
    }

    internal class ChampionRelation
    {
        #region Public Properties

        public string ChampionName { get; set; }

        public int Value { get; set; }

        #endregion
    }
}