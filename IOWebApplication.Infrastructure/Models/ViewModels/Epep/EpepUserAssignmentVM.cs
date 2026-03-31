using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class EpepUserAssignmentVM
    {
        public int Id { get; set; }
        public int EpepUserId { get; set; }
        public bool CanChange { get; set; }
        public int CaseId { get; set; }
        public string CourtName { get; set; }
        public string CaseInfo { get; set; }
        public string CaseNumber { get; set; }
        public string SideFullName { get; set; }
        public string SideName { get; set; }
        public string SideInfo { get; set; }
        public string EpepUserFullName { get; set; }
        public string EpepUserInfo { get; set; }
        public bool IsLawyer { get; set; }
        public bool CanSummon { get; set; }
    }
}
