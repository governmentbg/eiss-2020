// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.AspNetCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace IOWebApplication.Extensions
{
    public class DataTablesResponseDataContractResolver : DefaultContractResolver
    {
        public static readonly DataTablesResponseDataContractResolver Instance = new DataTablesResponseDataContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            NamingStrategy = new CamelCaseNamingStrategy();

            if (property.DeclaringType == typeof(DataTablesResponse))
            {
                if (property.PropertyName.Equals("TotalRecordsFiltered", StringComparison.OrdinalIgnoreCase))
                {
                    property.PropertyName = "recordsFiltered";
                }
                else if (property.PropertyName.Equals("TotalRecords", StringComparison.OrdinalIgnoreCase))
                {
                    property.PropertyName = "recordsTotal";
                }
                else if (property.PropertyName.Equals("Draw", StringComparison.OrdinalIgnoreCase))
                {
                    property.PropertyName = "draw";
                }
            }

            return property;
        }

    }
}
