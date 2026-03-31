// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class SentenceTypeHasVM
    {
        /// <summary>
        /// Период
        /// </summary>
        public bool HasPeriod { get; set; }

        /// <summary>
        /// Пари
        /// </summary>
        public bool HasMoney { get; set; }

        /// <summary>
        /// Пробация
        /// </summary>
        public bool HasProbation { get; set; }

        /// <summary>
        /// Ефективна присъда
        /// </summary>
        public bool IsEffective { get; set; }

        /// <summary>
        /// Предварително задържане
        /// </summary>
        public bool HasPreliminaryDetention { get; set; }
    }
}
