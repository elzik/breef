using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw
{
    public class RedditDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDouble(out double doubleSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds((long)doubleSeconds).UtcDateTime;
            }

            throw new JsonException("Invalid Unix timestamp for DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var unixSeconds = new DateTimeOffset(value
                .ToUniversalTime()).ToUnixTimeSeconds();

            writer.WriteNumberValue(unixSeconds);
        }
    }
}