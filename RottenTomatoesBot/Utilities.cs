using Discord;
using Discord.WebSocket;
using System.Net;
using System.Text;
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

        // Get the (custom) Discord emoji based on the meter class
        public static string IconToEmoji(string meterClass)
        {
            meterClass = meterClass.ToLower();
            if (meterClass == "rotten")
                return "<:rotten:477137965672431628>";
            else if (meterClass == "certified_fresh")
                return "<:certifiedfresh:477137965848723477>";
            else if (meterClass == "fresh")
                return "<:tomato:477141676650004481>";
            return "";
        }

        // Print help (available commands and resources)
        public static async Task PrintHelp(ISocketMessageChannel channel)
        {
            StringBuilder text = new StringBuilder()
                .AppendLine("Here are the available commands:")
                .AppendLine()
                .AppendLine("To search for a movie...")
                .AppendLine("*Type `!rt <name of movie>`")
                .AppendLine("*Choose one of the options with `!rt choose <number>`")
                .AppendLine("*To cancel a search `!rt cancel`")
                .AppendLine()
                .AppendLine("To view upcoming movies...")
                .AppendLine("*Type `!rt upcoming`")
                .AppendLine("*Or type `!rt upcoming movies`")
                .AppendLine()
                .AppendLine("To view the top box office...")
                .AppendLine("*Type `!rt box office`")
                .AppendLine()
                .AppendLine("To invite the bot to your server, type `!rt invite`")
                .AppendLine()
                .AppendLine("To view the source code, type `!rt github`");
            await SendEmbed(channel, "Rotten Tomatoes", text.ToString(), false, "Please report issues on my Discord server (!rtdiscord)");
        }

        // Print help for how to use the !rt opening and !rt coming soon commands
        public static async Task SendUpcomingHelp(ISocketMessageChannel Channel)
        {
            StringBuilder text = new StringBuilder()
                .AppendLine("To view movies opening **this week**...")
                .AppendLine("*Type `!rt opening`")
                .AppendLine("*Or `!rt opening this week`")
                .AppendLine()
                .AppendLine("To view movies **coming soon to theaters**...")
                .AppendLine("*Type `!rt coming soon`");
            await SendEmbed(Channel, "Upcoming Movies", text.ToString(), false);
        }

        // DM the invite link to a user
        public static async Task DMInviteLink(SocketGuildUser user, ISocketMessageChannel channel)
        {
            await user.SendMessageAsync("https://discordapp.com/oauth2/authorize?client_id=477287091798278145&scope=bot&permissions=3072");
            await SendEmbed(channel, "Rotten Tomatoes", $"The invite link has been DMed to you, {user.Mention}!", false);
        }
    }
}
