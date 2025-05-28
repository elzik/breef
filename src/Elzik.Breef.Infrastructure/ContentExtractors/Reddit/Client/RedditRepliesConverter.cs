using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client
{
    public class RedditRepliesConverter : JsonConverter<RedditListing>
    {
        public override RedditListing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && reader.GetString() == "")
            {
                return null;
            }
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return JsonSerializer.Deserialize<RedditListing>(ref reader, options);
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, RedditListing value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}