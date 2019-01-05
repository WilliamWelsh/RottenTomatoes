// Massive respect to https://quicktype.io/
// Auto generated with https://quicktype.io/

// This is used for movies that are under "Opening This Week"
// https://www.rottentomatoes.com/browse/opening/

namespace OpeningThisWeekJSON
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class MoviesOpeningThisWeek
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("tomatoIcon")]
        public string TomatoIcon { get; set; }

        [JsonProperty("tomatoScore")]
        public long? TomatoScore { get; set; }

        [JsonProperty("popcornIcon")]
        public string PopcornIcon { get; set; }

        [JsonProperty("popcornScore")]
        public long PopcornScore { get; set; }

        [JsonProperty("theaterReleaseDate")]
        public string TheaterReleaseDate { get; set; }

        [JsonProperty("mpaaRating")]
        public string MpaaRating { get; set; }

        [JsonProperty("synopsis")]
        public string Synopsis { get; set; }

        [JsonProperty("synopsisType")]
        public string SynopsisType { get; set; }

        [JsonProperty("mainTrailer")]
        public MainTrailer MainTrailer { get; set; }

        [JsonProperty("posters")]
        public Posters Posters { get; set; }

        [JsonProperty("actors", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Actors { get; set; }

        [JsonProperty("runtime", NullValueHandling = NullValueHandling.Ignore)]
        public string Runtime { get; set; }

        [JsonProperty("dvdReleaseDate", NullValueHandling = NullValueHandling.Ignore)]
        public string DvdReleaseDate { get; set; }
    }

    public partial class MainTrailer
    {
        [JsonProperty("sourceId")]
        public Uri SourceId { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }
    }

    public partial class Posters
    {
        [JsonProperty("thumborId")]
        public string ThumborId { get; set; }

        [JsonProperty("primary")]
        public Uri Primary { get; set; }
    }

    public partial class MoviesOpeningThisWeek
    {
        public static MoviesOpeningThisWeek[] FromJson(string json) => JsonConvert.DeserializeObject<MoviesOpeningThisWeek[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this MoviesOpeningThisWeek[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
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

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (long.TryParse(value, out l))
            {
                return l;
            }
            throw new ArgumentNullException("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
