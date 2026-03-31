using Integration.Epep;
using IOWebApplication.Infrastructure.Contracts;
using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

namespace IOWebApplication.Infrastructure.Services
{
    public class EpepConnectionService : IEpepConnectionService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<EpepConnectionService> logger;
        public EpepConnectionService(IConfiguration _configuration,
            ILogger<EpepConnectionService> _logger)
        {
            configuration = _configuration;
            logger = _logger;
        }

        public async Task<IeCaseServiceClient> Connect()
        {
            var endpointUrl = configuration.GetValue<string>("EPEP:Endpoint");
            if (endpointUrl.StartsWith("https"))
            {
                return await connectHttps(endpointUrl);
            }
            else
            {
                return await connectHttp(endpointUrl);
            }
        }

        async Task<IeCaseServiceClient> connectHttp(string url)
        {
            var myBinding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            myBinding.SendTimeout = new TimeSpan(0, 2, 30);
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            EndpointAddress myEndpoint = new EndpointAddress(url);

            IeCaseServiceClient serviceClient = new IeCaseServiceClient(myBinding, myEndpoint);

            try
            {
                await serviceClient.OpenAsync();
                return serviceClient;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "EpepConnectionService");
                return null;
            }
        }

        async Task<IeCaseServiceClient> connectHttps(string url)
        {
            BasicHttpsBinding myBinding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
            if (!string.IsNullOrEmpty(configuration.GetValue<string>("EPEP:CertificatePath")))
            {
                myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            }
            myBinding.SendTimeout = new TimeSpan(0, 2, 30);
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            EndpointAddress myEndpoint = new EndpointAddress(url);

            IeCaseServiceClient serviceClient = new IeCaseServiceClient(myBinding, myEndpoint);

            if (!string.IsNullOrEmpty(configuration.GetValue<string>("EPEP:CertificatePath")))
            {
                serviceClient.ClientCredentials.ClientCertificate.Certificate =
                    new X509Certificate2(configuration.GetValue<string>("EPEP:CertificatePath"),
                       configuration.GetValue<string>("EPEP:CertificatePassword"));
            }
            try
            {
                await serviceClient.OpenAsync();
                return serviceClient;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "EpepConnectionService");
                return null;
            }
        }

        public async Task Reconnect(IeCaseServiceClient serviceClient)
        {
            if (serviceClient == null)
            {
                return;
            }
            ICommunicationObject conn = (ICommunicationObject)serviceClient;
            if (conn.State != CommunicationState.Opened)
            {
                try
                {
                    await serviceClient.OpenAsync();
                }
                catch { }
            }
        }
    }
}
