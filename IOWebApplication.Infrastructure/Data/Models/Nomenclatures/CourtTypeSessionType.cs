using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове заседания към вид съд/вид дело
    /// </summary>
    [Table("nom_court_type_session_type")]
    public class CourtTypeSessionType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [Column("session_type_id")]
        public int SessionTypeId { get; set; }

        [Display(Name = "Начална дата")]
        [Column("date_start")]
        public DateTime? DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        [Column("date_end")]
        public DateTime? DateEnd { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }

        [ForeignKey(nameof(SessionTypeId))]
        public virtual SessionType SessionType { get; set; }
    }
}
