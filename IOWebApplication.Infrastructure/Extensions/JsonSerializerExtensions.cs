// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace System.Text.Json
{

    /// <summary>
    /// System.Text.Json.JsonSerializer with default camel case naming policy
    /// </summary>
    public static class JsonTextSerializer
    {
        static JsonSerializerOptions options => new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public static T Deserialize<T>(string jsonText) where T : class
        {
            return JsonSerializer.Deserialize<T>(jsonText, options);
        }

        public static string Serialize<T>(T model) where T : class
        {
            //if (typeof(T).IsAssignableFrom(typeof(IQueryable)))
            //{
            //    return JsonSerializer.Serialize(((IQueryable)model).ToList(), options);
            //}

            return JsonSerializer.Serialize(model, options);
        }

        public static string Serialize<T>(IQueryable<T> model) where T : class
        {
            if (model == null) return string.Empty;

            return JsonSerializer.Serialize(model.ToList(), options);
        }
    }
}
