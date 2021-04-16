using System;
using DataTables.AspNet.Core;
using IOWebApplication.Components;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.UserContext;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.AuditLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    [DisableAudit]
    [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
    public class AuditLogController : BaseController
    {
        private readonly IAuditLogService service;

        public AuditLogController(IAuditLogService _service)
        {
            service = _service;
        }

        public IActionResult Index()
        {
            var dtNow = DateTime.Now;
            var model = new AuditLogFilterVM()
            {
                DateFrom = dtNow.AddHours(-16),
                DateTo = dtNow.ForceEndDate()
            };


            ViewBag.Operation_ddl = service.GetDDL_Operation();
            return View(model);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, DateTime DateFrom, DateTime DateTo, string RegNumber, string Operation, string UserId)
        {
            if (UserId == "0")
                UserId = null;

            var data = service.AuditLog_SelectNew(DateFrom, DateTo, RegNumber, Operation, UserId, userContext.CourtId);
            return request.GetResponse(data);
        }
    }
}