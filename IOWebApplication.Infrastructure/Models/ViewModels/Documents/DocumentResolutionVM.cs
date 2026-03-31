using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentResolutionVM
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentNumber { get; set; }
        public int ResolutionTypeId { get; set; }
        public string ResolutionTypeName { get; set; }
        public string RegNumber { get; set; }
        public DateTime? RegDate { get; set; }
        public DateTime? DeclaredDate { get; set; }
        public int JudgeCount { get; set; }
        public string JudgeName { get; set; }
        public int JudgeDecisionLawunitId { get; set; }
        public string JudgeUserId { get; set; }
        public string JudgeName2 { get; set; }

        public int? JudgeDecisionLawunit2Id { get; set; }
        public string JudgeUser2Id { get; set; }
        public string JudgePosition { get; set; }
        public string JudgePosition2 { get; set; }
        public string StateName { get; set; }
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public string CourtCity { get; set; }
        public string Content { get; set; }

        public string GetFileTitle
        {
            get
            {
                if (RegDate != null)
                {
                    return $"{ResolutionTypeName} {RegNumber}/{RegDate:dd.MM.yyyy}";
                }
                else
                {
                    return $"{ResolutionTypeName}";
                }
            }
        }
    }
}
