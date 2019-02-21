// This is used for movies that are under "Opening This Week"
// https://www.rottentomatoes.com/browse/opening/

namespace RottenTomatoes.JSONs
{
    using Newtonsoft.Json;

    public partial class MoviesOpeningThisWeek
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("tomatoIcon")]
        public string TomatoIcon { get; set; }

        [JsonProperty("tomatoScore")]
        public long? TomatoScore { get; set; }

        [JsonProperty("theaterReleaseDate")]
        public string TheaterReleaseDate { get; set; }
    }

    public partial class MoviesOpeningThisWeek
    {
        public static MoviesOpeningThisWeek[] FromJson(string json) => JsonConvert.DeserializeObject<MoviesOpeningThisWeek[]>(json, Converter.Settings);
    }
}
