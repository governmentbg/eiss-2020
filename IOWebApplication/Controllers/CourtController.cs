using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IOWebApplication.Controllers
{
    public class CourtController : AdminBaseController
    {
        private readonly ICommonService commonService;
        private readonly INomenclatureService nomService;
        private readonly ICourtRegionService regionService;
        private readonly IMigrationDataService migrationDataService;
        private readonly ICourtStampCertificateService stampService;
        public CourtController(
            ICommonService _commonService,
            INomenclatureService _nomService,
            ICourtRegionService _regionService,
            IMigrationDataService _migrationDataService,
            ICourtStampCertificateService _stampService
            )

        {
            commonService = _commonService;
            nomService = _nomService;
            regionService = _regionService;
            migrationDataService = _migrationDataService;
            stampService = _stampService;
        }

        //public IActionResult FillCourtAddress()
        //{
        //    commonService.FillCourtAddress();
        //    return null;
        //}

        /// <summary>
        /// Страница за Съдилища
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(bool md = false)
        {
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            ViewBag.MigrateData = md;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCourts().DeleteOrDisableLast();
            return View();
        }

        /// <summary>
        /// Извличане на данни за съдилища
        /// </summary>
        /// <param name="request"></param>
        /// <param name="courtTypeId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int courtTypeId)
        {
            var data = commonService.CourtsByType(courtTypeId);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Редакция на съд
        /// </summary>
        /// <param name="id"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public IActionResult Edit(int id, string returnUrl)
        {
            if (id <= 0)
                id = userContext.CourtId;
            SetViewbag(returnUrl, id);
            var model = commonService.Court_GetById(id);
            if (model.AddressId == null)
            {
                model.AddressId = 0;
                model.CourtAddress = new Address();
            }
            AddAuditInfo(AuditConstants.Operations.View, "Данни за текущия съд", model.Label, SourceTypeSelectVM.Court);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Валидация преди запис на съд
        /// </summary>
        /// <param name="model"></param>
        void ValidateCourt(Court model)
        {
            if (string.IsNullOrEmpty(model.CourtAddress.CityCode))
            {
                ModelState.AddModelError("", "Въведете адрес");
            }
            if (model.CourtAddress.AddressTypeId <= 0)
            {
                ModelState.AddModelError("", "Въведете вид адрес");
            }
        }

        /// <summary>
        /// Запис на съд
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Edit(ICollection<IFormFile> imageFile, Court model, string returnUrl)
        {
            SetViewbag(returnUrl, model.Id);
            ValidateCourt(model);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            var currentId = model.Id;

            if (imageFile != null && imageFile.Count() > 0)
            {
                var file = imageFile.First();
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    model.CourtLogo = "data:" + (file.ContentType) + ";base64, " + Convert.ToBase64String(ms.ToArray());
                }
            }
            if (commonService.CourtSaveData(model))
            {
                AddAuditInfo(AuditConstants.Operations.Update, "Данни за текущия съд", model.Label, SourceTypeSelectVM.Court);
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id, returnUrl = returnUrl });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        void SetViewbag(string returnUrl, int courtId)
        {
            ViewBag.CourtTypeId_ddl = nomService.GetDropDownList<CourtType>();
            ViewBag.CourtRegionId_ddl = regionService.CourtRegionSelectDDL();
            ViewBag.returnUrl = returnUrl;
            if (returnUrl == "/")
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCurrentCourt(courtId, returnUrl).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForCourt(courtId, returnUrl).DeleteOrDisableLast();

            ViewBag.CountriesDDL = nomService.GetCountries();
            ViewBag.AddressTypesDDL = nomService.GetDropDownList<AddressType>();
            SetHelpFile(HelpFileValues.Nom9);
        }

        public IActionResult MigrateData(int courtId)
        {
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }


            var result = migrationDataService.MigrateForCourt(courtId);
            return Content(result);
        }
        //public IActionResult md_lg()
        //{
        //    if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
        //    {
        //        return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
        //    }

        //    int[] caseGroups = { 2 };
        //    var result = migrationDataService.MigrateLoadGroupLinkFromCourtType(8, 2, 6, 2, caseGroups);
        //    return Content(result);
        //}
        //public IActionResult MigrateLawyers()
        //{
        //    var result = migrationDataService.MigrateLawyers();
        //    return Content(result);
        //}

        public IActionResult StampCertificates()
        {
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }
            var filter = new CourtStampCertificateFilterVM();
            ViewBag.OrderMode_ddl = new List<SelectListItem>() {
            new SelectListItem("По име",""),
            new SelectListItem("Скоро изтичащи","expiring")
            };
            return View(filter);
        }

        [HttpPost]
        public IActionResult StampCertificates_ListData(IDataTablesRequest request, CourtStampCertificateFilterVM filter)
        {
            var data = stampService.Select(filter);
            return request.GetResponse(data);
        }

        public async Task<IActionResult> StampCertificateAdd(int courtId)
        {
            var court = await commonService.GetReadonlyAsync<Court>(courtId);

            ViewBag.courtId = court.Id;
            ViewBag.courtName = court.Label;

            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> StampCertificateAdd(int courtId, ICollection<IFormFile> file, string passHash, bool nocheck)
        {
            SaveResultVM saveResult = null;
            using (var ms = new MemoryStream())
            {
                file.FirstOrDefault().CopyTo(ms);
                saveResult = await stampService.AddCertificate(courtId, ms.ToArray(), passHash, nocheck);
            }
            return Json(saveResult);
        }

        [HttpPost]
        public IActionResult StampCertificate_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator))
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
            }


            if (commonService.SaveExpireInfo<CourtStampCertificate>(model))
            {
                //SetAuditContextDelete(docService, SourceTypeSelectVM.Document, model.LongId);
                SetSuccessMessage(MessageConstant.Values.DocumentExpireOK);
                return Json(new { result = true, redirectUrl = Url.Action(nameof(StampCertificates)) });
            }
            else
            {
                return Json(new { result = false, message = MessageConstant.Values.SaveFailed });
            }
        }
    }
}