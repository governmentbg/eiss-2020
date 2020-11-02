// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class HeritageReportVM
    {
        [Display(Name = "№ по ред")]
        public int Index { get; set; }

        [Display(Name = "№ на входящ регистър")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Дата на заявлението")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocumentDate { get; set; }

        [Display(Name = "Собствено, бащино и  фамилно име на заявителя и местожителството му")]
        public string Notifier { get; set; }

        [Display(Name = "Отричане от наследство")]
        public string RefuseHeritage { get; set; }

        [Display(Name = "Приемане на наследство по опис")]
        public string AcceptHeritage { get; set; }

        [Display(Name = "От наследството на кого се отрича или го приема под опис")]
        public string ResultData { get; set; }

        [Display(Name = "Какъв се пада  заявителя на лицето, наследството на кого се е отразило")]
        public string PersonNames { get; set; }

        [Display(Name = "Забележка")]
        public string Description { get; set; }   
        
        public DateTime ActDeclaredDate { get; set; }
    }

    public class HeritageFilterReportVM
    {
        [Display(Name = "От дата на акт")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата на акт")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "От дата на влизане в законна сила")]
        public DateTime? FromActInforcedDate { get; set; }

        [Display(Name = "До дата на влизане в законна сила")]
        public DateTime? ToActInforcedDate { get; set; }

        [Display(Name = "Номер на акт")]
        public string NumberAct { get; set; }

    }
}
