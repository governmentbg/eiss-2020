using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Data.DW.Models
{
  [Table("dw_document_link")]
  public class DWDocumentLink : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public long Id { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }

  

    /// <summary>
    /// Посока на движение на документ: Входящи, Изходящи, вътрешен
    /// </summary>
    [Column("document_direction_id")]
    public int? DocumentDirectionId { get; set; }

    [Column("document_direction_name")]
    public string DocumentDirectionName { get; set; }

    /// <summary>
    /// Предходен документ ID
    /// </summary>
    [Column("prev_document_id")]
    public long? PrevDocumentId { get; set; }

    /// <summary>
    /// Предходен документ номер
    /// </summary>
    [Column("prev_document_number")]
    public string PrevDocumentNumber { get; set; }

    /// <summary>
    /// Предходен документ дата
    /// </summary>
    [Column("prev_document_date")]
    public DateTime? PrevDocumentDate { get; set; }

    [Column("description")]
    public string Description { get; set; }


  }
}
