// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IOWebApplication.Core.Models
{
    public class HierarchicalNomenclatureDisplayItem : NomenclatureDisplayItem
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("rootId")]
        public int RootId { get; set; }

        [JsonProperty("locations")]
        public List<SelectListItem> Locations { get; set; }
    }
}
