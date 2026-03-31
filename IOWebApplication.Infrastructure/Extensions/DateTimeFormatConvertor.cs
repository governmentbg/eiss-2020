// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IOWebApplication.Infrastructure.Extensions
{

    public class DateTimeFormatConvertor : JsonConverter<DateTime>
    {
        public DateTimeFormatConvertor()
        {
        }
        private readonly string Format = "dd.MM.yyyy HH:mm:ss";
        public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options)
        {
            writer.WriteStringValue(date.ToString(Format));
        }
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strValue = reader.GetString();
            throw new Exception($"DateTime: {strValue}");
            //return DateTime.ParseExact(strValue, Format, null);
        }
    }

}
