using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using IOWebApplication.Infrastructure.Models.ViewModels.RegixReport;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за електронна папка
    /// </summary>
    public class CaseElectronicFolderVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int CourtId { get; set; }


        public bool IsOnlyFiles { get; set; }

        /// <summary>
        /// Име на съд
        /// </summary>
        public string CourtLabel { get; set; }

        /// <summary>
        /// Група на дело
        /// </summary>
        public string CaseGroupLabel { get; set; }

        /// <summary>
        /// Вид на дело
        /// </summary>
        public string CaseTypeLabel { get; set; }

        /// <summary>
        /// Шифър на дело
        /// </summary>
        public string CaseCodeLabel { get; set; }

        /// <summary>
        /// Статус на дело
        /// </summary>
        public string CaseStateLabel { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string RegNumber { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string RegNumberText { get; set; }

        /// <summary>
        /// Съдия
        /// </summary>
        public string JudgeRapporteur { get; set; }

        /// <summary>
        /// Идентификатор на иницииращ документ
        /// </summary>
        public long DocumentId { get; set; }

        /// <summary>
        /// Дата на иницииращ документ
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// Стринг за документ
        /// </summary>
        public string DocumentLabel { get; set; }

        /// <summary>
        /// Стринг за документ
        /// </summary>
        public string DocumentShortLabel { get; set; }

        /// <summary>
        /// Описание на документ
        /// </summary>
        public string DocumentDescription { get; set; }

        /// <summary>
        /// DocumentSecret
        /// </summary>
        public bool DocumentSecret { get; set; }

        /// <summary>
        /// DocumentRestriction
        /// </summary>
        public bool DocumentRestriction { get; set; }

        /// <summary>
        /// Идентификатор на иницииращ документ
        /// </summary>
        public long? AssignmentDocumentId { get; set; }

        /// <summary>
        /// Дата на иницииращ документ
        /// </summary>
        public DateTime? AssignmentDocumentDate { get; set; }

        /// <summary>
        /// Стринг за документ
        /// </summary>
        public string AssignmentDocumentLabel { get; set; }

        /// <summary>
        /// Стринг за документ
        /// </summary>
        public string AssignmentDocumentShortLabel { get; set; }

        /// <summary>
        /// Описание на документ
        /// </summary>
        public string AssignmentDocumentDescription { get; set; }

        /// <summary>
        /// Дата на дело
        /// </summary>
        public DateTime RegDate { get; set; }

        /// <summary>
        /// CaseReasonLabel
        /// </summary>
        public string CaseReasonLabel { get; set; }

        /// <summary>
        /// Описание на статус
        /// </summary>
        public string CaseStateDescription { get; set; }

        /// <summary>
        /// Архивен номер
        /// </summary>
        public string ArchRegNumber { get; set; }

        /// <summary>
        /// Дата на архива
        /// </summary>
        public DateTime? ArchRegDate { get; set; }

        /// <summary>
        /// Дата на влизане в сила
        /// </summary>
        public DateTime? CaseInforcedDate { get; set; }

        /// <summary>
        /// ЕИСПП номер
        /// </summary>
        public string EISSPNumber { get; set; }

        /// <summary>
        /// Списък със заседания
        /// </summary>
        public virtual ICollection<CaseSessionElectronicFolderVM> CaseSessions { get; set; }

        /// <summary>
        /// Протоколи от разпределение
        /// </summary>
        public virtual ICollection<CaseSelectionProtokolListVM> CaseSelectionProtokols { get; set; }

        /// <summary>
        /// Съпровождащи документи
        /// </summary>
        public virtual ICollection<DocumentInfoVM> CaseInDocuments { get; set; }

        /// <summary>
        /// Изходящи документи
        /// </summary>
        public virtual ICollection<DocumentInfoVM> CaseOutDocuments { get; set; }

        /// <summary>
        /// Лица по делото
        /// </summary>
        public virtual ICollection<CasePersonListVM> CasePersons { get; set; }

        /// <summary>
        /// Състав по делото
        /// </summary>
        public virtual ICollection<CaseLawUnitVM> CaseLawUnits { get; set; }

        /// <summary>
        /// Актове по дело
        /// </summary>
        public virtual ICollection<CaseSessionActVM> CaseSessionFinalActs { get; set; }

        /// <summary>
        /// Свързани дела
        /// </summary>
        public virtual ICollection<DocumentCaseInfo> DocumentCaseInfos { get; set; }

        /// <summary>
        /// Класификация на делото
        /// </summary>
        public virtual ICollection<CaseClassification> CaseClassifications { get; set; }

        /// <summary>
        /// Движения по делото / Свързани дела
        /// </summary>
        public virtual ICollection<CaseMigrationVM> CaseMigrations { get; set; }

        /// <summary>
        /// Свързани дела на външни институции
        /// </summary>
        public virtual ICollection<DocumentInstitutionCaseInfo> DocumentInstitutionCaseInfos { get; set; }

        /// <summary>
        /// Суми по дело
        /// </summary>
        public virtual ICollection<PaymentCaseVM> PaymentCases { get; set; }

        /// <summary>
        /// Изпълнителни листове
        /// </summary>
        public virtual ICollection<ExecListVM> ExecLists { get; set; }

        /// <summary>
        /// Резолюции
        /// </summary>
        public virtual ICollection<DocumentResolutionListVM> DocumentResolutions { get; set; }

        /// <summary>
        /// Справки външни системи
        /// </summary>
        public virtual ICollection<RegixListVM> RegixReports { get; set; }

        /// <summary>
        /// Разходен касов ордер
        /// </summary>
        public virtual ICollection<ExpenseOrderVM> ExpenseOrders { get; set; }

        /// <summary>
        /// Заявленията за достъп до дело в ЕПЕП
        /// </summary>
        public virtual ICollection<DocumentDecisionCaseListVM> DocumentDecisionCaseLists { get; set; }

        /// <summary>
        /// Други иницииращи документи
        /// </summary>
        public virtual ICollection<DocumentInfoVM> DocumentsOtherFromSameCourt { get; set; }

        /// <summary>
        /// Други иницииращи документи - Други съдилища
        /// </summary>
        public virtual ICollection<DocumentInfoVM> DocumentsOtherFromDifferentCourt { get; set; }

        /// <summary>
        /// Задачи
        /// </summary>
        public virtual ICollection<WorkTaskReportVM> WorkTaskReports { get; set; }

        /// <summary>
        /// Документи към дело
        /// </summary>
        public virtual ICollection<DocumentResolutionListVM> DocumentResolutionCase { get; set; }

        /// <summary>
        /// Срещи за медиация
        /// </summary>
        public virtual ICollection<MediationCaseSessionListDataVM> MediationCaseSessions { get; set; }

        /// <summary>
        /// Суми по документи
        /// </summary>
        public virtual ICollection<ObligationVM> MoneyDocument { get; set; }

        /// <summary>
        /// Свързани документи към иницииращ документ
        /// </summary>
        public virtual ICollection<DocumentLinkEFVM> DocumentLinks { get; set; }

        /// <summary>
        /// Доказателства към дело
        /// </summary>
        public virtual ICollection<CaseEvidenceEFVM> Evidences { get; set; }

        public bool IsSpecialAccess
        {
            get
            {
                return CaseClassifications.Any(c => c.ClassificationId == NomenclatureConstants.CaseClassifications.SpecialAccess);
            }
        }
    }
}
