using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Филтриране на основание резултати от заседание - по основен вид дело
    /// </summary>
    [Table("nom_session_result_base_rule")]
    public class SessionResultBaseRule
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("session_result_base_id")]
        public int SessionResultBaseId { get; set; }

        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Начална дата")]
        [Column("date_start")]
        public DateTime? DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        [Column("date_end")]
        public DateTime? DateEnd { get; set; }

        [ForeignKey(nameof(SessionResultBaseId))]
        public virtual SessionResultBase SessionResultBase { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
