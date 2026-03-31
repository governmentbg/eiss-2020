// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
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

    public class EpepErrorVM
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string? Details { get; set; }
    }
}
