// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppTSActualDataPersonVM : PersonNamesBase
    {
        public string Sid { get; set; }
        public string SexCode { get; set; }
        public string Sex { get; set; }

        public IList<EisppTSActualDataPersonCrimeVM> PersonCrimes { get; set; }
        public IList<Address> Addresses { get; set; }

        public EisppTSActualDataPersonVM()
        {
            PersonCrimes = new List<EisppTSActualDataPersonCrimeVM>();
            Addresses = new List<Address>();
        }       
    }
}
