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
  [Table("dw_error_log")]
  public class DWErrorLog
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("court_id")]
    public int CourtId { get; set; }

    [Column("court_name")]
    public string CourtName { get; set; }

    [Column("table_name")]
    public string TableName { get; set; }


    [Column("table_id")]
    public long TableId { get; set; }

    [Column("error_message")]
    public string ErrorMessage { get; set; }


    [Column("error_date")]
    public DateTime ErrorDate { get; set; }



  }
}
