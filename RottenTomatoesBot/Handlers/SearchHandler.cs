using System;
using Discord;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using RottenTomatoes.JSONs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RottenTomatoes
{
    // A result item
    public class ResultItem : IEquatable<ResultItem>
    {
        public uint ResultNumber { get; }

        public MovieResult Movie { get;}
        public TVResult TVShow { get;}
        public CelebResult Celeb { get;}

        public ResultItem(uint ResultNumber, MovieResult Movie, TVResult TVShow, CelebResult Celebrity)
        {
            this.ResultNumber = ResultNumber;
            this.Movie = Movie;
            this.TVShow = TVShow;
            Celeb = Celebrity;
        }

        public bool Equals(ResultItem other) => ResultNumber == other.ResultNumber;
        public override bool Equals(object obj) => Equals(obj as ResultItem);
        public override int GetHashCode() => 0; // idk
    }

    // Search Rotten Tomatoes
    public class SearchHandler
    {
        // To see if it's possible to cancel the selection
        bool isSelectionBeingMade;

        // This is the new list made with searched movies ordered by newest to oldest for ease of selection
       readonly List<ResultItem> resultItems = new List<ResultItem>();

        // Reset the handler by clearing the movies and saying there is no selection being made
        private void Reset()
        {
            resultItems.Clear();
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
                await RTCancel(context.Channel).ConfigureAwait(false);
                return;
            }

            isSelectionBeingMade = true;

            // Clear the list to rewrite current selection
            resultItems.Clear();

            // Get the website html
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

            var results = SearchResults.FromJson(json);
            uint resultCount = 0;

            var embed = new EmbedBuilder()
                .WithTitle("Rotten Tomatoes Search")
                .WithColor(Utilities.red)
                .WithFooter("Via RottenTomatoes.com");

            // Deserialize the json into our list of movies
            #region Movie Results
            if (results.MovieCount > 0)
            {
                var movieResults = results.Movies;

                // Here we make a list of years so we can order our movies list by release date
                List<long> movieYears = new List<long>();
                foreach (var m in movieResults)
                    movieYears.Add(m.Year);

                // Sort the array, then reverse it so it's highest to lowest (newest movies first)
                long[] array = movieYears.ToArray();
                Array.Sort(array);
                Array.Reverse(array);

                var Movies = new List<MovieResult>();
                // Loop through every movie for each year, and if that movie comes out that year,
                // then add that movie to the movies list so they're in order
                for (int i = 0; i < array.Length; i++)
                    for (int n = 0; n < movieResults.Length; n++)
                        if (array[i] == movieResults[n].Year && !Movies.Contains(movieResults.ElementAt(n)))
                        {
                            resultCount++;
                            resultItems.Add(new ResultItem(resultCount, movieResults.ElementAt(n), null, null));
                            Movies.Add(movieResults.ElementAt(n));
                        }

                // If there's only one movie, go ahead and show that result
                if (Movies.Count == 1)
                {
                    await TryToSelect(1, context.Channel).ConfigureAwait(false);
                    return;
                }

                // Create the selection text
                // Example: 1 The Avengers: Infinity War 2018
                StringBuilder selection = new StringBuilder();
                foreach (var m in resultItems)
                    selection.AppendLine($"`{resultItems.IndexOf(m) + 1}` {m.Movie.Name} `{m.Movie.Year}`");

                embed.AddField("Movies", selection);
            }
            #endregion

            #region TV Show Results
            if (results.TvCount > 0)
            {
                var tvResults = results.TvSeries;
                foreach (var tvResult in tvResults)
                {
                    resultCount++;
                    resultItems.Add(new ResultItem(resultCount, null, tvResult, null));
                }

                StringBuilder selection = new StringBuilder();
                foreach (var result in resultItems)
                    if (result.TVShow != null)
                        selection.AppendLine($"`{resultItems.IndexOf(result) + 1}` {result.TVShow.Title} `{result.TVShow.StartYear} - {(result.TVShow.EndYear == 0 ? "" : result.TVShow.EndYear.ToString())}`");

                embed.AddField("TV Shows", selection.ToString());
            }
            #endregion

            #region Celebrity Results
            if (results.ActorCount > 0)
            {
                var celebResults = results.Actors;
                foreach (var celebrity in celebResults)
                {
                    resultCount++;
                    resultItems.Add(new ResultItem(resultCount, null, null, celebrity));
                }

                StringBuilder selection = new StringBuilder();
                foreach (var result in resultItems)
                    if (result.Celeb != null)
                        selection.AppendLine($"`{resultItems.IndexOf(result) + 1}` {result.Celeb.Name}");

                embed.AddField("Celebrities", selection.ToString());
            }
            #endregion

            // If there is only one result, then display it
            if (resultItems.Count == 1)
                await PrintResult(context.Channel, resultItems.ElementAt(0)).ConfigureAwait(false);
            else // Otherwise, send all the results
                await context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        // Attempt to select a movie with !rt choose <number>
        public async Task TryToSelect(int selection, ISocketMessageChannel channel)
        {
            if (resultItems.Count == 0 || !isSelectionBeingMade)
            {
                await Utilities.SendEmbed(channel, "Rotten Tomatoes Search", "There's no active search on this server.\n\nTo search for a movie...\n*Type `!rt <name of movie>`\n*Choose one of the options with `!rt choose <number>`", false);
                return;
            }

            // because lists start at 0
            await PrintResult(channel, resultItems.ElementAt(selection - 1)).ConfigureAwait(false);
        }

        // Watch a score to see if it gets updated, or released
        public async Task WatchScore(int selection, ISocketMessageChannel channel)
        {
            if (resultItems.Count == 0 || !isSelectionBeingMade)
            {
                await Utilities.SendEmbed(channel, "Rotten Tomatoes Search", "There's no active search on this server.\n\nTo search for a movie...\n*Type `!rt <name of movie>`\n*Choose one of the options with `!rt choose <number>`", false);
                return;
            }

            // because lists start at 0
            var result = resultItems.ElementAt(selection - 1);

            if (result.Movie != null)
            {
                await Data.Movies.PrintMovie(channel, result.Movie);
            }
            else if (result.Celeb != null)
            {
                await Utilities.SendEmbed(channel, "Rotten Tomatoes Search", "You cannot watch a score on a celebrity.", false);
            }
        }

        // Print a result
        public async Task PrintResult(ISocketMessageChannel channel, ResultItem result)
        {
            if (result.Movie != null)
                await Data.Movies.PrintMovie(channel, result.Movie);
            else if (result.TVShow != null)
                await Data.TVShows.PrintTVShow(channel, result.TVShow);
            else if (result.Celeb != null)
                await Data.Celebrities.PrintCeleb(channel, result.Celeb);
        }
    }
}
