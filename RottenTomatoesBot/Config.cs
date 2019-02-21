using System.IO;
using DiscordBotsList.Api;
using RottenTomatoes.JSONs;
using RottenTomatoes.Handlers;
using System.Collections.Generic;

namespace RottenTomatoes
{
    static class Config
    {
        public static readonly List<ServerHandler> Servers = new List<ServerHandler>();
        public static readonly WatchlistHandler WatchListHandler = new WatchlistHandler();

        public static readonly BotResources bot;

        public static readonly AuthDiscordBotListApi DblAPI;

        static Config()
        {
            string json = File.ReadAllText("Resources/resources.json");
            bot = BotResources.FromJson(json);

            DblAPI = new AuthDiscordBotListApi(477287091798278145, bot.BotsListToken);
        }
    }
}
