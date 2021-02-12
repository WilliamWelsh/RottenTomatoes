using System;
using Discord;
using System.Text;
using System.Linq;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using RottenTomatoes.Data;

namespace RottenTomatoes
{
    // A result that may be a movie, show, or person.
    public class ResultItem : IEquatable<ResultItem>
    {
        public Movie Movie { get; }

        public ResultItem(Movie Movie)
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
        private bool isSelectionBeingMade;

        // This is the new list made with searched movies ordered by newest to oldest for ease of selection
        private List<ResultItem> resultItems = new List<ResultItem>();

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
                await channel.SendEmbed("Rotten Tomatoes Search", "Selection cancelled.", false);
                Reset();
            }
            else
                await channel.SendEmbed("Rotten Tomatoes Search", "There's no active search on this server.\n\nTo search for a movie...\n*Type `!rt <name of movie>`\n*Choose one of the options with `!rt choose <number>`", false);
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
            var json = Utilities.DownloadString($"https://www.rottentomatoes.com/search?search={search}");

            //If there's no result, tell the user and then stop.
            if (json.Contains("Sorry, no results found for"))
            {
                await context.Channel.SendEmbed("Rotten Tomatoes Search", $"Sorry, no results were found for \"{search}\"\n\nTry reformatting your search if the title contains colons, hyphens, etc.", false);
                return;
            }

            // Get that nice json :)
            dynamic data = JsonConvert.DeserializeObject(json.CutBeforeAndAfter("<script id=\"movies-json\" type=\"application/json\">", "</script"));

            var embed = new EmbedBuilder()
                .WithTitle("Rotten Tomatoes Search")
                .WithColor(Utilities.Red)
                .WithFooter("Via RottenTomatoes.com | To choose, do !rt choose <number>");

            foreach (var result in data.items)
            {
                var movie = new Movie
                {
                    Name = result.name,
                    Year = result.releaseYear,
                    Url = result.url,
                    Poster = result.imageUrl
                };

                // Tomatometer Score & Icon
                if (!result.ToString().Contains("tomatometerScore\": {}"))
                {
                    movie.CriticScore = $"{result.tomatometerScore.score}%";

                    if (result.tomatometerScore.certified == true)
                        movie.CriticScoreIcon = "<:certified_fresh:737761619375030422>";

                    if (result.tomatometerScore.certified == false &&
                        result.tomatometerScore.scoreSentiment == "POSITIVE")
                        movie.CriticScoreIcon = "<:fresh:737761619299270737>";

                    if (result.tomatometerScore.scoreSentiment == "NEGATIVE")
                        movie.CriticScoreIcon = "<:rotten:737761619299532874>";
                }

                resultItems.Add(new ResultItem(movie));
            }

            // If there's only one result, cut the crap and just print it
            // Waste of time doing `!rt choose `
            if (resultItems.Count == 1)
            {
                await TryToSelect(1, context.Channel).ConfigureAwait(false);
                return;
            }

            var text = new StringBuilder();
            for (int i = 0; i < resultItems.Count; i++)
                text.AppendLine($"`{i + 1}` {resultItems[i].Movie.Name} `{resultItems[i].Movie.Year}` {resultItems[i].Movie.CriticScore} {resultItems[i].Movie.CriticScoreIcon}");

            embed.WithDescription(text.ToString());

            searchMessage = await context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        // Attempt to select a movie with !rt choose <number>
        public async Task TryToSelect(int selection, ISocketMessageChannel channel)
        {
            // Check if there is anything to select
            if (resultItems.Count == 0 || !isSelectionBeingMade)
            {
                await channel.SendEmbed("Rotten Tomatoes Search", "There's no active search on this server.\n\nTo search for a movie...\n*Type `!rt <name of movie>`\n*Choose one of the options with `!rt choose <number>`", false);
                return;
            }

            // Because lists start at 0
            await PrintResult(channel, resultItems.ElementAt(selection - 1)).ConfigureAwait(false);

            // Delete the search results message
            if (searchMessage != null)
                await searchMessage.DeleteAsync();
        }

        // Print a result
        public async Task PrintResult(ISocketMessageChannel channel, ResultItem result)
        {
            if (result.Movie != null)
                await result.Movie.PrintToChannel(channel);
        }
    }
}