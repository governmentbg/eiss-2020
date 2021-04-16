using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentSelectPersonItemVM
    {
        public string Id { get; set; }
        public string RoleName { get; set; }
        public string Uic { get; set; }
        public string UicTypeLabel { get; set; }
        public string FullName { get; set; }
        public bool IsChecked { get; set; }

        public IList<DocumentSelectAddressVM> Addresses { get; set; }

        public DocumentSelectPersonItemVM()
        {
            Addresses = new List<DocumentSelectAddressVM>();
        }

        public void ConvertFromEisppPerson(EisppTSActualDataPersonVM source)
        {
            this.Id = source.Sid;
            this.Uic = source.Uic;
            this.UicTypeLabel = source.UicTypeLabel;
            this.FullName = source.FullName;
            this.IsChecked = true;
            foreach (var adr in source.Addresses)
            {
                DocumentSelectAddressVM newAdr = new DocumentSelectAddressVM()
                {
                    Id = 0,
                    IsChecked = true,
                    AddressTypeName = adr.AddressTypeId.ToString(),
                    FullAddress = adr.FullAddress
                };
                this.Addresses.Add(newAdr);
            }
        }
    }
}
