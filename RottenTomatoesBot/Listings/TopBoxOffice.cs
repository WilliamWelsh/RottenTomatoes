// The reason I don't use JSON for this is because the JSON does not contain how much money the movie has made
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RottenTomatoes
{
    class TopBoxOffice
    {
        // Box Office Movie Struct
        struct BoxOfficeMovie { public string meterClass; public string meterScore; public string title; public string moneyMade; }

        // Display the top 10 movies at the box office provided by Rotten Tomatoes
        public static async Task SendTopBoxOffice(ISocketMessageChannel Channel)
        {
            // Get the website data
            string data = Utilities.DownloadString("https://www.rottentomatoes.com");

            // Scrape everything away except for the top box office information
            data = ScrapeText(ref data, "<h2>Top Box Office</h2>", 0, "</table>");

            // We'll add every box office movie's data to this list
            List<BoxOfficeMovie> boxOfficeMovies = new List<BoxOfficeMovie>();

            // For each box office movie, get its data, add it to the list, and then remove it from the site data
            do
            {
                BoxOfficeMovie newMovie = new BoxOfficeMovie
                {
                    // Get the meter class
                    meterClass = ScrapeText(ref data, "<span class=\"icon tiny", 1, "\""),

                    // Get the meter score
                    meterScore = ScrapeText(ref data, "tMeterScore\">", 0, "<")
                };

                // Get the movie title
                data = data.Substring(data.IndexOf("\">") + 2);
                data = data.Substring(data.IndexOf("\">") + 2);
                newMovie.title = data.Substring(0, data.IndexOf("<"));

                // Get the money made
                data = data.Substring(data.IndexOf("\">") + 2);
                data = data.Substring(data.IndexOf("\">") + 2);
                newMovie.moneyMade = data.Substring(0, data.IndexOf("<"));

                // Add the new movie to the list
                boxOfficeMovies.Add(newMovie);

                // Get rid of this movies' row from the html data
                data = data.Substring(data.IndexOf("</tr>") + 4);
            }
            while (data.Contains("sidebarInTheaterOpening"));

            // Format the list
            StringBuilder description = new StringBuilder();
            foreach (var m in boxOfficeMovies)
                description.AppendLine($"{Utilities.IconToEmoji(m.meterClass)} {m.meterScore} **{m.title}** {m.moneyMade}").AppendLine();

            // Send the results
            await Utilities.SendEmbed(Channel, "Top Box Office", description.ToString(), false, "Via RottenTomatoes.com");
        }

        private static string ScrapeText(ref string text, string firstTarget, int firstTargetOffset, string lastTarget)
        {
            // The offset integer is for when the first target string has an escape character in it, causing an extra character
            text = text.Substring(text.IndexOf(firstTarget) + firstTarget.Length + firstTargetOffset);
            return text.Substring(0, text.IndexOf(lastTarget));
        }
    }
}
