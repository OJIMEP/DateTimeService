using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace DateTimeService
{
    public class HttpLogger : ILogger
    {
        private string filePath;
        private List<string> logsCache;
        private static object _lock = new object();
        public HttpLogger(string path)
        {
            filePath = path;
            logsCache = new List<string>();
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            //return logLevel == LogLevel.Trace;
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                lock (_lock)
                {
                    logsCache.Add(formatter(state, exception));
                    //File.AppendAllText(filePath, formatter(state, exception) + Environment.NewLine);
                }
            }
        }

        public void SendLogs()
        {

        }
    }
}
