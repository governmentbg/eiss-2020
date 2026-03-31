// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.Integrations.EpepRest;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{

    public class EpepExecListObligationM
    {
        public int ObligationId { get; set; }

        public EpepExecListSideVM Beneficiary { get; set; }

        public EpepExecListSideVM Debtor { get; set; }

        public int MoneyTypeId { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; }

        public string Description { get; set; }
    }

    public class EpepExecListSideVM
    {
        public int UicTypeId { get; set; }
        public string Uic { get; set; }
        public string FullName { get; set; }

        /// <summary>
        /// SourceTypeSelectVM:
        /// LawUnit
        /// DocumentPerson
        /// CaseLawUnit
        /// CasePerson
        /// </summary>
        public int? PersonSourceType { get; set; }

        public long? PersonSourceId { get; set; }
    }



}
