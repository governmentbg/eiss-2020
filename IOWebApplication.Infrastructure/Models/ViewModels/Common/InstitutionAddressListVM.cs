using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class InstitutionAddressListVM
    {
        public int InstitutionId { get; set; }

        public long AddressId { get; set; }

        public string FullAddress { get; set; }

        public string AddressTypeName { get; set; }
    }
}
