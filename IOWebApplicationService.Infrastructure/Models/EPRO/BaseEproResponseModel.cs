// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class BaseEproResponseModel : IBaseEproResponseModel
    {
        public ErrorModel Error { get; set; }
    }
}
