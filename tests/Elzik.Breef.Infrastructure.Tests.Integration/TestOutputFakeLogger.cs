using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration;

public class TestOutputFakeLogger<T>(ITestOutputHelper testOutputHelper) : ILogger<T>
{
    public FakeLogger<T> FakeLogger { get; } = new FakeLogger<T>();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => FakeLogger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => FakeLogger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // Call the FakeLogger's Log method
        FakeLogger.Log(logLevel, eventId, state, exception, formatter);

        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);
        if (!string.IsNullOrEmpty(message))
        {
            testOutputHelper.WriteLine($"{logLevel}: {message}");
        }

        if (exception != null)
        {
            testOutputHelper.WriteLine(exception.ToString());
        }
    }

    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }
}
