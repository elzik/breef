using Elzik.Breef.Infrastructure.Wallabag;
using Shouldly;
using System.Text.Json;

namespace Elzik.Breef.Tests.Infrastructure.Wallabag
{
    public class WallabagDateTimeConverterTests
    {
        private readonly WallabagDateTimeConverter _wallabagDateTimeConverter = new();

        [Fact]
        public void Read_ValidDate_ReturnsExpectedDate()
        {
            // Arrange
            var json = "\"2023-10-01T12:34:56Z\"";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            var result = _wallabagDateTimeConverter.Read(ref reader, typeof(DateTime), new JsonSerializerOptions());

            // Assert
            result.ToUniversalTime().ShouldBe(new DateTime(2023, 10, 1, 12, 34, 56, DateTimeKind.Utc));
        }

        [Theory]
        [InlineData("12345", "Expected string token.")]
        [InlineData("\"invalid-date\"", "Unable to convert \"invalid-date\" to a Wallabag DateTime.")]
        public void Read_InvalidInput_Throws(string testJson, string expectedMessage)
        {
            // Arrange
            var testReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(testJson));
            testReader.Read();

            // Act
            JsonException ex;
            try
            {
                _wallabagDateTimeConverter.Read(ref testReader, typeof(DateTime), new JsonSerializerOptions());
                throw new Exception("Expected JsonException was not thrown.");
            }
            catch (JsonException e)
            {
                ex = e;
            }

            // Assert
            ex.Message.ShouldBe(expectedMessage);
        }

        [Fact]
        public void Write_ShouldConvertDateTimeToString()
        {
            // Arrange
            var testDateTime = new DateTime(2023, 10, 1, 12, 34, 56, DateTimeKind.Utc);
            var testOptions = new JsonSerializerOptions { Converters = { _wallabagDateTimeConverter } };
            var testBuffer = new System.Buffers.ArrayBufferWriter<byte>();
            var testWriter = new Utf8JsonWriter(testBuffer);

            // Act
            _wallabagDateTimeConverter.Write(testWriter, testDateTime, testOptions);

            // Assert
            testWriter.Flush();
            var writtenJson = System.Text.Encoding.UTF8.GetString(testBuffer.WrittenMemory.ToArray());
            writtenJson.ShouldBe("\"2023-10-01T12:34:56Z\"");
        }
    }
}
