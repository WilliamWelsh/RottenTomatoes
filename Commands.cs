using System.Threading.Tasks;
using Discord.Commands;

namespace RottenTomatoes
{
    [RequireContext(ContextType.Guild)]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task PingAsync() => ReplyAsync("pong!");
    }
}