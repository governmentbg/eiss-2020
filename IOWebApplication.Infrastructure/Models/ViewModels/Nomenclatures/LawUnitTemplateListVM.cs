// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class LawUnitTemplateListVM
    {
        public int Id { get; set; }

        public string Label { get; set; }

        public string CaseGroup { get; set; }
        public string ActType { get; set; }

        public bool IsActive { get; set; }
    }
}
