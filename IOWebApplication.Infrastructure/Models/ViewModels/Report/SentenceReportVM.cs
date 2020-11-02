// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class SentenceReportVM
    {
        [Display(Name = "Номер по ред")]
        public int Index { get; set; }

        [Display(Name = "Собствено, бащино и фамилно име, местожителство на осъдения, на какво наказание е осъден и на кой текст от закона")]
        public string PersonData { get; set; }

        [Display(Name = "Номер на присъдата, по кое дело и от кой съд е постановена и кога е влязла в сила")]
        public string SentenceData { get; set; }

        [Display(Name = "На кой орган с кой номер и дата на писмото е изпратена присъдата за изпълнение")]
        public string SendData { get; set; }

        [Display(Name = "Кога е приведена присъдата в изпълнение, номер и дата на писмото, с което се съобщава за това и от кой орган")]
        public string ExecuteData { get; set; }

        [Display(Name = "Къде се изпълнява присъдата")]
        public string SentencePlace { get; set; }

        [Display(Name = "Последвало ли е помилване и какво е то (пълно, частично), номер и дата на указа")]
        public string AmnestyData { get; set; }

        [Display(Name = "Номер и дата на писмото, с което се съобщава, че наказанието е изпълнено и от кой орган")]
        public string SentenceFinishData { get; set; }

        [Display(Name = "Забележка")]
        public string Description { get; set; }

        public DateTime? SentDate { get; set; }
    }

    public class SentenceFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
    }
}
