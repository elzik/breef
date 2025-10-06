using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using Shouldly;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

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
        var wrappedJson = JsonSerializer
            .Serialize(new { created_utc = timestamp });

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
    public void Write_UtcDateTime_WritesCorrectUnixTimestamp()
    {
        // Arrange
        var testDate = new TestDate
        {
            Date = new DateTime(2025, 5, 19, 18, 18, 5, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(testDate, _options);

        // Assert
        json.ShouldContain("\"created_utc\":1747678685");
    }

    [Fact]
    public void Write_LocalDateTime_ConvertsToUtcAndWritesCorrectUnixTimestamp()
    {
        // Arrange
        var localTime = new DateTime(2025, 5, 19, 18, 18, 5, DateTimeKind.Local);
        var expectedUtcTime = localTime.ToUniversalTime();
        var expectedUnixSeconds = new DateTimeOffset(expectedUtcTime).ToUnixTimeSeconds();

        var testDate = new TestDate { Date = localTime };

        // Act
        var json = JsonSerializer.Serialize(testDate, _options);

        // Assert
        json.ShouldContain($"\"created_utc\":{expectedUnixSeconds}");
    }

    [Fact]
    public void Write_UnspecifiedDateTime_TreatsAsUtcAndWritesCorrectUnixTimestamp()
    {
        // Arrange
        var unspecifiedTime = new DateTime(2025, 5, 19, 18, 18, 5, DateTimeKind.Unspecified);
        // When DateTimeKind.Unspecified, it's treated as UTC directly (SpecifyKind to UTC)
        var utcTime = DateTime.SpecifyKind(unspecifiedTime, DateTimeKind.Utc);
        var expectedUnixSeconds = new DateTimeOffset(utcTime).ToUnixTimeSeconds();

        var testDate = new TestDate { Date = unspecifiedTime };

        // Act
        var json = JsonSerializer.Serialize(testDate, _options);

        // Assert
        json.ShouldContain($"\"created_utc\":{expectedUnixSeconds}");
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Write_AllDateTimeKinds_ProducesValidUnixTimestamp(DateTimeKind kind)
    {
        // Arrange
        var baseTime = new DateTime(2025, 5, 19, 18, 18, 5, DateTimeKind.Unspecified);
        var dateTime = kind switch
        {
            DateTimeKind.Utc => DateTime.SpecifyKind(baseTime, DateTimeKind.Utc),
            DateTimeKind.Local => DateTime.SpecifyKind(baseTime, DateTimeKind.Local),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(baseTime, DateTimeKind.Unspecified),
            _ => baseTime
        };

        var testDate = new TestDate { Date = dateTime };

        // Act
        var json = JsonSerializer.Serialize(testDate, _options);

        // Assert
        json.ShouldNotBeNull();
        json.ShouldContain("\"created_utc\":");

        // Extract the timestamp and verify it's a valid number
        var startIndex = json.IndexOf("\"created_utc\":") + "\"created_utc\":".Length;
        var endIndex = json.IndexOf('}', startIndex);
        var timestampStr = json[startIndex..endIndex];

        long.TryParse(timestampStr, out var timestamp).ShouldBeTrue();
        timestamp.ShouldBeGreaterThan(0);
    }

    private class TestDate
    {
        [JsonPropertyName("created_utc")]
        [JsonConverter(typeof(RedditDateTimeConverter))]
        public DateTime Date { get; set; }
    }
}