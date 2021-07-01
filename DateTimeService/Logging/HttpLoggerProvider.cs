using Microsoft.Extensions.Logging;

namespace DateTimeService
{
    public class HttpLoggerProvider : ILoggerProvider
    {
        private readonly string host;
        private readonly int port;
        private readonly int portHttp;
        public HttpLoggerProvider(string _host, int _port, int _portHttp)
        {
            host = _host;
            port = _port;
            portHttp = _portHttp;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new HttpLogger(host, port, portHttp);
        }

        public void Dispose()
        {
        }
    }
}
