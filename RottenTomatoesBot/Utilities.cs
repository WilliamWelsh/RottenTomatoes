using Discord;
using System.Net;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace RottenTomatoes
{
    static class Utilities
    {
        /// <summary>
        /// The color red (for embeds).
        /// </summary>
        public static readonly Color red = new Color(250, 50, 10);

        /// <summary>
        /// The Rotten Tomatoes logo.
        /// </summary>
        private const string logo = "https://cdn.discordapp.com/avatars/477287091798278145/11dac188844056c5dbbdef7015bffc8b.png?size=128";

        /// <summary>
        /// Print an embed
        /// </summary>
        public static async Task SendEmbed(ISocketMessageChannel channel, string title, string description, bool showLogo, string footer = null)
        {
            await channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle(title)
                .WithThumbnailUrl(showLogo ? logo : "")
                .WithDescription(description)
                .WithColor(red)
                .WithFooter(footer)
                .Build());
        }

        /// <summary>
        /// Download a website's HTML as a string
        /// </summary>
        public static string DownloadString(string URL)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(URL);
            }
        }

        /// <summary>
        /// Get the (custom) Discord emoji based on the meter class
        /// </summary>
        /// <param name="meterClass">The type of score</param>
        /// <returns></returns>
        public static string IconToEmoji(string meterClass)
        {
            meterClass = meterClass.ToLowerInvariant();
            if (meterClass == "rotten")
                return "<:rotten:477137965672431628>";
            else if (meterClass == "certified_fresh")
                return "<:certifiedfresh:477137965848723477>";
            else if (meterClass == "fresh")
                return "<:tomato:477141676650004481>";
            return "";
        }

        // Print help (available commands and resources)
        public static async Task PrintHelp(ISocketMessageChannel Channel)
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
                .AppendLine("To invite the bot to your server...")
                .AppendLine("*Type `!rt invite`")
                .AppendLine()
                .AppendLine("To view bot information...")
                .AppendLine("*Type `!rt info` or `!rt stats`")
                .AppendLine()
                .AppendLine("To view the source code...")
                .AppendLine("*Type `!rt github`");
            await SendEmbed(Channel, "Rotten Tomatoes", text.ToString(), false, "Please report issues on my Discord server (!rt discord)").ConfigureAwait(false);
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
            await SendEmbed(Channel, "Upcoming Movies", text.ToString(), false).ConfigureAwait(false);
        }

        // DM the invite link to a user
        public static async Task DMInviteLink(SocketGuildUser user, ISocketMessageChannel Channel)
        {
            await user.SendMessageAsync("https://discordapp.com/oauth2/authorize?client_id=477287091798278145&scope=bot&permissions=3072");
            await SendEmbed(Channel, "Rotten Tomatoes", $"The invite link has been DMed to you, {user.Mention}!", false).ConfigureAwait(false);
        }

        // Print bot info
        public static async Task PrintBotInfo(DiscordSocketClient Client, ISocketMessageChannel Channel)
        {
            System.Console.WriteLine("hi");
            await Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle("Bot Info")
                .WithColor(red)
                .WithThumbnailUrl(logo)
                .AddField("Library", "Discord.Net")
                .AddField("Servers", Client.Guilds.Count)
                .AddField("Members", TotalMemberCount(Client.Guilds).ToString("#,##0"))
                .AddField("Owner", "Reverse#0001")
                //.AddField("Total Votes", (await Config.DblAPI.GetMeAsync()).Points)
                .AddField("Links", "[Invite](https://discordapp.com/oauth2/authorize?client_id=477287091798278145&scope=bot&permissions=3072) | [Vote](\n\nhttps://discordbots.org/bot/477287091798278145/vote) | [GitHub](https://github.com/WilliamWelsh/RottenTomatoes) | [Support Server](https://discord.gg/n2AFRtu) ")
                .Build()).ConfigureAwait(false);
        }

        // Get the total number of members on every guild
        private static int TotalMemberCount(IReadOnlyCollection<SocketGuild> Guilds)
        {
            int total = 0;
            foreach (var Guild in Guilds)
                total += Guild.MemberCount;
            return total;
        }

        // Scrape Text from before and after
        public static string ScrapeText(ref string text, string firstTarget, int firstTargetOffset, string lastTarget)
        {
            // The offset integer is for when the first target string has an escape character in it, causing an extra character
            text = text.Substring(text.IndexOf(firstTarget) + firstTarget.Length + firstTargetOffset);
            return text.Substring(0, text.IndexOf(lastTarget));
        }

        /// <summary>
        /// Convert that stuff to actual characters
        /// </summary>
        /// <param name="text">Text containing HTML entities</param>
        /// <returns></returns>
        public static string DecodeHTMLStuff(string text) => WebUtility.HtmlDecode(text);
    }
}
