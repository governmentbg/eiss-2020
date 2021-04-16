using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Хора в съда, участващи в комисията
    /// </summary>
    [Table("common_court_archive_committee_lawunit")]
    public class CourtArchiveCommitteeLawUnit
    {
        [Column("court_archive_committee_id")]
        public int CourtArchiveCommitteeId { get; set; }

        [Column("law_unit_id")]
        public int LawUnitId { get; set; }

        [Column("date_from")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtArchiveCommitteeId))]
        public virtual CourtArchiveCommittee CourtArchiveCommittee { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }
    }
}
