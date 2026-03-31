using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace IOWebApplication.Controllers
{
    public class CourtDutyController : BaseController
    {
        private readonly ICourtDutyService service;

        public CourtDutyController(ICourtDutyService _service)
        {
            service = _service;
        }

        /// <summary>
        /// Страница с дежурства
        /// </summary>
        /// <param name="kind">Вид: null - дежурство, 1 - заместване</param>
        /// <returns></returns>
        public IActionResult Index()
        {
            SetHelpFile(HelpFileValues.Nom8);
            addToAudit(AuditConstants.Operations.List, null);
            CourtDutyFilterVM filter = new()
            {
                CourtId = userContext.CourtId
            };

            return View(filter);
        }

        /// <summary>
        /// Извличане на данните за дежурства
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, CourtDutyFilterVM filter)
        {
            var data = service.CourtDuty_Select(filter);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на дежурство/заместване
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            var model = new CourtDuty()
            {
                CourtId = userContext.CourtId,
                CheckCourtDutyLawUnits = service.FillCheckListVMs(userContext.CourtId, null).ToList()
            };
            SetHelpFile(HelpFileValues.Nom8);
            addToAudit(AuditConstants.Operations.View, null);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на дежурство
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetCourtDuty_ById(id);
            SetHelpFile(HelpFileValues.Nom8);
            addToAudit(AuditConstants.Operations.View, model);
            return View(nameof(Edit), model);
        }

        private string IsValid(CourtDuty model)
        {
            if (!model.CheckCourtDutyLawUnits.Any(x => x.Checked))
            {
                return "Няма избрани съдии.";
            }

            if (model.DateTo != null && model.DateTo < model.DateFrom)
                return "Дата до не може да е по малка от Дата от";

            return string.Empty;
        }

        /// <summary>
        /// Запис на дежурство
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(CourtDuty model)
        {
            SetHelpFile(HelpFileValues.Nom8);
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CourtDuty_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                if (currentId == 0)
                {
                    addToAudit(AuditConstants.Operations.Append, model);
                }
                else
                {
                    addToAudit(AuditConstants.Operations.Update, model);
                }
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction("Index");
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Зареждане на лица за избор с CheckList
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult LawUnits(int id)
        {
            ViewBag.backUrl = Url.Action("Index", "CourtDuty");
            return View("CheckListViewVM", service.CheckListViewVM_Fill(userContext.CourtId, id));
        }

        /// <summary>
        /// Запис на избрани лица от CheckList
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult LawUnits(CheckListViewVM model)
        {
            if (service.CourtDutyLawUnit_SaveData(model))
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            ViewBag.backUrl = Url.Action("Index", "CourtDuty");
            return View("CheckListViewVM", model);
        }

        void addToAudit(string operation, CourtDuty model)
        {
            var baseInfo = string.Empty;
            var addInfo = string.Empty;
            var operationType = $"Дежурство към съд";

            if (model?.Id > 0)
            {
                baseInfo = model.Label;
                addInfo = $"От {model.DateFrom:dd.MM.yyyy} до {model.DateTo:dd.MM.yyyy}";
            }

            AddAuditInfo(operation, baseInfo, addInfo, operationType);
        }
    }
}