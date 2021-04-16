using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CasePersonAddressListVM
    {
        public int Id { get; set; }

        public string AddressTypeName { get; set; }

        public string FullAddress { get; set; }

        public bool ForNotification { get; set; }
    }
}
