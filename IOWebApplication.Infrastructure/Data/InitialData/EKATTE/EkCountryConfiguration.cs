// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Data.InitialData.EKATTE
{
    class EkCountryConfiguration : IEntityTypeConfiguration<EkCountry>
    {
        public void Configure(EntityTypeBuilder<EkCountry> builder)
        {
            List<EkCountry> countries = new List<EkCountry>();

            var jsonData = FileHelper.GetTextFromFile("InitialData/EKATTE/ek_countries.json");

            if (!String.IsNullOrEmpty(jsonData))
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };

                countries = JsonConvert.DeserializeObject<List<EkCountry>>(jsonData, new JsonSerializerSettings() { ContractResolver = contractResolver });

                builder.HasData(countries);
            }
        }
    }
}
