// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.Cdn
{
    public class CdnUploadResult
    {
        public bool Succeded { get; set; }
        public string ErrorMessage { get; set; }
        public string FileId { get; set; }
    }
}
