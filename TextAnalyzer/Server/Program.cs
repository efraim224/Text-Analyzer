using Common;
using System;
using System.ServiceModel;

namespace Server
{
    class Program
    {
        private static ServiceHost host = null;

        /// <summary>
        /// Implement self-hosted WCF server
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                string hostAddress = "net.tcp://localhost:8080/TextAnalyzer";
                Uri baseAddress = new Uri(hostAddress);

                NetTcpBinding hostBinding = new NetTcpBinding();
                hostBinding.Security.Mode = SecurityMode.None;
                hostBinding.MaxReceivedMessageSize = 2 * 1024 * 1024;
                hostBinding.ReceiveTimeout = TimeSpan.FromMinutes(5);

                // Create the ServiceHost
                host = new ServiceHost(typeof(TextAnalyzerImplementation));
                host.AddServiceEndpoint(typeof(ITextAnalyzerContract), hostBinding, hostAddress);
                host.Open();

                Console.WriteLine("The server host is listening on: {0}", hostAddress);

                Console.Write("Press any key to exit...");
                Console.ReadKey();
            }
            catch (CommunicationException)
            {
                host.Abort();
            }
        }
    }
}
