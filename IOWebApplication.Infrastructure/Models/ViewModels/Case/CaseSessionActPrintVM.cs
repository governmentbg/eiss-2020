using System;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActPrintVM
    {
        public int Id { get; set; }
        public int ActTypeId { get; set; }
        public string ActTypeCode { get; set; }
        public string ActFormatType { get; set; }
        public string ActTypeName { get; set; }
        public string BlankActTypeName { get; set; }
        public string BlankHeaderText { get; set; }
        public string ActKindDescription { get; set; }
        public string ActKindBlankName { get; set; }
        public int ActDirection { get; set; }
        public string ActRegNumber { get; set; }
        public string ActRegDate { get; set; }
        public DateTime? ActDeclaredDate { get; set; }
        public string ActRegYear { get; set; }
        public string BlankDecisionText { get; set; }
        public int CourtId { get; set; }
        public string CourtCity { get; set; }
        public string CourtName { get; set; }
        public string CourtLogo { get; set; }
        public string CourtParent { get; set; }
        public bool IsMixedJuryVKS_VAS { get; set; }

        public int? RelatedActId { get; set; }
        public string RelatedActTypeName { get; set; }
        public string RelatedActNumber { get; set; }
        public string RelatedActDate { get; set; }
        public string RelatedActYear { get; set; }
        public string RelatedActText { get; set; }
        public string RelatedActDispositive { get; set; }

        /// <summary>
        /// Състав
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// отделение/колегия
        /// </summary>
        public string CompartmentType { get; set; }
        public string CompartmentName { get; set; }

        public string SessionTypeName { get; set; }
        public int SessionStateId { get; set; }

        public string SessionActLabel { get; set; }

        public int CaseSessionId { get; set; }
        public DateTime SessionDate { get; set; }
        public bool SessionIdOpen { get; set; }
        public int CaseId { get; set; }
        public string CaseTypeName { get; set; }
        public string CaseRegShortNumber { get; set; }
        public string CaseRegNumber { get; set; }
        public int CaseRegYear { get; set; }

        /// <summary>
        /// Председател на състава
        /// </summary>
        public string JudgeChairman { get; set; }
        public string JudgeReporter { get; set; }
        public List<LabelValueVM> JudgeList { get; set; }
        public List<LabelValueVM> AllJudgeList { get; set; }
        public List<string> JuryList { get; set; }
        public string SecretaryName { get; set; }
        public List<string> SecretaryList { get; set; }
        public List<string> ProsecutorList { get; set; }
        public bool ChairmanSignOnly { get; set; }

        public List<string> LeftSide { get; set; }
        public string LeftSideName { get; set; }
        public string[] LeftSideOnlyName { get; set; }
        public string LeftSidesWithAddress { get; set; }
        public string LeftSideCurrentAddress { get; set; }
        public string LeftSideWorkAddress { get; set; }
        public List<string> RightSide { get; set; }
        public string RightSideName { get; set; }
        public string RightSidesWithAddress { get; set; }
        public string RightSideCurrentAddress { get; set; }
        public string RightSideWorkAddress { get; set; }
        public string[] RightSidesOnlyName { get; set; }
        public string LeftSide_410_417 { get; set; }
        public string LeftSideWithOutRole_410_417 { get; set; }
        public string RightSide_410_417 { get; set; }
        public string LeftRightSide_410_417 { get; set; }
        public string LeftWithOutRoleRightSide_410_417 { get; set; }
        public string LeftRightSide_410_417_Expenses { get; set; }
        public int LeftSide_410_417_Count { get; set; }
        public int RightSide_410_417_Count { get; set; }

        public bool HeaderOnly { get; set; }
        public string MainBody { get; set; }
        public string Dispositiv { get; set; }
        public string Coordinations { get; set; }
        public string ActTerm { get; set; }
        public string AnswerActRegNumber { get; set; }

        /// <summary>
        ///Връща Съдия докладчик или ако няма първия съдия от заседанието
        /// </summary>
        public string SDorFirstJudge { get; set; }    
        
        public bool CaseByDocumentRequest { get; set; }

        /// <summary>
        /// Генериране на партида
        /// </summary>
        public bool GenerateExecProcess { get; set; }

        /// <summary>
        /// Номер на заповедта за изпълнение
        /// </summary>
        public string F_NUM_ACT_Z {  get; set; }

        /// <summary>
        /// Име на длъжник и дата на връчване на призовка
        /// </summary>
        public string F_DEBTOR_410_417_DELIVERY_DATA { get; set; }

        /// <summary>
        /// номер на съпровождащ документ от тип възражение
        /// </summary>
        public string F_AssignmentDocument_Num_V { get; set; }

        /// <summary>
        /// номер на съпровождащ документ от точен тип възражение 414 а
        /// </summary>
        public string F_AssignmentDocument_Num_V_414a { get; set; }

        public CaseSessionActPrintVM()
        {
            HeaderOnly = false;
            ChairmanSignOnly = false;
            JudgeList = new List<LabelValueVM>();
            AllJudgeList = new List<LabelValueVM>();
            JuryList = new List<string>();
            SecretaryList = new List<string>();
            ProsecutorList = new List<string>();
        }
    }
}
