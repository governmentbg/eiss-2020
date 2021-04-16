using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// ПОС устройства в съд
    /// </summary>
    [Table("common_court_pos_device")]
    public class CourtPosDevice : BaseCommonNomenclature
    {
        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("court_bank_account_id")]
        [Display(Name = "По сметка")]
        public int CourtBankAccountId { get; set; }

        [Column("tid")]
        [Display(Name = "Номер на ПОС устройство")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Tid { get; set; }

        [Column("bic")]
        [Display(Name = "Обслужваща банка BIC")]
        public string BIC { get; set; }

        [Column("bank_name")]
        [Display(Name = "Обслужваща банка")]
        public string BankName { get; set; }


        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CourtBankAccountId))]
        public virtual CourtBankAccount CourtBankAccount { get; set; }

    }
}
