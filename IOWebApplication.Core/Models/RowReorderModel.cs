// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;

namespace IOWebApplication.Core.Models
{
    /// <summary>
    /// Модел с информация за пренареждането 
    /// на редове в DataTable
    /// </summary>
    public class RowreorderModel
    {
        /// <summary>
        /// Нов пореден номер
        /// </summary>
        [JsonProperty("newOrderNumber")]
        public int NewOrderNumber { get; set; }

        /// <summary>
        /// Стар пореден номер
        /// </summary>
        [JsonProperty("oldOrderNumber")]
        public int OldOrderNumber { get; set; }
    }
}
