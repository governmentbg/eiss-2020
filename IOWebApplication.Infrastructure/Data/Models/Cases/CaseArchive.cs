using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Архивиране на дела
    /// </summary>
    [Table("case_archive")]
    public class CaseArchive : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_act_id")]
        [Display(Name = "Акт")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int CaseSessionActId { get; set; }

        [Column("archive_index_id")]
        [Display(Name = "Номенклатурен индекс")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int? ArchiveIndexId { get; set; }

        [Column("reg_number")]
        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Column("reg_date")]
        [Display(Name = "Дата")]
        public DateTime RegDate { get; set; }

        [Column("description")]
        [Display(Name = "Данни за запазени документи")]
        public string Description { get; set; }

        [Column("book_number")]
        [Display(Name = "Номер на Том")]
        public int? BookNumber { get; set; }

        [Column("book_year")]
        [Display(Name = "Година на Том")]
        public int? BookYear { get; set; }

        [Column("storage_years")]
        [Display(Name = "Срок на съхранение години")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма или равна на 0")]
        public int StorageYears { get; set; }

        [Column("description_info")]
        [Display(Name = "Допълнителна информация")]
        public string DescriptionInfo { get; set; }

        [Column("is_old_number")]
        [Display(Name = "Стар номер")]
        public bool? IsOldNumber { get; set; }

        [Column("archive_link")]
        [Display(Name = "Архивна връзка")]
        public string ArchiveLink { get; set; }

        [Column("description_info_destroy")]
        [Display(Name = "Допълнителна информация за унищожаването")]
        public string DescriptionInfoDestroy { get; set; }

        [Column("date_destroy")]
        [Display(Name = "Дата на унищожаване")]
        public DateTime? DateDestroy { get; set; }

        [Column("act_destroy_label")]
        [Display(Name = "Протокол за унищожаване")]
        public string ActDestroyLabel { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(ArchiveIndexId))]
        public virtual CourtArchiveIndex CourtArchiveIndex { get; set; }
    }
}
