// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Заместване на съдии
    /// </summary>
    [Table("common_court_lawunit_substitution")]
    public class CourtLawUnitSubstitution : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("lawunit_id")]
        [Display(Name = "Съдия")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int LawUnitId { get; set; }

        [Column("substitute_lawunit_id")]
        [Display(Name = "Заместник")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int SubstituteLawUnitId { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime DateTo { get; set; }

        [Column("description")]
        [Display(Name = "Основание")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Description { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(SubstituteLawUnitId))]
        public virtual LawUnit SubstituteLawUnit { get; set; }
    }
}
