using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace RottenTomatoes
{
    public static class InteractionSearchHandler
    {
        // Search Rotten Tomatoes for movies and create a selection
        public static async Task SearchRottenTomatoes(SocketSlashCommand command)
        {
            // Get our input from the interaction
            var search = command.Data.Options.ElementAt(0).Value.ToString();

            // Make a list of results
            var resultItems = new List<SearchResultItem>();

            // Get the website html
            var data = await WebUtils.DownloadString($"https://www.rottentomatoes.com/search?search={search}");

            //If there's no result, tell the user and then stop.
            if (data.Contains("Sorry, no results found for"))
            {
                await command.FollowupAsync(embed: new EmbedBuilder()
                    .WithTitle("Rotten Tomatoes Search")
                    .WithDescription($"Sorry, no results were found for \"{search}\"\n\nTry reformatting your search if the title contains colons, hyphens, etc.")
                    .WithColor(EmbedUtils.Red)
                    .Build());
                return;
            }

            // Slim down the data
            data = data
                .CutBefore(
                    "<search-page-result slot=\"movie\" skeleton=\"panel\" type=\"movie\" data-qa=\"search-result\">")
                .CutBeforeAndAfter("<ul slot=\"list\">", "</ul>");

            do
            {
                var temp = data.CutAfter("</search-page-media-row>");
                resultItems.Add(new SearchResultItem(new Movie(temp)));

                data = data.CutBefore("</search-page-media-row>");
            } while (data.Contains("search-page-media-row"));

            var buttons = new ComponentBuilder();
            for (int i = 0; i < (resultItems.Count <= 5 ? resultItems.Count : 5); i++)
            {
                var text = $"{resultItems[i].Movie.CriticScore} {resultItems[i].Movie.Name} ({resultItems[i].Movie.Year})";

                // Decode the HTML
                text = HttpUtility.HtmlDecode(text);

                // Button Labels can only be 80 characters
                if (text.Length > 80)
                    text = $"{text.Substring(0, 77)}...";

                var customId = resultItems[i].Movie.Url.CutBefore("/m/");

                // Custom IDs can only be 100 characters (skip it otherwise)
                if (customId.Length > 100)
                    continue;

                buttons.WithButton(text, customId: customId, ButtonStyle.Danger, row: i, emote: Emote.Parse(resultItems[i].Movie.CriticScoreIcon));
            }

            await command.FollowupAsync("Please select a result or search again.", component: buttons.Build());
        }

        // Print a movie to an interaction
        public static async Task PrintToInteraction(SocketMessageComponent interaction)
        {
            var movie = new Movie { Url = $"https://www.rottentomatoes.com/m/{interaction.Data.CustomId}" };

            // Get the HTML & JSON from the RT page
            var rawHTML = await WebUtils.DownloadString(movie.Url);

            var html = new HtmlDocument();
            html.LoadHtml(rawHTML);

            // Get the JSON from the HTML
            dynamic JSON = JsonConvert.DeserializeObject(rawHTML.CutBeforeAndAfter("<script id=\"score-details-json\" type=\"application/json\">", "</script>"));

            // Title
            movie.Name = HttpUtility.HtmlDecode(JSON.scoreboard.title.ToString());

            // Tomatometer
            movie.CriticScore = JSON.scoreboard.tomatometerScore == null ? "N/A" : $"{JSON.scoreboard.tomatometerScore}%";

            // Year
            movie.Year = JSON.scoreboard.info.ToString();

            switch (JSON.scoreboard.tomatometerState.ToString())
            {
                case "certified-fresh":
                    movie.CriticScoreIcon = "<:certified_fresh:737761619375030422>";
                    break;

                case "fresh":
                    movie.CriticScoreIcon = "<:fresh:737761619299270737>";
                    break;

                case "rotten":
                    movie.CriticScoreIcon = "<:rotten:737761619299532874>";
                    break;

                default:
                    movie.CriticScoreIcon = "<:notomatometer:891357892417028127>";
                    break;
            }

            // Audience Score
            movie.AudienceScore = JSON.modal.audienceScoreAll.score == null ? "N/A" : $"{JSON.modal.audienceScoreAll.score}%";

            // Audience Score Icon
            switch (JSON.modal.audienceScoreAll.audienceClass.ToString())
            {
                case "upright":
                    movie.AudienceIcon = "<:audienceliked:737761619328761967>";
                    break;

                case "spilled":
                    movie.AudienceIcon = "<:audiencedisliked:737761619416842321>";
                    break;

                default:
                    movie.AudienceIcon = "<:noaudiencescore:891357892442198047>";
                    break;
            }

            // Critic Consensus
            movie.CriticsConsensus = html.Text.Contains("<span data-qa=\"critics-consensus\">") ?
                html.DocumentNode.SelectSingleNode("//p[contains(@class, 'what-to-know__section-body')]").InnerText.Trim().Replace("Read critic reviews", "") :
                "No consensus yet.";

            movie.Poster = rawHTML.CutBefore("class=\"posterImage js-lazy")
                .CutBefore("data-src=\"")
                .CutAfter("\"");

            // Create a pretty embed & send it
            await interaction.UpdateAsync(x =>
            {
                x.Components = new ComponentBuilder().WithButton("View on RottenTomatoes.com", style: ButtonStyle.Link, url: movie.Url).Build();
                x.Content = null;
                x.Embed = new EmbedBuilder()
                    .WithTitle($"{movie.Name} - {movie.Year}")
                    .WithColor(EmbedUtils.Red)
                    .WithThumbnailUrl(movie.Poster)
                    .AddField("Tomatometer", $"{movie.CriticScoreIcon} {movie.CriticScore}")
                    .AddField("Audience Score", $"{movie.AudienceIcon} {movie.AudienceScore}")
                    .AddField("Critics Consensus", movie.CriticsConsensus)
                    .Build();
            });
        }
    }
}