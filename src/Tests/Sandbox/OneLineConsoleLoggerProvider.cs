namespace Sandbox
{
    using System;

    using Microsoft.Extensions.Logging;

    public sealed class OneLineConsoleLoggerProvider : ILoggerProvider
    {
        private readonly bool useConsole;

        public OneLineConsoleLoggerProvider(bool useConsole)
        {
            this.useConsole = useConsole;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName, this.useConsole);
        }

        public void Dispose()
        {
        }

        public class CustomConsoleLogger : ILogger
        {
            private readonly bool useConsole;

            private readonly string categoryName;

            public CustomConsoleLogger(string categoryName, bool useConsole)
            {
                this.useConsole = useConsole;
                this.categoryName = categoryName;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!this.IsEnabled(logLevel) || !this.useConsole)
                {
                    return;
                }

                if (logLevel > LogLevel.Information)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.BackgroundColor = ConsoleColor.White;
                }

                var logLevelString = logLevel == LogLevel.Information ? "Info" : logLevel.ToString();
                Console.WriteLine($"{logLevelString}: {this.categoryName}[{eventId.Id}]: {formatter(state, exception)}");

                if (logLevel > LogLevel.Information)
                {
                    Console.ResetColor();
                }
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel >= LogLevel.Information;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}
