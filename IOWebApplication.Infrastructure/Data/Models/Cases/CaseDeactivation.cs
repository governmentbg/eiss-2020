using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Регистър за анулиране на дела
    /// </summary>
    [Table("case_deactivation")]
    public class CaseDeactivation : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        [Display(Name ="Съдебно дело")]
        public int CaseId { get; set; }

        [Column("deactivate_user_id")]
        [Display(Name = "Извършил анулирането")]
        public string DeactivateUserId { get; set; }        

        [Column("description")]
        [Display(Name = "Основание")]
        public string Description { get; set; }

        [Column("declared_date")]
        [Display(Name = "Дата на постановяване")]
        public DateTime? DeclaredDate { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }       

        [ForeignKey(nameof(DeactivateUserId))]
        public virtual ApplicationUser DeactivateUser { get; set; }

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
