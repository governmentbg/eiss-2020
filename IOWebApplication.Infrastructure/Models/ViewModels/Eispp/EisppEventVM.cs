// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppEventVM
    {
        /// <summary>
        /// ид дело
        /// </summary>
        public int CaseId { get; set; }

        public string SourceType { get; set; }

        public string SourceId { get; set; }
        /// <summary>
        /// ЕИСПП Събитие
        /// </summary>
        [Display(Name = "Събитие")]
        [Range(-7, int.MaxValue, ErrorMessage = "Изберете Събитие")]
        public int EventType { get; set; }

        /// <summary>
        /// Лице за което се отнася събитието
        /// попълва се само когато събитието е за едно лице
        /// </summary>
        [Display(Name = "Лице")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Лице")]
        public int? CasePersonId { get; set; }

        /// <summary>
        /// Вид дело
        /// </summary>
        [Display(Name = "Вид дело")]
        public string CaseType { get; set; }

        /// <summary>
        /// година на делото
        /// </summary>
        [Display(Name = "Година")]
        public int Year { get; set; }

        /// <summary>
        /// кратък номер, а не 14-цифрен  номер
        /// </summary>
        [Display(Name = "Номер")]
        public int ShortNumber { get; set; }

        /// <summary>
        /// Свързано дело което се получава
        /// </summary>
        [Display(Name = "Свързано дело")]
        public string ConnectedCaseId { get; set; }

        public int CaseTypeId { get; set; }
        public int CaseCodeId { get; set; }
        
        /// <summary>
        /// точен вид дело
        /// системен код на елемент от nmk_dlosig
        /// </summary>
        [Display(Name = "Точен вид дело")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Точен вид дело")]
        public int? ExactCaseType { get; set; }

        /// <summary>
        /// Акт/протокол
        /// </summary>
        [Display(Name = "Акт/Протокол")]
        public int? CaseSessionActId { get; set; }

        /// <summary>
        /// Отменена мяркa
        /// </summary>
        [Display(Name = "Отменена мяркa за процесуална принуда")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Oтменена мяркa")]
        public int? PersonOldMeasureId { get; set; }

        /// <summary>
        /// Наложена мяркa
        /// </summary>
        [Display(Name = "Наложена мяркa за процесуална принуда")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Наложена мяркa")]
        public int? PersonMeasureId { get; set; }

        [Display(Name = "ЕИСПП номер на НП")]
        [Required(ErrorMessage = "Невалиден {0}.")]
        [RegularExpression("[А-Я]{3}[0-9]{8}[В-Г]{1}[А-Я]{2}", ErrorMessage = "Невалиден {0}.")]
        [Remote(action: "VerifyEISPPNumber", controller: "Eispp")]
        public string EISPPNumber { get; set; }

    }
}
