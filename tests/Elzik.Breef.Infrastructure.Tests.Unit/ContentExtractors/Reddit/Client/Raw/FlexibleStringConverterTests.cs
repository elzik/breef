using System.Text.Json;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client.Raw;

public class FlexibleStringConverterTests
{
    [Fact]
    public void Read_StringValue_ReturnsString()
    {
        // Arrange
        var json = "\"test123\"";
        var options = new JsonSerializerOptions();

        // Act
        var result = JsonSerializer.Deserialize<string>(json, options);

        // Assert
        result.ShouldBe("test123");
    }

    [Fact]
    public void Read_NumericValue_ReturnsStringRepresentation()
    {
        // Arrange
        var json = "123456";
        var options = new JsonSerializerOptions
        {
            Converters = { new Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.FlexibleStringConverter() }
        };

        // Act
        var result = JsonSerializer.Deserialize<string>(json, options);

        // Assert
        result.ShouldBe("123456");
    }

    [Fact]
    public void Read_NullValue_ReturnsNull()
    {
        // Arrange
        var json = "null";
        var options = new JsonSerializerOptions
        {
            Converters = { new Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.FlexibleStringConverter() }
        };

        // Act
        var result = JsonSerializer.Deserialize<string?>(json, options);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Read_WithGalleryItemModel_HandlesNumericId()
    {
        // Arrange
        var json = """
        {
            "media_id": "abc123",
            "id": 456789
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.RawRedditGalleryItem>(json);

        // Assert
        result.ShouldNotBeNull();
        result.MediaId.ShouldBe("abc123");
        result.Id.ShouldBe("456789");
    }

    [Fact]
    public void Read_WithRedditPostStructure_HandlesGalleryDataWithNumericIds()
    {
        // Arrange - Simulate the structure that was causing the original error
        var json = """
        [
            {
                "kind": "Listing",
                "data": {
                    "children": [
                        {
                            "kind": "t3",
                            "data": {
                                "id": "1nzkay2",
                                "title": "Test Post",
                                "is_gallery": true,
                                "gallery_data": {
                                    "items": [
                                        {
                                            "media_id": "abc123",
                                            "id": 456789
                                        },
                                        {
                                            "media_id": "def456",
                                            "id": 789012
                                        }
                                    ]
                                }
                            }
                        }
                    ]
                }
            }
        ]
        """;

        // Act
        var result = JsonSerializer.Deserialize<Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.RawRedditPost>(json);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        
        var postData = result[0].Data.Children[0].Data;
        postData.Id.ShouldBe("1nzkay2");
        postData.IsGallery.ShouldBeTrue();
        postData.GalleryData.ShouldNotBeNull();
        postData.GalleryData.Items.ShouldNotBeNull();
        postData.GalleryData.Items.Count.ShouldBe(2);
        
        // These were the problematic numeric IDs that caused the original error
        postData.GalleryData.Items[0].Id.ShouldBe("456789");
        postData.GalleryData.Items[1].Id.ShouldBe("789012");
        
        postData.GalleryData.Items[0].MediaId.ShouldBe("abc123");
        postData.GalleryData.Items[1].MediaId.ShouldBe("def456");
    }

    [Fact]
    public void Write_StringValue_WritesStringValue()
    {
        // Arrange
        var value = "test123";
        var options = new JsonSerializerOptions
        {
            Converters = { new Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.FlexibleStringConverter() }
        };

        // Act
        var result = JsonSerializer.Serialize(value, options);

        // Assert
        result.ShouldBe("\"test123\"");
    }

    [Fact]
    public void Write_NullValue_WritesNull()
    {
        // Arrange
        string? value = null;
        var options = new JsonSerializerOptions
        {
            Converters = { new Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.FlexibleStringConverter() }
        };

        // Act
        var result = JsonSerializer.Serialize(value, options);

        // Assert
        result.ShouldBe("null");
    }
}