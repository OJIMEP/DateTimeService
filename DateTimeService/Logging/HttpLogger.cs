using DateTimeService.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace DateTimeService
{
    public class HttpLogger : ILogger
    {
        private readonly string logsHost;
        private readonly int logsPort;
        readonly UdpClient udpClient;
        
        public HttpLogger(string host, int port)
        {
            logsHost = host;
            logsPort = port;
            udpClient = new UdpClient(logsHost, logsPort);
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

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {

                var logMessage = new ElasticLogMessage();
                if (!formatter(state, exception).Contains("ResponseContent"))
                {
                    var logElement = new ElasticLogElement
                    {
                        TimeSQLExecution = 0,
                        ErrorDescription = formatter(state, exception),
                        Status = "Info"
                    };

                    var logstringElement = JsonSerializer.Serialize(logElement);
                    logMessage.message.Add(logstringElement);
                }
                else
                {
                    logMessage.message.Add(formatter(state, exception));
                }
                

                var resultLog = JsonSerializer.Serialize(logMessage);

                Byte[] sendBytes = Encoding.UTF8.GetBytes(resultLog);
                
                try
                {
                    await udpClient.SendAsync(sendBytes, sendBytes.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }


            }
        
        }
    }
}
