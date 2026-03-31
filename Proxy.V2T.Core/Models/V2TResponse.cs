// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace Proxy.V2T.Core.Models
{
    public class V2TResponse
    {
        public bool IsAuthorized { get; set; }
        public bool GeneralError { get; set; }
        public string Response { get; set; }
    }
}
