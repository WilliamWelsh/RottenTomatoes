// This is used for getting the bot's discord token and DiscordBotsList API token
using Newtonsoft.Json;

namespace RottenTomatoes.JSONs
{
    public partial class BotResources
    {
        [JsonProperty("botToken")]
        public string BotToken { get; set; }

        [JsonProperty("BotsListToken")]
        public string BotsListToken { get; set; }

        [JsonProperty("pushToken")]
        public string PushToken { get; set; }

        [JsonProperty("pushUser")]
        public string PushUser { get; set; }

        [JsonProperty("omdbAPIToken")]
        public string OMDbAPIToken { get; set; }
    }

    public partial class BotResources
    {
        public static BotResources FromJson(string json) => JsonConvert.DeserializeObject<BotResources>(json, Converter.Settings);
    }
}
