using Discord;
using System;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RottenTomatoes.Data
{
    static class Actors
    {
        public static async Task PrintActor(ISocketMessageChannel channel, SearchResultsJSON.Actor actorToPrint)
        {
            var celeb = ScrapeData(new Actor(actorToPrint));

            var movieStats = new StringBuilder()
                .AppendLine($"**Total**: {celeb.movieCredits}")
                .AppendLine($"**Fresh**: {celeb.freshCount}")
                .AppendLine($"**Rotten**: {celeb.rottenCount}")
                .AppendLine($"**Certified Fresh**: {celeb.certifiedCount}")
                .AppendLine($"**No Score Yet**: {celeb.noScoreCount}")
                .AppendLine($"**Average Score**: {celeb.averageScore}%");

            var embed = new EmbedBuilder()
                .WithTitle(celeb.actorData.Name)
                .WithDescription("")
                .WithThumbnailUrl(celeb.actorData.Image.ToString())
                .WithColor(Utilities.red)
                .AddField("Highest Rated Movie", celeb.highestMovie)
                .AddField("Lowest Rated Movie", celeb.lowestMovie)
                .AddField("Movie Stats", movieStats.ToString())
                .AddField("Link", $"[View Full Page]({celeb.url})");

            await channel.SendMessageAsync(null, false, embed.Build());
        }

        // Get all the data we want on a celebrity
        private static Actor ScrapeData(Actor celeb)
        {
            string html = Utilities.DownloadString(celeb.url);

            // Highest Movie & HighestIcon
            #region Highest Movie
            string highestData = Utilities.ScrapeText(ref html, "Highest Rated:", 0, "</a>");

            string highestIcon = Utilities.IconToEmoji(Utilities.ScrapeText(ref highestData, "icon tiny ", 0, "\""));
            string highestScore = Utilities.ScrapeText(ref highestData, "tMeterScore\">", 0, "<");

            celeb.highestMovie = Utilities.ScrapeText(ref highestData, "unstyled articleLink", 0, "</span>");
            celeb.highestMovie = $"{highestIcon} {highestScore} {celeb.highestMovie.Substring(celeb.highestMovie.IndexOf("<span>") + 6)}";
            #endregion

            // Lowest Movie & HighestIcon
            #region Lowest Movie
            string lowestData = Utilities.ScrapeText(ref html, "Lowest Rated:", 0, "</a>");

            string lowestIcon = Utilities.IconToEmoji(Utilities.ScrapeText(ref lowestData, "icon tiny ", 0, "\""));
            string lowestScore = Utilities.ScrapeText(ref lowestData, "tMeterScore\">", 0, "<");

            celeb.lowestMovie = Utilities.ScrapeText(ref lowestData, "unstyled articleLink", 0, "</span>");
            celeb.lowestMovie = $"{lowestIcon} {lowestScore} {celeb.lowestMovie.Substring(celeb.lowestMovie.IndexOf("<span>") + 6)}";
            #endregion

            // Set all the class counts and average score
            #region Total Movies & Average Score
            int moviesWithScore = 0;
            int totalMovieScore = 0;

            // Movie Credits
            string movies = Utilities.ScrapeText(ref html, "<tbody>", 0, "</tbody");
            do
            {
                celeb.movieCredits++;
                string row = movies.Substring(0, movies.IndexOf("</tr>"));

                if (row.Contains("noRating"))
                {
                    celeb.noScoreCount++;
                }
                else
                {
                    moviesWithScore++;
                    // Determine if it's fresh, rotten, or certified fresh
                    string icon = Utilities.ScrapeText(ref row, "icon tiny ", 0, "\"");
                    if (icon == "certified_fresh")
                        celeb.certifiedCount++;
                    else if (icon == "fresh")
                        celeb.freshCount++;
                    else
                        celeb.rottenCount++;

                    totalMovieScore += int.Parse(Utilities.ScrapeText(ref row, "tMeterScore\">", 0, "%"));
                }

                // Remove the row we just analyzed
                movies = movies.Substring(movies.IndexOf("</tr>") + 4);
            } while (movies.Contains("td"));

            celeb.averageScore = totalMovieScore / moviesWithScore;
            #endregion

            return celeb;
        }
    }

    // A celebrity stats
    public class Actor : IEquatable<Actor>
    {
        public SearchResultsJSON.Actor actorData { get; }

        public string url { get; set; }

        public string highestMovie { get; set; }
        public string lowestMovie { get; set; }

        public uint movieCredits { get; set; }

        public int averageScore { get; set; }

        public uint certifiedCount { get; set; }
        public uint freshCount { get; set; }
        public uint rottenCount { get; set; }
        public uint noScoreCount { get; set; }

        public Actor(SearchResultsJSON.Actor ActorData)
        {
            actorData = ActorData;
            url = $"https://www.rottentomatoes.com{actorData.Url}";
            movieCredits = 0;
            certifiedCount = 0;
            freshCount  = 0;
            rottenCount = 0;
            noScoreCount = 0;
        }

        public bool Equals(Actor other) => actorData == other.actorData;
        public override bool Equals(object obj) => Equals(obj as Actor);
        public override int GetHashCode() => 0; // idk
    }
}
