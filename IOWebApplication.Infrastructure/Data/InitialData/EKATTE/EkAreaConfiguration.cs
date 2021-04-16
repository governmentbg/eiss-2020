using IOWebApplication.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Data.InitialData.EKATTE
{
    public class EkAreaConfiguration : IEntityTypeConfiguration<EkArea>
    {
        public void Configure(EntityTypeBuilder<EkArea> builder)
        {
            List<EkArea> areas = new List<EkArea>();

            var jsonData = FileHelper.GetTextFromFile("InitialData/EKATTE/ek_areas.json");

            if (!String.IsNullOrEmpty(jsonData))
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };

                areas = JsonConvert.DeserializeObject<List<EkArea>>(jsonData, new JsonSerializerSettings() { ContractResolver = contractResolver });

                builder.HasData(areas);
            }
        }
    }
}
