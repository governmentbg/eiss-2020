using IOWebApplication.Infrastructure.Attributes;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("epep_user")]
    public class EpepUser : IUserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("document_id")]
        public long? DocumentId { get; set; }

        [Display(Name ="Вид потребител")]
        [Column("epep_user_type_id")]
        [IORequired]
        public int EpepUserTypeId { get; set; }

        [Column("full_name")]
        [Display(Name = "Име")]
        [IORequired]
        public string FullName { get; set; }

        [Column("address")]
        [Display(Name = "Адрес")]
        public string Address { get; set; }

        [Column("email")]
        [Display(Name = "Електронна поща")]
        [Required(ErrorMessage ="Въведете {0}.")]
        public string Email { get; set; }

        [Column("uic")]
        [Display(Name = "ЕГН")]
        [IORequired]
        public string Uic { get; set; }

        [Column("birth_date")]
        [Display(Name = "Дата на раждане")]
        [IORequired]
        public DateTime? BirthDate { get; set; }

        [Column("lawyer_number")]
        [Display(Name = "Адвокат No")]
        [IORequired]
        public string LawyerNumber { get; set; }

        [Column("lawyer_lawunit_id")]
        [Display(Name = "Адвокат")]
        public int? LawyerLawUnitId { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("epep_id")]
        public Guid? EpepId { get; set; }

        [Column("document_decision_id")]
        public long? DocumentDecisionId { get; set; }

        

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

        [Column("user_id")]
        public string UserId { get; set; }
        [Column("date_wrt")]
        public DateTime DateWrt { get; set; }

        [Column("date_transfered_dw")]
        public DateTime? DateTransferedDW { get; set; }

        [ForeignKey(nameof(LawyerLawUnitId))]
        public virtual LawUnit LawyerLawUnit { get; set; }


        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey(nameof(EpepUserTypeId))]
        public virtual EpepUserType EpepUserType { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }
    }
}
