using Discord;
using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RottenTomatoes.Data
{
    static class Movies
    {
        public static async Task PrintMovie(ISocketMessageChannel channel, SearchResultsJSON.Movie movieToPrint)
        {
            var movie = ScrapeSomeData(new MovieData(movieToPrint));

            // Create a pretty embed
            var embed = new EmbedBuilder()
                .WithTitle($"{movie.data.Name} ({movie.data.Year})")
                .WithColor(Utilities.red)
                .WithThumbnailUrl(movie.data.Image.ToString());

            // If the score is 0 but doesn't have the rotten emoji, it's because it doesn't have a score yet
            string score = (movie.data.MeterScore == null && movie.data.MeterClass == "N/A") ? "No Score Yet" : $"{movie.data.MeterScore}%";

            // Add embed fields
            embed.AddField("Tomatometer", $"{Utilities.IconToEmoji(movie.data.MeterClass)} {score}")
                .AddField("Audience Score", $"{movie.audienceEmoji} {movie.audienceScore}% {movie.audienceSuffix}")
                .AddField("Critics Consensus", movie.criticsConsensus)
                .AddField("Link", $"[View full page on Rotten Tomatoes]({movie.url})")
                .WithFooter("Via RottenTomatoes.com");

            await channel.SendMessageAsync(null, false, embed.Build());
        }

        private static MovieData ScrapeSomeData(MovieData movie)
        {
            Console.WriteLine(movie.url);
            // Get the website data so we can scrap audience scores and the critic consensus
            string html = Utilities.DownloadString(movie.url);

            // Cut everything except the audience score information
            string temp = html.Substring(html.IndexOf("#audience_reviews") + 17);
            temp = temp.Substring(temp.IndexOf("vertical-align:top") + 20);

            // Check to see if it's the score or the "want to see" part and set the suffix
            movie.audienceEmoji = temp.Contains("want to see") ? "<:wanttosee:477141676717113354>" : "";
            movie.audienceSuffix = temp.Contains("want to see") ? "want to see" : "liked it";

            // Set the audience score/want to see percentage
            movie.audienceScore = int.Parse(temp.Substring(0, temp.IndexOf("%<")));

            // If it's not the want to see percentage, set which emoji should be used based on the score
            if (movie.audienceEmoji != "<:wanttosee:477141676717113354>")
                movie.audienceEmoji = movie.audienceScore >= 60 ? "<:audienceliked:477141676478038046>" : "<:audiencedisliked:477141676486295562>";

            // Set the critic consensus by scraping the website
            if (html.Contains("Critics Consensus:</span>"))
            {
                movie.criticsConsensus = html.Substring(html.IndexOf("Critics Consensus:</span>") + 25);
                // Regex is used here to get rid of html elements because RT uses <em> for italics
                movie.criticsConsensus = Regex.Replace(movie.criticsConsensus.Substring(0, movie.criticsConsensus.IndexOf("</p>")), "<.*?>", string.Empty);
            }
            else
                movie.criticsConsensus = "No consensus yet.";

            return movie;
        }
    }

    // Movie data
    public class MovieData : IEquatable<MovieData>
    {
        public SearchResultsJSON.Movie data { get; }

        public string score { get; set; }

        public int audienceScore { get; set; }
        public string audienceEmoji { get; set; }
        public string audienceSuffix { get; set; }

        public string criticsConsensus { get; set; }

        public string url { get; set; }

        public MovieData(SearchResultsJSON.Movie Data)
        {
            data = Data;
            url = $"https://www.rottentomatoes.com{data.Url}";
        }

        public bool Equals(MovieData other) => data == other.data;
        public override bool Equals(object obj) => Equals(obj as MovieData);
        public override int GetHashCode() => 0; // idk
    }
}
