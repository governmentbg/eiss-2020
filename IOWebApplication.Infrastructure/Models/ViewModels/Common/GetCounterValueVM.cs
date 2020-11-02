// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class GetCounterValueVM
    {
        [Column("value")]
        public int Value { get; set; }
        [Column("preffix")]
        public string Prefix { get; set; }
        [Column("suffix")]
        public string Suffix { get; set; }
        [Column("digit_count")]
        public int DigitCount { get; set; }

        public string GetStringValue()
        {
            return string.Format("{0}{1:D" + this.DigitCount.ToString() + "}{2}", this.Prefix, this.Value, this.Suffix);
        }
    }
}
