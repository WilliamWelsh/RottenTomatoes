// This is used for getting the bot's discord token and DiscordBotsList API token

namespace RottenTomatoes
{
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class BotResources
    {
        [JsonProperty("botToken")]
        public string BotToken { get; set; }

        [JsonProperty("BotsListToken")]
        public string BotsListToken { get; set; }
    }

    public partial class BotResources
    {
        public static BotResources FromJson(string json) => JsonConvert.DeserializeObject<BotResources>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this BotResources self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
