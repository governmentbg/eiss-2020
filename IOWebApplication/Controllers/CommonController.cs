using System;
using System.Collections.Generic;
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
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class CommonController : BaseController
    {
        private readonly ICommonService service;
        private readonly INomenclatureService nomenclatureService;

        public CommonController(ICommonService _service,
                                INomenclatureService _nomenclatureService)
        {
            service = _service;
            nomenclatureService = _nomenclatureService;
        }

        /// <summary>
        /// Страница с адреси
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexAddress()
        {
            ViewBag.AddressTypeId_ddl = nomenclatureService.GetDropDownList<AddressType>();
            ViewBag.CountryCode_ddl = nomenclatureService.GetCountries();
            var model = new AddressFilterVM()
            {
                CountryCode = NomenclatureConstants.CountryBG
            };
            return View(model);
        }

        /// <summary>
        /// Метод за извличане на данни за адреси
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataAddress(IDataTablesRequest request, AddressFilterVM model)
        {
            var data = service.Address_Select(model);
            return request.GetResponse(data);
        }

        private void SetViewbagAddress()
        {
            ViewBag.AddressTypesDDL = nomenclatureService.GetDropDownList<AddressType>();
            ViewBag.CountriesDDL = nomenclatureService.GetCountries();
        }

        /// <summary>
        /// Валидация при добавяне на адрес
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidAddress(Address model)
        {
            if (model.AddressTypeId < 1)
                return "Изберете вид адрес";

            if (string.IsNullOrEmpty(model.CountryCode))
                return "Изберете държава";

            if (string.IsNullOrEmpty(model.CityCode))
                return "Изберете населено място";

            return string.Empty;
        }

        /// <summary>
        /// Добавяне на нов адрес
        /// </summary>
        /// <returns></returns>
        public IActionResult AddAddress()
        {
            SetViewbagAddress();
            var model = new Address()
            {
                CountryCode = NomenclatureConstants.CountryBG
            };
            return View(nameof(EditAddress), model);
        }

        /// <summary>
        /// Метод за редакция на адрес
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditAddress(long id)
        {
            SetViewbagAddress();
            var model = service.GetById<Address>(id);
            return View(nameof(EditAddress), model);
        }

        /// <summary>
        /// Запис на адрес
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditAddress(Address model)
        {
            SetViewbagAddress();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditAddress), model);
            }

            string _isvalid = IsValidAddress(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditAddress), model);
            }

            var currentId = model.Id;
            if (service.Address_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditAddress), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditAddress), model);
        }

        /// <summary>
        /// Страница с банкови сметки към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult BankAccount(int sourceType, long sourceId)
        {
            ViewBag.sourceType = sourceType;
            ViewBag.sourceId = sourceId;
            ViewBag.breadcrumbs = service.BankAccount_LoadBreadCrumbs(sourceType, sourceId);

            return View();
        }

        /// <summary>
        /// Извличане на данните за банкови сметки към обект
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BankAccountListData(IDataTablesRequest request, int sourceType, long sourceId)
        {
            var data = service.BankAccount_Select(sourceType, sourceId);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на банкови сметки към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult AddBankAccount(int sourceType, long sourceId)
        {
            SetViewBagBankAccount(sourceType, sourceId);
            BankAccountEditVM model = new BankAccountEditVM();
            model.SourceType = sourceType;
            model.SourceId = sourceId;
            return View(nameof(EditBankAccount), model);
        }

        /// <summary>
        /// Редакция на банкови сметки към обект
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditBankAccount(int id)
        {
            var model = service.BankAccount_GetById(id);
            SetViewBagBankAccount(model.SourceType, model.SourceId);
            return View(nameof(EditBankAccount), model);
        }

        /// <summary>
        /// Запис на банкови сметки към обект
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditBankAccount(BankAccountEditVM model)
        {
            if (!ModelState.IsValid)
            {
                SetViewBagBankAccount(model.SourceType, model.SourceId);
                return View(nameof(EditBankAccount), model);
            }
            if (service.BankAccount_SaveData(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditBankAccount), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetViewBagBankAccount(model.SourceType, model.SourceId);
            return View(nameof(EditBankAccount), model);
        }

        private void SetViewBagBankAccount(int sourceType, long sourceId)
        {
            ViewBag.breadcrumbs = service.BankAccount_LoadBreadCrumbsAddEdit(sourceType, sourceId);
        }

        /// <summary>
        /// Темплейти за статистиката
        /// </summary>
        /// <returns></returns>
        public IActionResult ExcelReportTemplate()
        {
            return View();
        }

        /// <summary>
        /// Извличане на данните за темплйтите за статистиката
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExcelReportTemplateListData(IDataTablesRequest request)
        {
            var data = service.ExcelReportTemplate_Select();

            return request.GetResponse(data);
        }
        private void SetViewBagExcelReportTemplate()
        {
            ViewBag.CourtTypeId_ddl = nomenclatureService.GetDropDownList<CourtType>();
            ViewBag.ReportTypeId_ddl = nomenclatureService.GetDDL_ExcelReportTemplateReportType(true, false);
        }

        /// <summary>
        /// Добавяне на темплейт
        /// </summary>
        /// <returns></returns>
        public IActionResult AddExcelReportTemplate()
        {
            SetViewBagExcelReportTemplate();
            ExcelReportTemplate model = new ExcelReportTemplate();
            return View(nameof(EditExcelReportTemplate), model);
        }

        public IActionResult EditExcelReportTemplate(int id)
        {
            var model = service.GetById<ExcelReportTemplate>(id);
            SetViewBagExcelReportTemplate();
            return View(nameof(EditExcelReportTemplate), model);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult EditExcelReportTemplate(ICollection<IFormFile> files, ExcelReportTemplate model)
        {
            SetViewBagExcelReportTemplate();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditExcelReportTemplate));
            }

            if (model.Id < 1)
            {
                if (files == null || files.Count() < 1)
                {
                    SetErrorMessage("Няма избран файл.");
                    return View(nameof(EditExcelReportTemplate), new { model });
                }
            }

            var currentId = model.Id;
            if (service.ExcelReportTemplate_SaveData(files, model))
            {
                this.SaveLogOperation(currentId == 0, model.Id, null, nameof(EditExcelReportTemplate));
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditExcelReportTemplate), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditExcelReportTemplate), model);
        }

        /// <summary>
        /// Сваляне на файл
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<FileResult> DownloadFile(int id)
        {
            var model = await service.GetReadonlyAsync<ExcelReportTemplate>(id);
            return File(model.Content, model.ContentType, model.FileName);
        }

        #region FilterTemplates

        /// <summary>
        /// Страница с шаблони за филтри за справки
        /// </summary>
        /// <param name="FilterTemplateTypeId">Тип шаблон</param>
        /// <returns></returns>
        public IActionResult IndexFilterTemplates(int FilterTemplateTypeId)
        {
            var model = new FilterTemplatesFilterVM()
            {
                FilterTemplateTypeId = FilterTemplateTypeId
            };
            return View(model);
        }

        /// <summary>
        /// Страница с шаблони за филтри за специализирана справка
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexFilterTemplatesSpecializedReport()
        {
            var model = new FilterTemplatesFilterVM()
            {
                FilterTemplateTypeId = NomenclatureConstants.FilterTemplateTypeConstants.SpecializedReport
            };
            return View("IndexFilterTemplates", model);
        }

        /// <summary>
        /// Метод за извличане на данни за шаблони за филтри за справки
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListDataFilterTemplates(IDataTablesRequest request, FilterTemplatesFilterVM filter)
        {
            var data = service.GetFilterTemplates_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на нов шаблон за филтър на специализирана справка
        /// </summary>
        /// <returns></returns>
        public IActionResult AddFilterTemplatesSpecializedReport()
        {
            SetViewbagFilterTemplatesSpecializedReport();
            var model = new FilterTemplatesSpecializedReportEditVM()
            {
                FilterTemplateTypeId = NomenclatureConstants.FilterTemplateTypeConstants.SpecializedReport,
                IsActive = true,
                SpecializedReportFilter = new SpecializedReportFilterVM()
                {
                    DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                    DateTo = DateTime.Now
                }
            };
            return View(nameof(EditFilterTemplatesSpecializedReport), model);
        }

        /// <summary>
        /// Метод за редакция на шаблон за филтър на специализирана справка
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<IActionResult> EditFilterTemplatesSpecializedReport(int id)
        {
            SetViewbagFilterTemplatesSpecializedReport();
            FilterTemplatesSpecializedReportEditVM model = await service.GetFilterTemplatesSpecializedReportById(id);
            return View(nameof(EditFilterTemplatesSpecializedReport), model);
        }

        /// <summary>
        /// Запис на шаблон за филтър на специализирана справка
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditFilterTemplatesSpecializedReport(FilterTemplatesSpecializedReportEditVM model)
        {
            SetViewbagFilterTemplatesSpecializedReport();
            if (!ModelState.IsValid)
            {
                return View(nameof(EditFilterTemplatesSpecializedReport), model);
            }

            string _isvalid = IsValidFilterTemplatesSpecializedReport(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditFilterTemplatesSpecializedReport), model);
            }

            var currentId = model.Id;
            int? saveId = await service.SaveFilterTemplatesSpecializedReport(model);
            if (saveId != null)
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditFilterTemplatesSpecializedReport), new { id = saveId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return View(nameof(EditFilterTemplatesSpecializedReport), model);
        }

        /// <summary>
        /// Зареждане на номенклатурни листове за добавяне/редакция на шаблон за филтър на специализирана справка
        /// </summary>
        private void SetViewbagFilterTemplatesSpecializedReport()
        {
            ViewBag.SpecializedReportFilter_CourtId_ddl = nomenclatureService.GetDropDownList<Court>();
            ViewBag.SpecializedReportFilter_InstanceId_ddl = nomenclatureService.GetDropDownList<CaseInstance>();
            ViewBag.SpecializedReportFilter_CaseClassificationIds_ddl = nomenclatureService.GetDropDownList<Classification>();
            ViewBag.SpecializedReportFilter_CaseGroupIds_ddl = nomenclatureService.GetDropDownList<CaseGroup>();
            ViewBag.SpecializedReportFilter_ActComplainResultId_ddl = nomenclatureService.GetDropDownList<ActComplainResult>();
            ViewBag.SpecializedReportFilter_CaseStateId_ddl = nomenclatureService.GetDropDownList<CaseState>();
        }

        /// <summary>
        /// Валидация при добавяне/редакция на шаблон за филтър на специализирана справка
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private string IsValidFilterTemplatesSpecializedReport(FilterTemplatesSpecializedReportEditVM model)
        {
            if (string.IsNullOrEmpty(model.Label))
                return "Въведете име на шаблон";

            if ((model.SpecializedReportFilter.DateTo - model.SpecializedReportFilter.DateFrom).Days > 365)
                return "Периода на справката е по-голям от година";

            return string.Empty;
        }

        #endregion
    }
}
