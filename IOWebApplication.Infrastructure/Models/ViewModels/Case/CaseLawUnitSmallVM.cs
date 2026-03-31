// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Данни за лице от дело
    /// </summary>
    public class CaseLawUnitSmallVM
    {
        /// <summary>
        /// Идентификатор на лицето от делото
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на лицето
        /// </summary>
        public string LawUnitUserId { get; set; }

        /// <summary>
        /// Идентификатор на съдебно ниво
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Име на съдебно ниво
        /// </summary>
        public string DepartmentLabel { get; set; }

        /// <summary>
        /// Идентификатор на лицето
        /// </summary>
        public int LawUnitId { get; set; }

        /// <summary>
        /// Име на лице
        /// </summary>
        public string LawUnitName { get; set; }

        /// <summary>
        /// Идентификатор на ролята на лицето в делото
        /// </summary>
        public int JudgeRoleId { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Флаг дали е съдия по заместване
        /// </summary>
        public bool? IsLawUnitSubstitution { get; set; }
    }
}
