using Discord;
using System.Net;
using Discord.WebSocket;
using System.Threading.Tasks;
namespace RottenTomatoes
{
    class Utilities
    {
        // Red color for all embeds
        public static Color embedColor = new Color(250, 50, 10);

        // RT Logo
        private const string logo = "https://cdn.discordapp.com/avatars/477287091798278145/11dac188844056c5dbbdef7015bffc8b.png?size=128";

        // Send an embed
        public static async Task SendEmbed(ISocketMessageChannel channel, string title, string description, bool showLogo, string footer = null)
        {
            await channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle(title)
                .WithThumbnailUrl(showLogo ? logo : "")
                .WithDescription(description)
                .WithColor(embedColor)
                .WithFooter(footer)
                .Build());
        }

        // Download a url into a string and then dispose the web client
        public static string DownloadString(string url)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}
