// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Epep;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace IOWebApplication.Infrastructure.Utils
{
    public class EpepHelper
    {
        public static string GetName(Person person, Entity entity)
        {
            if (person != null)
                return $"{person.Firstname} {person.Secondname} {person.Lastname}";
            if (entity != null)
                return entity.Name;

            return string.Empty;
        }
        public static string GetUIC(Person person, Entity entity)
        {
            if (person != null)
                return person.EGN;
            if (entity != null)
                return entity.Bulstat;

            return string.Empty;
        }

        public static string GenerateToken(int numberOfBytes = 32)
        {
            var result = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(numberOfBytes));
            result = result.Replace("_", "").ToLower();
            return result;
        }
    }
}
