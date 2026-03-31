// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за обединяване на дела бързо производство
    /// </summary>
    public class MergerCaseFastProcessVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Съд на делото
        /// </summary>
        public int CourtId { get; set; }

        /// <summary>
        /// Идентификатор на иницииращ документ
        /// </summary>
        public long DocumentId { get; set; }

        /// <summary>
        /// Информация за делото
        /// </summary>
        public string CaseLabel { get; set; }

        /// <summary>
        /// От съд
        /// </summary>
        [Display(Name = "Съд")]
        [IORequired]
        public int FromCourtId { get; set; }

        /// <summary>
        /// От дело
        /// </summary>
        [Display(Name = "Избор дело")]
        public int? FromCaseId { get; set; }

        /// <summary>
        /// Дали съществува в иницииращият документ връзка с друго дело
        /// </summary>
        public bool IsExistsDocumentCaseInfo {  get; set; }        
    }
}
