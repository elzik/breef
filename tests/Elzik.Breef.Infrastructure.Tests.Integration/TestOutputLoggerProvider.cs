using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration
{
    public class TestOutputLoggerProvider(ITestOutputHelper testOutputHelper) : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(_testOutputHelper, categoryName);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
