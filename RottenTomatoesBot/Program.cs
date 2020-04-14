using System;
using Discord;
using System.IO;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    class Program
    {
        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        private const bool testMode = false;

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(Config.bot.BotToken)) return;
            DiscordSocketClient _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += Log;

            // I use test mode so I don't have to connect 300+ servers for when I'm developing/fixing/testing/whatever
            await _client.LoginAsync(TokenType.Bot, testMode ? File.ReadAllText("C:/Users/willi/Documents/repos/testBotToken.txt") : Config.bot.BotToken);

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
