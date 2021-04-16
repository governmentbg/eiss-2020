using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("epep_user_assignment")]
    public class EpepUserAssignment : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("epep_user_id")]
        public int EpepUserId { get; set; }

        [Display(Name = "Изберете дело")]
        [Column("case_id")]
        public int CaseId { get; set; }

        [Display(Name = "Страна по делото")]
        [Column("case_person_id")]
        public int CasePersonId { get; set; }

        [Column("date_from")]
        public DateTime DateFrom { get; set; }

        [Column("epep_id")]
        public Guid? EpepId { get; set; }

        //===============================================================

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

       

        [ForeignKey(nameof(EpepUserId))]
        public virtual EpepUser EpepUser { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }
    }
}
