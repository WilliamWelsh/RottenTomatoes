using System;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RottenTomatoes
{
    static class TopBoxOffice
    {
        // Display the top 10 movies at the box office provided by Rotten Tomatoes
        public static async Task SendTopBoxOffice(ISocketMessageChannel Channel)
        {
            // Get the website data
            string data = Utilities.DownloadString("https://www.rottentomatoes.com");

            // Scrape everything away except for the top box office information
            data = Utilities.ScrapeText(ref data, "<h2>Top Box Office</h2>", 0, "</table>");

            // We'll add every box office movie's data to this list
            List<BoxOfficeMovie> boxOfficeMovies = new List<BoxOfficeMovie>();

            // For each box office movie, get its data, add it to the list, and then remove it from the site data
            do
            {
                BoxOfficeMovie newMovie = new BoxOfficeMovie
                {
                    // Get the meter class
                    MeterClass = Utilities.ScrapeText(ref data, "<span class=\"icon tiny", 1, "\""),

                    // Get the meter score
                    MeterScore = Utilities.ScrapeText(ref data, "tMeterScore\">", 0, "<")
                };

                // Get the movie title
                data = data.Substring(data.IndexOf("\">") + 2);
                data = data.Substring(data.IndexOf("\">") + 2);
                newMovie.Title = data.Substring(0, data.IndexOf("<"));

                // Get the money made
                data = data.Substring(data.IndexOf("\">") + 2);
                data = data.Substring(data.IndexOf("\">") + 2);
                newMovie.MoneyMade = data.Substring(0, data.IndexOf("<"));

                // Add the new movie to the list
                boxOfficeMovies.Add(newMovie);

                // Get rid of this movies' row from the html data
                data = data.Substring(data.IndexOf("</tr>") + 4);
            }
            while (data.Contains("sidebarInTheaterOpening"));

            // Format the list
            StringBuilder description = new StringBuilder();
            foreach (var m in boxOfficeMovies)
                description.AppendLine($"{Utilities.IconToEmoji(m.MeterClass)} {m.MeterScore} **{m.Title}** {m.MoneyMade}").AppendLine();

            // Send the results
            await Utilities.SendEmbed(Channel, "Top Box Office", description.ToString(), false, "Via RottenTomatoes.com");
        }
    }

    // Movie data for a single movie on the top box office list
    public class BoxOfficeMovie : IEquatable<BoxOfficeMovie>
    {
        public string MeterClass { get; set; }
        public string MeterScore { get; set; }
        public string Title { get; set; }
        public string MoneyMade { get; set; }

        public bool Equals(BoxOfficeMovie other) => Title == other.Title;
        public override bool Equals(object obj) => Equals(obj as BoxOfficeMovie);
        public override int GetHashCode() => 0; // idk
    }
}
