using Microsoft.Extensions.Logging;

namespace Horizon.Application.Logging;

public static class LoggerExtensions
{
    public static LoggerScopeBuilder With(this ILogger logger, string key, object value) =>
        new (logger, key, value);
}