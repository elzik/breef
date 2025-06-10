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
                    Children = new List<RedditChild>()
                }
            };
        }

        if (reader.TokenType == JsonTokenType.String && reader.GetString() == "")
        {
            return new RedditListing
            {
                Data = new RedditListingData
                {
                    Children = new List<RedditChild>()
                }
            };
        }

        var listing = JsonSerializer.Deserialize<RedditListing>(ref reader, options);
        if (listing?.Data?.Children == null)
        {
            if (listing?.Data == null)
                listing.Data = new RedditListingData();
            listing.Data.Children = new List<RedditChild>();
        }
        return listing;
    }

    public override void Write(Utf8JsonWriter writer, RedditListing value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}