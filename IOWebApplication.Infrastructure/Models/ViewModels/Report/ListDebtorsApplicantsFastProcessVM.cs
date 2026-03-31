// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка списък на длъжници/заявители
    /// </summary>
    public class ListDebtorsApplicantsFastProcessVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Флаг дали е от един и същ съд с на потребителя
        /// </summary>
        public bool IsLinkCase { get; set; }

        /// <summary>
        /// Име на лице (имена на лице с качество Длъжник/заявител в делото)
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Номер на дело (номер на образуваното дело)
        /// </summary>
        public string CaseRegNum { get; set; }

        /// <summary>
        /// Вх. номер (входящ номер от единия регистър на заповедни производства)
        /// </summary>
        public string DocumentRegNum { get; set; }

        /// <summary>
        /// Идентификатор на документ (идентификатор от единия регистър на заповедни производства)
        /// </summary>
        public long? AssignmentDocumentId { get; set; }

        /// <summary>
        /// Съд (съд, в който е образувано делото)
        /// </summary>
        public string CaseCourtLabel { get; set; }

        /// <summary>
        /// Вх. номер от ЕПЕП
        /// </summary>
        public string DocumentRegNumEpep { get; set; }

        /// <summary>
        /// Статус на дело (статус на делото към момента на изготвяне на справката)
        /// </summary>
        public string CaseStateName { get; set; }
    }

    /// <summary>
    /// Модел за филтър за справка списък на длъжници/заявители
    /// </summary>
    public class ListDebtorsApplicantsFilterFastProcessVM
    {
        /// <summary>
        /// От дата на регистрация (дело)
        /// </summary>
        [Display(Name = "От дата на регистрация (дело)")]
        public DateTime? CaseRegDateFrom { get; set; }

        /// <summary>
        /// До дата на регистрация (дело)
        /// </summary>
        [Display(Name = "До дата на регистрация (дело)")]
        public DateTime? CaseRegDateTo { get; set; }

        /// <summary>
        /// Длъжници/Заявители – въвеждане ръчно имена на лице
        /// </summary>
        [Display(Name = "Длъжници/Заявители")]
        public string NamePerson { get; set; }

        /// <summary>
        /// Идентификатор на лице - въвеждане ръчно на идентификатора на лице 
        /// </summary>
        [Display(Name = "Идентификатор")]
        public string IdentifikatorPerson { get; set; }
    }
}
