// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.AuditLog
{
    public class AuditLogContextInfoModelVM
    {
        public int CourtId { get; set; }
        public string UserId { get; set; }
        /// <summary>
        /// Тип на обекта
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// Идентификатор на обекта
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// Операция
        /// Редактиране, Насрочване, Образуване, 
        /// Номенклатури, Регистриране
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Информация за базовия обект
        /// Писмо 1222, НОХД 123322123
        /// </summary>
        public string BaseObject { get; set; }

        /// <summary>
        /// Информация за достъпения обект
        /// Заседание, Решение, Уведомление, Определение ...
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Описание на достъпения обект
        /// Номер и дата или друго пояснение в зависимост от типа на обекта
        /// </summary>
        public string ObjectInfo { get; set; }
    }
}
