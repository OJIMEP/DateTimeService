using DateTimeService.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace DateTimeService
{
    public class HttpLogger : ILogger
    {
        private readonly string logsHost;
        private readonly int logsPortUdp;
        private readonly int logsPortHttp;
        readonly UdpClient udpClient;
        readonly HttpClient httpClient;

        public HttpLogger(string host, int port, int portHttp)
        {
            logsHost = host;
            logsPortUdp = port;
            logsPortHttp = portHttp;
            udpClient = new UdpClient(logsHost, logsPortUdp);
            httpClient = new HttpClient();
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

                    if (exception != null)
                    {
                        logElement.ErrorDescription += ";" + exception.Message;
                        logElement.Status = "Error";
                        logElement.AdditionalData.Add("StackTrace", exception.StackTrace);
                    }

                    var logstringElement = JsonSerializer.Serialize(logElement);
                    logMessage.Message.Add(logstringElement);
                }
                else
                {
                    logMessage.Message.Add(formatter(state, exception));
                }
                

                var resultLog = JsonSerializer.Serialize(logMessage);

                Byte[] sendBytes = Encoding.UTF8.GetBytes(resultLog);
                
                try
                {
                    if (sendBytes.Length > 60000)
                    {
                        var result = await httpClient.PostAsync(new Uri("http://" + logsHost + ":" + logsPortHttp.ToString("D")), new StringContent(resultLog, Encoding.UTF8, "application/json"));
                    }                        
                    else
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
