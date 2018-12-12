using Discord.Commands;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    [RequireContext(ContextType.Guild)]
    public class CommandDefinitons : ModuleBase<SocketCommandContext>
    {
        [Command("rt")]
        public async Task SearchRottenTomatoes([Remainder]string search)
        {
            bool foundServer = false;
            foreach (var s in Config.Servers)
            {
                if (s.serverID == Context.Guild.Id)
                {
                    await s.RT.SearchRottenTomatoes(search, Context);
                    foundServer = true;
                    return;
                }
            }

            if (!foundServer)
            {
                var newServer = new Config.ServerHandler();
                newServer.serverID = Context.Guild.Id;
                newServer.RT = new RottenTomatoesHandler();
                await newServer.RT.SearchRottenTomatoes(search, Context);
                Config.Servers.Add(newServer);
            }
        }

        [Command("rt choose")]
        public async Task SelectRottenTomatoes(int selection)
        {
            bool foundServer = false;
            foreach (var s in Config.Servers)
            {
                if (s.serverID == Context.Guild.Id)
                {
                    await s.RT.TryToSelect(selection, Context.Channel);
                    foundServer = true;
                    return;
                }
            }

            if (!foundServer)
            {
                var newServer = new Config.ServerHandler();
                newServer.serverID = Context.Guild.Id;
                newServer.RT = new RottenTomatoesHandler();
                await newServer.RT.TryToSelect(selection, Context.Channel);
                Config.Servers.Add(newServer);
            }
        }
    }
}