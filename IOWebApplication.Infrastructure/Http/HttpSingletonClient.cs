using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Http
{
    public class HttpSingletonClient
    {
        public string CertificatePath { get; set; }
        public string CertificatePassword { get; set; }
        public bool ValidateServerCertificate { get; set; }
    }
}
