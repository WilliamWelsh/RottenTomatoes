// This is used for movies that are under search results
// https://www.rottentomatoes.com/search/?search=avengers

namespace RottenTomatoes.JSONs
{
    using System;
    using Newtonsoft.Json;

    public partial class SearchResults
    {
        [JsonProperty("actorCount")]
        public long ActorCount { get; set; }

        [JsonProperty("actors")]
        public CelebResult[] Actors { get; set; }

        [JsonProperty("movieCount")]
        public long MovieCount { get; set; }

        [JsonProperty("movies")]
        public MovieResult[] Movies { get; set; }

        [JsonProperty("tvCount")]
        public long TvCount { get; set; }

        [JsonProperty("tvSeries")]
        public TVResult[] TvSeries { get; set; }
    }

    public class CelebResult
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public Uri Image { get; set; }
    }

    public class MovieResult
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class TVResult
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("startYear")]
        public long StartYear { get; set; }

        [JsonProperty("endYear")]
        public long EndYear { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("meterClass")]
        public string MeterClass { get; set; }

        [JsonProperty("image")]
        public Uri Image { get; set; }

        [JsonProperty("meterScore", NullValueHandling = NullValueHandling.Ignore)]
        public long? MeterScore { get; set; }
    }

    public partial class SearchResults
    {
        public static SearchResults FromJson(string json) => JsonConvert.DeserializeObject<SearchResults>(json, Converter.Settings);
    }
}
