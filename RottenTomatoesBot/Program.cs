using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    class Program
    {
        private DiscordSocketClient _client;
        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        private const bool testMode = false;

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(Config.bot.BotToken)) return;
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += Log;
            if (testMode)
                await _client.LoginAsync(TokenType.Bot, System.IO.File.ReadAllText("C:/Users/willi/Documents/repos/testBotToken.txt"));
            else
                await _client.LoginAsync(TokenType.Bot, Config.bot.BotToken);
            await _client.StartAsync();
            await _client.SetGameAsync("!rt help", null, ActivityType.Watching);

            EventHandler _handler = new EventHandler();
            await _handler.InitializeAsync(_client);

            await Task.Delay(1000);
            await Config.WatchListHandler.SetUp(_client).ConfigureAwait(false);

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }
    }
}
