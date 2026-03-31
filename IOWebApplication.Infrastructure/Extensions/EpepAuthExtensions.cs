// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace IOWebApplication.Infrastructure.Extensions
{
    public static class EpepAuthExtensions
    {
        public static string ComputeHashFromData(string data, string secret)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? "")))
            {
                byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return ToHexString(computedHash);
            }
        }

        /// <summary>
        /// Кодира текст в шестнайсетичен код
        /// </summary>
        /// <param name="bytes">Текста за кодиране, 
        /// като масив от байтове</param>
        /// <returns>текст в шестнайсетичен код</returns>
        private static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }

        public static StringContent GetJsonContent(Object obj)
        {
            string jsonContent = JsonConvert.SerializeObject(obj);
            return new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }
    }
}
