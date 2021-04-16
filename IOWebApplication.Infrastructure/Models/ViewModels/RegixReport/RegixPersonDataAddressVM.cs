using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    /// <summary>
    /// Справка Regix НБД + Постоянен адрес + Настоящ адрес
    /// </summary>
    public class RegixPersonDataAddressVM
    {
        public RegixReportVM Report { get; set; }

        public RegixPersonAddressFilterVM PersonAddressFilter { get; set; }

        public RegixPersonDataResponseVM PersonDataResponse { get; set; }

        public RegixPersonAddressResponseVM PermanentAddressResponse { get; set; }

        public RegixPersonAddressResponseVM TemporaryAddressResponse { get; set; }

        public RegixPersonDataAddressVM()
        {
            Report = new RegixReportVM();
            PersonAddressFilter = new RegixPersonAddressFilterVM();
            PersonDataResponse = new RegixPersonDataResponseVM();
            PermanentAddressResponse = new RegixPersonAddressResponseVM();
            TemporaryAddressResponse = new RegixPersonAddressResponseVM();
        }
    }
}
