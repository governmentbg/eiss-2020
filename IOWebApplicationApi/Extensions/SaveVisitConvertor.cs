// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Delivery;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IOWebApplicationApi.Extensions
{

    public class SaveVisitConvertor : JsonConverter<DeliveryItemVisitMobileModel>
    {

        //2024-06-20T08:58:07.512Z
        private readonly string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public override void Write(Utf8JsonWriter writer, DeliveryItemVisitMobileModel data, JsonSerializerOptions options)
        {
            writer.WriteStringValue(System.Text.Json.JsonSerializer.Serialize(data));
        }
        public override DeliveryItemVisitMobileModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strValue = reader.GetString();
            throw new Exception($"SaveVisitConvertor raw value: {strValue}");
            //try
            //{
            //    var result = System.Text.Json.JsonSerializer.Deserialize<DeliveryItemVisitMobileModel>(strValue);
            //    if (result == null)
            //    {
            //        throw new Exception($"SaveVisitConvertor  NULL: {strValue}");
            //    }
            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception($"SaveVisitConvertor catch: {strValue}");
            //}
        }
    }

}
