using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Индекси по съдилища, за архивиране
    /// </summary>
    [Table("common_court_archive_index")]
    public class CourtArchiveIndex : BaseCommonNomenclature
    {
        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("court_archive_committee_id")]
        public int? CourtArchiveCommitteeId { get; set; }

        [Column("storage_years")]
        public int StorageYears { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CourtArchiveCommitteeId))]
        public virtual CourtArchiveCommittee CourtArchiveCommittee { get; set; }

        public virtual ICollection<CourtArchiveIndexCode> CourtArchiveIndexCodes { get; set; }

        public CourtArchiveIndex()
        {
            CourtArchiveIndexCodes = new HashSet<CourtArchiveIndexCode>();
        }
    }
}
