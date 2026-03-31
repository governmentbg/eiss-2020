// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Election
{
    public class ElectionLogVM
    {
        public DateTime DateWrt { get; set; }
        public string UserName { get; set; }
        public string Operation { get; set; }
        public string Description { get; set; }
        public string AfectedElectionPerson { get; set; }
    }
}
