// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за страни по делото за срещата
    /// </summary>
    public class MediationCasePersonVM
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
        /// Идентификатор на среща
        /// </summary>
        public int MediationCaseSessionId { get; set; }

        /// <summary>
        /// Данни за лице
        /// </summary>
        [Display(Name = "Данни за лице")]
        public string CasePersonDataLabel { get; set; }

        /// <summary>
        /// Адрес на лице
        /// </summary>
        [Display(Name = "Адрес на лице")]
        public string CasePersonAddress { get; set; }

        /// <summary>
        /// Идентификатор на статус на лицето след срещата - присъства и т.н.
        /// </summary>
        [Display(Name = "Статус на лицето след срещата")]
        public int? MediationPersonSessionStateId { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [Display(Name = "Забележка")]
        public string Description { get; set; }
    }
}
