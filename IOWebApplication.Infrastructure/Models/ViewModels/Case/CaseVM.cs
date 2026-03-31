using System;
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

        [Display(Name = "Номер на дело")]
        public string RegNumberText { get; set; }

        [Display(Name = "Дата на образуване")]
        public DateTime RegDate { get; set; }

        [Display(Name = "Местоположение на дело")]
        public string LastMovment { get; set; }

        [Display(Name = "Последно движение")]
        public string LastMigration { get; set; }

        [Display(Name = "Архивен номер на дело")]
        public string ArchRegNumber { get; set; }

        public bool HasArchive { get; set; }

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


        [Display(Name = "Основни страни")]
        public string CasePersonMain { get; set; }

        public bool IsSecret { get; set; }
        public bool IsRestriction { get; set; }
        public bool IsUnderAge { get; set; }
        public bool IsDeceased { get; set; }
        public bool IsSpecial { get; set; }

        /// <summary>
        /// Флаг дали има подписан изпълнителен лист
        /// </summary>
        public bool IsExistsSignedExecList { get; set; }

        /// <summary>
        /// Делото на първа инстанция е електронно бързо производство
        /// </summary>
        public bool IsFirsInstantsFastProcessCase { get; set; }

        /// <summary>
        /// Флаг оказващ делото дали е бързо производство
        /// </summary>
        public bool IsFastProcess { get; set; }

        /// <summary>
        /// Стринг с номера на дела дали съществува дело с тези хора за бързо производство
        /// </summary>
        public string IsExsistCaseWithSamePeople { get; set; }

        /// <summary>
        /// Документ регистратура централно управление
        /// </summary>
        public long? AssignmentDocumentId { get; set; }

        /// <summary>
        /// Id на електронен документ от ЕПЕП
        /// </summary>
        public long? ElectronicDocumentId { get; set; }

        /// <summary>
        /// Има плащане по иницииращ документ
        /// </summary>
        public bool ExistsPayToAssignmentDocument { get; set; }

        /// <summary>
        /// Флаг, дали подлежи на медиация делото
        /// </summary>
        public bool IsMediation { get; set; }

        /// <summary>
        /// Флаг, дали има обвързано дело за бързо производство
        /// </summary>
        public bool IsCaseCode_0602_0604 { get; set; }

        /// <summary>
        /// Флаг показващ дали са заредени данни за сходни дела за бързо производство
        /// </summary>
        public bool IsReadSimilarCases { get; set; }

        public string DebugInfo { get; set; }
    }
}
