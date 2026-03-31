// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за редакция на пари на медиатори за проведена среща
    /// </summary>
    public class MediationObligationVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int CourtId { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Идентификатор на среща
        /// </summary>
        public int MediationCaseSessionId { get; set; }

        /// <summary>
        /// Сума
        /// </summary>
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }
    }
}
