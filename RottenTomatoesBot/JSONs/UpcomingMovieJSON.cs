// This is used for movies that are under "Coming Soon to 
// theaters https://www.rottentomatoes.com/browse/upcoming/

using System;
using Newtonsoft.Json;

namespace RottenTomatoes.JSONs
{

    public partial class UpComingMovies
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("tomatoIcon")]
        public TomatoIcon TomatoIcon { get; set; }

        [JsonProperty("tomatoScore")]
        public long? TomatoScore { get; set; }

        [JsonProperty("popcornIcon")]
        public PopcornIcon PopcornIcon { get; set; }

        [JsonProperty("popcornScore")]
        public long PopcornScore { get; set; }

        [JsonProperty("theaterReleaseDate")]
        public string TheaterReleaseDate { get; set; }

        [JsonProperty("mpaaRating")]
        public MpaaRating MpaaRating { get; set; }

        [JsonProperty("synopsis")]
        public string Synopsis { get; set; }

        [JsonProperty("synopsisType")]
        public SynopsisType SynopsisType { get; set; }

        [JsonProperty("runtime", NullValueHandling = NullValueHandling.Ignore)]
        public string Runtime { get; set; }

        [JsonProperty("posters")]
        public Posters Posters { get; set; }
    }

    public class Posters
    {
        [JsonProperty("thumborId")]
        public string ThumborId { get; set; }

        [JsonProperty("primary")]
        public Uri Primary { get; set; }
    }

    public enum MpaaRating { Nr, Pg, Pg13, R };

    public enum PopcornIcon { Anticipated, Na };

    public enum SynopsisType { Consensus, Synopsis };

    public enum TomatoIcon { CertifiedFresh, Fresh, Na, Rotten };

    public partial class UpComingMovies
    {
        public static UpComingMovies[] FromJson(string json) => JsonConvert.DeserializeObject<UpComingMovies[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this UpComingMovies[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal class MpaaRatingConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(MpaaRating) || t == typeof(MpaaRating?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "NR")
                return MpaaRating.Nr;
            else if (value == "PG")
                return MpaaRating.Pg;
            else if (value == "PG13")
                return MpaaRating.Pg13;
            else if (value == "R")
                return MpaaRating.R;
            throw new ArgumentException("Cannot unmarshal type MpaaRating");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (MpaaRating)untypedValue;
            switch (value)
            {
                case MpaaRating.Nr:
                    serializer.Serialize(writer, "NR");
                    return;
                case MpaaRating.Pg:
                    serializer.Serialize(writer, "PG");
                    return;
                case MpaaRating.Pg13:
                    serializer.Serialize(writer, "PG13");
                    return;
                case MpaaRating.R:
                    serializer.Serialize(writer, "R");
                    return;
                default:
                    serializer.Serialize(writer, "N/A");
                    return;
            }
            throw new ArgumentException("Cannot marshal type MpaaRating");
        }

        public static readonly MpaaRatingConverter Singleton = new MpaaRatingConverter();
    }

    internal class PopcornIconConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(PopcornIcon) || t == typeof(PopcornIcon?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "NA")
                return PopcornIcon.Na;
            else if (value == "anticipated")
                return PopcornIcon.Anticipated;
            throw new ArgumentException("Cannot unmarshal type PopcornIcon");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (PopcornIcon)untypedValue;
            switch (value)
            {
                case PopcornIcon.Na:
                    serializer.Serialize(writer, "NA");
                    return;
                case PopcornIcon.Anticipated:
                    serializer.Serialize(writer, "anticipated");
                    return;
                default:
                    serializer.Serialize(writer, "unknown");
                    return;
            }
            throw new ArgumentException("Cannot marshal type PopcornIcon");
        }

        public static readonly PopcornIconConverter Singleton = new PopcornIconConverter();
    }

    internal class SynopsisTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(SynopsisType) || t == typeof(SynopsisType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "consensus")
                return SynopsisType.Consensus;
            else if (value == "synopsis")
                return SynopsisType.Synopsis;
            throw new ArgumentException("Cannot unmarshal type SynopsisType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (SynopsisType)untypedValue;
            switch (value)
            {
                case SynopsisType.Consensus:
                    serializer.Serialize(writer, "consensus");
                    return;
                case SynopsisType.Synopsis:
                    serializer.Serialize(writer, "synopsis");
                    return;
                default:
                    serializer.Serialize(writer, "unknown");
                    return;
            }
            throw new ArgumentException("Cannot marshal type SynopsisType");
        }

        public static readonly SynopsisTypeConverter Singleton = new SynopsisTypeConverter();
    }

    internal class TomatoIconConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TomatoIcon) || t == typeof(TomatoIcon?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "NA")
                return TomatoIcon.Na;
            else if (value == "certified_fresh")
                return TomatoIcon.CertifiedFresh;
            else if (value == "fresh")
                return TomatoIcon.Fresh;
            else if (value == "rotten")
                return TomatoIcon.Rotten;
            throw new ArgumentException("Cannot unmarshal type TomatoIcon");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TomatoIcon)untypedValue;
            switch (value)
            {
                case TomatoIcon.Na:
                    serializer.Serialize(writer, "NA");
                    return;
                case TomatoIcon.CertifiedFresh:
                    serializer.Serialize(writer, "certified_fresh");
                    return;
                case TomatoIcon.Fresh:
                    serializer.Serialize(writer, "fresh");
                    return;
                case TomatoIcon.Rotten:
                    serializer.Serialize(writer, "rotten");
                    return;
                default:
                    serializer.Serialize(writer, "unknown");
                    return;
            }
            throw new ArgumentException("Cannot marshal type TomatoIcon");
        }

        public static readonly TomatoIconConverter Singleton = new TomatoIconConverter();
    }
}
