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

        var listing = JsonSerializer.Deserialize<RedditListing>(ref reader, options) 
            ?? throw new InvalidOperationException("No Reddit listing was deserialized from the JSON.");

        listing.Data ??= new RedditListingData();
        listing.Data.Children ??= [];

        return listing;
    }

    public override void Write(Utf8JsonWriter writer, RedditListing value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}