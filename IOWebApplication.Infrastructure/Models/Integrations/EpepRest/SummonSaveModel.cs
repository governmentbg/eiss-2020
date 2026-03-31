// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Epep;
using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    public class SummonSaveModel
    {
        public Summon Summon { get; set; }

        /// <summary>
        /// Идентфикатор на лице, получател на призовката, за връчване през ЕПЕП, в противен случай-null
        /// </summary>
        public Guid? UserId { get; set; }
    }
}
