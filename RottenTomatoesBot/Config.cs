﻿using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RottenTomatoes
{
    class Config
    {
        public struct ServerHandler { public ulong serverID; public RottenTomatoesHandler RT; }

        public static readonly List<ServerHandler> Servers = new List<ServerHandler>();

        public static readonly BotResources bot;
        public struct BotResources { public string BotToken { get; set; } }

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
                bot = JsonConvert.DeserializeObject<BotResources>(json);
            }
        }
    }
}
