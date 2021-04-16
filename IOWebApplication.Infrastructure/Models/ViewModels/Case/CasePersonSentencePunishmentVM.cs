using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonSentencePunishmentVM
    {
        public int Id { get; set; }
        public int CasePersonSentenceId { get; set; }
        public bool IsSummaryPunishment { get; set; }
        public string IsSummaryPunishmentText { get; set; }
        public string SentenceTypeLabel { get; set; }
        public decimal SentenseMoney { get; set; }
        public string SentenceText { get; set; }
        public bool IsMainPunishment { get; set; }
        public string IsMainPunishmentText { get; set; }
    }
}
