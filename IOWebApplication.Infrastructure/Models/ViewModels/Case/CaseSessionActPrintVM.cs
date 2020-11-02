// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActPrintVM
    {
        public int Id { get; set; }
        public string ActTypeCode { get; set; }
        public string ActFormatType { get; set; }
        public string ActTypeName { get; set; }
        public string BlankActTypeName { get; set; }
        public string BlankHeaderText { get; set; }
        public string ActKindDescription { get; set; }
        public string ActKindBlankName { get; set; }
        public string ActRegNumber { get; set; }
        public string ActRegDate { get; set; }
        public DateTime? ActDeclaredDate { get; set; }
        public string ActRegYear { get; set; }
        public string BlankDecisionText { get; set; }
        public string CourtCity { get; set; }
        public string CourtName { get; set; }
        public string CourtLogo { get; set; }
        public string CourtParent { get; set; }

        public string RelatedActTypeName { get; set; }
        public string RelatedActNumber { get; set; }
        public string RelatedActDate { get; set; }
        public string RelatedActYear { get; set; }
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
        public DateTime SessionDate { get; set; }
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
        public List<string> ProsecutorList { get; set; }
        public bool ChairmanSignOnly { get; set; }

        public List<string> LeftSide { get; set; }
        public string LeftSideName { get; set; }
        public string LeftSideCurrentAddress { get; set; }
        public string LeftSideWorkAddress { get; set; }
        public List<string> RightSide { get; set; }
        public string RightSideName { get; set; }
        public string RightSideCurrentAddress { get; set; }
        public string RightSideWorkAddress { get; set; }

        public bool HeaderOnly { get; set; }
        public string MainBody { get; set; }
        public string Dispositiv { get; set; }
        public string ActTerm { get; set; }
        public string AnswerActRegNumber { get; set; }

        public CaseSessionActPrintVM()
        {
            HeaderOnly = false;
            ChairmanSignOnly = false;
            JudgeList = new List<LabelValueVM>();
            AllJudgeList = new List<LabelValueVM>();
            JuryList = new List<string>();
            //SecretaryList = new List<string>();
            ProsecutorList = new List<string>();
        }
    }
}
