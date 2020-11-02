// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Constants
{
    class StatisticsConstants
    {
        public class ReportTypes
        {
            //Несвършени
            public const int Unfinished = 1;

            //Постъпили за периода
            public const int Incoming = 2;

            //свършени по същество
            public const int FinishedNoStop = 3;

            //свършени прекратени
            public const int FinishedStop = 4;

            //свършени в 3 месечен срок
            public const int Finished3months = 5;
        }
    }
}
