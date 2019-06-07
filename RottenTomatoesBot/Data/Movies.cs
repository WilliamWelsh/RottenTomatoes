using System;
using Discord;
using HtmlAgilityPack;
using Discord.WebSocket;
using RottenTomatoes.JSONs;
using System.Threading.Tasks;

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
                //.AddField("Audience Score", $"{movie.AudienceText}") // TODO: The audience score is loaded with java now, so figure out how to get it
                .AddField("Critics Consensus", movie.CriticsConsensus)
                .AddField("Link", $"[View full page on Rotten Tomatoes]({movie.Url})")
                .WithFooter("Via RottenTomatoes.com");

            await channel.SendMessageAsync(null, false, embed.Build());
        }

        // Scrape movie data
        private static MovieData ScrapeSomeData(MovieData movie)
        {
            string html = Utilities.DownloadString(movie.Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            //if (html.Contains("Audience Score <br /> Not Yet Available"))
            //{
            //    movie.AudienceText = "No Score Yet";
            //}
            //else
            //{
            //    // Check to see if it's the score or the "want to see" part and set the suffix
            //    string audienceSuffix = doc.DocumentNode.SelectNodes("//strong[contains(@class, 'mop-ratings-wrap__text--small')]")[1].InnerText;
                
            //    // Set the audience score/want to see percentage
            //    int audienceScore = int.Parse(doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'mop-ratings-wrap__percentage mop-ratings-wrap__percentage--audience mop-ratings-wrap__percentage--small')]").InnerText.Replace("%", ""));
                
            //    // I think they completely remove the "want to see" related stuff
            //    string audienceEmoji = audienceScore >= 60 ? "<:audienceliked:477141676478038046>" : "<:audiencedisliked:477141676486295562>";

            //    movie.AudienceText = $"{audienceEmoji} {audienceScore}% {audienceSuffix}";
            //}

            if (html.Contains("No consensus yet."))
                movie.CriticsConsensus = "No consensus yet.";
            else
                movie.CriticsConsensus = doc.DocumentNode.SelectSingleNode("//p[contains(@class, 'mop-ratings-wrap__text mop-ratings-wrap__text--concensus')]").InnerText;
            movie.CriticsConsensus = Utilities.DecodeHTMLStuff(movie.CriticsConsensus);

            return movie;
        }
    }

    // Movie data
    public class MovieData : IEquatable<MovieData>
    {
        public MovieResult Data { get; }

        public string Score { get; set; }

        public string AudienceText { get; set; }

        public string CriticsConsensus { get; set; }

        public string Url { get; set; }

        public MovieData(MovieResult data)
        {
            Data = data;
            Url = $"https://www.rottentomatoes.com{Data.Url}";
        }

        public bool Equals(MovieData other) => Data == other.Data;
        public override bool Equals(object obj) => Equals(obj as MovieData);
        public override int GetHashCode() => 0; // idk
    }
}
