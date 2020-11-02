// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class RegixPersonAddressVM
    {
        public int AddressTypeId { get; set; }

        public RegixReportVM Report { get; set; }

        public RegixPersonAddressFilterVM PersonAddressFilter { get; set; }

        public RegixPersonAddressResponseVM PersonAddressResponse { get; set; }

        public RegixPersonAddressVM()
        {
            Report = new RegixReportVM();
            PersonAddressFilter = new RegixPersonAddressFilterVM();
            PersonAddressResponse = new RegixPersonAddressResponseVM();
        }
    }
    public class RegixPersonAddressFilterVM
    {
        [Display(Name = "ЕГН")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string EgnFilter { get; set; }
    }

    public class RegixPersonAddressResponseVM
    {
        [Display(Name = "Държава:")]
        public string CountryName { get; set; }

        [Display(Name = "Област:")]
        public string DistrictName { get; set; }

        [Display(Name = "Община:")]
        public string MunicipalityName { get; set; }

        [Display(Name = "Населено място:")]
        public string SettlementName { get; set; }

        [Display(Name = "Район:")]
        public string CityArea { get; set; }

        [Display(Name = "Локализационна единица:")]
        public string LocationName { get; set; }

        [Display(Name = "Номер:")]
        public string BuildingNumber { get; set; }

        [Display(Name = "Вход:")]
        public string Entrance { get; set; }

        [Display(Name = "Етаж:")]
        public string Floor { get; set; }

        [Display(Name = "Апартамент:")]
        public string Apartment { get; set; }

        [Display(Name = "Дата на заявяване:")]
        public string FromDate { get; set; }
    }
}
