// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    public class AuthTokenRequestVM
    {
        public string Data { get; set; }
        public string Hash { get; set; }
    }

    public class AuthTokenVM
    {
        public bool Result { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }

        public string Token { get; set; }
        public DateTime? ExpiresIn { get; set; }
    }

    /// <summary>
    /// Информация на грешка при изпълнение
    /// </summary>
    public class ServiceErrorVM
    {
        /// <summary>
        /// Код на грешка
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Уточнения към грешката
        /// </summary>
        public string? Details { get; set; }
    }
}
