using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace RottenTomatoes
{
    [RequireContext(ContextType.Guild)]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Alias("rt ping")]
        public Task PingAsync() => ReplyAsync("pong!");

        [Command("rt")]
        public async Task SelectRottenTomatoesWithoutChooseText(int selection)
        {
            using (Context.Channel.EnterTypingState())
            {
                await ActiveServerHandlers.GetServerHandler(Context.Guild.Id).SearchHandler
                    .TryToSelect(selection, Context.Channel);
            }
        }

        [Command("rt choose")]
        public async Task SelectRottenTomatoes(int selection)
        {
            using (Context.Channel.EnterTypingState())
            {
                await ActiveServerHandlers.GetServerHandler(Context.Guild.Id).SearchHandler
                    .TryToSelect(selection, Context.Channel);
            }
        }

        [Command("rt")]
        public async Task PrintHelp() => await Context.Channel.PrintHelp();

        // Most commands
        [Command("rt")]
        public async Task SearchForMovie([Remainder] string search)
        {
            using (Context.Channel.EnterTypingState())
            {
                search = search.ToLower();

                switch (search)
                {
                    // !rt invite
                    case "invite":
                        await Context.Channel.DMInviteLink((SocketGuildUser)Context.User);
                        break;

                    // !rt github
                    case "github":
                        await Context.Channel.SendEmbed("Rotten Tomatoes Source Code", "Here is the source code for this bot:\nhttps://github.com/WilliamWelsh/RottenTomatoes", false);
                        break;

                    // !rt help
                    case "help":
                        await Context.Channel.PrintHelp();
                        break;

                    // !rt info and !rt stats
                    case "info":
                    case "stats":
                        await Context.Channel.PrintBotInfo(Context.Client);
                        break;

                    // !rt discord
                    case "discord":
                        await Context.Channel.SendEmbed("Bot Help", "Hello, if you need help with the bot, or need to report a bug, or request a new feature, please join my server and contact me\n(`Reverse#0069`)\nhttps://discord.gg/ga9V5pa", true);
                        break;

                    // !rt privacy
                    case "privacy":
                        await Context.Channel.SendEmbed("Privacy", $"The only data this bot stores is the server ID (`{Context.Guild.Id}`) of this server. This is temporary, and all data is deleted everytime the bot restarts or goes offline. Your server ID is used to create a server handler to handle your search request. This way, multiple searches may be done at the same time in separate servers. The bot requires this system to work effectively. If you wish to not participate, please remove the bot from your server. Your server ID is not used for anything else.\n\nIf you wish to speak to the developer (`Reverse#0069`) please type `!rt discord` and join the support server, or just add me.", true);
                        break;

                    // !rt box office
                    case "box office":
                        await BoxOffice.Print(Context.Channel);
                        break;

                    // They must be wanting to search on RT
                    default:
                        await ActiveServerHandlers.GetServerHandler(Context.Guild.Id).SearchHandler
                            .SearchRottenTomatoes(search, Context);
                        break;
                }
            }
        }
    }
}