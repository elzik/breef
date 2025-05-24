using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.Wallabag
{
    public class WallabagDateTimeConverter : JsonConverter<DateTime>
    {
        private const string DateFormat = "yyyy-MM-ddTHH:mm:ssK";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected string token.");
            }

            var dateString = reader.GetString();
            if (DateTime.TryParseExact(dateString, DateFormat, 
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            {
                return date;
            }

            throw new JsonException($"Unable to convert \"{dateString}\" to a Wallabag DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateFormat));
        }
    }
}