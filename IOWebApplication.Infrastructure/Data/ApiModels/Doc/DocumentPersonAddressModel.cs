// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;

namespace IOWebApplication.Infrastructure.Data.ApiModels.Doc
{
    public class DocumentPersonAddressModel
    {
        //Nom
        [JsonProperty("addressType")]
        public string AddressType { get; set; }

        //EKATTE, 2digits
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        //EKATTE
        [JsonProperty("cityCode")]
        public string CityCode { get; set; }

        [JsonProperty("streetFullAddress")]
        public string StreetFullAddress { get; set; }

        //EKATTE
        [JsonProperty("residentionAreaCode")]
        public string ResidentionAreaCode { get; set; }

        [JsonProperty("residentionAreaName")]
        public string ResidentionAreaName { get; set; }
        //EKATTE
        [JsonProperty("streetCode")]
        public string StreetCode { get; set; }

        [JsonProperty("streetName")]
        public string StreetName { get; set; }

        [JsonProperty("foreignAddress")]
        public string ForeignAddress { get; set; }

        [JsonProperty("streetNumber")]
        public string StreetNumber { get; set; }

        [JsonProperty("subNumber")]
        public string SubNumber { get; set; }

        [JsonProperty("entrance")]
        public string Entrance { get; set; }

        [JsonProperty("floor")]
        public string Floor { get; set; }

        [JsonProperty("appartment")]
        public string Appartment { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
