using System;
using Discord;
using HtmlAgilityPack;
using Discord.WebSocket;
using RottenTomatoes.JSONs;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace RottenTomatoes.Data
{
    static class TVShows
    {
        public static async Task PrintTVShow(ISocketMessageChannel channel, TVResult tvResult)
        {
            var show = ScrapeData(new TVShow(tvResult));

            string endYear = show.Data.EndYear == 0 ? "" : show.Data.EndYear.ToString();
            string score = show.Data.MeterScore == null ? "No Score Yet" : $"{show.Data.MeterScore}%";

            StringBuilder seasonDescription = new StringBuilder();
            for (int i = 0; i < show.Seasons.Count; i++)
                seasonDescription.AppendLine($"`{i+1}` {show.Seasons[i].Name}");

            var embed = new EmbedBuilder()
                .WithTitle($"{show.Data.Title} ({show.Data.StartYear} - {endYear})")
                .WithColor(Utilities.red)
                .WithThumbnailUrl(show.Data.Image.ToString())
                .AddField("Average Tomatometer", $"{Utilities.IconToEmoji(show.Data.MeterClass)} {score}")
                .AddField("Average Audience Score", show.AverageAudienceScore)
                .AddField("Series Info", show.SeriesInfo)
                .AddField("Seasons", seasonDescription.ToString())
                .AddField("Link", $"[View Full Page]({show.URL})");

            await channel.SendMessageAsync(null, false, embed.Build());
        }

        // Get all the data we want on a show
        private static TVShow ScrapeData(TVShow show)
        {
            string html = Utilities.DownloadString(show.URL);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (doc.DocumentNode.InnerHtml.Contains("superPageFontColor audience-score-align"))
            {
                show.AverageAudienceScore = doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'superPageFontColor audience-score-align')]").InnerText;
                // If that element has an "upright" class, then they liked it, if they don't then it has the "spilled" class
                if (doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'meter media')]").InnerHtml.Contains("upright"))
                    show.AverageAudienceScore = $"<:audienceliked:477141676478038046>{show.AverageAudienceScore} liked";
                else
                    show.AverageAudienceScore = $"<:audiencedisliked:477141676486295562>{show.AverageAudienceScore} liked";
            }
            else
            {
                show.AverageAudienceScore = "No Score Yet";
            }

            show.SeriesInfo = doc.GetElementbyId("movieSynopsis").InnerText;

            #region Seasons
            dynamic seasons = html.Substring(html.IndexOf("<script type=\"application/ld+json\">") + 45);
            seasons = seasons.Substring(0, seasons.IndexOf("<"));
            seasons = JsonConvert.DeserializeObject(seasons);
            Console.WriteLine(seasons.ToString());
            seasons = seasons.containsSeason;
            foreach (var season in seasons)
            {
                Console.WriteLine("there's a season");
                string name = season.name;
                string url = season.url;
                show.Seasons.Add(new TVSeason(name, url));
            }
            show.Seasons.Reverse();
            #endregion
            
            return show;
        }
    }

    // A show stats
    public class TVShow : IEquatable<TVShow>
    {
        public TVResult Data { get; }

        public string URL { get;}

        public string AverageAudienceScore { get; set; }

        public string SeriesInfo { get; set; }

        public List<TVSeason> Seasons { get; set; }

        public TVShow(TVResult ShowData)
        {
            Data = ShowData;
            URL = $"https://www.rottentomatoes.com{Data.Url}";
            Seasons = new List<TVSeason>();
        }

        public bool Equals(TVShow other) => Data == other.Data;
        public override bool Equals(object obj) => Equals(obj as TVShow);
        public override int GetHashCode() => 0; // idk
    }
}
