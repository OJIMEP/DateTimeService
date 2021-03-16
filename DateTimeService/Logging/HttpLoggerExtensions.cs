using Microsoft.Extensions.Logging;

namespace DateTimeService.Logging
{
    public static class HttpLoggerExtensions
    {
        public static ILoggerFactory AddFile(this ILoggerFactory factory, string host, int port)
        {
            factory.AddProvider(new HttpLoggerProvider(host,port));
            return factory;
        }
    }
}
