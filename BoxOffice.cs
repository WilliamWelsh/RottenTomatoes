using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace RottenTomatoes
{
    public static class BoxOffice
    {
        public static async Task Print(ISocketMessageChannel channel)
        {
            var data = await WebUtils.DownloadString("https://www.rottentomatoes.com/browse/in-theaters?minTomato=0&maxTomato=100&genres=1;2;4;5;6;8;9;10;11;13;18;14&sortBy=popularity");

            data = data.CutBeforeAndAfter("document.getElementById('main-row')", "mps,")
                .CutBefore("},")
                .CutBefore("},");

            data = data.Substring(0, data.LastIndexOf("]") + 1);

            dynamic resultItems = JsonConvert.DeserializeObject(data);

            var result = new StringBuilder();

            string icon;

            for (int i = 0; i < 10; i++)
            {
                switch (resultItems[i].tomatoIcon.ToString())
                {
                    case "certified_fresh":
                        icon = "<:certified_fresh:737761619375030422>";
                        break;

                    case "fresh":
                        icon = "<:fresh:737761619299270737>";
                        break;

                    case "rotten":
                        icon = "<:rotten:737761619299532874>";
                        break;

                    default:
                        icon = "";
                        break;
                }

                result.AppendLine($"`{i + 1}` {resultItems[i].title} `{resultItems[i].theaterReleaseDate}` {resultItems[i].tomatoScore}% {icon}");
            }

            await channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle("Top Box Office")
                .WithColor(EmbedUtils.Red)
                .WithFooter("Via RottenTomatoes.com")
                .WithDescription(result.ToString())
                .Build());
        }
    }
}