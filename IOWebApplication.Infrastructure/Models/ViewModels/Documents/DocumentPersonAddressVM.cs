// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class DocumentPersonAddressVM
    {
        public long Id { get; set; }
        public int PersonIndex { get; set; }
        public int Index { get; set; }
        public bool Collapsed { get; set; }

        public Address Address { get; set; }

        public DocumentPersonAddressVM()
        {
            Address = new Address();
            Collapsed = true;
        }

        public string GetPrefix
        {
            get
            {
                return string.Format("{0}[{1}].{2}", nameof(DocumentVM.DocumentPersons), PersonIndex, nameof(DocumentPersonVM.Addresses));
            }
        }
        public string GetPath
        {
            get
            {
                return string.Format("{0}[{1}]", this.GetPrefix, Index);
            }
        }
    }
}
