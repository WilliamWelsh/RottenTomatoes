using System.IO;
using Newtonsoft.Json;
using DiscordBotsList.Api;
using RottenTomatoes.JSONs;
using System.Collections.Generic;

namespace RottenTomatoes
{
    static class Config
    {
        public static readonly List<ServerHandler> Servers = new List<ServerHandler>();
        public static readonly Handlers.WatchlistHandler WatchListHandler = new Handlers.WatchlistHandler();

        public static readonly BotResources bot;

        public static readonly AuthDiscordBotListApi DblAPI;

        static Config()
        {
            if (!Directory.Exists("Resources"))
                Directory.CreateDirectory("Resources");

            if (!File.Exists("Resources/resources.json"))
            {
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText("Resources/resources.json", json);
            }
            else
            {
                string json = File.ReadAllText("Resources/resources.json");
                bot = BotResources.FromJson(json);
            }

            DblAPI = new AuthDiscordBotListApi(477287091798278145, bot.BotsListToken);
        }
    }
}
