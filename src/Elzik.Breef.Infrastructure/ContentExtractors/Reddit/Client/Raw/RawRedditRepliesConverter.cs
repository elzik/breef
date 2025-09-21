using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

public class RawRedditRepliesConverter : JsonConverter<RawRedditListing>
{
    public override RawRedditListing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new RawRedditListing
            {
                Data = new RedditListingData
                {
                    Children = []
                }
            };
        }

        if (reader.TokenType == JsonTokenType.String && reader.GetString() == "")
        {
            return new RawRedditListing
            {
                Data = new RedditListingData
                {
                    Children = []
                }
            };
        }

        // Create new options without this converter to prevent infinite recursions
        var optionsWithoutThisConverter = new JsonSerializerOptions(options);
        optionsWithoutThisConverter.Converters.Remove(optionsWithoutThisConverter.Converters.FirstOrDefault(c => c is RawRedditRepliesConverter));

        var listing = JsonSerializer.Deserialize<RawRedditListing>(ref reader, optionsWithoutThisConverter)
            ?? throw new InvalidOperationException("No Reddit listing was deserialized from the JSON.");

        listing.Data ??= new RedditListingData();
        listing.Data.Children ??= [];

        return listing;
    }

    public override void Write(Utf8JsonWriter writer, RawRedditListing value, JsonSerializerOptions options)
    {
        // Create new options without this converter to prevent infinite recursion
        var optionsWithoutThisConverter = new JsonSerializerOptions(options);
        optionsWithoutThisConverter.Converters.Remove(optionsWithoutThisConverter.Converters.FirstOrDefault(c => c is RawRedditRepliesConverter));

        JsonSerializer.Serialize(writer, value, optionsWithoutThisConverter);
    }
}