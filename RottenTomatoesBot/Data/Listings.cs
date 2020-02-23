using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    static class Listings
    {
        // Print the top 10 movies at the box office: 
        public static async Task SendTopBoxOffice(ISocketMessageChannel Channel) => await TopBoxOffice.SendTopBoxOffice(Channel);

        // Send the top 10 upcoming movies this week
        public static async Task SendUpcomingMoviesThisWeek(ISocketMessageChannel Channel)
        {
            // Get the website data
            string data = Utilities.DownloadString("https://www.rottentomatoes.com/browse/opening");

            // Scrape everyting away except for the JSON
            data = data.Substring(data.IndexOf("[{\"id\":"));
            data = data.Substring(0, data.IndexOf("]}]") + 3);

            var Movies = JSONs.MoviesOpeningThisWeek.FromJson(data);

            // Format the results (only show the first 10)
            int amount = Movies.Length > 10 ? 10 : Movies.Length;
            StringBuilder results = new StringBuilder();
            for (int i = 0; i < amount; i++)
            {
                string IconAndScore = Movies[i].TomatoIcon.ToString() == "NA" ? "(No CriticScore Yet)" : $"{Utilities.IconToEmoji(Movies[i].TomatoIcon.ToString())} {Movies[i].TomatoScore}%";
                results.AppendLine($"{IconAndScore} **{Movies[i].Title}** {Movies[i].TheaterReleaseDate}");
            }

            await Utilities.SendEmbed(Channel, "Opening This Week", results.ToString(), false, "Via RottenTomatoes.com");
        }

        // Send the top 10 upcoming movies to theaters
        public static async Task SendUpcomingMovies(ISocketMessageChannel Channel)
        {
            // Get the website data
            string data = Utilities.DownloadString("https://www.rottentomatoes.com/browse/upcoming");
            
            // Scrape everyting away except for the JSON
            data = data.Substring(data.IndexOf("[{\"id\":"));
            data = data.Substring(0, data.IndexOf("]}]") + 3);

            var Movies = JSONs.UpComingMovies.FromJson(data);

            // Format the results (only show the first 10)
            StringBuilder results = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                string IconAndScore = Movies[i].TomatoIcon.ToString() == "Na" ? "(No CriticScore Yet)" : $"{Utilities.IconToEmoji(Movies[i].TomatoIcon.ToString())} {Movies[i].TomatoScore}%";
                results.AppendLine($"{IconAndScore} **{Movies[i].Title}** {Movies[i].TheaterReleaseDate}");
            }

            await Utilities.SendEmbed(Channel, "Coming Soon to Theaters", results.ToString(), false, "Via RottenTomatoes.com");
        }
    }
}
