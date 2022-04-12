using Common;
using System;
using System.ServiceModel;

/// <summary>
/// WCF Client for TextAnalyzerContract
/// </summary>
namespace Client
{
    /// <summary>
    /// Client tasks uses WCF ChannelFactory to initiate a channel with the host
    /// </summary>
    class Client
    {
        private ITextAnalyzerContract client = null;
        private ChannelFactory<ITextAnalyzerContract> factory = null;

        private string hostAddress;

        /// <summary>
        /// Client constructor
        /// </summary>
        /// <param name="address">the host address</param>
        public Client(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException();
            hostAddress = address;
        }

        /// <summary>
        /// TextAnalyzerClient uses NetTcp binding to open connection to a host
        /// </summary>
        public ITextAnalyzerContract TextAnalyzerClient
        {
            get
            {
                try
                {
                    NetTcpBinding clientBinding = new NetTcpBinding();
                    clientBinding.Security.Mode = SecurityMode.None;
                    clientBinding.ReceiveTimeout = TimeSpan.FromMinutes(5);
                    clientBinding.SendTimeout = TimeSpan.FromMinutes(5);

                    // Create the endpoint
                    factory = new ChannelFactory<ITextAnalyzerContract>(clientBinding, hostAddress);

                    client = factory.CreateChannel();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Client Error:", ex);
                }
                return client;
            }
        }
    }
}
