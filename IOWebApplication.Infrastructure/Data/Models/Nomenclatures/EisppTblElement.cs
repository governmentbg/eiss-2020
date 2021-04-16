using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// ЕИСПП - елементи от номенклатури
    /// </summary>
    [Table("nom_eispp_tbl_element")]
    public class EisppTblElement
    {
        [Column("eispp_tbl_code")]
        public string EisppTblCode { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("label")]
        public string Label { get; set; }

        [Column("system_name")]
        public string SystemName { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("standart_no")]
        public int? StandartNo { get; set; }

        [Column("date_start")]
        public DateTime DateStart { get; set; }

        [Column("date_end")]
        public DateTime? DateEnd { get; set; }
        
        [Column("date_wrt")]
        public DateTime? DateWrt { get; set; }

        [ForeignKey(nameof(EisppTblCode))]
        public virtual EisppTbl EisppTbl { get; set; }
    }
}
