using Microsoft.Extensions.Logging;

namespace DateTimeService.Logging
{
    public static class HttpLoggerExtensions
    {
        public static ILoggerFactory AddFile(this ILoggerFactory factory, string filePath)
        {
            factory.AddProvider(new HttpLoggerProvider(filePath));
            return factory;
        }
    }
}
