using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.VKS
{
    /// <summary>
    /// Полугодишен избор на съдии за ВКС
    /// </summary>
    [Table("vks_selection")]
    public class VksSelection : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_department_id")]
        public int CourtDepartmentId { get; set; }
        [Column("vks_selection_header_id")]
        public int VksSelectionHeaderId { get; set; }

        [ForeignKey(nameof(CourtDepartmentId))]
        public virtual CourtDepartment CourtDepartment { get; set; }

        [ForeignKey(nameof(VksSelectionHeaderId))]
        public virtual VksSelectionHeader VksSelectionHeader { get; set; }
        public virtual ICollection<VksSelectionMonth> Months { get; set; }
        public virtual ICollection<VksSelectionLawunit> SelectionLawunit { get; set; }
        public virtual ICollection<VksSelectionProtocol> Protocols { get; set; }
        public VksSelection()
        {
            Months = new HashSet<VksSelectionMonth>();
            SelectionLawunit = new HashSet<VksSelectionLawunit>();
            Protocols = new HashSet<VksSelectionProtocol>();
        }


    }
}
