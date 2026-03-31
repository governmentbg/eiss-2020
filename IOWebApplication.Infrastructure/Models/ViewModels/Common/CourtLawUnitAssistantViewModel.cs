// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Модел за преглед на асистент/помощник/секретар
    /// </summary>
    public class CourtLawUnitAssistantViewModel
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Асистент/помощник/секретар
        /// </summary>
        public string LawUnitFullName { get; set; }

        /// <summary>
        /// Роля
        /// </summary>
        public string JudgeRoleLabel { get; set; }
    }

    public class CourtLawUnitAssistantFilterViewModel
    {
        /// <summary>
        /// Идентификатор на лице към съдилище
        /// </summary>
        public int CourtLawUnitId { get; set; }
    }
}
