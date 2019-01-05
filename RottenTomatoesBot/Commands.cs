using Discord.Commands;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    [RequireContext(ContextType.Guild)]
    public class CommandDefinitons : ModuleBase<SocketCommandContext>
    {
        [Command("rt")]
        public async Task RTHelp() => await Utilities.PrintHelp(Context.Channel);

        [Command("rt")]
        public async Task SearchRottenTomatoes([Remainder]string search)
        {
            foreach (var Server in Config.Servers)
            {
                if (Server.GuildID == Context.Guild.Id)
                {
                    await Server.Handler.SearchRottenTomatoes(search, Context);
                    return;
                }
            }

            var newServer = new ServerHandler(Context.Guild.Id, new RottenTomatoesHandler());
            await newServer.Handler.SearchRottenTomatoes(search, Context);
            Config.Servers.Add(newServer);
        }

        [Command("rt choose")]
        public async Task SelectRottenTomatoes(int selection)
        {
            foreach (var Server in Config.Servers)
            {
                if (Server.GuildID == Context.Guild.Id)
                {
                    await Server.Handler.TryToSelect(selection, Context.Channel);
                    return;
                }
            }

            var newServer = new ServerHandler(Context.Guild.Id, new RottenTomatoesHandler());
            await newServer.Handler.TryToSelect(selection, Context.Channel);
            Config.Servers.Add(newServer);
        }

        // Display how many servers the bot is on, and total amount of users
        // This is pretty much just for me
        [Command("rtstats")]
        public async Task DisplayStats()
        {
            int serverCount = 0, totalMembers = 0;
            foreach (var guild in Context.Client.Guilds)
            {
                serverCount++;
                totalMembers += guild.MemberCount;
            }

            string description = $"Total Servers: {serverCount.ToString("#,##0")}\nTotal Members: {totalMembers.ToString("#,##0")}";

            await Utilities.SendEmbed(Context.Channel, "Bot Stats", description, true);
        }

        // Prints a link to my Discord
        // This is so a user can report something to me, or ask for a new feature
        [Command("rtdiscord")]
        public async Task RTDiscord()
        {
            await Utilities.SendEmbed(Context.Channel, "RT Help", "Hello, if you need help with the bot, or need to report a bug, or request a new feature, please join my server and contact me\n(My name is Reverse)\nhttps://discord.gg/qsc8YMS", true);
        }
    }
}