// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
    }
}
