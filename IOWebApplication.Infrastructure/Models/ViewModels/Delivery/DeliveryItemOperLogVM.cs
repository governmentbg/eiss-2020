// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemOperLogVM
    {
        public int DeliveryItemId { get; set; }
        [Display(Name = "Дата запис")]
        public DateTime DateWrt { get; set; }

        [Display(Name = "Потребител")]
        public string UserName { get; set; }

        [Display(Name = "Екран")]
        public string PageLabel { get; set; }

        [Display(Name = "Действие")]
        public string Action { get; set; }

        [Display(Name = "Изготвена в съд")]
        public string FromCourtName { get; set; }

        [Display(Name = "За доставка в съд")]
        public string ToCourtName { get; set; }

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
        public string StateName { get; set; }
        [Display(Name = "Дело")]
        public string CaseInfo { get; set; }
        public DateTime? DateOper { get; set; }
        
        public string OperName { get; set; }

    }
}
