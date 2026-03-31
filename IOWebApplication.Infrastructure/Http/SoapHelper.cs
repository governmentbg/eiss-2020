
using System;

namespace IOWebApplication.Infrastructure.Http
{
    public class SoapHelper
    {
        public static string ConstructSoap(string xml, string action, string actionNamespace)
        {
            return String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
                                    <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
                                        <s:Body>
                                            <{0} xmlns=""{1}"">
                                                {2}
                                            </{0}>
                                        </s:Body>
                                    </s:Envelope>", action, actionNamespace, xml);
        }
    }
}
