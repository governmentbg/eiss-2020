// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Http;

namespace IOWebApplication.Infrastructure.Services.Mocks
{
    public class MockHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; } = null;
    }
}
