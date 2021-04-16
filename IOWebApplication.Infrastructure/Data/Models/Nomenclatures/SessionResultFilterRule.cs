using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Резултати по заседание филтрирани по SessionTypeGroupId, CaseGroupId и CaseInstanceId
    /// </summary>
    [Table("nom_session_result_filter_rule")]
    public class SessionResultFilterRule
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("session_result_id")]
        public int SessionResultId { get; set; }

        [Column("session_type_group_id")]
        public int? SessionTypeGroupId { get; set; }

        [Column("case_group_id")]
        public int? CaseGroupId { get; set; }

        [Column("court_type_id")]
        public int? CourtTypeId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [ForeignKey(nameof(SessionResultId))]
        public virtual SessionResult SessionResult { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }
    }
}
