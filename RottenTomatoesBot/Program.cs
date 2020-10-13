using System;
using Discord;
using System.IO;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace RottenTomatoes
{
    internal class Program
    {
        private static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            // Position the console
            var ptr = GetConsoleWindow();
            MoveWindow(ptr, 2010, 0, 550, 355, true);

            if (string.IsNullOrEmpty(Config.bot.BotToken)) return;
            DiscordSocketClient _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += Log;

            // I use test mode so I don't have to connect 300+ servers for when I'm developing/fixing/testing/whatever
            await _client.LoginAsync(TokenType.Bot, Config.IS_TESTING ? File.ReadAllText("C:/Users/willi/Documents/repos/testBotToken.txt") : Config.bot.BotToken);

            await _client.StartAsync();
            await _client.SetGameAsync("!rt help", null, ActivityType.Watching);

            var _handler = new EventHandler();
            await _handler.InitializeAsync(_client);

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    }
}