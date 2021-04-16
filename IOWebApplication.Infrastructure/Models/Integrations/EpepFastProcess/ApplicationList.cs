using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    [Serializable]
    [XmlRoot("AppGUIDs")]
    public class ApplicationList
    {
        [XmlElement("guid")]
        public string[] Guids { get; set; }
    }
}
