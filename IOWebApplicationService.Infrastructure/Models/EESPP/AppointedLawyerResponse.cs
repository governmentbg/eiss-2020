using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace IOWebApplicationService.Infrastructure.Models.EESPP
{
    public class AppointedLawyerResponse
    {
        /// <summary>
        /// Уникален идентификатор на искането
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Съд, който подава искането
        /// </summary>
        public string JusticeCourtId { get; set; }

        /// <summary>
        /// Назначени защитници
        /// </summary>
        [XmlArray("Lawyers"), XmlArrayItem("LawyerInfo")]
        public AppointedLawyer[] Lawyers { get; set; }
    }
}
