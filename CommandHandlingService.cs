using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RottenTomatoes
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _discord.MessageReceived += MessageReceivedAsync;

            _discord.InteractionCreated += OnInteractionAsync;
        }

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            switch (interaction)
            {
                // Slash Command
                case SocketSlashCommand commandInteraction:
                    // /rt
                    if (commandInteraction.CommandName == "rt")
                    {
                        if (commandInteraction.Data.Options == null)
                            await commandInteraction.RespondAsync(embed: new EmbedBuilder()
                                    .WithColor(EmbedUtils.Red)
                                    .WithTitle("Rotten Tomatoes")
                                    .WithImageUrl("https://cdn.discordapp.com/attachments/735282082963652749/891459194820100116/ezgif.com-gif-maker.gif")
                                    .WithDescription("To search for a movie...\n`/rt <name of movie>`\nThen click on the movie\n\nTo view the top box office...\n`/boxoffice`\n\nIf you need help, join the support server (link below)")
                                    .WithThumbnailUrl(EmbedUtils.Logo)
                                    .Build(),
                                component: new ComponentBuilder()
                                    .WithButton("Support Server", style: ButtonStyle.Link, url: "https://discord.gg/ga9V5pa")
                                    .WithButton("Bot Invite Link", style: ButtonStyle.Link, url: "https://discord.com/api/oauth2/authorize?client_id=477287091798278145&permissions=67584&scope=bot%20applications.commands")
                                    .WithButton("GitHub", style: ButtonStyle.Link, url: "https://github.com/WilliamWelsh/RottenTomatoes")
                                    .Build());
                        else
                        {
                            await commandInteraction.DeferAsync();
                            await InteractionSearchHandler.SearchRottenTomatoes(commandInteraction);
                        }
                    }

                    // /boxoffice
                    else if (commandInteraction.CommandName == "boxoffice")
                    {
                        await commandInteraction.DeferAsync();
                        await BoxOffice.Print(commandInteraction);
                    }
                    break;

                // Button Click
                case SocketMessageComponent componentInteraction:
                    await InteractionSearchHandler.PrintToInteraction(componentInteraction);
                    break;

                default:
                    break;
            }
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system & bot messages
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            // Print the help if someone tags the bot
            if (message.Content.Contains(_discord.CurrentUser.Mention))
            {
                await message.Channel.PrintHelp();
                return;
            }

            // Check for command prefix
            var argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);

            if (message.Content.StartsWith("!rt"))
            {
                Console.WriteLine($"{context.User}: {context.Message}");
                await context.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                    .WithColor(EmbedUtils.Red)
                    .WithTitle("Rotten Tomatoes")
                    .WithImageUrl(
                        "https://cdn.discordapp.com/attachments/735282082963652749/891459194820100116/ezgif.com-gif-maker.gif")
                    .WithDescription(
                        "Please ask your server owner to re-invite the bot using the link below, then you will be able to use SLASH commands (`/rt`).\n\nUse this link to re-invite the bot: https://discord.com/api/oauth2/authorize?client_id=477287091798278145&permissions=67584&scope=bot%20applications.commands\n\nIf you need help, you can join the support server here: https://discord.gg/ga9V5pa")
                    .WithThumbnailUrl(EmbedUtils.Logo)
                    .Build());
            }
        }
    }
}