using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class RedditDateTimeConverterTests
{
    private readonly JsonSerializerOptions _options;

    public RedditDateTimeConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new RedditDateTimeConverter() }
        };
    }

    [Theory]
    [InlineData(1747678685, "2025-05-19T18:18:05Z")]
    [InlineData(1747678685.0, "2025-05-19T18:18:05Z")]
    public void Read_ValidUnixTimestamp_ReturnsExpectedDateTime(object timestamp, string expectedUtc)
    {
        // Arrange
        var json = timestamp is double
            ? $"{timestamp:0.0}"
            : $"{timestamp}";
        var wrappedJson = $"{{\"created_utc\": {json} }}";

        // Act
        var result = JsonSerializer.Deserialize<TestDate>(wrappedJson, _options);

        // Assert
        result.ShouldNotBeNull();
        result!.Date.ShouldBe(DateTime
            .Parse(expectedUtc, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));
    }

    [Fact]
    public void Read_InvalidToken_ThrowsJsonException()
    {
        // Arrange
        var json = "{\"created_utc\": \"not_a_number\"}";

        // Act & Assert
        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<TestDate>(json, _options));
    }

    [Fact]
    public void Write_WritesUnixTimestamp()
    {
        // Arrange
        var testDate = new TestDate
        {
            Date = new DateTime(2025, 5, 19, 18, 18, 5, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(testDate, _options);

        // Assert
        json.ShouldContain("\"created_utc\":1747678685.0");
    }

    private class TestDate
    {
        [JsonPropertyName("created_utc")]
        [JsonConverter(typeof(RedditDateTimeConverter))]
        public DateTime Date { get; set; }
    }
}