using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService
{
    public class HttpLoggerProvider : ILoggerProvider
    {
        private string path;
        public HttpLoggerProvider(string _path)
        {
            path = _path;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new HttpLogger(path);
        }

        public void Dispose()
        {
        }
    }
}
