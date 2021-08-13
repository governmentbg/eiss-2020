using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseVM
    {
        public int Id { get; set; }
        public int CourtId { get; set; }
        public string CourtLabel { get; set; }
        public int CaseGroupId { get; set; }
        
        [Display(Name = "Основен вид дело")]
        public string CaseGroupLabel { get; set; }
        public int? CaseTypeId { get; set; }
        
        [Display(Name = "Точен вид дело")]
        public string CaseTypeLabel { get; set; }
        public string CaseTypeCode { get; set; }
        public int CaseInstanceId { get; set; }
        public int? CaseCodeId { get; set; }

        [Display(Name = "Група по натовареност")]
        public string LoadGroupLinkLabel { get; set; }

        [Display(Name = "Шифри по точен вид дело")]
        public string CaseCodeLabel { get; set; }
        public int? ProcessPriorityId { get; set; }
        [Display(Name = "Вид производство")]
        public string ProcessPriorityLabel { get; set; }
        public int CaseStateId { get; set; }
        [Display(Name = "Статус")]
        public string CaseStateLabel { get; set; }
        public long DocumentId { get; set; }
        public string DocumentLabel { get; set; }
        
        [Display(Name = "Документ: ")]
        public string DocumentName { get; set; }

        public int DocumentTypeId { get; set; }

        public string DocumentTypeName { get; set; }

        [Display(Name = "ЕИСПП номер на НП")]
        public string EISSPNumber { get; set; }
        
        [Display(Name = "Кратък номер")]
        public string ShortNumber { get; set; }

        [Display(Name = "Кратък номер")]
        public int ShortNumberVal { get; set; }

        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }
        
        [Display(Name = "Дата на образуване")]
        public DateTime RegDate { get; set; }

        [Display(Name = "Местоположение на дело")]
        public string LastMovment { get; set; }

        [Display(Name = "Последно движение")]
        public string LastMigration { get; set; }

        [Display(Name = "Архивен номер на дело")]
        public string ArchRegNumber { get; set; }

        [Display(Name = "Архивна дата на образуване")]
        public DateTime? ArchRegDate { get; set; }

        [Display(Name = "Номенклатурен индекс")]
        public string ArchiveIndexLabel { get; set; }

        [Display(Name = "Архивна връзка")]
        public string ArchiveLink { get; set; }

        [Display(Name = "Срок на съхранение години")]
        public int? StorageYears { get; set; }

        [Display(Name = "Номер на Том")]
        public int? BookNumber { get; set; }

        [Display(Name = "Година на Том")]
        public int? BookYear { get; set; }

        [Display(Name = "Основание за образуване")]
        public string CaseReasonLabel { get; set; }

        [Display(Name = "Основание")]
        public string CaseStateDescription { get; set; }

        [Display(Name = "Влизане в законна сила")]
        public DateTime? CaseInforcedDate { get; set; }

        [Display(Name = "Съдия-докладчик")]
        public string JudgeReport { get; set; }

        [Display(Name = "Отделение/Състав")]
        public string DepartmentOtdelenieText { get; set; }
    }
}
