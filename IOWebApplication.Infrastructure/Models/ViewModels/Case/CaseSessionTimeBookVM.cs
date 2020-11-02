// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionTimeBookVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }

        [Display(Name = "№ по ред")]
        public int Number { get; set; }
        
        [Display(Name = "Дата на заседанието")]
        public DateTime DateSession { get; set; }
        
        [Display(Name = "№ и дата на образуване на делото")]
        public string CaseRegNumDate { get; set; }
        
        [Display(Name = "Характер")]
        public string CaseGroupe { get; set; }
        
        [Display(Name = "Ищец")]
        public string LeftSide { get; set; }
        
        [Display(Name = "Ответник")]
        public string RightSide { get; set; }
        
        [Display(Name = "Състав на съда, Председател, Членове")]
        public string CourtComposition { get; set; }
        
        [Display(Name = "Докладчик")]
        public string Rapporteur { get; set; }
        
        [Display(Name = "Прокурор")]
        public string Prosecutor { get; set; }
        
        [Display(Name = "Секретар")]
        public string Secretary { get; set; }
        
        [Display(Name = "Решение")]
        public string ResultAndAct { get; set; }
        
        [Display(Name = "Дата, за която е отложено; причини за отлагането")]
        public string Description { get; set; }

        [Display(Name = "Дата на предаване на делото в канцеларията")]
        public string DateCase { get; set; }

        [Display(Name = "Подпис на служителя")]
        public string Signature { get; set; }
    }
}
