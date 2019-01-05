using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    class Program
    {
        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(Config.bot.BotToken)) return;
            DiscordSocketClient _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, Config.bot.BotToken);
            await _client.StartAsync();
            await _client.SetGameAsync("!rt help", null, ActivityType.Watching);
            EventHandler _handler = new EventHandler();
            await _handler.InitializeAsync(_client);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }
    }
}
