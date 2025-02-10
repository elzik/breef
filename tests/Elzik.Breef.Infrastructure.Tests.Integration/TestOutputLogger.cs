using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration
{
    public class TestOutputLogger(ITestOutputHelper testOutputHelper, string categoryName) : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
        private readonly string _categoryName = categoryName;

        IDisposable? ILogger.BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter != null)
            {
                _testOutputHelper.WriteLine($"{logLevel}: {_categoryName} - {formatter(state, exception)}");
            }
        }
    }
}