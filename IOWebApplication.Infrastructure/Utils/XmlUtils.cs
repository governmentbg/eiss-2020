// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.EISPP;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Utils
{
    /// <summary>
    /// Методи за работа с XML
    /// </summary>
    public static class XmlUtils
    {
        /// <summary>
        /// Десериализира XML текст към EisppMessage
        /// Опитва се да коригира невалидни данни в 
        /// XML-ите
        /// </summary>
        /// <param name="message">XML</param>
        /// <returns></returns>
        public static EisppMessage DeserializeEisppMessage(string message)
        {
            EisppMessage result = null;

            XDocument xdoc = XDocument.Parse(message);
            xdoc.Descendants()
                .SelectMany(e => e.Attributes()
                    .Where(a => string.IsNullOrEmpty(a.Value)))
                .Remove();

            xdoc.Descendants()
                .SelectMany(e => e.Attributes()
                    .Where(a => a.Value.Contains("T00:00:00.000")))
                .ToList()
                .ForEach(a =>
                {
                    DateTime current;

                    if (DateTime.TryParse(a.Value, out current))
                    {
                        a.Value = current.ToString("yyyy-MM-dd");
                    }
                });

            XmlSerializer serializer = new XmlSerializer(typeof(EisppMessage));
            using (var reader = xdoc.Root.CreateReader())
            {
                result = (EisppMessage)serializer.Deserialize(reader);
            }

            return result;
        }

        /// <summary>
        /// Десериализира XML към произволен клас
        /// </summary>
        /// <typeparam name="T">Тип на обект към който се десериализира (трябва да е клас)</typeparam>
        /// <param name="xml">XML, който се десериализира</param>
        /// <returns></returns>
        public static T DeserializeXml<T>(string xml) where T : class
        {
            T result = null;

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XDocument xdoc = XDocument.Parse(xml);
            using (XmlReader reader = xdoc.Root.CreateReader())
            {
                result = (T)serializer.Deserialize(reader);
            }

            return result;
        }

        /// <summary>
        /// Сериализира ЕИСПП Пакет
        /// </summary>
        /// <param name="package">Пакет за сериализация</param>
        /// <returns></returns>
        public static string SerializeEisppPackage(EisppPackage package)
        {
            return SerializeObject(package, true);
        }

        /// <summary>
        /// Сериализира произволен обект към XML
        /// </summary>
        /// <typeparam name="T">Тип на обекта за сериализация (трябва да е клас)</typeparam>
        /// <param name="objectToSerialize">Обект за сериализация</param>
        /// <param name="removeNamespaces">Дали от резултатния XML да се премахнат стандартните Namespace-и
        /// </param>
        /// <returns></returns>
        public static string SerializeObject<T>(T objectToSerialize, bool removeNamespaces = false) where T : class
        {
            string result;

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (TextWriter writer = new Utf8StringWriter())
            {
                if (removeNamespaces)
                {
                    serializer.Serialize(writer, objectToSerialize, ns);
                }
                else
                {
                    serializer.Serialize(writer, objectToSerialize);
                }
                
                result = writer.ToString();
            }

            return result;
        }

      

    }
}
