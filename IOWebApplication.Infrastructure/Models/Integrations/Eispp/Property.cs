// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// Property
    /// Параметър на ЕИСПП пакет
    /// </summary>
    public class Property
    {
        /// <summary>
        /// Създава празен обект за десериализация на пакет
        /// </summary>
        public Property()
        {

        }

        /// <summary>
        /// Създава предварително инициализиран обект
        /// </summary>
        /// <param name="name">име  на параметъра</param>
        /// <param name="value">стойност на параметъра</param>
        public Property(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// name
        /// име на параметъра
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// value
        /// стойност на параметъра
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }
}
