// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IBlankTemplateService : IBaseService
    {
        Task<SaveResultVM> BlankTemplateSaveData(BlankTemplate model);
        IQueryable<BlankTemplateListVM> BlankTemplateSelect();
        Task<List<SelectListItem>> GetBlankTemplates(int sourceType, int sourceId, int? caseId = null);
        Task<List<SelectListItem>> GetLawUnitTemplates(int actId);
        Task<SaveResultVM> LawUnitTemplateSaveData(LawUnitTemplate model);
        IQueryable<LawUnitTemplateListVM> LawUnitTemplateSelect();
    }
}
