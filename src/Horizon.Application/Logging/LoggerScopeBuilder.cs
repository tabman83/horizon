using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Horizon.Application.Logging;

public class LoggerScopeBuilder(ILogger logger, string initialKey, object initialValue)
{
    private readonly ILogger _logger = logger;
    private readonly Dictionary<string, object> _properties = new() { [initialKey] = initialValue };

    public LoggerScopeBuilder With(string key, object value)
    {
        _properties[key] = value;
        return this;
    }

    public IDisposable? BeginScope() => _logger.BeginScope(_properties);
}
