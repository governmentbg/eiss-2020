using IOWebApplication.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Справка Постъпили дела за период – първоинстанционни дела
    /// </summary>
    public class CaseFirstInstanceListReportVM
    {
        public int CaseId { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeName { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Дата на образуване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CaseRegDate { get; set; }

        [Display(Name = "Съдия докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Предмет и шифър")]
        public string CaseCodeName { get; set; }

        [Display(Name = "Източник на постъпване")]
        public string CaseCreateFromName
        {
            get
            {
                if (LifeCycleCount > 1)
                    return "Продължено под същия номер";
                else if (migration == null)
                    return "Новообразувано";
                else if (migration.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.AcceptJurisdiction)
                    return "Получено по подсъдност";
                else if (migration.ReturnCaseId > 0 && migration.ReturnCaseId != this.CaseId)
                    return "Върнато за ново разглеждане";

                return "Новообразувано";
            }
        }

        public CaseMigrationDataReportVM migration { get; set; }

        public int LifeCycleCount { get; set; }
    }

    /// <summary>
    /// Филтър за Справка Постъпили дела за период – първоинстанционни дела
    /// </summary>
    public class CaseFirstInstanceListFilterReportVM
    {
        [Display(Name = "От дата на образуване")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата на образуване")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Шифър")]
        public int CaseCodeId { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Източник на постъпване")]
        public int CaseCreateFromId { get; set; }
    }
}
