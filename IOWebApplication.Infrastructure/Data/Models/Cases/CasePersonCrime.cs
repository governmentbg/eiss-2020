﻿using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Престъпления към лица по НД по дело
    /// </summary>
    [Table("case_person_crimes")]
    public class CasePersonCrime : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Лице")]
        public int CasePersonId { get; set; }

        [Column("case_crime_id")]
        public int CaseCrimeId { get; set; }

        [Column("person_role_in_crime_id")]
        [Display(Name = "Роля на лицето в престъплението")]
        public int PersonRoleInCrimeId { get; set; }

        [Column("recidive_type_id")]
        [Display(Name = "Рецидив")]
        public int RecidiveTypeId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(CaseCrimeId))]
        public virtual CaseCrime CaseCrime { get; set; }

        [ForeignKey(nameof(PersonRoleInCrimeId))]
        public virtual PersonRoleInCrime PersonRoleInCrime { get; set; }

        [ForeignKey(nameof(RecidiveTypeId))]
        public virtual RecidiveType RecidiveType { get; set; }

        public virtual ICollection<CasePersonCrimePunishment> CasePersonCrimePunishments { get; set; }
  
        //################################################################################
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
    }
}
