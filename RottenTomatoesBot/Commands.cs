using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    [RequireContext(ContextType.Guild)]
    public class CommandDefinitons : ModuleBase<SocketCommandContext>
    {
        [Command("github")]
        public async Task PrintGitHubLink() => await Utilities.SendEmbed(Context.Channel, "Rotten Tomatoes Source Code", "Here is the source code for this bot:\nhttps://github.com/WilliamWelsh/RottenTomatoes", false);

        [Command("invite")]
        public async Task DMInvite() => await Utilities.DMInviteLink((SocketGuildUser)Context.User, Context.Channel);

        [Command("help")]
        public async Task PrintHelp() => await Utilities.PrintHelp(Context.Channel);

        [Command("box office")]
        public async Task PrintTopBoxOffice() => await Listings.SendTopBoxOffice(Context.Channel);

        [Command("upcoming")]
        [Alias("upcoming movies")]
        public async Task SendUpComingHelp() => await Utilities.SendUpcomingHelp(Context.Channel);

        [Command("comingsoon")]
        [Alias("coming soon")]
        public async Task SendUpcomingMovies() => await Listings.SendUpcomingMovies(Context.Channel);

        [Command("opening")]
        [Alias("opening this week")]
        public async Task SendUpcomingMoviesThisWeek() => await Listings.SendUpcomingMoviesThisWeek(Context.Channel);

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
        public async Task DisplayStats() => await Utilities.PrintBotInfo(Context.Client, Context.Channel);

        // Prints a link to my Discord
        // This is so a user can report something to me, or ask for a new feature
        [Command("discord")]
        public async Task RTDiscord() => await Utilities.SendEmbed(Context.Channel, "Bot Help", "Hello, if you need help with the bot, or need to report a bug, or request a new feature, please join my server and contact me\n(My name is Reverse)\nhttps://discord.gg/qsc8YMS", true);
    }
}