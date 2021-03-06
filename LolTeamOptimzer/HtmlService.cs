#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;

using HtmlAgilityPack;

#endregion

namespace LolTeamOptimizer
{

    #region using

    #endregion

    internal static class HtmlService
    {
        #region Public Methods and Operators

        public static IEnumerable<string> GatherChapionNames()
        {
            var client = new WebClient();
            var downloadString = client.DownloadString("http://www.lolcounter.com/champions");

            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString);

            var champNodes = doc.GetElementbyId("all-champions").Descendants("a").Where(div => div.Attributes["href"] != null).Where(div => div.Attributes["href"].Value.Contains("/champions/"));
            foreach (var champNode in champNodes)
            {
                yield return champNode.Attributes["href"].Value.Split('/').Last();
            }
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static IEnumerable<ChampionRelation> GatherChampionRelations(string championName, string relationPageName)
        {
            var client = new WebClient();
            var downloadString = client.DownloadString(string.Concat("http://www.lolcounter.com/champions/", championName, relationPageName));

            var doc = new HtmlDocument();
            doc.LoadHtml(downloadString);

            var champNodes = doc.GetElementbyId("championPage").Descendants("div").Where(div => div.Attributes["class"] != null).Where(div => div.Attributes["class"].Value.Contains("theinfo"));

            foreach (var champNode in champNodes)
            {
                // Gather Champ Name
                var name = champNode.Element("a").Attributes["href"].Value.Split('/').Last().ToLower();

                // Gather Up-Votes
                var upVotesString = ExtractVotesString(champNode, "tag_green");

                var upVotes = Convert.ToInt32(upVotesString);

                // Gather Up-Votes
                var downVotesString = ExtractVotesString(champNode, "tag_red");

                var downVotes = Convert.ToInt32(downVotesString);

                yield return new ChampionRelation { ChampionName = name, Value = upVotes - downVotes };
            }
        }

        private static string ExtractVotesString(HtmlNode champNode, string tag)
        {
            return champNode.Descendants("div").Where(div => div.Attributes["class"] != null).Single(div => div.Attributes["class"].Value.Contains(tag)).InnerText.Split(new[] { '\"' }, StringSplitOptions.RemoveEmptyEntries).Last().Replace(",", string.Empty);
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