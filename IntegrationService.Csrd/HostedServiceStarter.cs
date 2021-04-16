using IntegrationService.Csrd.Contracts;
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

        public HostedServiceStarter(
            ICsrdTHS _csrd,
            ICubipsaTHS _cubipsa,
            IEisppTHS _eispp,
            IIspnTHS _ispn
        )
        {
            csrd = _csrd;
            cubipsa = _cubipsa;
            eispp = _eispp;
            ispn = _ispn;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Task.Run(() => csrd.StartAsync(cancellationToken));

                Task.Run(() => cubipsa.StartAsync(cancellationToken));

                Task.Run(() => eispp.StartAsync(cancellationToken));

                Task.Run(() =>ispn.StartAsync(cancellationToken));

            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Task.Run(() => csrd.StopAsync(cancellationToken));

                Task.Run(() => cubipsa.StopAsync(cancellationToken));

                Task.Run(() => eispp.StopAsync(cancellationToken));

                Task.Run(() => ispn.StopAsync(cancellationToken));

            }, cancellationToken);
        }
    }

}
