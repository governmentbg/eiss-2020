// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за показване на пари на медиатори за проведена среща
    /// </summary>
    public class MediationObligationListDataVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mедиатор
        /// </summary>
        public string MediationMediatorFullName { get; set; }

        /// <summary>
        /// Сума
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// Номер на разходен касов ордер
        /// </summary>
        public string RegNumberExpenseOrder { get; set; }

        /// <summary>
        /// Идентификатор на разходен касов ордер
        /// </summary>
        public int ExpenseOrderId { get; set; }
    }
}
