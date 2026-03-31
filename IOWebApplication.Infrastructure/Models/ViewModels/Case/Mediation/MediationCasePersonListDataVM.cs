// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{

    /// <summary>
    /// Модел за извличане на данни за страни по делото за срещата
    /// </summary>
    public class MediationCasePersonListDataVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на лицето в делото
        /// </summary>
        public int CasePersonId { get; set; }

        /// <summary>
        /// Име на лицето
        /// </summary>
        public string CasePersonFullName { get; set; }

        /// <summary>
        /// Вид лице
        /// </summary>
        public string CasePersonRoleName { get; set; }

        /// <summary>
        /// Връзки лица
        /// </summary>
        public string CasePersonLinkForPerson { get; set; }

        /// <summary>
        /// От
        /// </summary>
        public DateTime CasePersonDateFrom { get; set; }

        /// <summary>
        /// До
        /// </summary>
        public DateTime? CasePersonDateTo { get; set; }

        /// <summary>
        /// Статус на лицето след срещата - присъства и т.н.
        /// </summary>
        public string MediationPersonSessionStateLabel { get; set; }
    }
}
