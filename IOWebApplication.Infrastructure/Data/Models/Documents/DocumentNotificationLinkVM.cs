// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    public class DocumentNotificationLinkVM
    {
        public int Id { get; set; }
        public long PersonId { get; set; }
        public long PersonRelId { get; set; }
        public long? PersonSecondRelId { get; set; }
        public int LinkDirectionId { get; set; }
        public int? LinkDirectionSecondId { get; set; }
        public string PersonName { get; set; }
        public string PersonRelName { get; set; }
        public string PersonSecondRelName { get; set; }
        public string PersonRole { get; set; }
        public string PersonRelRole { get; set; }
        public string PersonSecondRelRole { get; set; }
        public string LinkTemplate { get; set; }
        public string SecondLinkTemplate { get; set; }
        public string Label { get; set; }
        public bool isXFirst { get; set; }
    }
}
