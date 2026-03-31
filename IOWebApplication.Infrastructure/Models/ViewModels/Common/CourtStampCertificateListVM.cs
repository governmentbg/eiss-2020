// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtStampCertificateListVM
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public IEnumerable<CourtStampCertificateVM> Certificates { get; set; }

        public bool IsExpiring
        {
            get
            {
                return Certificates.Any(c => c.IsExpiring);
            }
        }
    }

    public class CourtStampCertificateVM
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string Subject { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public bool IsDeactivated { get; set; }
        public bool IsExpired
        {
            get
            {
                return DateTo < DateTime.Now;
            }
        }
        public bool IsExpiring
        {
            get
            {
                return DateTo < DateTime.Now.AddMonths(1);
            }
        }
    }

    public class CourtStampCertificateFilterVM
    {
        [Display(Name = "Съд")]
        public string CourtName { get; set; }
        [Display(Name = "Валидно към дата")]
        public DateTime? ValidTo { get; set; }
        [Display(Name = "Покажи анулирани")]
        public bool ShowExpired { get; set; }

        [Display(Name = "Подреди по")]
        public string OrderMode { get; set; }
    }
}
