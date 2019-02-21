using Newtonsoft.Json;
using System.Collections.Generic;

namespace RottenTomatoes.JSONs
{
    public partial class Watchlist
    {
        [JsonProperty("Movies")]
        public List<WatchlistMovie> Movies { get; set; }
    }

    public partial class WatchlistMovie
    {
        [JsonProperty("GuildID")]
        public ulong GuildId { get; set; }

        [JsonProperty("ChannelID")]
        public ulong ChannelId { get; set; }

        [JsonProperty("MovieLink")]
        public string MovieLink { get; set; }
    }

    public partial class Watchlist
    {
        public static Watchlist FromJson(string json) => JsonConvert.DeserializeObject<Watchlist>(json, Converter.Settings);
    }

    public static class SerializeWatchlist
    {
        public static string ToJson(this Watchlist self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}
