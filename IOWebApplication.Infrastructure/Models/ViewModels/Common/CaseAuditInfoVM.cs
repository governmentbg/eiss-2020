using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CaseAuditInfoVM
    {
        public int CourtId { get; set; }
        public int CaseId { get; set; }
        public int CaseCodeId { get; set; }
        public int CaseStateId { get; set; }
        public string Info { get; set; }
        public int[] JudgeLawUnits { get; set; }
        public int[] AllMigrationJudgeLawUnits { get; set; }
        public int[] OtherCourtsJudgeLawUnits { get; set; }
        public int[] OtherCourts { get; set; }
        public int[] ToCourts { get; set; }
        public int[] AllMigrationCourts { get; set; }
        public int[] InitMigrations { get; set; }
        public int LastInMigrationCaseId { get; set; }
        public bool IsRestrictedAccess { get; set; }
        public bool IsSpecialAccess { get; set; }
        public bool IsFastProcess { get; set; }
        public CaseAuditInfoVM()
        {
            JudgeLawUnits = new int[] { };
            AllMigrationJudgeLawUnits = new int[] { };
            OtherCourtsJudgeLawUnits = new int[] { };
            OtherCourts = new int[] { };
            ToCourts = new int[] { };
            AllMigrationCourts = new int[] { };
            InitMigrations = new int[] { };
            IsRestrictedAccess = false;
        }
    }
}
