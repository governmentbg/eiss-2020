using IOWebApplication.Infrastructure.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IISPNHttpRequester 
    {
        HttpRequester requester { get; set; }
    }
}
