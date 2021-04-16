using IOWebApplication.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Data.InitialData.EKATTE
{
    public class EkMunincipalityConfiguration : IEntityTypeConfiguration<EkMunincipality>
    {
        public void Configure(EntityTypeBuilder<EkMunincipality> builder)
        {
            List<EkMunincipality> areas = new List<EkMunincipality>();

            var jsonData = FileHelper.GetTextFromFile("InitialData/EKATTE/ek_munincipalities.json");

            if (!String.IsNullOrEmpty(jsonData))
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };

                areas = JsonConvert.DeserializeObject<List<EkMunincipality>>(jsonData, new JsonSerializerSettings()
                { ContractResolver = contractResolver });

                builder.HasData(areas);
            }
        }
    }
}
