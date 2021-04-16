using Integration.Epep;
using IOWebApplication.Infrastructure.Contracts;
using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Services
{
    public class EpepConnectionService : IEpepConnectionService
    {
        private readonly IConfiguration configuration;
        public EpepConnectionService(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public async Task<IeCaseServiceClient> Connect()
        {
            BasicHttpsBinding myBinding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
            myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            myBinding.SendTimeout = new TimeSpan(0, 2, 30);
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            EndpointAddress myEndpoint = new EndpointAddress(configuration.GetValue<string>("EPEP:Endpoint"));

            IeCaseServiceClient serviceClient = new IeCaseServiceClient(myBinding, myEndpoint);

            serviceClient.ClientCredentials.ClientCertificate.Certificate =
                new X509Certificate2(configuration.GetValue<string>("EPEP:CertificatePath"),
                   configuration.GetValue<string>("EPEP:CertificatePassword"));

            try
            {
                await serviceClient.OpenAsync();
                return serviceClient;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
