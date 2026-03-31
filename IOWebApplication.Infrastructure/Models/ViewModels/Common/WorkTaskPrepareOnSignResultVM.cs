// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkTaskPrepareOnSignResultVM
    {
        public bool Result { get; set; }
        public string ErrorMessage { get; set; }
        public string FileContentBase64 { get; set; }
    }
}
