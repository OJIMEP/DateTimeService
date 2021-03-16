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
        private string logsHost;
        private int logsPort;
        UdpClient udpClient;
        TcpClient tcpClient;
        //NetworkStream netStream;

        public HttpLogger(string host, int port)
        {
            logsHost = host;
            logsPort = port;
            //udpClient = new UdpClient(logsHost, logsPort);
            tcpClient = new TcpClient(logsHost, logsPort);
            
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
                logMessage.message.Add(formatter(state, exception));

                var resultLog = JsonSerializer.Serialize(logMessage);

                // UdpClient udpClient = new UdpClient("192.168.2.16", 5048);
                Byte[] sendBytes = Encoding.UTF8.GetBytes(resultLog);
                var netStream = tcpClient.GetStream();

                if (netStream.CanWrite)
                {
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                }
                else
                {
                    Console.WriteLine("You cannot write data to this stream.");
                    tcpClient.Close();

                    // Closing the tcpClient instance does not close the network stream.
                    netStream.Close();
                    return;
                }
                if (netStream.CanRead)
                {
                    // Reads NetworkStream into a byte buffer.
                    byte[] bytes = new byte[tcpClient.ReceiveBufferSize];

                    // Read can return anything from 0 to numBytesToRead.
                    // This method blocks until at least one byte is read.
                    if (netStream.DataAvailable)
                    { 
                        netStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize); 
                    }
                    

                    // Returns the data received from the host to the console.
                    string returndata = Encoding.UTF8.GetString(bytes);

                    Console.WriteLine("This is what the host returned to you: " + returndata);
                }
                else
                {
                    Console.WriteLine("You cannot read data from this stream.");
                    tcpClient.Close();

                    // Closing the tcpClient instance does not close the network stream.
                    netStream.Close();
                    return;
                }

                netStream.Close();
                //try
                //{
                //    await udpClient.SendAsync(sendBytes, sendBytes.Length);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine(e.ToString());
                //}


            }
        }
    }
}
