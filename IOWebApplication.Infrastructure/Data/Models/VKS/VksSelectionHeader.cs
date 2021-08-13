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
  [Table("vks_selection_header")]
  public class VksSelectionHeader : UserDateWRT
  {
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("kolegia_id")]
    public int KolegiaId { get; set; }

    [Column("selection_year")]
    public int SelectionYear { get; set; }

    /// <summary>
    /// Полугодие: 1,2
    /// </summary>
    [Column("period_no")]
    public int PeriodNo { get; set; }
    [Column("months")]
    public string Months { get; set; }
    [Column("vks_selection_state_id")]
    public int VksSelectionStateId { get; set; }

    [Column("state_date")]
    public DateTime StateDate { get; set; }

    [ForeignKey(nameof(KolegiaId))]
    public virtual CourtDepartment Kolegia { get; set; }

    [ForeignKey(nameof(VksSelectionStateId))]
    public virtual VksSelectionState VksSelectionState{ get; set; }

    public virtual ICollection<VksSelection> Selections { get; set; }
    public VksSelectionHeader()
    {
      Selections = new HashSet<VksSelection>();

    }

  }
}
