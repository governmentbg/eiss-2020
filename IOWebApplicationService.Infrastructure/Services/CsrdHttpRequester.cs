using IOWebApplication.Infrastructure.Http;
using IOWebApplicationService.Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class CsrdHttpRequester : ICsrdHttpRequester
    {
        private HttpRequester _requester;
        public HttpRequester requester
        {
            get
            {
                return _requester;
            }
            set { _requester = value; }
        }
    }
}
