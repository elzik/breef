using Xunit.Abstractions;
using Microsoft.Extensions.Logging;

namespace Elzik.Breef.Domain.Tests.Integration
{
    public partial class ContentSummariserTests
    {
        public class TestOutputLoggerProvider : ILoggerProvider
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public TestOutputLoggerProvider(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new TestOutputLogger(_testOutputHelper, categoryName);
            }

            public void Dispose()
            {
            }
        }
    }
}
