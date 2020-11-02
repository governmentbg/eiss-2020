// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawUnitTaskChangeVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        [Display(Name = "Съдебно дело")]
        public string CaseNumber { get; set; }
        [Display(Name = "Вид акт")]
        public string ActType { get; set; }
        [Display(Name = "Акт номер")]
        public string ActNumber { get; set; }
        [Display(Name = "Акт дата")]
        public DateTime ActDate { get; set; }
        [Display(Name = "Дата на задачата")]
        public DateTime TaskDate { get; set; }
        [Display(Name = "Дата на промяната")]
        public DateTime ChangeDate { get; set; }
        [Display(Name = "Вид задача")]
        public string TaskTypeName { get; set; }
        [Display(Name = "Основание")]
        public string Description { get; set; }
        [Display(Name = "Изпълнител на задачата")]
        public string OldTaskUserName { get; set; }
        [Display(Name = "Нов изпълнител на задачата")]
        public string NewTaskUserName { get; set; }
        [Display(Name = "Промяната извършена от")]
        public string ChangeUserName { get; set; }
    }
}
