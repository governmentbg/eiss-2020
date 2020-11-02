// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// NPRFZLSTA
    /// NPRFZL 
    /// За сега не е ясно, кое е вярното
    /// Статус на НП за конкретно лице
    /// </summary>
    public class PersonCriminalProceeding
    {
        /// <summary>
        /// fzlsid
        /// Системен идентификатор на лице
        /// задължително
        /// </summary>
        [XmlAttribute("fzlsid")]
        public int PersonId { get; set; }

        /// <summary>
        /// nprfzldta
        /// Дата на статуса на НП за лице
        /// </summary>
        [Display(Name = "Дата на статуса")]
        [XmlAttribute("nprfzldta", DataType = "date")]
        public DateTime StatusDate { get; set; }
        [XmlIgnore]
        [Display(Name = "Дата на статус")]
        public DateTime? StatusDateVM
        {
            get
            {
                return StatusDate > defaultDate ? (DateTime?)StatusDate : (DateTime?)null;
            }
            set
            {
                StatusDate = value ?? defaultDate;
            }
        }
        /// <summary>
        /// nprfzlkcv
        /// Качество на лице в НП
        /// Номенклатура nmk_nprfzlkcv
        /// </summary>
        [Display(Name = "Качество на лице в НП")]
        [XmlAttribute("nprfzlkcv")]
        public int PersonRole { get; set; }

        /// <summary>
        /// nprfzlosn
        /// Основание за статус на НП за лице
        /// Основания за налагане на административно наказание ??? 
        /// номенклатура nmk_osn_adm_nkz
        /// </summary>
        [Display(Name = "Основание за статус")]
        [XmlAttribute("nprfzlosn")]
        public int StatusReason { get; set; }


        /// <summary>
        /// nprfzlsts
        /// Статус на НП за лице
        /// номенклатура nmk_nprfzlsts
        /// </summary>
        [Display(Name = "Статус на НП за лице")]
        [XmlAttribute("nprfzlsts")]
        public int Status { get; set; }

        /// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		private static readonly DateTime defaultDate = default;

        /// <summary>
        /// Игнорира празна дата
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeStatusDate()
        {
            return StatusDate != defaultDate;
        }
    }
}