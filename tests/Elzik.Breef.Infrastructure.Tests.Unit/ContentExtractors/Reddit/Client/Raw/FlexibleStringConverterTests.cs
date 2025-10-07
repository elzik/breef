using System.Text.Json;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client.Raw;

public class FlexibleStringConverterTests
{
    private readonly JsonSerializerOptions _optionsWithConverter = new()
    {
        Converters = { new Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.FlexibleStringConverter() }
    };

    [Fact]
    public void Read_NumericValue_ReturnsStringRepresentation()
    {
        // Arrange
        var numericJson = "123456";

        // Act
        var result = JsonSerializer.Deserialize<string>(numericJson, _optionsWithConverter);

        // Assert
        result.ShouldBe("123456");
    }

    [Fact]
    public void Read_StringValue_ReturnsString()
    {
        // Arrange
        var stringJson = "\"test123\"";

        // Act
        var result = JsonSerializer.Deserialize<string>(stringJson, _optionsWithConverter);

        // Assert
        result.ShouldBe("test123");
    }

    [Fact]
    public void Read_NullValue_ReturnsNull()
    {
        // Arrange
        var nullJson = "null";

        // Act
        var result = JsonSerializer.Deserialize<string?>(nullJson, _optionsWithConverter);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Read_LargeIntegerValue_ReturnsStringRepresentation()
    {
        // Arrange
        var largeIntegerJson = Int64.MaxValue.ToString();

        // Act
        var result = JsonSerializer.Deserialize<string>(largeIntegerJson, _optionsWithConverter);

        // Assert
        result.ShouldBe(Int64.MaxValue.ToString());
    }

    [Fact]
    public void Read_BooleanValue_ThrowsJsonException()
    {
        // Arrange
        var booleanJson = "true";

        // Act & Assert
        var exception = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<string>(booleanJson, _optionsWithConverter));
        exception.Message.ShouldBe("Cannot convert True to string");
    }

    [Fact]
    public void Read_WithGalleryItemModel_HandlesNumericId()
    {
        // Arrange
        var galleryItemJson = """
        {
            "media_id": "abc123",
            "id": 456789
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.RawRedditGalleryItem>(galleryItemJson);

        // Assert
        result.ShouldNotBeNull();
        result.MediaId.ShouldBe("abc123");
        result.Id.ShouldBe("456789");
    }

    [Fact]
    public void Read_WithRedditPostStructure_HandlesGalleryDataWithNumericIds()
    {
        var redditPostWithNumericGalleryDataIds = """
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
        var result = JsonSerializer.Deserialize<Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.RawRedditPost>(redditPostWithNumericGalleryDataIds);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        
        var postData = result[0].Data.Children[0].Data;
        postData.Id.ShouldBe("1nzkay2");
        postData.IsGallery.ShouldBeTrue();
        postData.GalleryData.ShouldNotBeNull();
        postData.GalleryData.Items.ShouldNotBeNull();
        postData.GalleryData.Items.Count.ShouldBe(2);
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

        // Act
        var result = JsonSerializer.Serialize(value, _optionsWithConverter);

        // Assert
        result.ShouldBe("\"test123\"");
    }

    [Fact]
    public void Write_NullValue_WritesNull()
    {
        // Arrange
        string? value = null;

        // Act
        var result = JsonSerializer.Serialize(value, _optionsWithConverter);

        // Assert
        result.ShouldBe("null");
    }

    [Fact]
    public void Read_DirectNull_CallsConverter()
    {
        // Arrange
        var converter = new Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.FlexibleStringConverter();
        var options = new JsonSerializerOptions();
        var jsonUtf8 = "null"u8.ToArray();
        var reader = new Utf8JsonReader(jsonUtf8);
        reader.Read(); // Position the reader on the null token

        // Act
        var result = converter.Read(ref reader, typeof(string), options);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Write_DirectNull_CallsConverter()
    {
        // Arrange
        var converter = new Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw.FlexibleStringConverter();
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, null, options);
        writer.Flush();

        // Assert
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        json.ShouldBe("null");
    }
}