using Integration.Eispp;
using IOWebApplication.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    public class EisppConnectionService : IEisppConnectionService
    {
        private readonly IConfiguration configuration;

        private readonly ILogger logger;

        public EisppConnectionService(
            IConfiguration _configuration,
            ILogger<EisppConnectionService> _logger)
        {
            configuration = _configuration;
            logger = _logger;
        }

        public async Task<EisppServiceClient> Connect()
        {
            BasicHttpBinding myBinding = new BasicHttpBinding();
            myBinding.SendTimeout = new TimeSpan(0, 2, 30);
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            EndpointAddress myEndpoint = new EndpointAddress(configuration.GetValue<string>("EISPP:Endpoint"));

            EisppServiceClient serviceClient = new EisppServiceClient(myBinding, myEndpoint);

            try
            {
                await serviceClient.OpenAsync();
                return serviceClient;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while opening EISPP proxy client");
                return null;
            }
        }
    }
}
