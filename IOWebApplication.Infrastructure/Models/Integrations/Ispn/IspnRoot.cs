// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Ispn
{
    //<xs:schema attributeFormDefault="unqualified" elementFormDefault="qualified" xmlns:xs="http://www.w3.org/2001/XMLSchema"><Transfer>
    
    [XmlRoot("schema", Namespace = "http://www.w3.org/2001/XMLSchema", IsNullable = false)]
    [Serializable]
    public partial class IspnRoot
    {
        [XmlElement("Transfer", Namespace = "")]
        public Transfer Transfer { get; set; }

        [XmlAttribute("attributeFormDefault")]
        public string AttributeFormDefault { get; set; } = "unqualified";

        [XmlAttribute("elementFormDefault")]
        public string ElementFormDefault { get; set; } = "qualified";
    }
}
