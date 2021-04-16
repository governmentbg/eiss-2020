using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMigrationVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int InitialCaseId { get; set; }
        public string CaseRegNumber { get; set; }
        public string CaseSessionAct { get; set; }
        public DateTime CaseRegDate { get; set; }
        public string CaseCourtName { get; set; }
        public int MigrationDirection { get; set; }
        public int MigrationTypeId { get; set; }
        public string MigrationTypeName { get; set; }
        public string Description { get; set; }
        public string SentFromName { get; set; }
        public string SentToName { get; set; }
        public int? SendToCortId { get; set; }
        public bool CanEdit { get; set; }
        public bool CanAccept { get; set; }
        public DateTime DateWrt { get; set; }
        public string OutDocumentLabel { get; set; }
        public DateTime? OutDocumentDate { get; set; }
        public bool? IsReturned { get; set; }
        public string CaseStateName { get; set; }
    }
}
