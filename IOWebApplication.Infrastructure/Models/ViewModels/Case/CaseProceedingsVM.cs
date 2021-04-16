using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseProceedingsVM
    {
        public int Id { get; set; }

        // Номер Дело
        public string RegNumber { get; set; }

        // Дата дело
        public DateTime RegDate { get; set; }

        // Основен вид дело
        public string CaseGroupLabel { get; set; }

        // Точен вид дело
        public string CaseTypeLabel { get; set; }

        // Шифър
        public string CaseCodeLabel { get; set; }

        // Статус
        public string CaseStateLabel { get; set; }

        // Основание за образуване
        public string CaseReasonLabel { get; set; }

        // Основание
        public string CaseStateDescription { get; set; }

        // Съдия докладчик
        public string JudgeRapporteur { get; set; }

        // Архивиране
        public string ArchRegNumber { get; set; }

        // Последно движение
        public string LastMigration { get; set; }

        // Дата на архивиране
        public DateTime? ArchRegDate { get; set; }

        // Иницииращ документ
        public string DocumentLabel { get; set; }

        // Дата на влизане в законна сила
        public DateTime? CaseInforcedDate { get; set; }

        // Индикатори
        public virtual ICollection<CaseClassification> CaseClassifications { get; set; }

        // Страни
        public virtual ICollection<CasePersonListVM> CasePersons { get; set; }

        // Състав
        public virtual ICollection<CaseLawUnitVM> CaseLawUnits { get; set; }

        // Заседания, актове и документи
        public virtual ICollection<CaseProceedingsObjectsVM> CaseProceedingsObjects { get; set; }

        // Свързани дела
        public virtual ICollection<CaseMigrationVM> CaseMigrations { get; set; }

        // Свързани дела други системи
        public virtual ICollection<DocumentCaseInfo> DocumentCaseInfos { get; set; }

        // Свързани дела на външни институции
        public virtual ICollection<DocumentInstitutionCaseInfo> DocumentInstitutionCaseInfos { get; set; }
    }
}
