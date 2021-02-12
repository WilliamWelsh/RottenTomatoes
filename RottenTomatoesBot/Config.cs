using System.IO;
using DiscordBotsList.Api;
using RottenTomatoes.JSONs;
using System.Collections.Generic;

namespace RottenTomatoes
{
    internal static class Config
    {
        public static readonly List<ServerHandler> Servers = new List<ServerHandler>();

        public static readonly BotResources bot;

        public static readonly AuthDiscordBotListApi DblAPI;

        public static readonly bool IS_TESTING = false;

        static Config()
        {
            var json = File.ReadAllText("Resources/resources.json");
            bot = BotResources.FromJson(json);

            DblAPI = new AuthDiscordBotListApi(477287091798278145, bot.BotsListToken);
        }
    }
}