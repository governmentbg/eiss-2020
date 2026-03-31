// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Sisma
{
    #region EISS models
    public class SismaMqRequestModel
    {
        public int CatalogNo { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
    #endregion

    [XmlRoot("sisma")]
    public class SismaModel
    {
        [Required]
        [XmlElement("context")]
        public SismaContextModel Context { get; set; }

        [Required]
        [XmlArray("codes")]
        [XmlArrayItem("code")]
        public SismaCodeModel[] Codes { get; set; }
    }

    public class SismaContextModel
    {
        [XmlElement("fromFTP")]
        public bool FromFTP { get; set; }

        [Required]
        [XmlElement("integrationType")]
        public string IntegrationType { get; set; }

        [Required]
        [XmlElement("reportType")]
        public string ReportType { get; set; }

        [Required]
        [XmlElement("periodNumber")]
        public int PeriodNumber { get; set; }

        [Required]
        [XmlElement("periodYear")]
        public int PeriodYear { get; set; }

        [Required]
        [XmlElement("methodName")]
        public string MethodName { get; set; }
    }

    public class SismaCodeModel
    {
        [Required]
        [XmlAttribute("ibdCode")]
        public string IbdCode { get; set; }

        [XmlAttribute("count")]
        public int Count { get; set; }

        [XmlAttribute("amount")]
        public decimal Amount { get; set; }

        [XmlArray("details")]
        [XmlArrayItem("detail")]
        public SismaCodeDetailModel[] Details { get; set; }
    }

    public class SismaCodeDetailModel
    {
        [XmlAttribute("entityCode")]
        public string EntityCode { get; set; }

        [XmlAttribute("subjectCode")]
        public string SubjectCode { get; set; }

        [XmlAttribute("subjectName")]
        public string SubjectName { get; set; }

        [XmlAttribute("count")]
        public int Count { get; set; }

        [XmlAttribute("amount")]
        public decimal Amount { get; set; }
    }

    public class SismaResponseModel
    {
        [Required]
        [XmlElement("resultCode")]
        public string ResultCode { get; set; }

        [Required]
        [XmlElement("message")]
        public string Message { get; set; }
    }

    public class SismaConstants
    {
        public class Methods
        {
            public const string Add = "add";
            public const string Edit = "edit";
            public const string Delete = "delete";
        }
        public class ErrorCodes
        {
            public const string OK = "ok";
            public const string InvalidCode = "1";
            public const string InvalidValue = "2";
            public const string InvalidReport = "3";
            public const string GeneralException = "99";
        }
    }
}
