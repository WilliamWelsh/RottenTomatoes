using System;
using Discord;
using HtmlAgilityPack;
using Discord.WebSocket;
using RottenTomatoes.JSONs;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

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

            string rawHTML = Utilities.DownloadString(movieToPrint.Url);

            var html = new HtmlDocument();
            html.LoadHtml(rawHTML);

            // Critic Score
            var score = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'col-sm-17 col-xs-24 score-panel-wrap')]");
            movie.CriticScore = score.SelectSingleNode("//span[contains(@class, 'mop-ratings-wrap__percentage')]").InnerText.Trim();

            // Critic Score Icon
            if (score.InnerHtml.Contains("certified_fresh"))
                movie.CriticScoreIcon = "<:certifiedfresh:477137965848723477>";
            else if (score.InnerHtml.Contains("rotten"))
                movie.CriticScoreIcon = "<:rotten:477137965672431628>";
            else if (score.InnerHtml.Contains("fresh"))
                movie.CriticScoreIcon = "<:tomato:477141676650004481>";

            // Audience Score
            var audienceScore = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'mop-ratings-wrap__half audience-score')]");

            Console.WriteLine("0");
            movie.AudienceScore = rawHTML.Substring(rawHTML.IndexOf("mop-ratings-wrap__half audience-score\">"));
            movie.AudienceScore = movie.AudienceScore.Substring(movie.AudienceScore.IndexOf("mop-ratings-wrap__percentage\">") + 30);
            movie.AudienceScore = movie.AudienceScore.Substring(0, movie.AudienceScore.IndexOf("<"));
            movie.AudienceScore = movie.AudienceScore.Replace("\n", "");


            // Audience Score Icon
            if (audienceScore.InnerHtml.Contains("upright"))
                movie.AudienceIcon = "<:audienceliked:477141676478038046>";
            else if (audienceScore.InnerHtml.Contains("spilled"))
                movie.AudienceIcon = "<:audiencedisliked:477141676486295562>";
            else
            {
                movie.AudienceIcon = "";
            }

            // Critic Consensus
            if (html.Text.Contains("mop-ratings-wrap__text mop-ratings-wrap__text--concensus"))
                movie.CriticsConsensus = html.DocumentNode.SelectSingleNode("//p[contains(@class, 'mop-ratings-wrap__text mop-ratings-wrap__text--concensus')]").InnerText.Trim();
            else
                movie.CriticsConsensus = "No consensus yet.";

            // Poster
            movie.Poster = html.DocumentNode.SelectSingleNode("//img[contains(@class, 'posterImage js-lazyLoad')]").Attributes["data-src"].Value;
            //movie.Poster = movie.Poster.Substring(0, movie.Poster.IndexOf(" "));

            // Create a pretty embed
            var embed = new EmbedBuilder()
                .WithTitle($"{movie.Data.Name} ({movie.Data.Year})")
                .WithColor(Utilities.red)
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
        }

        public bool Equals(MovieData other) => Data == other.Data;
        public override bool Equals(object obj) => Equals(obj as MovieData);
        public override int GetHashCode() => 0; // idk
    }
}
