using System;
using Discord;
using System.Text;
using System.Linq;
using Discord.Rest;
using HtmlAgilityPack;
using Discord.Commands;
using Discord.WebSocket;
using RottenTomatoes.JSONs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RottenTomatoes
{
    // A result that may be a movie, show, or person.
    public class ResultItem : IEquatable<ResultItem>
    {
        public MovieResult Movie { get;}

        public ResultItem(MovieResult Movie)
        {
            this.Movie = Movie;
        }

        public bool Equals(ResultItem other) => Movie == other.Movie;
        public override bool Equals(object obj) => Equals(obj as ResultItem);
        public override int GetHashCode() => 0; // idk
    }

    // Handle searching Rotten Tomatoes.
    public class SearchHandler
    {
        // To see if it's possible to cancel the selection
        bool isSelectionBeingMade;

        // This is the new list made with searched movies ordered by newest to oldest for ease of selection
        List<ResultItem> resultItems = new List<ResultItem>();

        // The message that contains search results (to be delete)
        private RestUserMessage searchMessage;

        // Reset the handler by clearing the movies and saying there is no selection being made
        private void Reset()
        {
            resultItems.Clear();
            isSelectionBeingMade = false;
        }

        // Cancel the current selection
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
                await RTCancel(context.Channel).ConfigureAwait(false);
                return;
            }

            isSelectionBeingMade = true;

            // Clear the list to rewrite current selection
            resultItems.Clear();

            // Get the website html
            string json = Utilities.DownloadString($"https://letterboxd.com/search/films/{search}");

            //If there's no result, tell the user and then stop.
            if (json.Contains("There were no matches for your search term."))
            {
                await Utilities.SendEmbed(context.Channel, "Rotten Tomatoes Search", $"Sorry, no results were found for \"{search}\"\n\nTry reformatting your search if the title contains colons, hyphens, etc.", false);
                return;
            }

            // Get that nice json :)
            json = Utilities.CutBefore(json, "<ul class=\"results\">");
            json = Utilities.CutBeforeAndAfter(json, "<li> ", "</ul>");

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(json);

            var results = html.DocumentNode.SelectNodes("//span[contains(@class, 'film-title-wrapper')]");

            var embed = new EmbedBuilder()
                .WithTitle("Rotten Tomatoes Search")
                .WithColor(Utilities.Red)
                .WithFooter("Via RottenTomatoes.com");

            foreach (var result in results)
            {
                var movie = new MovieResult();

                movie.Name = Utilities.DecodeHTMLStuff(result.SelectSingleNode(".//a/text()").InnerText.Trim());

                if (result.SelectSingleNode(".//small") != null)
                    movie.Year = int.Parse(result.SelectSingleNode(".//small").FirstChild.InnerHtml.Trim());
                else
                    movie.Year = 0;

                movie.Url = "https://letterboxd.com" + result.SelectSingleNode(".//a").Attributes["href"].Value;

                resultItems.Add(new ResultItem(movie));
            }

            // If there's only one result, cut the crap and just print it
            // Waste of time doing `!rt choose `
            if (resultItems.Count == 1)
            {
                await TryToSelect(1, context.Channel).ConfigureAwait(false);
                return;
            }

            // Order the movies by release date
            resultItems = resultItems.OrderBy(r => r.Movie.Year).Reverse().ToList();

            var text = new StringBuilder();
            for (int i = 0; i < resultItems.Count; i++)
                text.AppendLine($"`{i + 1}` {resultItems[i].Movie.Name} {(resultItems[i].Movie.Year == 0 ? "" : "`" + resultItems[i].Movie.Year + "`")}");

            embed.WithDescription(text.ToString());

            searchMessage = await context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        // Attempt to select a movie with !rt choose <number>
        public async Task TryToSelect(int selection, ISocketMessageChannel channel)
        {
            // Check if there is anything to select
            if (resultItems.Count == 0 || !isSelectionBeingMade)
            {
                await Utilities.SendEmbed(channel, "Rotten Tomatoes Search", "There's no active search on this server.\n\nTo search for a movie...\n*Type `!rt <name of movie>`\n*Choose one of the options with `!rt choose <number>`", false);
                return;
            }

            // Because lists start at 0
            await PrintResult(channel, resultItems.ElementAt(selection - 1)).ConfigureAwait(false);

            // Delete the search results message
            await searchMessage.DeleteAsync();
        }

        // Print a result
        public async Task PrintResult(ISocketMessageChannel channel, ResultItem result)
        {
            if (result.Movie != null)
                await Data.Movies.PrintMovie(channel, result.Movie);
        }
    }
}
