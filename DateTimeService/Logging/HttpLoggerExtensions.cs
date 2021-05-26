using Microsoft.Extensions.Logging;

namespace DateTimeService.Logging
{
    public static class HttpLoggerExtensions
    {
        public static ILoggerFactory AddHttp(this ILoggerFactory factory, string host, int port, int portHttp)
        {
            factory.AddProvider(new HttpLoggerProvider(host,port, portHttp));
            return factory;
        }
    }
}
