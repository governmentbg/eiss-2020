﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class PersonDataVM
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public DateTime? BirthDay { get; set; }
    }
}
