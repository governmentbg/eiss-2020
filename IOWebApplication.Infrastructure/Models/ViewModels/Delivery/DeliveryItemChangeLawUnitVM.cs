// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryItemChangeLawUnitVM
    {
        [Display(Name = "Изпратена към")]
        public int CourtId { get; set; }
        
        [Display(Name = "Призовкар")]
        public int? LawUnitId { get; set; }

        [Display(Name = "Район за доставка")]
        public int? DeliveryAreaId { get; set; }

        [Display(Name = "Нов съд за разнасяне")]
        public int NewCourtId { get; set; }

        [Display(Name = "Нов призовкар от")]
        public int NewLawUnitType { get; set; } = NomenclatureConstants.LawUnitTypes.MessageDeliverer;

        [Display(Name = "Нов призовкар")]
        public int NewLawUnitId { get; set; }

        [Display(Name = "Нов район за доставка")]
        public int NewDeliveryAreaId { get; set; }

        [Display(Name = "Статус")]
        public int NotificationStateId { get; set; }

    }
}
