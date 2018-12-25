namespace PressCenters.Worker.Runner
{
    using System;

    using Microsoft.Extensions.Logging;

    public class OneLineConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName);
        }

        public void Dispose()
        {
        }

        public class CustomConsoleLogger : ILogger
        {
            private readonly string categoryName;

            public CustomConsoleLogger(string categoryName)
            {
                this.categoryName = categoryName.Replace("PressCenters.Worker.Runner.", string.Empty)
                    .Replace("PressCenters.Worker.Tasks.", string.Empty)
                    .Replace("PressCenters.Worker.Common.", string.Empty);
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!this.IsEnabled(logLevel))
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
