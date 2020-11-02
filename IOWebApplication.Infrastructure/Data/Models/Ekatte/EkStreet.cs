// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models
{
    [Table("ek_streets")]
    public class EkStreet
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("ekatte")]
        public string Ekatte { get; set; }

        [Column("name")]
        [Display(Name = "Име")]
        public string Name { get; set; }

        [Column("date_from")]
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// 1-Улица,булевард; 2-квартал,местност,район-други
        /// </summary>
        [Column("street_type")]
        [Display(Name = "Вид")]
        public int? StreetType { get; set; }
    }
}
