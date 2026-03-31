// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за среща към дело
    /// </summary>
    public class MediationCaseSessionVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Идентификатор на вид среща: Информационна среща; Процедура по медиация.
        /// </summary>
        [Display(Name = "Вид среща")]
        public int MediationTypeId { get; set; }

        /// <summary>
        /// Идентификатор на място на което се провежда
        /// </summary>
        [Display(Name = "Място на провеждане")]
        public int? MediationLocationId { get; set; }

        /// <summary>
        /// Допълнително пояснение за място на което се провежда
        /// </summary>
        [Display(Name = "Допълнително пояснение за място на което се провежда")]
        public string MediationLocationDescription { get; set; }

        /// <summary>
        /// От
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Статус на срещата
        /// </summary>
        [Display(Name = "Статус на срещата")]
        public int? MediationStateId { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [Display(Name = "Причина за непровеждане/забележка/основание")]
        public string Description { get; set; }

        /// <summary>
        /// Идентификатори на свързани дела
        /// </summary>
        [Display(Name = "Отбелязване на свързани дела към основно дело")]
        public string[] LinkCaseIds { get; set; }

        /// <summary>
        /// Стринг с идентификатори на свързани дела, разделени със запетая
        /// </summary>
        public string StringLinkCaseIds
        {
            get
            {
                if ((LinkCaseIds != null) && LinkCaseIds.Length > 0)
                    return string.Join(",", LinkCaseIds);
                else
                    return string.Empty;
            }
        }
    }
}
