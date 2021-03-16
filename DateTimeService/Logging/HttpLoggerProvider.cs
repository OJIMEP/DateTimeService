using Microsoft.Extensions.Logging;

namespace DateTimeService
{
    public class HttpLoggerProvider : ILoggerProvider
    {
        private readonly string host;
        private readonly int port;
        public HttpLoggerProvider(string _host, int _port)
        {
            host = _host;
            port = _port;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new HttpLogger(host,port);
        }

        public void Dispose()
        {
        }
    }
}
