// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_address")]
    public class Address
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("address_type_id")]
        [Display(Name ="Вид адрес")]
        public int AddressTypeId { get; set; }

        [Column("country_code")]
        [Display(Name ="Държава")]
        public string CountryCode { get; set; }

        [Column("district_code")]
        public string DistrictCode { get; set; }

        [Column("municipality_code")]
        public string MunicipalityCode { get; set; }

        [Column("city_code")]
        [Display(Name ="Населено място")]
        public string CityCode { get; set; }

        [Column("region_code")]
        [Display(Name ="Регион")]
        public string RegionCode { get; set; }

        [Column("street_code")]
        [Display(Name ="Улица")]
        public string StreetCode { get; set; }

        [Column("foreign_address")]
        [Display(Name ="Адрес")]
        public string ForeignAddress { get; set; }

        [Column("block")]
        [Display(Name ="Блок")]
        public int? Block { get; set; }

        [Column("residential_area_code")]
        [Display(Name = "Квартал/ж.к.")]
        public string ResidentionAreaCode { get; set; }

        [Column("street_number")]
        [Display(Name ="Ул.номер")]
        public int? StreetNumber { get; set; }

        [Column("sub_number")]
        [Display(Name ="под-номер")]
        public string SubNumber { get; set; }

        [Column("entrance")]
        [Display(Name ="Вход")]
        public string Entrance { get; set; }

        [Column("floor")]
        [Display(Name ="Етаж")]
        public string Floor { get; set; }

        [Column("appartment")]
        [Display(Name ="Апартамент/офис")]
        public string Appartment { get; set; }

        [Column("phone")]
        [Display(Name ="Телефон")]
        public string Phone { get; set; }

        [Column("fax")]
        [Display(Name ="Факс")]
        public string Fax { get; set; }

        [Column("email")]
        [Display(Name ="Електронна поща")]
        public string Email { get; set; }

        [Column("description")]
        [Display(Name ="Забележка")]
        public string Description { get; set; }

        [Column("full_address")]
        public string FullAddress { get; set; }

        [Column("sub_block")]
        [Display(Name = "под-номер")]
        public string SubBlock { get; set; }

        [ForeignKey(nameof(AddressTypeId))]
        public virtual AddressType AddressType { get; set; }

        public Address()
        {
            CountryCode = NomenclatureConstants.CountryBG;
        }
    }
}
