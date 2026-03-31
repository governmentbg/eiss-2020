// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IOWebApplication.Infrastructure.Extensions
{

    public class JsonDateTimeFormatConvertor : JsonConverter<DateTime>
    {
        public JsonDateTimeFormatConvertor(string format = "yyyy-MM-ddTHH:mm:ss.fffZ")
        {
            this.Format = format;
        }
        //2024-06-20T08:58:07.512Z
        private string Format = "";
        public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options)
        {
            writer.WriteStringValue(date.ToString(Format));
        }
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strValue = reader.GetString();
            try
            {
                return DateTime.ParseExact(strValue, Format, null);
            }
            catch (Exception ex)
            {
                throw new Exception($"JsonDateTimeFormatConvertor.DateTime: {strValue}");
            }
        }
    }

}
