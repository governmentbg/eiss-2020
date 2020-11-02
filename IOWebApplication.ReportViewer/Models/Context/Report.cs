using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.ReportViewer.Data.Context
{
    /// <summary>
    /// Справки
    /// </summary>
    [Table("common_report", Schema = "public")]
    public class Report
    {
        [Key]
        [Column("id")]

        public int Id { get; set; }

        [Column("court_type_id")]
        [Display(Name = "Тип")]
        public int CourtTypeId { get; set; }

        [Column("order_number")]
        [Display(Name = "Номер по ред")]
        public int OrderNumber { get; set; }

        [Column("report_name")]
        [Display(Name = "Справка")]
        public string ReportName { get; set; }
        [Column("report_path")]
        [Display(Name = "Адрес")]
        public string ReportPath { get; set; }
    }
}
