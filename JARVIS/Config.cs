using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RottenTomatoes
{
    class Config
    {
        private const string resourcesFolder = "Resources";
        private const string resourcesFile = "resources.json";

        private static string GetFilePath(string file) => $"{resourcesFolder}/{file}";

        public static RottenTomatoesHandler RT = new RottenTomatoesHandler();

        public struct ServerHandler { public ulong serverID; public RottenTomatoesHandler RT; }

        public static List<ServerHandler> Servers = new List<ServerHandler>();

        public static BotResources bot;

        static Config()
        {
            if (!Directory.Exists(resourcesFolder))
                Directory.CreateDirectory(resourcesFolder);

            if (!File.Exists(GetFilePath(resourcesFile)))
            {
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText(GetFilePath(resourcesFile), json);
            }
            else
            {
                string json = File.ReadAllText(GetFilePath(resourcesFile));
                bot = JsonConvert.DeserializeObject<BotResources>(json);
            }
        }

        public struct BotResources { public string botToken; }
    }
}
