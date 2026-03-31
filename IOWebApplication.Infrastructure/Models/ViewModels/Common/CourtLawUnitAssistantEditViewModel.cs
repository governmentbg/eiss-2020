// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Модел за редакция на асистент/помощник/секретар
    /// </summary>
    public class CourtLawUnitAssistantEditViewModel
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на лице към съдилище
        /// </summary>
        public int CourtLawUnitId { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int CourtId { get; set; }

        /// <summary>
        /// Идентификатор на асистент/помощник/секретар
        /// </summary>
        [Display(Name = "Съдебен служител")]
        public int LawUnitId { get; set; }

        /// <summary>
        /// Идентификатор на роля
        /// </summary>
        [Display(Name = "Роля")]
        public int JudgeRoleId { get; set; }
    }
}
