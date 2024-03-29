﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.7.3081.0.
// 
//Регистър на уведомленията за трудовите договори и уведомления за промяна на работодател\Справка за актуално състояние на всички/действащите трудови договори
namespace IOWebApplication.Infrastructure.Models.Regix.GetEmploymentContracts
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts", IsNullable = false)]
    public enum ContractsFilterType
    {

        /// <remarks/>
        All,

        /// <remarks/>
        Active,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts", IsNullable = false)]
    public enum EContractReasonType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("01")]
        Item01,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("02")]
        Item02,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("03")]
        Item03,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("04")]
        Item04,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("05")]
        Item05,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("06")]
        Item06,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("07")]
        Item07,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("08")]
        Item08,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("09")]
        Item09,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("10")]
        Item10,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        Item11,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        Item12,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        Item13,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        Item14,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        Item15,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        Item16,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts", IsNullable = false)]
    public enum EikTypeType
    {

        /// <remarks/>
        Bulstat,

        /// <remarks/>
        EGN,

        /// <remarks/>
        LNC,

        /// <remarks/>
        SystemNo,

        /// <remarks/>
        BulstatCL,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts/Request")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts/Request", IsNullable = false)]
    public partial class EmploymentContractsRequest
    {

        private IdentityTypeRequest identityField;

        private ContractsFilterType contractsFilterField;

        private bool contractsFilterFieldSpecified;

        /// <remarks/>
        public IdentityTypeRequest Identity
        {
            get
            {
                return this.identityField;
            }
            set
            {
                this.identityField = value;
            }
        }

        /// <remarks/>
        public ContractsFilterType ContractsFilter
        {
            get
            {
                return this.contractsFilterField;
            }
            set
            {
                this.contractsFilterField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContractsFilterSpecified
        {
            get
            {
                return this.contractsFilterFieldSpecified;
            }
            set
            {
                this.contractsFilterFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts/Request")]
    public partial class IdentityTypeRequest
    {

        private string idField;

        private EikTypeType tYPEField;

        /// <remarks/>
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public EikTypeType TYPE
        {
            get
            {
                return this.tYPEField;
            }
            set
            {
                this.tYPEField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts/Response")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts/Response", IsNullable = false)]
    public partial class EmploymentContractsResponse
    {

        private ResponseIdentityType identityField;

        private EContract[] eContractsField;

        private StatusType statusField;

        private ContractsFilterType contractsFilterField;

        private bool contractsFilterFieldSpecified;

        private System.DateTime reportDateField;

        private bool reportDateFieldSpecified;

        /// <remarks/>
        public ResponseIdentityType Identity
        {
            get
            {
                return this.identityField;
            }
            set
            {
                this.identityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts", IsNullable = false)]
        public EContract[] EContracts
        {
            get
            {
                return this.eContractsField;
            }
            set
            {
                this.eContractsField = value;
            }
        }

        /// <remarks/>
        public StatusType Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public ContractsFilterType ContractsFilter
        {
            get
            {
                return this.contractsFilterField;
            }
            set
            {
                this.contractsFilterField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContractsFilterSpecified
        {
            get
            {
                return this.contractsFilterFieldSpecified;
            }
            set
            {
                this.contractsFilterFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime ReportDate
        {
            get
            {
                return this.reportDateField;
            }
            set
            {
                this.reportDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ReportDateSpecified
        {
            get
            {
                return this.reportDateFieldSpecified;
            }
            set
            {
                this.reportDateFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts")]
    public partial class ResponseIdentityType
    {

        private string idField;

        private EikTypeType tYPEField;

        private bool tYPEFieldSpecified;

        /// <remarks/>
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public EikTypeType TYPE
        {
            get
            {
                return this.tYPEField;
            }
            set
            {
                this.tYPEField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TYPESpecified
        {
            get
            {
                return this.tYPEFieldSpecified;
            }
            set
            {
                this.tYPEFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts")]
    public partial class EContract
    {

        private string contractorBulstatField;

        private string contractorNameField;

        private string individualEIKField;

        private string individualNamesField;

        private System.DateTime startDateField;

        private bool startDateFieldSpecified;

        private System.DateTime lastAmendDateField;

        private bool lastAmendDateFieldSpecified;

        private System.DateTime endDateField;

        private bool endDateFieldSpecified;

        private EContractReasonType reasonField;

        private bool reasonFieldSpecified;

        private System.DateTime timeLimitField;

        private bool timeLimitFieldSpecified;

        private string ecoCodeField;

        private string professionCodeField;

        private decimal remunerationField;

        private bool remunerationFieldSpecified;

        private string professionNameField;

        private string eKATTECodeField;

        /// <remarks/>
        public string ContractorBulstat
        {
            get
            {
                return this.contractorBulstatField;
            }
            set
            {
                this.contractorBulstatField = value;
            }
        }

        /// <remarks/>
        public string ContractorName
        {
            get
            {
                return this.contractorNameField;
            }
            set
            {
                this.contractorNameField = value;
            }
        }

        /// <remarks/>
        public string IndividualEIK
        {
            get
            {
                return this.individualEIKField;
            }
            set
            {
                this.individualEIKField = value;
            }
        }

        /// <remarks/>
        public string IndividualNames
        {
            get
            {
                return this.individualNamesField;
            }
            set
            {
                this.individualNamesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime StartDate
        {
            get
            {
                return this.startDateField;
            }
            set
            {
                this.startDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartDateSpecified
        {
            get
            {
                return this.startDateFieldSpecified;
            }
            set
            {
                this.startDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime LastAmendDate
        {
            get
            {
                return this.lastAmendDateField;
            }
            set
            {
                this.lastAmendDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastAmendDateSpecified
        {
            get
            {
                return this.lastAmendDateFieldSpecified;
            }
            set
            {
                this.lastAmendDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime EndDate
        {
            get
            {
                return this.endDateField;
            }
            set
            {
                this.endDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EndDateSpecified
        {
            get
            {
                return this.endDateFieldSpecified;
            }
            set
            {
                this.endDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        public EContractReasonType Reason
        {
            get
            {
                return this.reasonField;
            }
            set
            {
                this.reasonField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ReasonSpecified
        {
            get
            {
                return this.reasonFieldSpecified;
            }
            set
            {
                this.reasonFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime TimeLimit
        {
            get
            {
                return this.timeLimitField;
            }
            set
            {
                this.timeLimitField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TimeLimitSpecified
        {
            get
            {
                return this.timeLimitFieldSpecified;
            }
            set
            {
                this.timeLimitFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string EcoCode
        {
            get
            {
                return this.ecoCodeField;
            }
            set
            {
                this.ecoCodeField = value;
            }
        }

        /// <remarks/>
        public string ProfessionCode
        {
            get
            {
                return this.professionCodeField;
            }
            set
            {
                this.professionCodeField = value;
            }
        }

        /// <remarks/>
        public decimal Remuneration
        {
            get
            {
                return this.remunerationField;
            }
            set
            {
                this.remunerationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RemunerationSpecified
        {
            get
            {
                return this.remunerationFieldSpecified;
            }
            set
            {
                this.remunerationFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string ProfessionName
        {
            get
            {
                return this.professionNameField;
            }
            set
            {
                this.professionNameField = value;
            }
        }

        /// <remarks/>
        public string EKATTECode
        {
            get
            {
                return this.eKATTECodeField;
            }
            set
            {
                this.eKATTECodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://egov.bg/RegiX/NRA/EmploymentContracts")]
    public partial class StatusType
    {

        private int codeField;

        private bool codeFieldSpecified;

        private string messageField;

        /// <remarks/>
        public int Code
        {
            get
            {
                return this.codeField;
            }
            set
            {
                this.codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CodeSpecified
        {
            get
            {
                return this.codeFieldSpecified;
            }
            set
            {
                this.codeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string Message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }
    }
}
