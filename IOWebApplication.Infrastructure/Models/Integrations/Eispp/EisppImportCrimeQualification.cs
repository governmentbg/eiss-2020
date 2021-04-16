﻿// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    public class EisppImportCrimeQualification
    {
        /// <summary>
        /// Номер на ред от ексела
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Име на група
        /// </summary>
        public string GroupName4 { get; set; }

        /// <summary>
        /// Име на група
        /// </summary>
        public string GroupName3 { get; set; }

        /// <summary>
        /// Име на група
        /// </summary>
        public string GroupName2 { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Системен идентификатор
        /// </summary>
        public string SystemCode { get; set; }

        /// <summary>  ЕКАТЕ Вид От дата До дата
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Вид акт/з..
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
        
        public override string ToString()
        {
            return $"{SystemCode} {Name}";
        }

    }
}
