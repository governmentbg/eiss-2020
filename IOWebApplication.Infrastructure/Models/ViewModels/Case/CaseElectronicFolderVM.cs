using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using IOWebApplication.Infrastructure.Models.ViewModels.RegixReport;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseElectronicFolderVM
    {
        public int Id { get; set; }
        public bool IsOnlyFiles { get; set; }
        public string CourtLabel { get; set; }
        public string CaseGroupLabel { get; set; }
        public string CaseTypeLabel { get; set; }
        public string CaseCodeLabel { get; set; }
        public string CaseStateLabel { get; set; }
        public string RegNumber { get; set; }
        public string JudgeRapporteur { get; set; }
        public long DocumentId { get; set; }
        public string DocumentLabel { get; set; }
        public bool DocumentSecret { get; set; }
        public bool DocumentRestriction { get; set; }
        public DateTime RegDate { get; set; }
        public string CaseReasonLabel { get; set; }
        public string CaseStateDescription { get; set; }
        public string ArchRegNumber { get; set; }
        public DateTime? ArchRegDate { get; set; }
        public DateTime? CaseInforcedDate { get; set; }
        public virtual ICollection<CaseSessionElectronicFolderVM> CaseSessions { get; set; }
        public virtual ICollection<CaseSelectionProtokolListVM> CaseSelectionProtokols { get; set; }
        public virtual ICollection<DocumentInfoVM> CaseInDocuments { get; set; }
        public virtual ICollection<DocumentInfoVM> CaseOutDocuments { get; set; }
        public virtual ICollection<CasePersonListVM> CasePersons { get; set; }
        public virtual ICollection<CaseLawUnitVM> CaseLawUnits { get; set; }
        public virtual ICollection<CaseSessionActVM> CaseSessionFinalActs { get; set; }
        public virtual ICollection<DocumentCaseInfo> DocumentCaseInfos { get; set; }
        public virtual ICollection<CaseClassification> CaseClassifications { get; set; }
        public virtual ICollection<CaseMigrationVM> CaseMigrations { get; set; }
        public virtual ICollection<DocumentInstitutionCaseInfo> DocumentInstitutionCaseInfos { get; set; }
        public virtual ICollection<PaymentCaseVM> PaymentCases { get; set; }
        public virtual ICollection<ExecListVM> ExecLists { get; set; }
        public virtual ICollection<DocumentResolutionListVM> DocumentResolutions { get; set; }
        public virtual ICollection<RegixListVM> RegixReports { get; set; }
        public virtual ICollection<ExpenseOrderVM> ExpenseOrders { get; set; }
        public virtual ICollection<DocumentDecisionCaseListVM> DocumentDecisionCaseLists { get; set; }
    }
}
