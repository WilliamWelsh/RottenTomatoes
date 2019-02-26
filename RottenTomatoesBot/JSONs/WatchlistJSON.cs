using Newtonsoft.Json;
using System.Collections.Generic;

namespace RottenTomatoes.JSONs
{
    public partial class WatchlistJSON
    {
        [JsonProperty("Movies")]
        public List<WatchlistMovie> Movies { get; set; }
    }

    public class WatchlistMovie
    {
        [JsonProperty("GuildID")]
        public ulong GuildId { get; set; }

        [JsonProperty("ChannelID")]
        public ulong ChannelId { get; set; }

        [JsonProperty("MovieLink")]
        public string MovieLink { get; set; }
    }

    public partial class WatchlistJSON
    {
        public static WatchlistJSON FromJson(string json) => JsonConvert.DeserializeObject<WatchlistJSON>(json, Converter.Settings);
    }

    public static class SerializeWatchlist
    {
        public static string ToJson(this WatchlistJSON self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}
