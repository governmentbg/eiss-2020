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
    public class EkRegionConfiguration : IEntityTypeConfiguration<EkRegion>
    {
        public void Configure(EntityTypeBuilder<EkRegion> builder)
        {
            List<EkRegion> areas = new List<EkRegion>();

            var jsonData = FileHelper.GetTextFromFile("InitialData/EKATTE/ek_regions.json");

            if (!String.IsNullOrEmpty(jsonData))
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };

                areas = JsonConvert.DeserializeObject<List<EkRegion>>(jsonData, new JsonSerializerSettings()
                { ContractResolver = contractResolver });

                builder.HasData(areas);
            }
        }
    }
}
