// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    // Област Община  тип н.м.Населено място Район   Системен идентификатор  Системно име    ЕКАТЕ Вид От дата До дата
    public class EisppImportEktte
    {
        /// <summary>
        /// Номер на ред от ексела
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Област
        /// </summary>
        public string Oblast { get; set; }

        /// <summary>
        /// Община
        /// </summary>
        public string Obstina { get; set; }

        /// <summary>
        /// тип н.м.
        /// </summary>
        public string TypeNM { get; set; }

        /// <summary>
        /// Населено място
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Населено място
        /// </summary>
        public string Rajon { get; set; }

        /// <summary>
        /// Системен идентификатор
        /// </summary>
        public string SystemCode { get; set; }

        /// <summary>
        /// Системно име    ЕКАТЕ Вид От дата До дата
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// ЕКАТЕ Вид От дата До дата
        /// </summary>
        public string EktteCode { get; set; }

        /// <summary>
        /// Вид акт/з.. От дата До дата
        /// </summary>
        public string Active { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        public string DateFrom { get; set; }
        /// <summary>
        /// До дата
        /// </summary>
        public string DateTo { get; set; }
    }
}
