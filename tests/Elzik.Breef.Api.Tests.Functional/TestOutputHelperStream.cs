using Xunit.Abstractions;

namespace Elzik.Breef.Api.Tests.Functional
{
    public class TestOutputHelperStream(ITestOutputHelper testOutputHelper) : Stream
    {
        private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
        private readonly MemoryStream _memoryStream = new();

        public override void Write(byte[] buffer, int offset, int count)
        {
            _memoryStream.Write(buffer, offset, count);
            _testOutputHelper.WriteLine(System.Text.Encoding.UTF8.GetString(buffer, offset, count));
        }

        public override void Flush() => _memoryStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _memoryStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _memoryStream.Seek(offset, origin);
        public override void SetLength(long value) => _memoryStream.SetLength(value);
        public override bool CanRead => _memoryStream.CanRead;
        public override bool CanSeek => _memoryStream.CanSeek;
        public override bool CanWrite => _memoryStream.CanWrite;
        public override long Length => _memoryStream.Length;
        public override long Position { get => _memoryStream.Position; set => _memoryStream.Position = value; }
    }
}