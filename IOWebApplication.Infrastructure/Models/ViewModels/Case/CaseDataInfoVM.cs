// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел с информация за дело
    /// </summary>
    public class CaseDataInfoVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///Пълен 14 цифрен номер на дело
        /// </summary>
        public string RegNumber { get; set; }

        /// <summary>
        /// Кратък 5 цифрен номер на делото
        /// </summary>
        public string ShortNumber { get; set; }

        /// <summary>
        /// Дата на дело
        /// </summary>
        public DateTime RegDate { get; set; }

        /// <summary>
        /// Код на тип на дело
        /// </summary>
        public string CaseTypeCode { get; set; }

        /// <summary>
        /// Тип дело
        /// </summary>
        public string CaseTypeLabel { get; set; }

        /// <summary>
        /// Влизане в законна сила
        /// </summary>
        public DateTime? CaseInforcedDate { get; set; }

        /// <summary>
        /// Основен вид дело
        /// </summary>
        public string CaseGroupLabel { get; set; }

        /// <summary>
        /// Грипа на дело
        /// </summary>
        public int CaseGroupId { get; set; }

        /// <summary>
        /// Инстанция
        /// </summary>
        public int CaseInstanceId { get; set; }

        /// <summary>
        /// Дело код, кратък номер и година
        /// </summary>
        public string CaseTypeCodeShortNumberRegDate
        {
            get
            {
                return CaseTypeCode + " " + ShortNumber + "/" + RegDate.ToString("yyyy");
            }
        }
    }
}
