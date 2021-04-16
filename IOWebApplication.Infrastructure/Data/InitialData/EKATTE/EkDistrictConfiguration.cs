using IOWebApplication.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Data.InitialData.EKATTE
{
    public class EkDistrictConfiguration : IEntityTypeConfiguration<EkDistrict>
    {
        public void Configure(EntityTypeBuilder<EkDistrict> builder)
        {
            List<EkDistrict> areas = new List<EkDistrict>();

            var jsonData = FileHelper.GetTextFromFile("InitialData/EKATTE/ek_districts.json");

            if (!String.IsNullOrEmpty(jsonData))
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };

                areas = JsonConvert.DeserializeObject<List<EkDistrict>>(jsonData, new JsonSerializerSettings()
                { ContractResolver = contractResolver });

                builder.HasData(areas);
            }
        }
    }
}
