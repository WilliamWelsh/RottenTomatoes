using System;
using Discord;
using Newtonsoft.Json;
using HtmlAgilityPack;
using Discord.WebSocket;
using RottenTomatoes.JSONs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RottenTomatoes.Data
{
    static class TVShows
    {
        public static async Task PrintTVShow(ISocketMessageChannel channel, TVResult tvResult)
        {
            var show = ScrapeData(new TVShow(tvResult));

            string endYear = show.Data.EndYear == 0 ? "" : show.Data.EndYear.ToString();
            string score = show.Data.MeterScore == null ? "No Score Yet" : $"{show.Data.MeterScore}%";

            var embed = new EmbedBuilder()
                .WithTitle($"{show.Data.Title} ({show.Data.StartYear} - {endYear})")
                .WithColor(Utilities.red)
                .WithThumbnailUrl(show.Data.Image.ToString())
                .AddField("Average Tomatometer", $"{Utilities.IconToEmoji(show.Data.MeterClass)} {score}")
                .AddField("Average Audience Score", show.AverageAudienceScore)
                .AddField($"Seasons ({show.Seasons.Count})", $"Type `!rt season <number>` to view details on a season.")
                .AddField("Series Info", show.SeriesInfo)
                .AddField("Link", $"[View Full Page]({show.URL})");

            await channel.SendMessageAsync(null, false, embed.Build());
        }

        // Get all the data we want on a show
        private static TVShow ScrapeData(TVShow show)
        {
            string html = Utilities.DownloadString(show.URL);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Set the audience score
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

            show.SeriesInfo = Utilities.DecodeHTMLStuff(doc.GetElementbyId("movieSynopsis").InnerText);

            #region Seasons
            dynamic seasons = doc.DocumentNode.SelectSingleNode("//script[contains(@type, 'application/ld+json')]").InnerText;
            seasons = JsonConvert.DeserializeObject(seasons);
            seasons = seasons.containsSeason;
            foreach (var season in seasons)
            {
                string name = season.name;
                string url = season.url;
                show.Seasons.Add(new TVSeasonItem(name, url)); // dynamic doesn't like putting season.name here :(
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

        public List<TVSeasonItem> Seasons { get; set; }

        public TVShow(TVResult ShowData)
        {
            Data = ShowData;
            URL = $"https://www.rottentomatoes.com{Data.Url.Replace("/s01", "")}";
            Seasons = new List<TVSeasonItem>();
        }

        public bool Equals(TVShow other) => Data == other.Data;
        public override bool Equals(object obj) => Equals(obj as TVShow);
        public override int GetHashCode() => 0; // idk
    }
}
