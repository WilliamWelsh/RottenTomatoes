using System;
using Discord;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RottenTomatoes
{
    class RottenTomatoesHandler
    {
        // To see if it's possible to cancel the selection
        bool isSelectionBeingMade = false;

        // Movie item format
        struct Movie
        {
            public string Name { get; set; }
            public int Year { get; set; }
            public string url { get; set; }
            public string Image { get; set; }
            public string MeterClass { get; set; }
            public int MeterScore { get; set; }
        }

        // Box Office Movie Struct
        struct BoxOfficeMovie { public string meterClass; public string meterScore; public string title; public string moneyMade; }

        // Has to be like this to get the results in a json fromat
        struct SearchResults { public List<Movie> movies { get; set; } }

        // This is the new list made with searched movies ordered by newest to oldest for ease of selection
        List<Movie> movies = new List<Movie>();

        // Reset the handler by clearing the movies and saying there is no selection being made
        private void Reset()
        {
            movies.Clear();
            isSelectionBeingMade = false;
        }

        // !rt cancel (to cancel the current selection)
        private async Task RTCancel(ISocketMessageChannel channel)
        {
            if (isSelectionBeingMade)
            {
                await Utilities.SendEmbed(channel, "Rotten Tomatoes Search", "Selection cancelled.", false);
                Reset();
            }
            else
                await Utilities.SendEmbed(channel, "Rotten Tomatoes Search", "There's no active search on this server.\n\nTo search for a movie...\n*Type `!rt <name of movie>`\n*Choose one of the options with `!rt choose <number>`", false);
        }

        // Search Rotten Tomatoes for movies and create a selection
        public async Task SearchRottenTomatoes(string search, SocketCommandContext context)
        {
            if (search == "cancel")
            {
                await RTCancel(context.Channel);
                return;
            }
            else if (search == "box office")
            {
                await GetTopBoxOffice(context.Channel);
                return;
            }
            else if (search == "help" || string.IsNullOrEmpty(search))
            {
                await PrintHelp(context.Channel);
                return;
            }
            else if (search == "invite")
            {
                await DMInviteLink((SocketGuildUser)context.User, context.Channel);
                return;
            }
            else if (search == "github")
            {
                await Utilities.SendEmbed(context.Channel, "Rotten Tomatoes Source Code", "Here is the source code for this bot:\nhttps://github.com/WilliamWelsh/RottenTomatoes", false);
                return;
            }

            isSelectionBeingMade = true;

            // Clear the list to rewrite current selection
            movies.Clear();

            // Get the website code and cut everything away that isn't the json we want
            string json = Utilities.DownloadString($"https://www.rottentomatoes.com/search/?search={search}");

            // If there's no result, tell the user and then stop.
            if (json.Contains("Sorry, no results found"))
            {
                await Utilities.SendEmbed(context.Channel, "Rotten Tomatoes Search", $"Sorry, no results were found for \"{search}\"\n\nTry reformatting your search if the title contains colons, hyphens, etc.", false);
                return;
            }

            // Get that nice json :)
            json = json.Substring(json.IndexOf($"RT.PrivateApiV2FrontendHost, '{search}', ") + 33 + search.Length);
            json = json.Substring(0, json.IndexOf(");"));

            // Deserialize the json into our list of movies
            SearchResults results = JsonConvert.DeserializeObject<SearchResults>(json);

            // Here we make a list of years so we can order our movies list by release date
            List<int> movieYears = new List<int>();
            foreach (var m in results.movies)
                movieYears.Add(m.Year);

            // Sort the array, then reverse it so it's highest to lowest (newest movies first)
            int[] array = movieYears.ToArray();
            Array.Sort(array);
            Array.Reverse(array);

            // Loop through every movie for each year, and if that movie comes out that year,
            // then add that movie to the movies list so they're in order
            for (int i = 0; i < array.Length; i++)
                for (int n = 0; n < results.movies.Count; n++)
                    if (array[i] == results.movies[n].Year && !movies.Contains(results.movies.ElementAt(n)))
                        movies.Add(results.movies.ElementAt(n));

            // If there's only one movie, go ahead and show that result
            if (movies.Count == 1)
            {
                await TryToSelect(1, context.Channel);
                return;
            }

            // Create the selection text
            // Example: 1 The Avengers: Infinity War 2018
            StringBuilder selection = new StringBuilder();
            foreach (var m in movies)
                selection.AppendLine($"`{movies.IndexOf(m) + 1}` {m.Name} `{m.Year}`");

            // Print our selection embedded
            await Utilities.SendEmbed(context.Channel, "Rotten Tomatoes Search", $"{selection}\n**To select**, type `!rt choose <number>`\n\n**To cancel**, type `!rt cancel`.", false, "Via RottenTomatoes.com");
        }

        // Attempt to select a movie with !rt choose <number>
        public async Task TryToSelect(int selection, ISocketMessageChannel channel)
        {
            if (movies.Count == 0 || !isSelectionBeingMade)
            {
                await Utilities.SendEmbed(channel, "Rotten Tomatoes Search", "There's no active search on this server.\n\nTo search for a movie...\n*Type `!rt <name of movie>`\n*Choose one of the options with `!rt choose <number>`", false);
                return;
            }
            // because lists start at 0
            var movie = movies.ElementAt(selection - 1);

            // Get the website data so we can scrap audience scores and the critic consensus
            string html = Utilities.DownloadString($"https://www.rottentomatoes.com{movie.url}");

            int audienceScore = 0;

            // Cut everything except the audience score information
            string temp = html.Substring(html.IndexOf("#audience_reviews") + 17);
            temp = temp.Substring(temp.IndexOf("vertical-align:top") + 20);

            // Check to see if it's the score or the "want to see" part and set the suffix
            string audienceEmoji = temp.Contains("want to see") ? "<:wanttosee:477141676717113354>" : "";
            string audienceSuffix = temp.Contains("want to see") ? "want to see" : "liked it";

            // Set the audience score/want to see percentage
            int.TryParse(temp.Substring(0, temp.IndexOf("%<")), out audienceScore);

            // If it's not the want to see percentage, set which emoji should be used based on the score
            if (audienceEmoji != "<:wanttosee:477141676717113354>")
                audienceEmoji = audienceScore >= 60 ? "<:audienceliked:477141676478038046>" : "<:audiencedisliked:477141676486295562>";
            
            // Set the critic consensus by scraping the website
            string criticsConsensus;
            if (html.Contains("Critics Consensus:</span>"))
            {
                criticsConsensus = html.Substring(html.IndexOf("Critics Consensus:</span>") + 25);
                // Regex is used here to get rid of html elements because RT uses <em> for italics
                criticsConsensus = Regex.Replace(criticsConsensus.Substring(0, criticsConsensus.IndexOf("</p>")), "<.*?>", string.Empty);
            }
            else
                criticsConsensus = "No consensus yet.";

            // Create a pretty embed
            var Embed = new EmbedBuilder()
                .WithTitle($"{movie.Name} ({movie.Year})")
                .WithColor(Utilities.embedColor)
                .WithThumbnailUrl(movie.Image);

            // If the score is 0 but doesn't have the rotten emoji, it's because it doesn't have a score yet
            string score = (movie.MeterScore == 0 && movie.MeterClass != "rotten") ? "No Score Yet" : $"{movie.MeterScore}%";

            // Add embed fields
            Embed.AddField("Tomatometer", $"{ClassToEmoji(movie.MeterClass)} {score}")
                .AddField("Audience Score", $"{audienceEmoji} {audienceScore}% {audienceSuffix}")
                .AddField("Critics Consensus", criticsConsensus)
                .WithFooter("Via RottenTomatoes.com");

            await channel.SendMessageAsync(null, false, Embed.Build());
        }

        // Display the top 10 movies at the box office provided by Rotten Tomatoes
        private async Task GetTopBoxOffice(ISocketMessageChannel channel)
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
                BoxOfficeMovie newMovie = new BoxOfficeMovie();

                // Get the meter class
                newMovie.meterClass = ScrapeText(ref data, "<span class=\"icon tiny", 1, "\"");

                // Get the meter score
                newMovie.meterScore = ScrapeText(ref data, "tMeterScore\">", 0, "<");

                // Get the movie title
                data = data.Substring(data.IndexOf("\">") + 2);
                data = data.Substring(data.IndexOf("\">") + 2);
                newMovie.title = data.Substring(0, data.IndexOf("<"));

                // Get the money made
                data = data.Substring(data.IndexOf("\">") + 2);
                data = data.Substring(data.IndexOf("\">") + 2);
                newMovie.moneyMade = data.Substring(0, data.IndexOf("<"));

                boxOfficeMovies.Add(newMovie);

                // Get rid of this movies' row from the html data
                data = data.Substring(data.IndexOf("</tr>") + 4);
            }
            while (data.Contains("sidebarInTheaterOpening"));

            // Print all the movies in order
            StringBuilder description = new StringBuilder();
            foreach (var m in boxOfficeMovies)
                description.AppendLine($"{ClassToEmoji(m.meterClass)} {m.meterScore} **{m.title}** {m.moneyMade}").AppendLine();

            await Utilities.SendEmbed(channel, "Top Box Office", description.ToString(), false, "Via RottenTomatoes.com");
        }

        private string ScrapeText(ref string text, string firstTarget, int firstTargetOffset, string lastTarget)
        {
            // The offset integer is for when the first target string has an escape character in it, causing an extra character
            text = text.Substring(text.IndexOf(firstTarget) + firstTarget.Length + firstTargetOffset);
            return text.Substring(0, text.IndexOf(lastTarget));
        }

        // Get the Discord emoji based on the meter class
        private string ClassToEmoji(string meterClass)
        {
            if (meterClass == "rotten")
                return "<:rotten:477137965672431628>";
            else if (meterClass == "certified_fresh")
                return "<:certifiedfresh:477137965848723477>";
            else if (meterClass == "fresh")
                return "<:tomato:477141676650004481>";
            return "";
        }

        // Print help (available commands and resources)
        private async Task PrintHelp(ISocketMessageChannel channel) => await Utilities.SendEmbed(channel, "Rotten Tomatoes", "Here are the available commands:\n\nTo search for a movie...\n*Type `!rt <name of movie>`\n*Choose one of the options with `!rt choose <number>`\n*To cancel a search `!rt cancel`\n\nTo invite the bot to your server, type `!rt invite`\n\nTo view the source code, type `!rt github`", false, "Please report issues on my Discord server (!rtdiscord)");

        private async Task DMInviteLink(SocketGuildUser user, ISocketMessageChannel channel)
        {
            await user.SendMessageAsync("https://discordapp.com/oauth2/authorize?client_id=477287091798278145&scope=bot&permissions=3072");
            await Utilities.SendEmbed(channel, "Rotten Tomatoes", $"The invite link has been DMed to you, {user.Mention}!", false);
        }
    }
}
