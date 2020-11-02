// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IOWebApplication.Controllers
{
    public class CaseDeadlineController : BaseController
    {
        private readonly INomenclatureService nomService;
        private readonly ICaseDeadlineService service;
        private readonly ICourtLawUnitService courtLawUnitService;
        private readonly ICaseService caseService;
        private readonly ICommonService commonService;
        public CaseDeadlineController(
            ICaseDeadlineService _service, 
            INomenclatureService _nomService, 
            ICourtLawUnitService _courtLawUnitService, 
            ICaseService _caseService,
            ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            courtLawUnitService = _courtLawUnitService;
            caseService = _caseService;
            commonService = _commonService;
        }
        public IActionResult Index(string filterJson)
        {
            CaseDeadLineFilterVM model = null;
            if (!string.IsNullOrEmpty(filterJson))
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
                model = JsonConvert.DeserializeObject<CaseDeadLineFilterVM>(filterJson, dateTimeConverter);
            } else
            {
                model = new CaseDeadLineFilterVM();
                model.DateEndTo = DateTime.Now;
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_CaseDeadLine().DeleteOrDisableLast();
            SetViewBag();
            return View(model);
        }
        public IActionResult IndexCase(int caseId)
        {
            if (!CheckAccess(service, SourceTypeSelectVM.CaseDeadLine, null, AuditConstants.Operations.View, caseId))
            {
                return Redirect_Denied();
            }
            CaseDeadLineFilterVM model = new CaseDeadLineFilterVM();
            // model.DateEndTo = DateTime.Now; да се виждат и бъдещите срокове
            model.CaseId = caseId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCase(caseId);
            SetViewBag();
            SetHelpFile(HelpFileValues.CaseDeadline);
            return View(nameof(Index), model);
        }
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, CaseDeadLineFilterVM filterData)
        {
            var data = service.CaseDeadLineSelect(filterData);
            return request.GetResponse(data);
        }

        void SetViewBag()
        {
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.LawUnitId_ddl = courtLawUnitService.LawUnitForCourt_SelectDDL(NomenclatureConstants.LawUnitTypes.Judge, userContext.CourtId);
            ViewBag.DeadlineTypeId_ddl = nomService.GetDropDownList<DeadlineType>();
        }
    }
}