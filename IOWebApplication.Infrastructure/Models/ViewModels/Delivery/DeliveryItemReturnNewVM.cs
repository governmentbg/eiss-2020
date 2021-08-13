// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemReturnNewVM
    {
        public int Id { get; set; }

        [Display(Name = "Дата на изготвяне / получаване")]
        public DateTime? DateAccepted { get; set; }
        public string DateAcceptedRep { get; set; }

        [Display(Name = "Номер на делото")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "За кое лице се отнася")]
        public string PersonName { get; set; }

        [Display(Name = "Връчител")]
        public string LawUnitName { get; set; }

        [Display(Name = "Дата на предаване")]
        public DateTime? DateToLawUnit { get; set; }
        public string DateToLawUnitRep { get; set; }
        
        [Display(Name = "На кого е връчена и дата на връчване")]
        public string DeliveryInfoRep { get; set; }
        public string DeliveryInfo { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string NotificationState { get; set; }
        public string ReturnReason { get; set; }

        [Display(Name = "Дата на връщане")]
        public DateTime? DateReturn { get; set; }
        public string DateReturnRep { get; set; }

        [Display(Name = "Дата на изпращане")]
        public DateTime? DateSend { get; set; }
        public string DateSendRep { get; set; }
    }
}
