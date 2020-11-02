// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemVM
    {
        public int Id { get; set; }

        public int CourtId { get; set; }
        public int FromCourtId { get; set; }

        public int? LawUnitId { get; set; }
        public int? DeliveryAreaId { get; set; }

        [Display(Name = "Изготвена в съд")]
        public string FromCourtName { get; set; }

        [Display(Name = "За доставка в съд")]
        public string CourtName { get; set; }

        [Display(Name = "Регистрационен номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Район за доставка")]
        public string AreaName { get; set; }

        [Display(Name = "Призовкар")]
        public string LawUnitName { get; set; }

        [Display(Name = "Име на лицето")]
        public string PersonName { get; set; }

        [Display(Name = "Адрес на лицето")]
        public string Address { get; set; }

        [Display(Name = "Статус")]
        public string StateName  { get; set; }

        [Display(Name = "Дата на изпращане")]
        public DateTime? DateSend { get; set; }

        [Display(Name = "Дата на приемане")]
        public DateTime? DateAccepted { get; set; }

        [Display(Name = "Дата на доставка")]
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "Изготвена на дата")]
        public DateTime? DateReady { get; set; }
        
        public bool CheckRow { get; set; }

        public string CaseInfo { get; set; }

        public int NotificationDeliveryGroupId { get; set; }
    }
}
