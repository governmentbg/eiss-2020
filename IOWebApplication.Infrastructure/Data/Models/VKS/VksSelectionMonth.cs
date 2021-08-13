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
  /// Месечно разпределение към Полугодишен избор на съдии за ВКС
  /// </summary>
  [Table("vks_selection_month")]
  public class VksSelectionMonth : UserDateWRT
  {
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("vks_selection_id")]
    public int VksSelectionId { get; set; }

    [Column("selection_month")]
    public int SelectionMonth { get; set; }

    /// <summary>
    /// Дата за заседаване: 1,2,3,4,5 или 6
    /// </summary>
    [Column("selection_day")]
    public int SelectionDay { get; set; }

    [Column("session_date")]
    public DateTime? SessionDate { get; set; }

    [Column("chairman_in")]
    public bool ChairmanIn { get; set; }

    //Id-та на участници, подредени във възходящ ред разделени със запетая
    //За бързо сравнение на състави в отделните месеци
    [Column("selection_hash")]
    public string SelectionHash { get; set; }
    public virtual ICollection<VksSelectionMonthLawunit> SelectionMonthLawunit { get; set; }

    [ForeignKey(nameof(VksSelectionId))]
    public virtual VksSelection VksSelection { get; set; }
    public VksSelectionMonth()
    {
      SelectionMonthLawunit = new HashSet<VksSelectionMonthLawunit>();
    
    }

  }
}
