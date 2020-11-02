// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Constants
{
    public static class MoneyConstants
    {
        public class ObligationStatus
        {
            /// <summary>
            /// Платени
            /// </summary>
            public const int StatusPaid = 1;
            public const string StatusPaidStr = "Платени";

            /// <summary>
            /// С остатък
            /// </summary>
            public const int StatusNotEnd = 2;
            public const string StatusNotEndStr = "С остатък";
        }

        public class PosPaymentResultStatus
        {
            public const string StatusOk = "ok";
        }

        public class ObligationComingType
        {
            public const int ComeCaseLawUnit = 1;
            public const int ComeCasePerson = 2;
            public const int ComeDocumentPerson = 3;
        }
    }
}
