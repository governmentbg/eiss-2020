using IO.RegixClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Models
{
    /// <summary>
    /// 3-те заявки към Regix
    /// </summary>
    public class RegixPersonAllDataResponse
    {
        /// <summary>
        /// НБД
        /// </summary>
        public PersonDataResponseType PersonDataResponseType { get; set; }

        /// <summary>
        /// Постоянен адрес
        /// </summary>
        public PermanentAddressResponseType PermanentAddressResponseType { get; set; }

        /// <summary>
        /// Настоящ адрес
        /// </summary>
        public TemporaryAddressResponseType TemporaryAddressResponseType { get; set; }
    }
}
