// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Election
{
    public class ElectionPersonVM
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Egn { get; set; }
        public string CourtName { get; set; }
        public string StateName { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateRemoved { get; set; }
        public DateTime? DeclarationDate { get; set; }

        public string DescriptionRemoved { get; set; }
    }
}
