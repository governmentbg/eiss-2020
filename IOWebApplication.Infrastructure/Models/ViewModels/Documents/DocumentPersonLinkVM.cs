// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentPersonLinkVM
    {
        public long DocumentId { get; set; }
        public DocumentPersonLinkSideVM[] Representatives { get; set; }
        public List<SelectListItem> MainSides { get; set; }
    }

    public class DocumentPersonLinkSideVM
    {
        public int RoleKind { get; set; }
        public long DocumentPersonId { get; set; }
        public string PersonGid { get; set; }
        public string PersonName { get; set; }
        public string RepresentsGid { get; set; }
        public string SavedRepresentsGid { get; set; }
    }
}
