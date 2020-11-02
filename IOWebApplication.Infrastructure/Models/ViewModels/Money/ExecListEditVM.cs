// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ExecListEditVM
    {
        public int Id { get; set; }

        public int ExecListTypeId { get; set; }

        [Display(Name ="Изпълнително дело")]
        public string CaseNumber { get; set; }

        [Display(Name = "Дата на връчване")]
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "Връчен на")]
        public string DeliveryPersonName { get; set; }

        public bool ForPopUp { get; set; }

        public string RegNumber { get; set; }
        public DateTime RegDate { get; set; }

        [Display(Name = "Описание")]
        public string Content { get; set; }

        [Display(Name = "Основание")]
        public int? ExecListLawBaseId { get; set; }

        public string ObligationIdStr { get; set; }

        public int CaseGroupId { get; set; }

        [Display(Name = "Съдия")]
        public int LawUnitSignId { get; set; }
    }
}
