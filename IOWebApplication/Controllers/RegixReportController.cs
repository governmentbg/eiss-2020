using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.Regix;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.RegixReport;
using Microsoft.AspNetCore.Mvc;
using Rotativa.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    /// <summary>
    /// Връзка с Regix
    /// </summary>
    public class RegixReportController : BaseController
    {
        private readonly IRegixReportService service;
        private readonly ICommonService commService;
        private readonly INomenclatureService nomService;
        private readonly ICdnService cdnService;


        public RegixReportController(IRegixReportService _service, ICommonService _commService,
            INomenclatureService _nomService, ICdnService _cdnService)
        {
            service = _service;
            commService = _commService;
            nomService = _nomService;
            cdnService = _cdnService;
        }

        public IActionResult Index()
        {
            //var test1 = service.GetStateOfPlay("1645824597854");
            //var test2 = service.FetchNomenclatures();
            var model = new RegixReportListFilterVM();
            var result = service.RegixReportList_Select(userContext.CourtId, model).ToList();
            return View();
        }

        void ValidateReason(RegixReportVM model)
        {
            if ((model.CaseId ?? 0) <= 0 && (model.DocumentId ?? 0) <= 0)
            {
                ModelState.AddModelError("", "Изберете поне едно от следните: Документ/Дело");
            }
        }

        private void SetRegixReportMainData(RegixReportVM model)
        {
            model.CourtId = userContext.CourtId;
            model.RegixRequestTypeId = NomenclatureConstants.RegixRequestTypes.FromReport;
        }

        #region PersonData
        /// <summary>
        /// Справка за физическо лице
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult PersonData(int? id)
        {
            RegixPersonDataVM model = null;
            if ((id ?? 0) > 0)
            {
                model = service.GetPersonalDataById(id ?? 0);
            }
            else
            {
                model = new RegixPersonDataVM();
                SetRegixReportMainData(model.Report);
            }

            SetHelpFile(HelpFileValues.Inquiry1);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка за физическо лице
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PersonData(RegixPersonDataVM model)
        {
            ValidateReason(model.Report);
            if (!ModelState.IsValid)
            {
                return View(nameof(PersonData), model);
            }
            var currentId = model.Report.Id;
            if (service.PersonData_SaveData(model))
            {
                await RegixReportSaveFile(NomenclatureConstants.RegixType.PersonData, model.Report.Id);
                this.SaveLogOperation(currentId == 0, model.Report.Id);
                SetSuccessMessage("Четенето премина успешно");
                return RedirectToAction(nameof(PersonData), new { id = model.Report.Id });
            }
            else
            {
                SetErrorMessage("Проблем при четене на данните");
            }
            return View(nameof(PersonData), model);
        }
        #endregion

        #region PersonAddress
        public void SetViewBagPersonAddress(int addressTypeId)
        {
            var addressType = service.GetById<RegixType>(addressTypeId);
            ViewBag.AddressTypeName = addressType.Label;
        }

        /// <summary>
        /// Справка за адрес
        /// </summary>
        /// <param name="addressTypeId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult PersonAddress(int addressTypeId, int? id)
        {
            SetViewBagPersonAddress(addressTypeId);
            RegixPersonAddressVM model = null;
            if ((id ?? 0) > 0)
            {
                model = service.GetPersonAddressById(id ?? 0);
            }
            else
            {
                model = new RegixPersonAddressVM();
                SetRegixReportMainData(model.Report);
                model.AddressTypeId = addressTypeId;
            }
            if (addressTypeId == NomenclatureConstants.RegixType.PersonPermanentAddress)
                SetHelpFile(HelpFileValues.Inquiry2);
            else if (addressTypeId == NomenclatureConstants.RegixType.PersonCurrentAddress)
                SetHelpFile(HelpFileValues.Inquiry3);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка за адрес
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PersonAddress(RegixPersonAddressVM model)
        {
            SetViewBagPersonAddress(model.AddressTypeId);
            ValidateReason(model.Report);
            if (!ModelState.IsValid)
            {
                return View(nameof(PersonAddress), model);
            }
            var currentId = model.Report.Id;
            if (service.PersonAddress_SaveData(model))
            {
                await RegixReportSaveFile(model.AddressTypeId, model.Report.Id);
                this.SaveLogOperation(currentId == 0, model.Report.Id);
                SetSuccessMessage("Четенето премина успешно");
                return RedirectToAction(nameof(PersonAddress), new { addressTypeId = model.AddressTypeId, id = model.Report.Id });
            }
            else
            {
                SetErrorMessage("Проблем при четене на данните");
            }
            return View(nameof(PersonAddress), model);
        }

        #endregion

        #region EmploymentContracts
        public void SetViewBagEmploymentContracts()
        {
            ViewBag.EmploymentContractsFilter_EikTypeId_ddl = commService.GetEnumSelectList<EikTypeTypeEmploymentContractsVM>();
            ViewBag.EmploymentContractsFilter_ContractsFilterTypeId_ddl = commService.GetEnumSelectList<ContractsFilterTypeEmploymentContractsVM>();
        }

        /// <summary>
        /// Справка трудови договори
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EmploymentContracts(int? id)
        {
            SetViewBagEmploymentContracts();
            RegixEmploymentContractsVM model = null;
            if ((id ?? 0) > 0)
            {
                model = service.GetEmploymentContractsById(id ?? 0);
            }
            else
            {
                model = new RegixEmploymentContractsVM();
                SetRegixReportMainData(model.Report);
            }
            SetHelpFile(HelpFileValues.Inquiry5);

            return View(model);
        }

        /// <summary>
        /// Извличане на Справка трудови договори
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EmploymentContracts(RegixEmploymentContractsVM model)
        {
            SetViewBagEmploymentContracts();
            ValidateReason(model.Report);
            if (!ModelState.IsValid)
            {
                return View(nameof(EmploymentContracts), model);
            }
            var currentId = model.Report.Id;
            if (service.EmploymentContracts_SaveData(model))
            {
                await RegixReportSaveFile(NomenclatureConstants.RegixType.EmploymentContracts, model.Report.Id);
                this.SaveLogOperation(currentId == 0, model.Report.Id);
                SetSuccessMessage("Четенето премина успешно");
                return RedirectToAction(nameof(EmploymentContracts), new { id = model.Report.Id });
            }
            else
            {
                SetErrorMessage("Проблем при четене на данните");
            }
            return View(nameof(EmploymentContracts), model);
        }
        #endregion

        #region CompensationByPaymentPeriod
        public void SetViewBagCompensationByPaymentPeriod()
        {
            ViewBag.CompensationByPaymentPeriodFilter_IdentifierTypeFilter_ddl = commService.GetEnumSelectList<IdentifierTypeCompensationByPaymentPeriodVM>();
        }

        /// <summary>
        /// Справка обезщетения
        /// </summary>
        /// <param name="compensationTypeId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CompensationByPaymentPeriod(int compensationTypeId, int? id)
        {
            SetViewBagCompensationByPaymentPeriod();
            RegixCompensationByPaymentPeriodVM model = null;
            if ((id ?? 0) > 0)
            {
                model = service.GetCompensationByPaymentPeriodById(id ?? 0);
            }
            else
            {
                var compensationType = service.GetById<RegixType>(compensationTypeId);

                model = new RegixCompensationByPaymentPeriodVM();
                SetRegixReportMainData(model.Report);
                model.CompensationTypeId = compensationTypeId;
                model.CompensationTypeName = compensationType.Label;
                model.CompensationByPaymentPeriodFilter.DateFromFilter = DateTime.Now;
                model.CompensationByPaymentPeriodFilter.DateToFilter = DateTime.Now;
            }

            if (compensationTypeId == NomenclatureConstants.RegixType.DisabilityCompensationByPaymentPeriod)
                SetHelpFile(HelpFileValues.Inquiry6);
            else if (compensationTypeId == NomenclatureConstants.RegixType.UnemploymentCompensationByPaymentPeriod)
                SetHelpFile(HelpFileValues.Inquiry7);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка обезщетения
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CompensationByPaymentPeriod(RegixCompensationByPaymentPeriodVM model)
        {
            SetViewBagCompensationByPaymentPeriod();
            ValidateReason(model.Report);
            if (!ModelState.IsValid)
            {
                return View(nameof(CompensationByPaymentPeriod), model);
            }
            var currentId = model.Report.Id;
            if (service.CompensationByPaymentPeriod_SaveData(model))
            {
                await RegixReportSaveFile(model.CompensationTypeId, model.Report.Id);
                this.SaveLogOperation(currentId == 0, model.Report.Id);
                SetSuccessMessage("Четенето премина успешно");
                return RedirectToAction(nameof(CompensationByPaymentPeriod), new { compensationTypeId = model.CompensationTypeId, id = model.Report.Id });
            }
            else
            {
                SetErrorMessage("Проблем при четене на данните");
            }
            return View(nameof(CompensationByPaymentPeriod), model);
        }
        #endregion

        #region PensionIncomeAmountReport
        public void SetViewBagPensionIncomeAmountReport()
        {
            ViewBag.PensionIncomeAmountFilter_IdentifierTypeFilter_ddl = commService.GetEnumSelectList<IdentifierTypePensionVM>();
        }

        /// <summary>
        /// Справка за доход от пенсия и добавки
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult PensionIncomeAmountReport(int? id)
        {
            SetViewBagPensionIncomeAmountReport();
            RegixPensionIncomeAmountVM model = null;
            if ((id ?? 0) > 0)
            {
                model = service.GetPensionIncomeAmountReportById(id ?? 0);
            }
            else
            {
                model = new RegixPensionIncomeAmountVM();
                SetRegixReportMainData(model.Report);
                model.PensionIncomeAmountFilter.DateFromFilter = DateTime.Now;
                model.PensionIncomeAmountFilter.DateToFilter = DateTime.Now;
            }
            SetHelpFile(HelpFileValues.Inquiry8);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка за доход от пенсия и добавки
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PensionIncomeAmountReport(RegixPensionIncomeAmountVM model)
        {
            SetViewBagPensionIncomeAmountReport();
            ValidateReason(model.Report);
            if (!ModelState.IsValid)
            {
                return View(nameof(PensionIncomeAmountReport), model);
            }
            var currentId = model.Report.Id;
            if (service.PensionIncomeAmountReport_SaveData(model))
            {
                await RegixReportSaveFile(NomenclatureConstants.RegixType.PensionIncomeAmount, model.Report.Id);
                this.SaveLogOperation(currentId == 0, model.Report.Id);
                SetSuccessMessage("Четенето премина успешно");
                return RedirectToAction(nameof(PensionIncomeAmountReport), new { id = model.Report.Id });
            }
            else
            {
                SetErrorMessage("Проблем при четене на данните");
            }
            return View(nameof(PensionIncomeAmountReport), model);
        }
        #endregion

        #region PersonalIdentityV2
        /// <summary>
        /// Справка за лице по документ за самоличност
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult PersonalIdentityV2(int? id)
        {
            RegixPersonalIdentityV2VM model = null;
            if ((id ?? 0) > 0)
            {
                model = service.GetPersonalIdentityV2ById(id ?? 0);
            }
            else
            {
                model = new RegixPersonalIdentityV2VM();
                SetRegixReportMainData(model.Report);
            }

            SetHelpFile(HelpFileValues.Inquiry9);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка за лице по документ за самоличност
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PersonalIdentityV2(RegixPersonalIdentityV2VM model)
        {
            ValidateReason(model.Report);
            if (!ModelState.IsValid)
            {
                return View(nameof(PersonalIdentityV2), model);
            }
            var currentId = model.Report.Id;
            if (service.PersonalIdentityV2_SaveData(model))
            {
                await RegixReportSaveFile(NomenclatureConstants.RegixType.PersonalIdentityV2, model.Report.Id);
                this.SaveLogOperation(currentId == 0, model.Report.Id);
                SetSuccessMessage("Четенето премина успешно");
                return RedirectToAction(nameof(PersonalIdentityV2), new { id = model.Report.Id });
            }
            else
            {
                SetErrorMessage("Проблем при четене на данните");
            }
            return View(nameof(PersonalIdentityV2), model);
        }
        #endregion

        #region ActualStateV3
        /// <summary>
        /// Справка търговски регистър
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult ActualStateV3(int? id)
        {
            RegixActualStateV3VM model = null;
            if ((id ?? 0) > 0)
            {
                (bool result, string errorMessage, RegixActualStateV3VM resultModel) = service.GetActualStateV3ById(id ?? 0);
                if (result == true)
                {
                    model = resultModel;
                }
                else
                {
                    model = new RegixActualStateV3VM();
                    SetRegixReportMainData(model.Report);
                    ModelState.AddModelError("", errorMessage);
                }
            }
            else
            {
                model = new RegixActualStateV3VM();
                SetRegixReportMainData(model.Report);
            }

            SetHelpFile(HelpFileValues.Inquiry10);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка търговски регистър
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ActualStateV3(RegixActualStateV3VM model)
        {
            ValidateReason(model.Report);
            if (!ModelState.IsValid)
            {
                return View(nameof(ActualStateV3), model);
            }
            var currentId = model.Report.Id;
            if (service.ActualStateV3_SaveData(model))
            {
                await RegixReportSaveFile(NomenclatureConstants.RegixType.ActualStateV3, model.Report.Id);
                this.SaveLogOperation(currentId == 0, model.Report.Id);
                SetSuccessMessage("Четенето премина успешно");
                return RedirectToAction(nameof(ActualStateV3), new { id = model.Report.Id });
            }
            else
            {
                SetErrorMessage("Проблем при четене на данните");
            }
            return View(nameof(ActualStateV3), model);
        }
        #endregion

        #region StateOfPlay
        /// <summary>
        /// Справка регистър БУЛСТАТ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult StateOfPlay(int? id)
        {
            RegixStateOfPlayVM model = null;
            if ((id ?? 0) > 0)
            {
                model = service.GetStateOfPlayById(id ?? 0);
            }
            else
            {
                model = new RegixStateOfPlayVM();
                SetRegixReportMainData(model.Report);
            }

            SetHelpFile(HelpFileValues.Inquiry11);
            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка регистър БУЛСТАТ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> StateOfPlay(RegixStateOfPlayVM model)
        {
            ValidateReason(model.Report);
            if (!ModelState.IsValid)
            {
                return View(nameof(StateOfPlay), model);
            }
            var currentId = model.Report.Id;
            if (service.StateOfPlay_SaveData(model))
            {
                await RegixReportSaveFile(NomenclatureConstants.RegixType.StateOfPlay, model.Report.Id);
                this.SaveLogOperation(currentId == 0, model.Report.Id);
                SetSuccessMessage("Четенето премина успешно");
                return RedirectToAction(nameof(StateOfPlay), new { id = model.Report.Id });
            }
            else
            {
                SetErrorMessage("Проблем при четене на данните");
            }
            return View(nameof(StateOfPlay), model);
        }
        #endregion

        /// <summary>
        /// Извличане на данни за лице
        /// </summary>
        /// <param name="uicType"></param>
        /// <param name="uic"></param>
        /// <param name="regixReasonDocumentId"></param>
        /// <param name="regixReasonCaseId"></param>
        /// <param name="regixReasonDescription"></param>
        /// <param name="regixReasonGuid"></param>
        /// <param name="regixRequestTypeId"></param>
        /// <returns></returns>
        public JsonResult PersonSearch(int uicType, string uic, long? regixReasonDocumentId, int? regixReasonCaseId, string regixReasonDescription, string regixReasonGuid, int? regixRequestTypeId)
        {
            AddAuditInfo("Преглед", $"Проверка за лице по идентификатор {uic}", "Проверка в НБД за имена на ФЛ и в ТР за наименование на ЮЛ");
            return Json(service.PersonSearch(uicType, uic, regixReasonDocumentId, regixReasonCaseId, regixReasonDescription, regixReasonGuid, regixRequestTypeId));
        }

        #region PersonDataAddress
        /// <summary>
        /// Справка за Лице + адреси
        /// </summary>
        /// <param name="addressTypeId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult PersonDataAddress(int? id)
        {
            RegixPersonDataAddressVM model = null;
            if ((id ?? 0) > 0)
            {
                model = service.GetPersonDataAddressById(id ?? 0);
            }
            else
            {
                model = new RegixPersonDataAddressVM();
                SetRegixReportMainData(model.Report);
            }
            SetHelpFile(HelpFileValues.Inquiry4);

            return View(model);
        }

        /// <summary>
        /// Извличане на данни Справка за лице + адреси
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PersonDataAddress(RegixPersonDataAddressVM model)
        {
            ValidateReason(model.Report);
            if (!ModelState.IsValid)
            {
                return View(nameof(PersonDataAddress), model);
            }
            var currentId = model.Report.Id;
            if (service.PersonDataAddress_SaveData(model))
            {
                await RegixReportSaveFile(NomenclatureConstants.RegixType.PersonDataAddress, model.Report.Id);
                this.SaveLogOperation(currentId == 0, model.Report.Id);
                SetSuccessMessage("Четенето премина успешно");
                return RedirectToAction(nameof(PersonDataAddress), new { id = model.Report.Id });
            }
            else
            {
                SetErrorMessage("Проблем при четене на данните");
            }
            return View(nameof(PersonDataAddress), model);
        }

        /// <summary>
        /// Експорт Справка за лице + адреси
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> PreviewPdfPersonDataAddress(int id)
        {
            RegixPersonDataAddressVM model = service.GetPersonDataAddressById(id);
            string html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_PersonDataAddressResponse.cshtml", model, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);
            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, id.ToString() + ".pdf");
        }
        #endregion

        /// <summary>
        /// Справка външни регистри
        /// </summary>
        /// <returns></returns>
        public IActionResult RegixReportList()
        {
            ViewBag.RegixTypeId_ddl = nomService.GetDropDownList<RegixType>();
            ViewBag.RegixRequestTypeId_ddl = nomService.GetDropDownList<RegixRequestType>();

            var model = new RegixReportListFilterVM();
            model.DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            model.DateTo = DateTime.Now;

            return View(model);
        }

        /// <summary>
        /// Извличане на данни за справка търсене външни регистри
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataRegixReportList(IDataTablesRequest request, RegixReportListFilterVM model)
        {
            var data = service.RegixReportList_Select(userContext.CourtId, model);

            return request.GetResponse(data);
        }

        private async Task<byte[]> RegixReportSaveFile(int regixTypeId, int id)
        {
            string html = "";

            switch (regixTypeId)
            {
                case NomenclatureConstants.RegixType.PersonData:
                    RegixPersonDataVM modelPerson = service.GetPersonalDataById(id);
                    html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_PersonDataResponse.cshtml", modelPerson, true);
                    break;
                case NomenclatureConstants.RegixType.PersonPermanentAddress:
                case NomenclatureConstants.RegixType.PersonCurrentAddress:
                    RegixPersonAddressVM modelAddress = service.GetPersonAddressById(id);
                    html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_PersonAddressResponse.cshtml", modelAddress, true);
                    break;
                case NomenclatureConstants.RegixType.EmploymentContracts:
                    RegixEmploymentContractsVM modelEmployment = service.GetEmploymentContractsById(id);
                    html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_EmploymentContractsResponse.cshtml", modelEmployment, true);
                    break;
                case NomenclatureConstants.RegixType.DisabilityCompensationByPaymentPeriod:
                case NomenclatureConstants.RegixType.UnemploymentCompensationByPaymentPeriod:
                    RegixCompensationByPaymentPeriodVM modelCompensation = service.GetCompensationByPaymentPeriodById(id);
                    html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_CompensationByPaymentPeriodResponse.cshtml", modelCompensation, true);
                    break;
                case NomenclatureConstants.RegixType.PensionIncomeAmount:
                    RegixPensionIncomeAmountVM modelPension = service.GetPensionIncomeAmountReportById(id);
                    html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_PensionIncomeAmountReportResponse.cshtml", modelPension, true);
                    break;
                case NomenclatureConstants.RegixType.PersonalIdentityV2:
                    RegixPersonalIdentityV2VM modelIdentityV2 = service.GetPersonalIdentityV2ById(id);
                    html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_PersonalIdentityV2Response.cshtml", modelIdentityV2, true);
                    break;
                case NomenclatureConstants.RegixType.ActualStateV3:
                    (bool result, string errorMessage, RegixActualStateV3VM modelActualStateV3) = service.GetActualStateV3ById(id);
                    html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_ActualStateV3Response.cshtml", modelActualStateV3, true);
                    break;
                case NomenclatureConstants.RegixType.StateOfPlay:
                    RegixStateOfPlayVM modelState = service.GetStateOfPlayById(id);
                    html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_StateOfPlayResponse.cshtml", modelState, true);
                    break;
                case NomenclatureConstants.RegixType.PersonDataAddress:
                    RegixPersonDataAddressVM modelPersonDataAddress = service.GetPersonDataAddressById(id);
                    html = await this.RenderPartialViewAsync("~/Views/RegixReport/", "_PersonDataAddressResponse.cshtml", modelPersonDataAddress, true);
                    break;
                default:
                    break;
            }

            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.RegixReport,
                SourceId = id.ToString(),
                FileName = "regixreport.pdf",
                ContentType = "application/pdf",
                Title = "Справка",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            return pdfBytes;
        }

        /// <summary>
        /// Печат на PDF
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> PrintPdf(int id)
        {
            var model = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.RegixReport, SourceId = id.ToString() });
            if (model == null)
            {
                //Ако няма записан файл(записват се от един момент натам) да създаде
                var regix = service.GetById<RegixReport>(id);
                await RegixReportSaveFile(regix.RegixTypeId, id);
                model = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.RegixReport, SourceId = id.ToString() });
            }

            return File(Convert.FromBase64String(model.FileContentBase64), model.ContentType, model.FileName);
        }
    }
}