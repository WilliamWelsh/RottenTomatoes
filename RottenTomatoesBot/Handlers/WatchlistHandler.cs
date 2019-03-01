﻿using System;
using System.IO;
using System.Timers;
using HtmlAgilityPack;
using Discord.Commands;
using Discord.WebSocket;
using RottenTomatoes.JSONs;
using System.Threading.Tasks;

namespace RottenTomatoes.Handlers
{
    class WatchlistHandler
    {
        private DiscordSocketClient Client;

        private WatchlistJSON Watchlist;

        // Set up the timer and start checking for movies
        public async Task SetUp(DiscordSocketClient client)
        {
            Client = client;
            Watchlist = WatchlistJSON.FromJson(File.ReadAllText("Resources/watchlist.json"));
            var Timer = new Timer
            {
                Interval = 60000, // Once a minute
                AutoReset = true,
                Enabled = true
            };
            await CheckAllMovies().ConfigureAwait(false);
            Timer.Elapsed += OnTimerTicked;
        }

        // Every miute, check all the movies for a score
        private async void OnTimerTicked(object sender, ElapsedEventArgs e) => await CheckAllMovies().ConfigureAwait(false);

        // CHeck all the movies for a socre
        private async Task CheckAllMovies()
        {
            for (int i = 0; i < Watchlist.Movies.Count; i++)
            {
                if (CheckForScore(Watchlist.Movies[i].MovieLink))
                {
                    await PrintMovie(Watchlist.Movies[i]).ConfigureAwait(true);
                    RemoveFromWatchList(Watchlist.Movies[i]);
                }
            }
        }
        
        // Add a movie to the watchlist
        public async Task AddToWatchlist(SocketCommandContext Context, string URL)
        {
            var newMovie = new WatchlistMovie
            {
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                MovieLink = URL
            };

            Watchlist.Movies.Add(newMovie);
            UpdateWatchList();

            await PrintMovie(newMovie).ConfigureAwait(false);
            await Context.Channel.SendMessageAsync("The score will be posted in this channel when a score is available.");
        }

        // Remove a movie from the watchlist
        private void RemoveFromWatchList(WatchlistMovie Movie)
        {
            Watchlist.Movies.Remove(Movie);
            UpdateWatchList();
        }

        // Update the watchlist variable and file
        private void UpdateWatchList()
        {
            File.WriteAllText("Resources/watchlist.json", Watchlist.ToJson());
            Watchlist = WatchlistJSON.FromJson(File.ReadAllText("Resources/watchlist.json"));
        }

        // Determine if the movie has a score
        private bool CheckForScore(string URL)
        {
            string html = Utilities.DownloadString(URL);

            // Meter Score
            string meterScore = Utilities.ScrapeText(ref html, "\"cag[score]\":\"", 0, "\",");

            return !string.IsNullOrEmpty(meterScore);
        }

        // Print a movie
        private async Task PrintMovie(WatchlistMovie WatchlistMovie)
        {
            string html = Utilities.DownloadString(WatchlistMovie.MovieLink);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var Movie = new MovieResult();

            // Name and Year
            string NameAndYear = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']").Attributes["content"].Value;
            Movie.Name = NameAndYear.Substring(0, NameAndYear.Length-7);
            Movie.Year = long.Parse(NameAndYear.Replace(Movie.Name, "").Replace(" ", "").Replace("(", "").Replace(")", ""));

            Movie.Url = WatchlistMovie.MovieLink.Replace("https://www.rottentomatoes.com", "");

            Movie.Image = new Uri(doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']").Attributes["content"].Value);

            html = Utilities.ScrapeText(ref html, "window.mpscall = ", 0, ";");

            // Meter Score
            string meterScore = Utilities.ScrapeText(ref html, "\"cag[score]\":\"", 0, "\",");
            if (string.IsNullOrEmpty(meterScore))
                Movie.MeterScore = null;
            else
                Movie.MeterScore = long.Parse(meterScore);

            // Meter Class
            string isCertified = Utilities.ScrapeText(ref html, "\"cag[certified_fresh]\":\"", 0, "\",");
            if (isCertified == "1")
            {
                Movie.MeterClass = "certified_fresh";
            }
            else
            {
                Movie.MeterClass = Utilities.ScrapeText(ref html, "\"cag[fresh_rotten]\":\"", 0, "\",");
                if (Movie.MeterClass == "NA")
                    Movie.MeterClass = "N/A";
            }
            
            var Guild = Client.GetGuild(WatchlistMovie.GuildId);
            await Data.Movies.PrintMovie(Guild.GetTextChannel(WatchlistMovie.ChannelId), Movie);
        }
    }
}
