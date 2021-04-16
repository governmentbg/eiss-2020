using IOWebApplication.Infrastructure.Contracts;
using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Integration.LegalActs;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Channels;

namespace IOWebApplication.Infrastructure.Services
{
    public class CubipsaConnectionService : ICubipsaConnectionService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        public CubipsaConnectionService(IConfiguration _configuration, ILogger<CubipsaConnectionService> _logger)
        {
            configuration = _configuration;
            logger = _logger;
        }
        public async Task<LegalActsServiceClient> Connect()
        {

            BasicHttpBinding httpBinding = new BasicHttpBinding();
            httpBinding.SendTimeout = new TimeSpan(0, 2, 30);
            httpBinding.MaxReceivedMessageSize = int.MaxValue;
            httpBinding.MaxBufferSize = int.MaxValue;
            httpBinding.MaxBufferPoolSize = int.MaxValue;
            httpBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            BasicHttpsBinding httpsBinding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
            httpsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            httpsBinding.SendTimeout = new TimeSpan(0, 2, 30);
            httpsBinding.MaxReceivedMessageSize = int.MaxValue;
            httpsBinding.MaxBufferSize = int.MaxValue;
            httpsBinding.MaxBufferPoolSize = int.MaxValue;
            httpsBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            EndpointAddress myEndpoint = new EndpointAddress(configuration.GetValue<string>("LegalActs:Endpoint"));

            Binding serviceBinding = httpBinding;
            var isHttps = myEndpoint.Uri.AbsoluteUri.StartsWith("https", StringComparison.InvariantCultureIgnoreCase);
            if (isHttps)
            {
                serviceBinding = httpsBinding;
            }

            var serviceClient = new LegalActsServiceClient(serviceBinding, myEndpoint);

            if (isHttps && !string.IsNullOrEmpty(configuration.GetValue<string>("LegalActs:CertificatePath")))
            {
                serviceClient.ClientCredentials.ClientCertificate.Certificate =
                        new X509Certificate2(configuration.GetValue<string>("LegalActs:CertificatePath"),
                            configuration.GetValue<string>("LegalActs:CertificatePassword"));
            }

            try
            {
                await serviceClient.OpenAsync();
                return serviceClient;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"CubipsaConnectionService OpenAsync");
                return null;
            }
        }
    }
}
