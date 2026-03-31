// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.Cais
{
    public class RegisterBulletinRequestModel
    {
        /// <summary>
        /// Номер на дело
        /// </summary>
        public int CaseNumber { get; set; }
        /// <summary>
        /// Година дело
        /// </summary>
        public int CaseYear { get; set; }

        /// <summary>
        /// ЕИСПП код на съд
        /// </summary>
        public string CourtCode { get; set; }

        /// <summary>
        /// Номер на бюлетин от ЕИСС
        /// </summary>
        public string BulletinNumber { get; set; }

        /// <summary>
        /// Дата на регистриране в Бюро Съдимост
        /// </summary>
        public DateTime DateRegistered { get; set; }
    }

    public class RegisterBulletinResultModel
    {
        /// <summary>
        /// Флаг за успешно отразяване
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Код на грешка
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Съобщение за грешка, ако има
        /// </summary>
        public string Message { get; set; }
    }
}
