using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class RedditRepliesConverter : JsonConverter<RedditListing>
{
    public override RedditListing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new RedditListing
            {
                Data = new RedditListingData
                {
                    Children = []
                }
            };
        }

        if (reader.TokenType == JsonTokenType.String && reader.GetString() == "")
        {
            return new RedditListing
            {
                Data = new RedditListingData
                {
                    Children = []
                }
            };
        }

        // Create new options without this converter to prevent infinite recursions
        var optionsWithoutThisConverter = new JsonSerializerOptions(options);
        optionsWithoutThisConverter.Converters.Remove(optionsWithoutThisConverter.Converters.FirstOrDefault(c => c is RedditRepliesConverter));

        var listing = JsonSerializer.Deserialize<RedditListing>(ref reader, optionsWithoutThisConverter)
            ?? throw new InvalidOperationException("No Reddit listing was deserialized from the JSON.");

        listing.Data ??= new RedditListingData();
        listing.Data.Children ??= [];

        return listing;
    }

    public override void Write(Utf8JsonWriter writer, RedditListing value, JsonSerializerOptions options)
    {
        // Create new options without this converter to prevent infinite recursion
        var optionsWithoutThisConverter = new JsonSerializerOptions(options);
        optionsWithoutThisConverter.Converters.Remove(optionsWithoutThisConverter.Converters.FirstOrDefault(c => c is RedditRepliesConverter));

        JsonSerializer.Serialize(writer, value, optionsWithoutThisConverter);
    }
}