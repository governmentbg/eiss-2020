// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public  class EisppEventFilterVM
    {
        public int CourtId { get; set; }

        public int CaseId { get; set; }

        /// <summary>
        /// Вид събитие
        /// </summary>
        [Display(Name = "Вид")]
        public int EventTypeId { get; set; }

        /// <summary>
        /// От дата на събитие
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime? EventDateFrom { get; set; }

        /// <summary>
        /// До дата на събитие
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? EventDateTo { get; set; }

        /// <summary>
        /// Събитие е към Case или sessionAct
        /// </summary>
        [Display(Name = "Към")]
        public int LinkType { get; set; }

        /// <summary>
        /// От дата на акт/протокол
        /// </summary>
        [Display(Name = "От дата акт/протокол")]
        public DateTime? ActDateFrom { get; set; }

        /// <summary>
        /// До дата на акт/протокол
        /// </summary>
        [Display(Name = "До дата акт/протокол")]
        public DateTime? ActDateTo { get; set; }


        /// <summary>
        /// Акт/Протокол 
        /// </summary>
        [Display(Name = "Акт/Протокол")]
        public int SessionActId { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

    }
}
