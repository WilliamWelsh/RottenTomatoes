using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace RottenTomatoes
{
    public class Movie
    {
        public string Name { get; set; }
        public string Year { get; set; }
        public string Url { get; set; }
        public string Poster { get; set; }

        public string CriticScore { get; set; }
        public string CriticScoreIcon { get; set; }
        public string CriticsConsensus { get; set; }

        public string AudienceScore { get; set; }
        public string AudienceIcon { get; set; }

        public Movie()
        {
            CriticScore = "N/A";
            AudienceScore = "N/A";
        }

        public Movie(string searchData)
        {
            CriticScore = "N/A";
            AudienceScore = "N/A";

            Name = searchData.CutBeforeAndAfter("<img alt=\"", "\"");
            Year = searchData.CutBeforeAndAfter("releaseyear=\"", "\"");
            Url = searchData.CutBeforeAndAfter("<a href=\"", "\"").Replace("rotten", "www.rotten");
            Poster = searchData.CutBefore("<img alt=")
                .CutBeforeAndAfter("src=\"", "\"");

            CriticScore = $"{searchData.CutBeforeAndAfter("tomatometerscore=\"", "\"")}%";
            if (CriticScore == "%")
                CriticScore = "N/A";

            switch (searchData.CutBeforeAndAfter("tomatometerstate=\"", "\""))
            {
                case "certified-fresh":
                    CriticScoreIcon = "<:certified_fresh:737761619375030422>";
                    break;

                case "fresh":
                    CriticScoreIcon = "<:fresh:737761619299270737>";
                    break;

                case "rotten":
                    CriticScoreIcon = "<:rotten:737761619299532874>";
                    break;

                default:
                    CriticScoreIcon = "";
                    break;
            }
        }

        public async Task PrintToChannel(ISocketMessageChannel channel)
        {
            // First, get some missing data
            // We need the critic consensus (review), and the audience score and icon

            // Get the HTML & JSON from the RT page
            var rawHTML = await WebUtils.DownloadString(Url);

            var html = new HtmlDocument();
            html.LoadHtml(rawHTML);

            // Get the JSON from the HTML
            dynamic JSON = JsonConvert.DeserializeObject(rawHTML.CutBeforeAndAfter("<script id=\"score-details-json\" type=\"application/json\">", "</script>"));

            // Audience Score
            AudienceScore = JSON.modal.audienceScoreAll.score == null ? "N/A" : $"{JSON.modal.audienceScoreAll.score}%";

            // Audience Score Icon
            switch (JSON.modal.audienceScoreAll.audienceClass.ToString())
            {
                case "upright":
                    AudienceIcon = "<:audienceliked:737761619328761967>";
                    break;

                case "spilled":
                    AudienceIcon = "<:audiencedisliked:737761619416842321>";
                    break;
            }

            // Critic Consensus
            CriticsConsensus = html.Text.Contains("<span data-qa=\"critics-consensus\">") ?
                html.DocumentNode.SelectSingleNode("//p[contains(@class, 'what-to-know__section-body')]").InnerText.Trim().Replace("Read critic reviews", "") :
                "No consensus yet.";

            // Create a pretty embed & send it
            await channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle($"{Name} ({Year})")
                .WithColor(EmbedUtils.Red)
                .WithThumbnailUrl(Poster)
                .AddField("Tomatometer", $"{CriticScoreIcon} {CriticScore}")
                .AddField("Audience Score", $"{AudienceIcon} {AudienceScore}")
                .AddField("Critics Consensus", CriticsConsensus)
                .AddField("Link", $"[View full page on Rotten Tomatoes]({Url})")
                .WithFooter("Via RottenTomatoes.com")
                .Build());
        }
    }
}