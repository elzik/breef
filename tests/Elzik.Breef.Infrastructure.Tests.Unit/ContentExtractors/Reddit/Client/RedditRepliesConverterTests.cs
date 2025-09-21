using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using System.Text;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class RedditRepliesConverterTests
{
    private readonly JsonSerializerOptions _deserializeOptions;
    private readonly JsonSerializerOptions _serializeOptions;

    public RedditRepliesConverterTests()
    {
        _deserializeOptions = new JsonSerializerOptions
        {
            Converters = { new RawRedditRepliesConverter() }
        };
        _serializeOptions = new JsonSerializerOptions(); // No custom converter
    }

    [Fact]
    public void Read_NullToken_ReturnsEmptyListing()
    {
        // Test the converter directly
        var converter = new RawRedditRepliesConverter();
        var json = "null";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Advance to the null token

        var result = converter.Read(ref reader, typeof(RawRedditListing), _deserializeOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Children);
        Assert.Empty(result.Data.Children);
    }

    [Fact]
    public void Read_EmptyString_ReturnsEmptyListing()
    {
        var json = "\"\"";
        var listing = JsonSerializer.Deserialize<RawRedditListing>(json, _deserializeOptions);

        Assert.NotNull(listing);
        Assert.NotNull(listing.Data);
        Assert.NotNull(listing.Data.Children);
        Assert.Empty(listing.Data.Children);
    }

    [Fact]
    public void Read_ValidListingJson_DeserializesCorrectly()
    {
        // Simple listing with one comment and no replies (prevents recursion)
        var json = """
        {
            "kind": "Listing",
            "data": {
                "after": null,
                "before": null,
                "children": [
                    {
                        "kind": "t1",
                        "data": {
                            "id": "comment1",
                            "author": "testuser",
                            "body": "This is a test comment",
                            "created_utc": 1640995200,
                            "replies": ""
                        }
                    }
                ]
            }
        }
        """;

        // Deserialize as a single RedditListing, not a List
        var listing = JsonSerializer.Deserialize<RawRedditListing>(json, _deserializeOptions);

        Assert.NotNull(listing);
        Assert.Equal("Listing", listing.Kind);
        Assert.NotNull(listing.Data);
        Assert.NotNull(listing.Data.Children);
        Assert.Single(listing.Data.Children);

        var child = listing.Data.Children[0];
        Assert.Equal("t1", child.Kind);
        Assert.Equal("comment1", child.Data.Id);
        Assert.Equal("testuser", child.Data.Author);
        Assert.Equal("This is a test comment", child.Data.Body);

        // Verify replies is handled correctly (empty string becomes empty listing)
        Assert.NotNull(child.Data.Replies);
        Assert.NotNull(child.Data.Replies.Data);
        Assert.Empty(child.Data.Replies.Data.Children);
    }

    [Fact]
    public void Write_SerializesCorrectly()
    {
        var listing = new RawRedditListing
        {
            Kind = "Listing",
            Data = new RedditListingData
            {
                Children = []
            }
        };

        var json = JsonSerializer.Serialize(listing, _serializeOptions);

        Assert.Contains("\"kind\":\"Listing\"", json);
        Assert.Contains("\"children\":[]", json);
    }
}