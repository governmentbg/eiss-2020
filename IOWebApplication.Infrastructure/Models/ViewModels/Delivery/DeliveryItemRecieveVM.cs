// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemRecieveVM
    {
        public int Id { get; set; }

        [Display(Name = "Получено от съд")]
        public string FromCourtName { get; set; }

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
    }
}
