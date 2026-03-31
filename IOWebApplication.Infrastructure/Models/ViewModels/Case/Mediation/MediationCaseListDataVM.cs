// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за извличане на данни за дела за медиация
    /// </summary>
    public class MediationCaseListDataVM
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
        /// Име на съд
        /// </summary>
        public string CourtLabel { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string RegNumber { get; set; }

        /// <summary>
        /// Дата на дело
        /// </summary>
        public DateTime RegDate { get; set; }

        /// <summary>
        /// Точен вид дело - код
        /// </summary>
        public string CaseTypeCode { get; set; }

        /// <summary>
        /// Съдия-докладчик
        /// </summary>
        public string JudgeReport { get; set; }

        /// <summary>
        /// Отделение/Състав
        /// </summary>
        public string DepartmentOtdelenieText { get; set; }

        /// <summary>
        /// Шифър на делото
        /// </summary>
        public string CaseCodeLabel { get; set; }

        /// <summary>
        /// Подшифър на делото
        /// </summary>
        public string CaseCodeSubLabel { get; set; }

        /// <summary>
        /// Вид производство
        /// </summary>
        public string ProcessPriorityLabel { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string CaseStateLabel { get; set; }
    }
}
