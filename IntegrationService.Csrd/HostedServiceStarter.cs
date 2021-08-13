using IntegrationService.Csrd.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationService.Csrd
{
    public class HostedServiceStarter : IHostedService
    {
        private readonly ICsrdTHS csrd;
        private readonly ICubipsaTHS cubipsa;
        private readonly IEisppTHS eispp;
        private readonly IIspnTHS ispn;
        private readonly IEproTHS epro;
        private readonly IElasticTHS elastic;
        private readonly IConfiguration configuration;

        public HostedServiceStarter(
            ICsrdTHS _csrd,
            ICubipsaTHS _cubipsa,
            IEisppTHS _eispp,
            IIspnTHS _ispn,
            IEproTHS _epro,
            IElasticTHS _elastic,
            IConfiguration _configuration
        )
        {

            configuration = _configuration;
            if (configuration.GetSection("EISPP").Exists())
            {
                eispp = _eispp;
            }
            if (configuration.GetSection("CSRD").Exists())
            {
                csrd = _csrd;
            }
            if (configuration.GetSection("ISPN").Exists())
            {
                ispn = _ispn;
            }
            if (configuration.GetSection("LegalActs").Exists())
            {
                cubipsa = _cubipsa;
            }
            if (configuration.GetSection("EPRO").Exists())
            {
                epro = _epro;
            }
            if (configuration.GetSection("ElasticSearch").Exists())
            {
                elastic = _elastic;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                if (csrd != null)
                {
                    Task.Run(() => csrd.StartAsync(cancellationToken));
                }

                if (cubipsa != null)
                {
                    Task.Run(() => cubipsa.StartAsync(cancellationToken));
                }

                if (eispp != null)
                {
                    Task.Run(() => eispp.StartAsync(cancellationToken));
                }

                if (ispn != null)
                {
                    Task.Run(() => ispn.StartAsync(cancellationToken));
                }

                if (epro != null)
                {
                    Task.Run(() => epro.StartAsync(cancellationToken));
                }
                if (elastic != null)
                {
                    Task.Run(() => elastic.StartAsync(cancellationToken));
                }

            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                if (csrd != null)
                {
                    Task.Run(() => csrd.StopAsync(cancellationToken));
                }

                if (cubipsa != null)
                {
                    Task.Run(() => cubipsa.StopAsync(cancellationToken));
                }

                if (eispp != null)
                {
                    Task.Run(() => eispp.StopAsync(cancellationToken));
                }

                if (ispn != null)
                {
                    Task.Run(() => ispn.StopAsync(cancellationToken));
                }

                if (epro != null)
                {
                    Task.Run(() => epro.StopAsync(cancellationToken));
                }

                if (elastic != null)
                {
                    Task.Run(() => elastic.StopAsync(cancellationToken));
                }
            }, cancellationToken);
        }
    }

}
