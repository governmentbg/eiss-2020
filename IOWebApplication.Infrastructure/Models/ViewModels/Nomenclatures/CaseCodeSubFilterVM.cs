// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    /// <summary>
    /// Филтър на подкод на шифър в дело
    /// </summary>
    public class CaseCodeSubFilterVM
    {
        /// <summary>
        /// Идентификатор на шифър
        /// </summary>
        [Display(Name = "Основен шифър")]
        public int? CaseCodeId { get; set; }
    }
}
