// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Data.InitialData.Nomenclatures
{
    public class InitialDataConfiguration<T> : IEntityTypeConfiguration<T>
        where T : class
    {
        private string filePath = string.Empty;
        public InitialDataConfiguration(string dataFilePath)
        {
            filePath = dataFilePath;
        }
        public void Configure(EntityTypeBuilder<T> builder)
        {
            var jsonData = FileHelper.GetTextFromFile(filePath);

            if (!String.IsNullOrEmpty(jsonData))
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };

                List<T> items = JsonConvert.DeserializeObject<List<T>>(jsonData, new JsonSerializerSettings() { ContractResolver = contractResolver });

                builder.HasData(items);
            }
        }
    }
}
