using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IOWebApplication.Infrastructure.Extensions
{
    public static class GenericExtensions
    {

        public static string GetMessageFault(this FaultException fex)
        {
            try
            {
                var errorElement = XElement.Parse(fex.CreateMessageFault().GetReaderAtDetailContents().ReadOuterXml());
                var errorDictionary = errorElement.Elements().ToDictionary(key => key.Name.LocalName, val => val.Value);
                return string.Join(";", errorDictionary);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Създава и изпраща HttpRequestMessage през текущия HttpClient 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="method">HttpMethod</param>
        /// <param name="data">Обект за изпращане</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SendMessage(this HttpClient client, HttpMethod method, object data = null)
        {
            HttpRequestMessage _message = new HttpRequestMessage();
            _message.Method = method;
            if (data != null)
            {
                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                _message.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            }
            return await client.SendAsync(_message);
        }

        public static T[] ValueToArray<T>(this T value)
        {
            var result = new List<T>();
            result.Add(value);
            return result.ToArray();
        }

        public static int[] StringToIntArray(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new List<int>().ToArray();
            }

            return value.Split(',').Select(x => int.Parse(x)).ToArray();
        }

        public static string TrimSpaces(this string model)
        {
            if (model == null)
            {
                return null;
            }
            var regex = new Regex("[ ]{2,}", RegexOptions.None);
            return regex.Replace(model, " ");
        }

        /// <summary>
        /// Съкращава имена до първа гласна буква
        /// </summary>
        /// <param name="name"></param>
        /// <param name="appendDot"></param>
        /// <returns></returns>
        public static string ToShortNameCyrlillic(this string name, bool appendDot = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            char[] vowelLetters = { 'А', 'Ъ', 'О', 'У', 'Е', 'И', 'Я', 'а', 'ъ', 'о', 'у', 'е', 'и', 'я' };
            char letterInVowel;
            string shortName = "";
            for (int i = 0; i < name.Length; i++)
            {
                letterInVowel = vowelLetters
                .FirstOrDefault(l => l == name[i]);

                if (letterInVowel != '\0' && i != 0)
                {
                    if (appendDot)
                    {
                        return $"{shortName}.";
                    }
                    else
                    {
                        return $"{shortName}";
                    }

                }

                shortName += name[i];
            }
            return name;
        }
    }
}
