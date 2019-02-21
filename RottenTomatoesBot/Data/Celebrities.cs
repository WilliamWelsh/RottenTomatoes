using System;
using Discord;
using System.Text;
using Discord.WebSocket;
using RottenTomatoes.JSONs;
using System.Threading.Tasks;

namespace RottenTomatoes.Data
{
    static class Celebrities
    {
        public static async Task PrintCeleb(ISocketMessageChannel channel, CelebResult actorToPrint)
        {
            var celeb = ScrapeData(new Celebrity(actorToPrint));

            var movieStats = new StringBuilder()
                .AppendLine($"**Total**: {celeb.MovieCredits}")
                .AppendLine($"<:tomato:477141676650004481> {celeb.FreshCount}")
                .AppendLine($"<:rotten:477137965672431628> {celeb.RottenCount}")
                .AppendLine($"<:certifiedfresh:477137965848723477> {celeb.CertifiedCount}")
                .AppendLine($"**No Score Yet**: {celeb.NoScoreCount}")
                .AppendLine($"**Average Score**: {celeb.AverageScore}%");

            var embed = new EmbedBuilder()
                .WithTitle(celeb.ActorData.Name)
                .WithDescription("")
                .WithThumbnailUrl(celeb.ActorData.Image.ToString())
                .WithColor(Utilities.red)
                .AddField("Highest Rated Movie", celeb.HighestMovie)
                .AddField("Lowest Rated Movie", celeb.LowestMovie)
                .AddField("Movie Stats", movieStats.ToString())
                .AddField("Link", $"[View Full Page]({celeb.URL})");

            await channel.SendMessageAsync(null, false, embed.Build());
        }

        // Get all the data we want on a celebrity
        private static Celebrity ScrapeData(Celebrity celeb)
        {
            string html = Utilities.DownloadString(celeb.URL);

            // Highest Movie & HighestIcon
            #region Highest Movie
            string highestData = Utilities.ScrapeText(ref html, "Highest Rated:", 0, "</a>");

            string highestIcon = Utilities.IconToEmoji(Utilities.ScrapeText(ref highestData, "icon tiny ", 0, "\""));
            string highestScore = Utilities.ScrapeText(ref highestData, "tMeterScore\">", 0, "<");

            celeb.HighestMovie = Utilities.ScrapeText(ref highestData, "unstyled articleLink", 0, "</span>");
            celeb.HighestMovie = $"{highestIcon} {highestScore} {celeb.HighestMovie.Substring(celeb.HighestMovie.IndexOf("<span>") + 6)}";
            #endregion

            // Lowest Movie & HighestIcon
            #region Lowest Movie
            string lowestData = Utilities.ScrapeText(ref html, "Lowest Rated:", 0, "</a>");

            string lowestIcon = Utilities.IconToEmoji(Utilities.ScrapeText(ref lowestData, "icon tiny ", 0, "\""));
            string lowestScore = Utilities.ScrapeText(ref lowestData, "tMeterScore\">", 0, "<");

            celeb.LowestMovie = Utilities.ScrapeText(ref lowestData, "unstyled articleLink", 0, "</span>");
            celeb.LowestMovie = $"{lowestIcon} {lowestScore} {celeb.LowestMovie.Substring(celeb.LowestMovie.IndexOf("<span>") + 6)}";
            #endregion

            // Set all the class counts and average score
            #region Total Movies & Average Score
            int moviesWithScore = 0;
            int totalMovieScore = 0;

            // Movie Credits
            string movies = Utilities.ScrapeText(ref html, "<tbody>", 0, "</tbody");
            do
            {
                celeb.MovieCredits++;
                string row = movies.Substring(0, movies.IndexOf("</tr>"));

                if (row.Contains("noRating"))
                {
                    celeb.NoScoreCount++;
                }
                else
                {
                    moviesWithScore++;
                    // Determine if it's fresh, rotten, or certified fresh
                    string icon = Utilities.ScrapeText(ref row, "icon tiny ", 0, "\"");
                    if (icon == "certified_fresh")
                        celeb.CertifiedCount++;
                    else if (icon == "fresh")
                        celeb.FreshCount++;
                    else
                        celeb.RottenCount++;

                    totalMovieScore += int.Parse(Utilities.ScrapeText(ref row, "tMeterScore\">", 0, "%"));
                }

                // Remove the row we just analyzed
                movies = movies.Substring(movies.IndexOf("</tr>") + 4);
            } while (movies.Contains("td"));

            celeb.AverageScore = totalMovieScore / moviesWithScore;
            #endregion

            return celeb;
        }
    }

    // A celebrity stats
    public class Celebrity : IEquatable<Celebrity>
    {
        public CelebResult ActorData { get; }

        public string URL { get; set; }

        public string HighestMovie { get; set; }
        public string LowestMovie { get; set; }

        public uint MovieCredits { get; set; }

        public int AverageScore { get; set; }

        public uint CertifiedCount { get; set; }
        public uint FreshCount { get; set; }
        public uint RottenCount { get; set; }
        public uint NoScoreCount { get; set; }

        public Celebrity(CelebResult ActorData)
        {
            this.ActorData = ActorData;
            URL = $"https://www.rottentomatoes.com{this.ActorData.Url}";
            MovieCredits = 0;
            CertifiedCount = 0;
            FreshCount  = 0;
            RottenCount = 0;
            NoScoreCount = 0;
        }

        public bool Equals(Celebrity other) => ActorData == other.ActorData;
        public override bool Equals(object obj) => Equals(obj as Celebrity);
        public override int GetHashCode() => 0; // idk
    }
}
