using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Страни в Обжалвания към съдебен акт
    /// </summary>
    [Table("case_session_act_complain_person")]
    public class CaseSessionActComplainPerson : UserDateWRT
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_session_act_complain_id")]
        public int CaseSessionActComplainId { get; set; }

        /// <summary>
        /// Страна по делото - жалейчик
        /// </summary>
        [Column("complain_case_person_id")]
        [Display(Name = "Жалбоподател")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int CasePersonId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionActComplainId))]
        public virtual CaseSessionActComplain CaseSessionActComplain { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }
    }
}
