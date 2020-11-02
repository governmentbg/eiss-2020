// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class Application
    {
        public string GUID { get; set; }
        public DateTime? DateApplicationReceived { get; set; }
        public int Number { get; set; }
        public int Year { get; set; }
        public int CourtNumber { get; set; }
        public string CourtName { get; set; }
        public Applicant[] Applicants { get; set; }
        public Debtor[] Debtors { get; set; }
        public Claimcircumstance[] ClaimCircumstances { get; set; }
        public Monetaryclaim[] MonetaryClaims { get; set; }
        public Bankaccount[] BankAccounts { get; set; }
        public AttachedDocument[] AttachedDocuments { get; set; }

        public Fee[] Fees { get; set; }
        public Signature Signature { get; set; }
    }
}