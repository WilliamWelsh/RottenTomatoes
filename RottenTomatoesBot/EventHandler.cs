using System;
using System.IO;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace RottenTomatoes
{
    class EventHandler
    {
        DiscordSocketClient _client;
        CommandService _service;
        StreamWriter stream = new FileInfo("Resources/log.txt").AppendText();

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            if (msg.HasStringPrefix("!", ref argPos))
            {
                if (msg.Author.IsBot) return;
                await _service.ExecuteAsync(context, argPos, null);
                if (msg.Content.StartsWith("!rt"))
                {
                    Console.WriteLine($"{context.Guild.Name}: {msg.Author}: {msg.Content}");
                    using (stream) { stream.WriteLine($"{context.Guild.Name}: {msg.Author}: {msg.Content}"); }
                }
            }
        }
    }
}
