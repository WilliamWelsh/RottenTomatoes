// Massive respect to https://quicktype.io/
// Auto generated with https://quicktype.io/

// This is used for movies that are under search results
// https://www.rottentomatoes.com/search/?search=avengers

namespace SearchResultsJSON
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SearchResults
    {
        [JsonProperty("actorCount")]
        public long ActorCount { get; set; }

        [JsonProperty("actors")]
        public Celebrity[] Actors { get; set; }

        [JsonProperty("criticCount")]
        public long CriticCount { get; set; }

        [JsonProperty("franchiseCount")]
        public long FranchiseCount { get; set; }

        [JsonProperty("franchises")]
        public Franchise[] Franchises { get; set; }

        [JsonProperty("movieCount")]
        public long MovieCount { get; set; }

        [JsonProperty("movies")]
        public Movie[] Movies { get; set; }

        [JsonProperty("tvCount")]
        public long TvCount { get; set; }

        [JsonProperty("tvSeries")]
        public TvSery[] TvSeries { get; set; }
    }

    public class Celebrity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public Uri Image { get; set; }
    }

    public class Franchise
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public Uri Image { get; set; }
    }

    public class Movie
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public Uri Image { get; set; }

        [JsonProperty("meterClass")]
        public string MeterClass { get; set; }

        [JsonProperty("meterScore", NullValueHandling = NullValueHandling.Ignore)]
        public long? MeterScore { get; set; }

        [JsonProperty("castItems", NullValueHandling = NullValueHandling.Ignore)]
        public CastItem[] CastItems { get; set; }

        [JsonProperty("subline")]
        public string Subline { get; set; }
    }

    public class CastItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class TvSery
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

    public static class Serialize
    {
        public static string ToJson(this SearchResults self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
