using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using RottenTomatoes.JSONs;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace RottenTomatoes.Data
{
    static class Movies
    {
        public static async Task PrintMovie(ISocketMessageChannel channel, MovieResult movieToPrint)
        {
            var movie = ScrapeSomeData(new MovieData(movieToPrint));

            // Create a pretty embed
            var embed = new EmbedBuilder()
                .WithTitle($"{movie.Data.Name} ({movie.Data.Year})")
                .WithColor(Utilities.red)
                .WithThumbnailUrl(movie.Data.Image.ToString());

            // If the score is 0 but doesn't have the rotten emoji, it's because it doesn't have a score yet
            string score = (movie.Data.MeterScore == null && movie.Data.MeterClass == "N/A") ? "No Score Yet" : $"{movie.Data.MeterScore}%";
            
            // Add embed fields
            embed.AddField("Tomatometer", $"{Utilities.IconToEmoji(movie.Data.MeterClass)} {score}")
                .AddField("Audience Score", $"{movie.AudienceText}")
                .AddField("Critics Consensus", movie.criticsConsensus)
                .AddField("Link", $"[View full page on Rotten Tomatoes]({movie.url})")
                .WithFooter("Via RottenTomatoes.com");

            await channel.SendMessageAsync(null, false, embed.Build());
        }

        // Scrape movie data
        private static MovieData ScrapeSomeData(MovieData movie)
        {
            string html = Utilities.DownloadString(movie.url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (html.Contains("Audience Score <br /> Not Available"))
            {
                movie.AudienceText = "No Score Yet";
            }
            else
            {
                // Check to see if it's the score or the "want to see" part and set the suffix
                movie.AudienceSuffix = doc.DocumentNode.SelectNodes("//strong[contains(@class, 'mop-ratings-wrap__text--small')]")[1].InnerText;

                // Set the audience score/want to see percentage
                movie.AudienceScore = int.Parse(doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'mop-ratings-wrap__percentage mop-ratings-wrap__percentage--audience mop-ratings-wrap__percentage--small')]").InnerText.Replace("%", ""));

                // If it's not the want to see percentage, set which emoji should be used based on the score
                if (movie.AudienceEmoji != "<:wanttosee:477141676717113354>")
                    movie.AudienceEmoji = movie.AudienceScore >= 60 ? "<:audienceliked:477141676478038046>" : "<:audiencedisliked:477141676486295562>";

                movie.AudienceText = $"{movie.AudienceEmoji} {movie.AudienceScore}% {movie.AudienceSuffix}";
            }

            movie.criticsConsensus = doc.DocumentNode.SelectSingleNode("//p[contains(@class, 'mop-ratings-wrap__text mop-ratings-wrap__text--concensus')]").InnerText;
            movie.criticsConsensus = Regex.Replace(movie.criticsConsensus, "<.*?>", string.Empty);

            return movie;
        }
    }

    // Movie data
    public class MovieData : IEquatable<MovieData>
    {
        public MovieResult Data { get; }

        public string Score { get; set; }

        public int AudienceScore { get; set; }
        public string AudienceEmoji { get; set; }
        public string AudienceSuffix { get; set; }
        public string AudienceText { get; set; }

        public string criticsConsensus { get; set; }

        public string url { get; set; }

        public MovieData(MovieResult data)
        {
            Data = data;
            url = $"https://www.rottentomatoes.com{Data.Url}";
        }

        public bool Equals(MovieData other) => Data == other.Data;
        public override bool Equals(object obj) => Equals(obj as MovieData);
        public override int GetHashCode() => 0; // idk
    }
}
