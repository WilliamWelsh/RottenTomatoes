using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    [RequireContext(ContextType.Guild)]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("github")]
        public async Task PrintGitHubLink() => await Context.Channel.SendEmbed("Rotten Tomatoes Source Code", "Here is the source code for this bot:\nhttps://github.com/WilliamWelsh/RottenTomatoes", false);

        [Command("invite")]
        public async Task DMInvite() => await Context.Channel.DMInviteLink((SocketGuildUser)Context.User);

        [Command("help")]
        public async Task PrintHelp() => await Context.Channel.PrintHelp();

        [Command("choose")]
        public async Task SelectRottenTomatoes(int selection)
        {
            foreach (var Server in Config.Servers)
            {
                if (Server.GuildID == Context.Guild.Id)
                {
                    await Server.SearchHandler.TryToSelect(selection, Context.Channel);
                    return;
                }
            }

            var newServer = new ServerHandler(Context.Guild.Id, new SearchHandler());
            await newServer.SearchHandler.TryToSelect(selection, Context.Channel);
            Config.Servers.Add(newServer);
        }

        [Command("stats")]
        [Alias("info")]
        public async Task DisplayStats() => await Context.Channel.PrintBotInfo(Context.Client);

        // Prints a link to my Discord
        // This is so a user can report something to me, or ask for a new feature
        [Command("discord")]
        public async Task RTDiscord() => await Context.Channel.SendEmbed("Bot Help", "Hello, if you need help with the bot, or need to report a bug, or request a new feature, please join my server and contact me\n(`Reverse#0069`)\nhttps://discord.gg/ga9V5pa", true);

        // Posts privacy information
        [Command("privacy")]
        public async Task PrivacyInfo() => await Context.Channel.SendEmbed("Privacy", $"The only data this bot stores is the server ID (`{Context.Guild.Id}`) of this server. This is temporary, and all data is deleted everytime the bot restarts or goes offline. Your server ID is used to create a server handler to handle your search request. This way, multiple searches may be done at the same time in separate servers. The bot requires this system to work effectively. If you wish to not participate, please remove the bot from your server. Your server ID is not used for anything else.\n\nIf you wish to speak to the developer (`Reverse#0069`) please type `!rt discord` and join the support server, or just add me.", true);
    }
}