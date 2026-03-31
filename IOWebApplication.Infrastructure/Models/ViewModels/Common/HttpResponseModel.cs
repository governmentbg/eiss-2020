// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Net;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class HttpResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ResponseContent { get; set; }
    }
}
