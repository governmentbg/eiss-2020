// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace Proxy.EISS.Models
{

    public class ApiTokenRequestVM
    {

        /// <summary>
        /// Днешна дата във формат yyyyMMdd: 20241010
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// HMAC256 хеширано съдържание като HEX със appSecret на полето Data,
        /// </summary>
        public string Hash { get; set; }
    }


    public class AuthTokenVM
    {
        /// <summary>
        /// Връща true при успешно получаване на ауторизационен токън
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Ауторизационен токън
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Срок на валидност на ауторизационен токън
        /// </summary>
        public DateTime ExpiresIn { get; set; }

        /// <summary>
        /// Съобщение за грешка, при неуспешно вземане на 
        /// </summary>
        public string Message { get; set; }

        public AuthTokenVM()
        {

        }

        public AuthTokenVM(bool result, string message = "")
        {
            this.Result = result;
            this.Message = message;
        }
    }
}
