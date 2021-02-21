using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;

namespace RottenTomatoes
{
    internal class Program
    {
        private static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private IDblSelfBot _dblApi;

        private DiscordSocketClient _client;

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                _client = services.GetRequiredService<DiscordSocketClient>();

                _client.Log += LogAsync;

                // Login
                await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("rtBotToken"));
                await _client.SetGameAsync("!rt help", null, ActivityType.Watching);
                await _client.StartAsync();

                // Crate Discord Bot List client (Top.gg)
                var discordBotList = new AuthDiscordBotListApi(477287091798278145, Environment.GetEnvironmentVariable("rtBotListToken"));
                _dblApi = await discordBotList.GetMeAsync();

                // These events will update the current amount of guilds the bot is in on Top.gg (_dblApi)
                _client.Ready += OnReady;
                _client.JoinedGuild += OnGuildJoined;
                _client.LeftGuild += OnGuildLeft;

                // Register commands
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                // Run forever
                await Task.Delay(Timeout.Infinite);
            }
        }

        private async Task OnReady() => await _dblApi.UpdateStatsAsync(_client.Guilds.Count);

        private async Task OnGuildLeft(SocketGuild arg) => await _dblApi.UpdateStatsAsync(_client.Guilds.Count);

        private async Task OnGuildJoined(SocketGuild arg) => await _dblApi.UpdateStatsAsync(_client.Guilds.Count);

        // Log
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        // Configure Services
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig()
                {
                    LogLevel = LogSeverity.Verbose,
                    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages
                }))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
    }
}