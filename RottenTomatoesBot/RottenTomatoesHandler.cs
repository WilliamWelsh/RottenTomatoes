using System;
using Discord;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RottenTomatoes
{
    public class RottenTomatoesHandler
    {
        // To see if it's possible to cancel the selection
        bool isSelectionBeingMade;

        // This is the new list made with searched movies ordered by newest to oldest for ease of selection
        List<SearchResultsJSON.Movie> Movies = new List<SearchResultsJSON.Movie>();

        // Reset the handler by clearing the movies and saying there is no selection being made
        private void Reset()
        {
            Movies.Clear();
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
        public async Task SearchRottenTomatoes(string search, SocketCommandContext Context)
        {
            if (search == "cancel")
            {
                await RTCancel(Context.Channel).ConfigureAwait(false);
                return;
            }

            isSelectionBeingMade = true;

            // Clear the list to rewrite current selection
            Movies.Clear();

            // Get the website html
            string json = Utilities.DownloadString($"https://www.rottentomatoes.com/search/?search={search}");

            // If there's no result, tell the user and then stop.
            if (json.Contains("Sorry, no results found"))
            {
                await Utilities.SendEmbed(Context.Channel, "Rotten Tomatoes Search", $"Sorry, no results were found for \"{search}\"\n\nTry reformatting your search if the title contains colons, hyphens, etc.", false);
                return;
            }

            // Get that nice json :)
            json = json.Substring(json.IndexOf($"RT.PrivateApiV2FrontendHost, '{search}', ") + 33 + search.Length);
            json = json.Substring(0, json.IndexOf(");"));

            // Deserialize the json into our list of movies
            var results = SearchResultsJSON.SearchResults.FromJson(json).Movies;

            // Here we make a list of years so we can order our movies list by release date
            List<long> movieYears = new List<long>();
            foreach (var m in results)
                movieYears.Add(m.Year);

            // Sort the array, then reverse it so it's highest to lowest (newest movies first)
            long[] array = movieYears.ToArray();
            Array.Sort(array);
            Array.Reverse(array);

            // Loop through every movie for each year, and if that movie comes out that year,
            // then add that movie to the movies list so they're in order
            for (int i = 0; i < array.Length; i++)
                for (int n = 0; n < results.Length; n++)
                    if (array[i] == results[n].Year && !Movies.Contains(results.ElementAt(n)))
                        Movies.Add(results.ElementAt(n));

            // If there's only one movie, go ahead and show that result
            if (Movies.Count == 1)
            {
                await TryToSelect(1, Context.Channel).ConfigureAwait(false);
                return;
            }

            // Create the selection text
            // Example: 1 The Avengers: Infinity War 2018
            StringBuilder selection = new StringBuilder();
            foreach (var m in Movies)
                selection.AppendLine($"`{Movies.IndexOf(m) + 1}` {m.Name} `{m.Year}`");

            // Print our selection embedded
            await Utilities.SendEmbed(Context.Channel, "Rotten Tomatoes Search", $"{selection}\n**To select**, type `!rt choose <number>`\n\n**To cancel**, type `!rt cancel`.", false, "Via RottenTomatoes.com");
        }

        // Attempt to select a movie with !rt choose <number>
        public async Task TryToSelect(int selection, ISocketMessageChannel channel)
        {
            if (Movies.Count == 0 || !isSelectionBeingMade)
            {
                await Utilities.SendEmbed(channel, "Rotten Tomatoes Search", "There's no active search on this server.\n\nTo search for a movie...\n*Type `!rt <name of movie>`\n*Choose one of the options with `!rt choose <number>`", false);
                return;
            }
            // because lists start at 0
            var Movie = Movies.ElementAt(selection - 1);

            // Get the website data so we can scrap audience scores and the critic consensus
            string html = Utilities.DownloadString($"https://www.rottentomatoes.com{Movie.Url}");

            // Cut everything except the audience score information
            string temp = html.Substring(html.IndexOf("#audience_reviews") + 17);
            temp = temp.Substring(temp.IndexOf("vertical-align:top") + 20);

            // Check to see if it's the score or the "want to see" part and set the suffix
            string audienceEmoji = temp.Contains("want to see") ? "<:wanttosee:477141676717113354>" : "";
            string audienceSuffix = temp.Contains("want to see") ? "want to see" : "liked it";

            // Set the audience score/want to see percentage
            int audienceScore = 0;
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
                .WithTitle($"{Movie.Name} ({Movie.Year})")
                .WithColor(Utilities.embedColor)
                .WithThumbnailUrl(Movie.Image.ToString());

            // If the score is 0 but doesn't have the rotten emoji, it's because it doesn't have a score yet
            string score = (Movie.MeterScore == null && Movie.MeterClass == "N/A") ? "No Score Yet" : $"{Movie.MeterScore}%";

            // Add embed fields
            Embed.AddField("Tomatometer", $"{Utilities.IconToEmoji(Movie.MeterClass)} {score}")
                .AddField("Audience Score", $"{audienceEmoji} {audienceScore}% {audienceSuffix}")
                .AddField("Critics Consensus", criticsConsensus)
                .WithFooter("Via RottenTomatoes.com");

            await channel.SendMessageAsync(null, false, Embed.Build());
        }
    }
}
