// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// DVJDLO
    /// Движение на дело
    /// </summary>
    public class CaseMigration
    {
        /// <summary>
        /// dvjdlosid
        /// системен идентификатор
        /// </summary>
        [XmlAttribute("dvjdlosid")]
        public int MigrationId { get; set; }

        /// <summary>
        /// dvjdta
        /// Дата на изпращане/получаване на дело
        /// </summary>
        [XmlAttribute("dvjdta", DataType = "date")]
        public DateTime MigrationDate { get; set; }

        /// <summary>
        /// dvjstripr
        /// Структура, която изпраща делото
        /// номенклатура nmk_strvid
        /// </summary>
        [XmlAttribute("dvjstripr")]
        public int SendingStructureId { get; set; }

        /// <summary>
        /// dvjstrplc
        /// Структура, която получава делото
        /// номенклатура nmk_strvid
        /// </summary>
        [XmlAttribute("dvjstrplc")]
        public int ReceiverStructureId { get; set; }

        /// <summary>
        /// dvjvid
        /// Вид движение на дело
        /// номенклатура nmk_dvjvid
        /// </summary>
        [XmlAttribute("dvjvid")]
        public int MigrationType { get; set; }

        /// <summary>
        /// dvjdlodvn
        /// Деловоден номер
        /// </summary>
        [XmlAttribute("dvjdlodvn")]
        public string RegistrationNumber { get; set; }

        /// <summary>
        /// Дата, която трябва да се игнорира
        /// </summary>
        private static readonly DateTime defaultDate = default;

        /// <summary>
        /// Игнорира празна дата
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeMigrationDate()
        {
            return MigrationDate != defaultDate;
        }
    }
}