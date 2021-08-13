using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IOWebApplication.Controllers
{
    public class HtmlTemplateController : GlobalAdminBaseController
    {
        private readonly IHtmlTemplate service;
        private readonly INomenclatureService nomService;
        private readonly IPrintDocumentService printDocumentService;
        private readonly ICommonService commonService;

        public HtmlTemplateController(IHtmlTemplate _service, INomenclatureService _nomService, IPrintDocumentService _printDocumentService,
                                   ICommonService _commonService)
        {
            service = _service;
            nomService = _nomService;
            printDocumentService = _printDocumentService;
            commonService = _commonService;
        }

        /// <summary>
        /// Страница с бланки на документи
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(int? htmlTemplateTypeId)
        {
            HtmlTemplateFilterVM model = new HtmlTemplateFilterVM() { HtmlTemplateTypeId = (htmlTemplateTypeId ?? -1) };
            SetViewbag();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplate().DeleteOrDisableLast();
            return View(model);
        }
        [HttpPost]
        public IActionResult Index([AllowHtml] string filterJson)
        {
            HtmlTemplateFilterVM model = null;
            if (!string.IsNullOrEmpty(filterJson))
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
                model = JsonConvert.DeserializeObject<HtmlTemplateFilterVM>(filterJson, dateTimeConverter);
            }
            SetViewbag();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplate().DeleteOrDisableLast();
            return View(model);
        }

        /// <summary>
        /// Страница с Връзки по вид съд/дело
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        public IActionResult IndexLink(int id, [AllowHtml] string filterJson)
        {
            SetViewbagIndex(id, filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplateLink(id).DeleteOrDisableLast();
            return View();
        }

        /// <summary>
        /// Страница с Описание на параметри по бланки
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        public IActionResult IndexParam(int id, [AllowHtml] string filterJson)
        {
            SetViewbagIndex(id, filterJson);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplateParam(id).DeleteOrDisableLast();
            return View();
        }
        
        private void SetViewbagIndex(int id, [AllowHtml] string filterJson)
        {
            var html = service.GetById<HtmlTemplate>(id);
            ViewBag.htmlId = id;
            ViewBag.htmlName = html.Label;
            ViewBag.filterJson = filterJson;
        }

        /// <summary>
        /// Извличане на данни за бланките
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterData"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, HtmlTemplateFilterVM filterData)
        {
            var data = service.HtmlTemplate_Select(filterData);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Извличане на данни за връзките
        /// </summary>
        /// <param name="request"></param>
        /// <param name="htmlId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataLink(IDataTablesRequest request, int htmlId)
        {
            var data = service.HtmlTemplateLink_Select(htmlId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Извличане на данни за параметрите
        /// </summary>
        /// <param name="request"></param>
        /// <param name="htmlId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataParam(IDataTablesRequest request, int htmlId)
        {
            var data = service.HtmlTemplateParam_Select(htmlId);
            return request.GetResponse(data);
        }

        [HttpPost]
        public IActionResult ListDataParamAll(IDataTablesRequest request)
        {
            var data = service.HtmlTemplateParamAll_Select();
            return request.GetResponse(data);
        }

        public void SetBreadcrums(int id, [AllowHtml] string filterJson)
        {
            ViewBag.filterJson = filterJson;

            if (id > 0)
                ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplateEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplateAdd().DeleteOrDisableLast();
        }

        /// <summary>
        /// Добавяне на бланка
        /// </summary>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        public IActionResult Add([AllowHtml] string filterJson)
        {
            SetBreadcrums(0, filterJson);
            var model = new HtmlTemplate();
            SetViewbag();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на бланка
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        public IActionResult Edit(int id, [AllowHtml] string filterJson)
        {
            SetBreadcrums(id, filterJson);
            var model = service.GetById<HtmlTemplate>(id);
            model.DateFrom = model.DateFrom.Date == (new DateTime(1, 1, 1)).Date ? DateTime.Now.AddYears(-1) : model.DateFrom;
            SetViewbag();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на бланка
        /// </summary>
        /// <param name="files"></param>
        /// <param name="model"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult EditPost(ICollection<IFormFile> files, HtmlTemplate model, string filterJson)
        {
            SetBreadcrums(model.Id, filterJson);
            SetViewbag();
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), new { model, filterJson });
            }

            if (model.Id < 1)
            {
                if (files == null || files.Count() < 1)
                {
                    SetErrorMessage("Няма избран файл.");
                    return View(nameof(Edit), new { model, filterJson });
                }
            }

            var currentId = model.Id;
            if (service.HtmlTemplate_SaveData(files, model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id, filterJson } );
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        void SetViewbag()
        {
            ViewBag.HtmlTemplateTypeId_ddl = nomService.GetDropDownList<HtmlTemplateType>();
        }

        /// <summary>
        /// Добавяне на връзка по вид съд/дело
        /// </summary>
        /// <param name="HtmlTemplateId"></param>
        /// <param name="htmlTemplateTypeId"></param>
        /// <returns></returns>
        public IActionResult AddLink(int HtmlTemplateId, int htmlTemplateTypeId)
        {
            var model = new HtmlTemplateLink()
            {
                HtmlTemplateId = HtmlTemplateId,
                IsActive = true
            };

            SetViewbagLink(htmlTemplateTypeId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplateLinkEdit(model.HtmlTemplateId, 0).DeleteOrDisableLast();
            return View(nameof(EditLink), model);
        }

        /// <summary>
        /// Редакция на връзка по вид съд/дело
        /// </summary>
        /// <param name="id"></param>
        /// <param name="htmlTemplateTypeId"></param>
        /// <returns></returns>
        public IActionResult EditLink(int id, int htmlTemplateTypeId)
        {
            var model = service.GetById<HtmlTemplateLink>(id);
            SetViewbagLink(htmlTemplateTypeId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplateLinkEdit(model.HtmlTemplateId, id).DeleteOrDisableLast();
            return View(nameof(EditLink), model);
        }

        void SetViewbagLink(int htmlTemplateTypeId)
        {
            var htmlTemplateFilterVM = new HtmlTemplateFilterVM();
            htmlTemplateFilterVM.HtmlTemplateTypeId = htmlTemplateTypeId;
            ViewBag.htmlTemplateTypeId = htmlTemplateTypeId;
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
            ViewBag.filterJson = JsonConvert.SerializeObject(htmlTemplateFilterVM, dateTimeConverter);

            ViewBag.CourtTypeId_ddl = nomService.GetDropDownList<CourtType>();
            ViewBag.CaseGroupId_ddl = nomService.GetDropDownList<CaseGroup>();
            ViewBag.SourceType_ddl = SourceTypeSelectVM.GetDDL_SourceType();
        }

        private string IsValid(HtmlTemplateLink model)
        {
            //Ако не е попълнен вид съд важи за всички
            //if (model.CourtTypeId < 1)
            //    return "Изберете вид съд";

            return string.Empty;
        }

        /// <summary>
        /// Запис на връзка по вид съд/дело
        /// </summary>
        /// <param name="model"></param>
        /// <param name="htmlTemplateTypeId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditLink(HtmlTemplateLink model, int htmlTemplateTypeId)
        {
            SetViewbagLink(htmlTemplateTypeId);
            ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplateLinkEdit(model.HtmlTemplateId, model.Id).DeleteOrDisableLast();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditLink), new { model, htmlTemplateTypeId });
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditLink), new { model, htmlTemplateTypeId });
            }

            var currentId = model.Id;
            if (service.HtmlTemplateLink_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditLink), new { id = model.Id, htmlTemplateTypeId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditLink), model);
        }

        /// <summary>
        /// Запис на файл
        /// </summary>
        /// <param name="files"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult ImportHtmlBlank(ICollection<IFormFile> files, HtmlTemplate model)
        {
            SetViewbag();
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            if (model.Id < 1)
            {
                if (files == null || files.Count() < 1)
                {
                    SetErrorMessage("Няма избран файл.");
                    return View(nameof(Edit), model);
                }
            }

            var currentId = model.Id;
            if (service.HtmlTemplate_ImportData(files, model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на параметри
        /// </summary>
        /// <returns></returns>
        public IActionResult ImportHtmlParam()
        {
            if (service.HtmlTemplate_ImportParam())
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View();
        }

        /// <summary>
        /// Сваляне на файл
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult DownloadFile(int id)
        {
            var model = service.GetById<HtmlTemplate>(id);
            return File(model.Content.ToArray(), model.ContentType, model.FileName);
        }

        /// <summary>
        /// Преглед на бланка
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Preview(int id)
        {
            if(id == 0)
            {
                return null;
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplatePreview(id).DeleteOrDisableLast();

            var model = service.GetById<HtmlTemplate>(id);
            var htmlModel = printDocumentService.ConvertToTinyMCVM(model, model.HaveSessionAct == true);
            ViewBag.PreviewTitle = model.Label;
            return View("Preview", htmlModel);
        }

        public IActionResult PreviewRaw(int id)
        {
            var model = service.GetById<HtmlTemplate>(id);
            var htmlModel = printDocumentService.ConvertToTinyMCVM(model, model.HaveSessionAct == true);
            ViewBag.PreviewTitle = model.Label;
            return View("PreviewRaw", htmlModel);
        }

        [DisableAudit]
        [Route("[controller]/Preview/{id}/style.css")]
        public IActionResult Style(int id)
        {
            var model = service.GetById<HtmlTemplate>(id);
            var htmlModel = printDocumentService.ConvertToTinyMCVM(model, false);
            var deffStyle = FormattingConstant.TinyMceTableDefStyle + FormattingConstant.PrintTableDefStyle;
            return Content(htmlModel?.Style ?? deffStyle, "text/css");
        }

        /// <summary>
        /// Извличане на незаредените параметри
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string HtmlTemplate_GetNotSetParam(int id)
        {
            var model = service.GetById<HtmlTemplate>(id);
            return service.HtmlTemplate_GetNotSetParam(model.Alias);
        }

        /// <summary>
        /// Създаване на бланка
        /// </summary>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        public IActionResult AddHtmlTemplateCreate(string filterJson)
        {
            var model = new HtmlTemplateCreateVM();
            SetViewbag();
            SetBreadcrums(0, filterJson);
            return View(nameof(EditHtmlTemplateCreate), model);
        }

        /// <summary>
        /// Редакция на бланка
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        public IActionResult EditHtmlTemplateCreate(int id, string filterJson)
        {
            SetViewbag();
            SetBreadcrums(id, filterJson);
            HtmlTemplateCreateVM model = service.GetById_HtmlTemplateCreate(id);
            return View("EditHtmlTemplateCreate", model);
        }

        /// <summary>
        /// Запис на бланка
        /// </summary>
        /// <param name="model"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditHtmlTemplateCreatePost(HtmlTemplateCreateVM model, string filterJson)
        {
            SetViewbag();
            SetBreadcrums(model.Id, filterJson);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditHtmlTemplateCreate), new { model });
            }

            var currentId = model.Id;
            if (service.HtmlTemplateCreate_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditHtmlTemplateCreate), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditHtmlTemplateCreate), model);
        }
        public async Task<IActionResult> EditTinyMCE(int sourceId, [AllowHtml] string filterJson)
        {
            ViewBag.filterJson = filterJson;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_HtmlTemplatePreview(sourceId).DeleteOrDisableLast();

            var model = service.GetById<HtmlTemplate>(sourceId);
            var htmlModel = printDocumentService.ConvertToTinyMCVM(model, model.HaveSessionAct == true);

            htmlModel.SourceId = sourceId;
            htmlModel.SourceType = 0;

            return View("EditTinyMCE", htmlModel);
        }
        [HttpPost]
        public async Task<IActionResult> EditTinyMCESave(TinyMCEVM htmlModel, [AllowHtml] string filterJson)
        {
            htmlModel.Text = await this.RenderPartialViewAsync("~/Views/HtmlTemplate/", "Preview.cshtml", htmlModel, true);
            int htmlTemplateTypeId = -1;
            if (!string.IsNullOrEmpty(filterJson))
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
                var model = JsonConvert.DeserializeObject<HtmlTemplateFilterVM>(filterJson, dateTimeConverter);
                htmlTemplateTypeId = model.HtmlTemplateTypeId;
            }
            if (service.HtmlTemplate_SaveDataTiny(htmlModel))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return RedirectToAction(nameof(Index), new { htmlTemplateTypeId });
        }
    }
}