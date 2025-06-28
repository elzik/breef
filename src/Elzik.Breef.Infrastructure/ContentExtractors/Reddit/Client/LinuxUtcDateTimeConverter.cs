using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client
{
    public class LinuxUtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetDouble(out double doubleSeconds))
                {
                    return DateTimeOffset.FromUnixTimeSeconds((long)doubleSeconds).UtcDateTime;
                }
            }

            throw new JsonException("Invalid Unix timestamp for DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var unixTime = new DateTimeOffset(value).ToUnixTimeSeconds();
            writer.WriteNumberValue(unixTime);
        }
    }
}