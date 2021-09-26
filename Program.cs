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
                await _client.SetGameAsync("/rt", type: ActivityType.Watching);
                await _client.StartAsync();

                // Crate Discord Bot List client (Top.gg)
                var discordBotList = new AuthDiscordBotListApi(477287091798278145, Environment.GetEnvironmentVariable("rtBotListToken"));
                _dblApi = await discordBotList.GetMeAsync();

                // Initialize HttpClient
                WebUtils.http = new HttpClient();

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

        // Update the server count
        private async Task UpdateServerCount()
        {
            // Update on the bot's status
            await _client.SetGameAsync($"/rt | {_client.Guilds.Count} servers", type: ActivityType.Watching);

            // Update on top.gg
            await _dblApi.UpdateStatsAsync(_client.Guilds.Count);
        }

        private async Task OnReady() => await UpdateServerCount();

        private async Task OnGuildLeft(SocketGuild arg) => await UpdateServerCount();

        private async Task OnGuildJoined(SocketGuild arg) => await UpdateServerCount();

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