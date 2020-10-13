using System;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;
using DiscordBotsList.Api.Objects;

namespace RottenTomatoes
{
    // Handle commands and guild updates.
    internal class EventHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            _client.MessageReceived += HandleCommandAsync;

            _client.Ready += OnReady;
        }

        // Update the server list on https://discordbots.org/bot/477287091798278145
        private async Task OnReady()
        {
            if (Config.IS_TESTING) return;
            IDblSelfBot me = await Config.DblAPI.GetMeAsync();
            await me.UpdateStatsAsync(_client.Guilds.Count);
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot) return;

            var Context = new SocketCommandContext(_client, msg);

            // If the user just mentions the bot or says !rt, print help, they might need help
            if (msg.Content == "!rt" || msg.Content.StartsWith("<@477287091798278145>"))
            {
                await Context.Channel.PrintHelp();
                return;
            }

            if (msg.Content == "!rt info")
            {
                await msg.Channel.PrintBotInfo(_client);
                return;
            }

            var argPos = 0;
            if (msg.HasStringPrefix("!rt ", ref argPos))
            {
                using (Context.Channel.EnterTypingState())
                {
                    var result = await _service.ExecuteAsync(Context, argPos, null);

                    if (msg.Content.StartsWith("!rt"))
                        Console.WriteLine($"{Context.Guild.Name}: {msg.Author}: {msg.Content}");

                    // Search rotten tomatoes
                    // Example: !rt avengers
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        var search = msg.Content.Substring(4, msg.Content.Length - 4); // Remove "!rt "
                        foreach (var Server in Config.Servers)
                        {
                            if (Server.GuildID == Context.Guild.Id)
                            {
                                await Server.SearchHandler.SearchRottenTomatoes(search, Context);
                                return;
                            }
                        }

                        var newServer = new ServerHandler(Context.Guild.Id, new SearchHandler());
                        await newServer.SearchHandler.SearchRottenTomatoes(search, Context);
                        Config.Servers.Add(newServer);
                    }
                }
            }
        }
    }
}
