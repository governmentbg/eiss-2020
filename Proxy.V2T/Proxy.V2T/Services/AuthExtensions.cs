using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.EISS.Services
{
    public static class AuthExtensions
    {
        public static bool CheckHashData(byte[] data, string hash, string secret)
        {
            //return true;
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? "")))
            {
                byte[] computedHash = hmac.ComputeHash(data);
                var computedHashText = ToHexString(computedHash);
                return hash == computedHashText;
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


        public static string GetBearerToken(this HttpRequest request, string? headerName = null)
        {
            const string AuthorizationHeaderName = "Authorization";
            if (request == null)
            {
                return null;
            }

            if (!request.Headers.TryGetValue(headerName ?? AuthorizationHeaderName, out var tokenHeaderValues))
            {
                return null;
            }

            var providedToken = tokenHeaderValues.FirstOrDefault();

            if (tokenHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(providedToken))
            {
                return null;
            }

            return providedToken
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault()
                ?.Trim();
        }
        public static async Task<byte[]> GetRequestData(this HttpRequest request)
        {
            byte[]? data = null;

            using (MemoryStream ms = new MemoryStream())
            {
                var body = request.Body;
                body.Seek(0, SeekOrigin.Begin);
                await body.CopyToAsync(ms);
                data = ms.ToArray();
                body.Seek(0, SeekOrigin.Begin);
            }


            if (data == null)
            {
                string queryValues = String.Join("", request.Query.Select(x => x.Value));
                data = Encoding.UTF8.GetBytes(queryValues);
            }

            return data;
        }
    }
}
