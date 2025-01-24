using Xunit.Abstractions;
using Microsoft.Extensions.Logging;

namespace Elzik.Breef.Domain.Tests.Integration
{
    public class TestOutputLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;

        public TestOutputLogger(ITestOutputHelper testOutputHelper, string categoryName)
        {
            _testOutputHelper = testOutputHelper;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                _testOutputHelper.WriteLine($"{logLevel}: {_categoryName} - {formatter(state, exception)}");
            }
        }
    }
}
