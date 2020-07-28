using System;
using Discord;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Discord.WebSocket;
using RottenTomatoes.JSONs;
using System.Threading.Tasks;

namespace RottenTomatoes.Data
{
    static class Movies
    {
        public static async Task PrintMovie(ISocketMessageChannel channel, MovieResult movieToPrint)
        {
            movieToPrint.Url = GetRTLink(movieToPrint.Url);

            if (movieToPrint.Url == "N/A")
            {
                await Utilities.PrintError(channel,
                    "The Rotten Tomatoes link for this film wasn't listed in the database. Sorry!");
                return;
            }

            var movie = new MovieData(movieToPrint);

            // Create a pretty embed
            var embed = new EmbedBuilder()
                .WithTitle($"{movie.Data.Name} ({movie.Data.Year})")
                .WithColor(Utilities.Red)
                .WithThumbnailUrl(movie.Poster)
                .AddField("Tomatometer", $"{movie.CriticScoreIcon} {movie.CriticScore}")
                .AddField("Audience Score", $"{movie.AudienceIcon} {movie.AudienceScore}")
                .AddField("Critics Consensus", movie.CriticsConsensus)
                .AddField("Link", $"[View full page on Rotten Tomatoes]({movie.Data.Url})")
                .WithFooter("Via RottenTomatoes.com");

            await channel.SendMessageAsync(null, false, embed.Build());
        }

        private static string GetRTLink(string letterboxdLink)
        {
            // Get the IMDb ID from the Letterboxd Page 
            var IMDbID = Utilities.DownloadString(letterboxdLink);
            IMDbID = IMDbID.Substring(IMDbID.IndexOf("imdb.com/title/") + 15);
            IMDbID = IMDbID.Substring(0, IMDbID.IndexOf("/"));

            // Get the Rotten Tomatoes link from the OMDb API
            dynamic RTLink = JsonConvert.DeserializeObject(Utilities.DownloadString($"https://www.omdbapi.com/?apikey={Config.bot.OMDbAPIToken}&tomatoes=true&i={IMDbID}"));
            return RTLink.tomatoURL;
        }
    }

    // Movie data
    public class MovieData : IEquatable<MovieData>
    {
        public MovieResult Data { get; }

        public string CriticScore { get; set; }
        public string CriticScoreIcon { get; set; }

        public string AudienceScore { get; set; }
        public string AudienceIcon { get; set; }

        public string CriticsConsensus { get; set; }

        public string Poster { get; set; }

        public MovieData(MovieResult data)
        {
            Data = data;

            string rawHTML = Utilities.DownloadString(Data.Url);

            var html = new HtmlDocument();
            html.LoadHtml(rawHTML);

            // Critic Score
            var score = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'col-sm-17 col-xs-24 score-panel-wrap')]");
            CriticScore = score.SelectSingleNode("//span[contains(@class, 'mop-ratings-wrap__percentage')]").InnerText.Trim();

            // Critic Score Icon
            if (score.InnerHtml.Contains("certified_fresh"))
                CriticScoreIcon = "<:certified_fresh:737761619375030422>";
            else if (score.InnerHtml.Contains("rotten"))
                CriticScoreIcon = "<:rotten:737761619299532874>";
            else if (score.InnerHtml.Contains("fresh"))
                CriticScoreIcon = "<:fresh:737761619299270737>";

            // Audience Score
            AudienceScore = Utilities.CutBefore(rawHTML, "mop-ratings-wrap__half audience-score\">");
            AudienceScore = Utilities.CutBeforeAndAfter(AudienceScore, "mop-ratings-wrap__percentage\">", "<").Replace("\n", "");

            // Audience Score Icon
            var audienceScoreNode = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'mop-ratings-wrap__half audience-score')]");
            if (audienceScoreNode.InnerHtml.Contains("upright"))
                AudienceIcon = "<:audienceliked:737761619328761967>";
            else if (audienceScoreNode.InnerHtml.Contains("spilled"))
                AudienceIcon = "<:audiencedisliked:737761619416842321>";
            else
                AudienceIcon = "";

            // Critic Consensus
            if (html.Text.Contains("mop-ratings-wrap__text mop-ratings-wrap__text--concensus"))
                CriticsConsensus = html.DocumentNode.SelectSingleNode("//p[contains(@class, 'mop-ratings-wrap__text mop-ratings-wrap__text--concensus')]").InnerText.Trim();
            else
                CriticsConsensus = "No consensus yet.";

            // Poster
            Poster = html.DocumentNode.SelectSingleNode("//img[contains(@class, 'posterImage js-lazyLoad')]").Attributes["data-src"].Value;
        }

        public bool Equals(MovieData other) => Data == other.Data;
        public override bool Equals(object obj) => Equals(obj as MovieData);
        public override int GetHashCode() => 0; // idk
    }
}
