﻿using System;
using Discord;
using System.Net;
using System.Linq;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RottenTomatoes
{
    internal static class Utilities
    {
        // The color red (for embeds).
        public static readonly Color Red = new Color(250, 50, 10);

        // The Rotten Tomatoes logo.
        private const string Logo = "https://cdn.discordapp.com/avatars/477287091798278145/11dac188844056c5dbbdef7015bffc8b.png?size=128";

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

        // Download a website's HTML as a string
        public static string DownloadString(string URL)
        {
            using (var client = new WebClient())
                return client.DownloadString(URL);
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
            await Channel.SendEmbed("Upcoming Movies", text.ToString(), false).ConfigureAwait(false);
        }

        // DM the invite link to a user
        public static async Task DMInviteLink(this ISocketMessageChannel Channel, SocketGuildUser user)
        {
            await user.SendMessageAsync("https://discordapp.com/oauth2/authorize?client_id=477287091798278145&scope=bot&permissions=3072");
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
                .AddField("Members", TotalMemberCount(Client.Guilds).ToString("#,##0"))
                .AddField("Developer", "Reverse#0069")
                .AddField("Color", "Use this suggested color for my role to match the embeds: `#fb3109`")
                //.AddField("Total Votes", (await Config.DblAPI.GetMeAsync()).Points)
                .AddField("Links", "[Invite](https://discordapp.com/oauth2/authorize?client_id=477287091798278145&scope=bot&permissions=3072) | [Vote](\n\nhttps://discordbots.org/bot/477287091798278145/vote) | [GitHub](https://github.com/WilliamWelsh/RottenTomatoes) | [Support Server](https://discord.gg/ga9V5pa)")
                .Build()).ConfigureAwait(false);
        }

        // Get the total number of members on every guild
        private static int TotalMemberCount(IReadOnlyCollection<SocketGuild> Guilds) => Guilds.Sum(Guild => Guild.MemberCount);

        // Convert that stuff to actual characters
        public static string DecodeHTMLStuff(string text) => WebUtility.HtmlDecode(text);

        // Print an error
        public static async Task PrintError(ISocketMessageChannel channel, string description) => await channel.SendEmbed("Error", description, false).ConfigureAwait(false);

        // Cut stuff before in a string
        public static string CutBefore(this string source, string target) =>
            source.Substring(source.IndexOf(target, StringComparison.Ordinal) + target.Length);

        // Cut stuff after in a string
        public static string CutAfter(this string source, string target) =>
            source.Substring(0, source.IndexOf(target, StringComparison.Ordinal));

        // Cut stuff before a string and cut stuff after a string
        public static string CutBeforeAndAfter(this string source, string targetOne, string targetTwo)
        {
            source = CutBefore(source, targetOne);
            return CutAfter(source, targetTwo);
        }
    }
}