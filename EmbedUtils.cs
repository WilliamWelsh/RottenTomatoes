using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RottenTomatoes
{
    public static class EmbedUtils
    {
        // The Rotten Tomatoes logo (bot's profile picture)
        public const string Logo = "https://cdn.discordapp.com/avatars/477287091798278145/11dac188844056c5dbbdef7015bffc8b.png?size=128";

        // The color red (for embeds).
        public static readonly Color Red = new Color(250, 50, 10);

        // Print an embed
        public static async Task SendEmbed(this ISocketMessageChannel channel, string title, string description, bool showLogo, string footer = null)
        {
            await channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle(title)
                .WithThumbnailUrl(showLogo ? Logo : "")
                .WithDescription(description)
                .WithColor(Red)
                .WithFooter(footer)
                .Build());
        }

        // Print help (available commands and resources)
        public static async Task PrintHelp(this ISocketMessageChannel Channel)
        {
            var text = new StringBuilder()
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
                .AppendLine("*Type `!rt github`")
                .AppendLine()
                .AppendLine("To join the support server or speak to the developer...")
                .AppendLine("*Type `!rt discord`")
                .AppendLine()
                .AppendLine("To view privacy statement...")
                .AppendLine("*Type `!rt privacy`")
                .AppendLine();
            await Channel.SendEmbed("Rotten Tomatoes", text.ToString(), false, "Please report issues on my Discord server (!rt discord)").ConfigureAwait(false);
        }

        // DM the invite link to a user
        public static async Task DMInviteLink(this ISocketMessageChannel Channel, SocketGuildUser user)
        {
            await user.SendMessageAsync("https://discord.com/api/oauth2/authorize?client_id=477287091798278145&permissions=67584&scope=bot%20applications.commands");
            await Channel.SendEmbed("Rotten Tomatoes", $"The invite link has been DMed to you, {user.Mention}!", false).ConfigureAwait(false);
        }

        // Print bot info
        public static async Task PrintBotInfo(this ISocketMessageChannel Channel, DiscordSocketClient Client)
        {
            await Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle("Bot Info")
                .WithColor(Red)
                .WithThumbnailUrl(Logo)
                .AddField("Library", "Discord.Net")
                .AddField("Servers", Client.Guilds.Count)
                .AddField("Members", Client.Guilds.Sum(Guild => Guild.MemberCount).ToString("#,##0"))
                .AddField("Developer", "Reverse#0069")
                .AddField("Color", "Use this suggested color for my role to match the embeds: `#fb3109`")
                //.AddField("Total Votes", (await Config.DblAPI.GetMeAsync()).Points)
                .AddField("Links", "[Invite](https://discord.com/api/oauth2/authorize?client_id=477287091798278145&permissions=67584&scope=bot%20applications.commands) | [Vote](\n\nhttps://discordbots.org/bot/477287091798278145/vote) | [GitHub](https://github.com/WilliamWelsh/RottenTomatoes) | [Support Server](https://discord.gg/ga9V5pa)")
                .Build()).ConfigureAwait(false);
        }
    }
}