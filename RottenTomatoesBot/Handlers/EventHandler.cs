using System;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;
using DiscordBotsList.Api.Objects;

namespace RottenTomatoes
{
    // Handle commands and guild updates.
    class EventHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            _client.MessageReceived += HandleCommandAsync;

            _client.JoinedGuild += UpdateDBlStatsASync;
            _client.LeftGuild += UpdateDBlStatsASync;
        }

        // Update the server list on https://discordbots.org/bot/477287091798278145
        private async Task UpdateDBlStatsASync(SocketGuild arg)
        {
            IDblSelfBot me = await Config.DblAPI.GetMeAsync();
            await me.UpdateStatsAsync(_client.Guilds.Count);
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot) return;

            var Context = new SocketCommandContext(_client, msg);

            using (Context.Channel.EnterTypingState())
            {
                // If the user just mentions the bot or says !rt, print help, they might need help
                if (msg.Content == "!rt" || msg.Content.StartsWith("<@477287091798278145>"))
                {
                    await Utilities.PrintHelp(Context.Channel);
                    return;
                }

                if (msg.Content == "!rt info")
                {
                    await Utilities.PrintBotInfo(_client, msg.Channel);
                    return;
                }

                int argPos = 0;
                if (msg.HasStringPrefix("!rt ", ref argPos))
                {
                    var result = await _service.ExecuteAsync(Context, argPos, null);

                    if (msg.Content.StartsWith("!rt"))
                        Console.WriteLine($"{Context.Guild.Name}: {msg.Author}: {msg.Content}");

                    // Search rotten tomatoes
                    // Example: !rt avengers
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        string search = msg.Content.Substring(4, msg.Content.Length - 4); // Remove "!rt "
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
