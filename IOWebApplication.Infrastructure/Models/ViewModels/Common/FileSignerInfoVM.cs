// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class FileSignerInfoVM
    {
        [Display(Name="Имена")]
        public string Name { get; set; }
        [Display(Name="Идентификатор")]
        public string Identifier { get; set; }
        [Display(Name="Номер сертификат")]
        public string CertificateNumber { get; set; }
        [Display(Name="Издател")]
        public string Issuer { get; set; }
        [Display(Name="Подписано на")]
        public DateTime SignedOn { get; set; }
        [Display(Name="Валиден до")]
        public DateTime ValidTo { get; set; }

        public bool FromApi { get; set; }
        
        [Display(Name = "Потребител ЕПЕП")]
        public string EpepUserName { get; set; }
        
        [Display(Name = "Потребител ЕПЕП")]
        public string CreateUserName { get; set; }

        [Display(Name = "Подадено на")]
        public DateTime ApplyDate { get; set; }

    }
}
