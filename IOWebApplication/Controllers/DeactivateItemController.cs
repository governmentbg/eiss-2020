using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IOWebApplication.Controllers
{
    public class DeactivateItemController : BaseController
    {
        private readonly IDeactivateItemService service;

        public DeactivateItemController(IDeactivateItemService _service)
        {
            this.service = _service;
        }

        public IActionResult Index()
        {
            var model = new DeactivateItemFilterVM();
            List<SelectListItem> sources = new List<SelectListItem>()
            {
                new SelectListItem("Документи",SourceTypeSelectVM.Document.ToString()),
                new SelectListItem("Прикачени документи",(SourceTypeSelectVM.Files - SourceTypeSelectVM.Document).ToString()),
                new SelectListItem("Уведомления и призовки",SourceTypeSelectVM.CaseNotification.ToString()),
                //new SelectListItem("Документи към призовки",(SourceTypeSelectVM.Files - SourceTypeSelectVM.CaseNotification).ToString()),
                new SelectListItem("Съдебни актове",SourceTypeSelectVM.CaseSessionAct.ToString())
            };
            ViewBag.SourceType_ddl = sources;
            return View(model);
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, DeactivateItemFilterVM filter)
        {
            var data = service.Select(filter);
            return request.GetResponse(data);
        }
    }
}
