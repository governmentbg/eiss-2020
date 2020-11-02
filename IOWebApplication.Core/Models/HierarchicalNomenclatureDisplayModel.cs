// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;
using System.Collections.Generic;

namespace IOWebApplication.Core.Models
{
    public class HierarchicalNomenclatureDisplayModel
    {
        [JsonProperty("data")]
        public List<HierarchicalNomenclatureDisplayItem> Data { get; set; }

        public HierarchicalNomenclatureDisplayModel()
        {
            Data = new List<HierarchicalNomenclatureDisplayItem>();
        }
    }
}
