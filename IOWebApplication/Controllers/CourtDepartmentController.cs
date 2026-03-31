using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class CourtDepartmentController : BaseController
    {
        private readonly ICourtDepartmentService service;
        private readonly INomenclatureService nomService;

        public CourtDepartmentController(ICourtDepartmentService _service, INomenclatureService _nomService)
        {
            service = _service;
            nomService = _nomService;
        }

        [TitleAudit(Operation = AuditConstants.Operations.List)]
        public IActionResult Index()
        {
            SetHelpFile(HelpFileValues.Nom11);
            return View();
        }

        /// <summary>
        /// Зареждане на списъка
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = service.CourtDepartment_Select(userContext.CourtId, request.Search?.Value);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне на елемент в CourtDepartment
        /// </summary>
        /// <returns></returns>
        [DisableAudit]
        public IActionResult Add()
        {
            var model = new CourtDepartment()
            {
                CourtId = userContext.CourtId,
                DateFrom = DateTime.Now
            };
            SetViewbag();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редактиране на елемент от CourtDepartment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<CourtDepartment>(id);
            model.ParentId = model.ParentId ?? 0;
            //model.DateFrom = (model.DateFrom < new DateTime(2000, 1, 1)) ? DateTime.Now.AddYears(-1) : model.DateFrom;
            SetViewbag();
            auditInfo(AuditConstants.Operations.View, model);
            return View(nameof(Edit), model);
        }

        private string IsValid(CourtDepartment model)
        {
            if (model.DepartmentTypeId < 0)
            {
                return "Няма избрано ниво";
            }

            if (model.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie)
            {
                if (model.ParentId == null)
                {
                    return "Няма избрано горно ниво";
                }
            }

            var parent = (model.ParentId != null) ? service.GetById<CourtDepartment>(model.ParentId) : null;

            switch (model.DepartmentTypeId)
            {
                case NomenclatureConstants.DepartmentType.Napravlenie:
                    {
                        //Всички направления са предварително подготвени и няма възможност за добавяне на нови
                        return "Не можете да добавяте направления.";

                        //if (model.ParentId != null)
                        //{
                        //    return "За направление горното ниво трябва да е текущият съд";
                        //}
                    }
                case NomenclatureConstants.DepartmentType.Kolegia:
                    {
                        if (model.ParentId == null)
                            return "Горното ниво на колегията трябва да е направление";

                        if (parent.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie)
                            return "Горното ниво на колегията трябва да е направление";
                    }
                    break;
                case NomenclatureConstants.DepartmentType.Otdelenie:
                    {
                        if (model.ParentId == null)
                            return "Горното ниво на отделението трябва да е направление/колегия";

                        if ((parent.DepartmentTypeId == NomenclatureConstants.DepartmentType.Systav) ||
                            (parent.DepartmentTypeId == NomenclatureConstants.DepartmentType.Otdelenie))
                            return "Горното ниво на отделението трябва да е направление/колегия";
                    }
                    break;
                case NomenclatureConstants.DepartmentType.Systav:
                    {
                        if (model.ParentId == null)
                            return "Горното ниво на състава трябва да е направление/колегия/отделение";

                        if (parent.DepartmentTypeId == NomenclatureConstants.DepartmentType.Systav)
                            return "Горното ниво на състава трябва да е направление/колегия/отделение";
                    }
                    break;
            }

            return string.Empty;
        }

        /// <summary>
        /// Запис
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisableAudit]
        [HttpPost]
        public IActionResult Edit(CourtDepartment model)
        {
            if (model.ParentId == 0)
            {
                model.ParentId = null;
            }
            SetViewbag();
            if (!ModelState.IsValid)
            {
                model.ParentId = model.ParentId ?? 0;
                return View(nameof(Edit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                model.ParentId = model.ParentId ?? 0;
                return View(nameof(Edit), model);
            }

            var currentId = model.Id;
            if (service.CourtDepartment_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                if (currentId == 0)
                {
                    auditInfo(AuditConstants.Operations.Append, model);
                }
                else
                {
                    auditInfo(AuditConstants.Operations.Update, model);
                }
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            model.ParentId = model.ParentId ?? 0;
            return View(nameof(Edit), model);
        }

        void auditInfo(string operation, CourtDepartment model, string add = "")
        {
            if (model != null)
            {
                var depType = service.GetPropById<DepartmentType, string>(x => x.Id == model.DepartmentTypeId, x => x.Label);

                AddAuditInfo(operation, $"{model.Label} ({depType})", add, "Съдебна структура");
            }
        }
        /// <summary>
        /// Попълване на данните за комбо боксовете
        /// </summary>
        void SetViewbag()
        {
            ViewBag.ParentId_ddl = service.GetDropDownList(userContext.CourtId);
            ViewBag.DepartmentTypeId_ddl = nomService.GetDropDownList<DepartmentType>();
            var selectListItems = nomService.GetDDL_ByCourtTypeInstanceList(userContext.CourtInstances, false, false);
            ViewBag.HasInstance = selectListItems.Count > 1;
            ViewBag.CaseInstanceId_ddl = selectListItems;
            SetHelpFile(HelpFileValues.Nom11);
        }
        [DisableAudit]
        public IActionResult AddLawUnits(int id)
        {
            var department = service.GetById<CourtDepartment>(id);

            if (department.DepartmentTypeId != NomenclatureConstants.DepartmentType.Systav)
            {
                if (userContext.CourtTypeId == NomenclatureConstants.CourtType.VKS)
                    return RedirectToAction(nameof(IndexLawUnit), new { id = id });
                else
                    return RedirectToAction(nameof(LawUnits), new { id = id });
            }
            else
                return RedirectToAction(nameof(IndexLawUnit), new { id = id });
        }

        public IActionResult IndexLawUnit(int id)
        {
            var department = service.GetById<CourtDepartment>(id);
            ViewBag.courtDepartmentId = id;
            ViewBag.DepName = department.Label;

            auditInfo(AuditConstants.Operations.View, department, "Преглед на съдии");
            return View();
        }

        [HttpPost]
        public IActionResult ListDataLawUnit(IDataTablesRequest request, int id)
        {
            var data = service.CourtDepartmentLawUnit_Select(id);
            return request.GetResponse(data);
        }

        /// <summary>
        /// Попълване на модел и извикване на вю със списък с чекове
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult LawUnits(int id)
        {
            ViewBag.backUrl = Url.Action("Index", "CourtDepartment");
            SetHelpFile(HelpFileValues.Nom11);
            var department = service.GetById<CourtDepartment>(id);
            auditInfo(AuditConstants.Operations.View, department, "Избор на  съдии");
            return View("CheckListViewVM", service.CheckListViewVM_Fill(userContext.CourtId, id));
        }

        /// <summary>
        /// Записа на данните от списъчното вю с чекове в CourtDepartmentLawUnit
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableAudit]
        public IActionResult LawUnits(CheckListViewVM model)
        {

            if (service.CourtDepartmentLawUnit_SaveData(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                var department = service.GetById<CourtDepartment>(model.ObjectId);
                auditInfo(AuditConstants.Operations.Update, department, "Избор на съдии");
            }
            else
                SetErrorMessage(MessageConstant.Values.SaveFailed);

            ViewBag.backUrl = Url.Action("Index", "CourtDepartment");
            return View("CheckListViewVM", model);
        }

        void SetViewbagLawUnit(int courtDepartpentId)
        {
            var department = service.GetById<CourtDepartment>(courtDepartpentId);
            ViewBag.courtDepartmentId = courtDepartpentId;
            ViewBag.depName = department.Label;

            ViewBag.JudgeDepartmentRoleId_ddl = nomService.GetDropDownList<JudgeDepartmentRole>();
            SetHelpFile(HelpFileValues.Nom11);
        }

        [DisableAudit]
        public IActionResult AddLawUnit(int CourtDepartmentId)
        {
            var model = new CourtDepartmentLawUnit()
            {
                CourtDepartmentId = CourtDepartmentId,
                //DateFrom = DateTime.Now
            };
            SetViewbagLawUnit(CourtDepartmentId);
            return View(nameof(EditLawUnit), model);
        }

        public IActionResult EditLawUnit(int id)
        {
            var model = service.GetById<CourtDepartmentLawUnit>(id);
            SetViewbagLawUnit(model.CourtDepartmentId);

            var department = service.GetById<CourtDepartment>(model.CourtDepartmentId);
            var lawUnit = service.GetPropById<LawUnit, string>(x => x.Id == model.LawUnitId, x => x.FullName);
            auditInfo(AuditConstants.Operations.View, department, lawUnit);
            return View(nameof(EditLawUnit), model);
        }

        /// <summary>
        /// Запис
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableAudit]
        public IActionResult EditLawUnit(CourtDepartmentLawUnit model)
        {
            SetViewbagLawUnit(model.CourtDepartmentId);
            if (!ModelState.IsValid)
            {
                return View(nameof(EditLawUnit), model);
            }

            string _isvalid = IsValid(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditLawUnit), model);
            }

            var currentId = model.Id;
            if (service.CourtDepartmentLawUnit_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                var department = service.GetById<CourtDepartment>(model.CourtDepartmentId);
                var lawUnit = service.GetPropById<LawUnit, string>(x => x.Id == model.LawUnitId, x => x.FullName);
                auditInfo((currentId == 0) ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, department, lawUnit);


                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(EditLawUnit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(nameof(EditLawUnit), model);
        }

        private string IsValid(CourtDepartmentLawUnit model)
        {
            if (model.LawUnitId < 0)
            {
                return "Няма избрано лице";
            }

            if ((model.JudgeDepartmentRoleId ?? 0) < 1)
            {
                return "Няма избрана роля";
            }

            var courtDepartmentLaws = service.CourtDepartmentLawUnit_Select(model.CourtDepartmentId).ToList();

            if (courtDepartmentLaws.Any(x => (x.LawUnitId == model.LawUnitId) &&
                                             ((model.Id > 0) ? x.Id != model.Id : true)))
            {
                return "Този съдия е добавен";
            }

            if (courtDepartmentLaws.Any(x => (x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel) &&
                                             ((model.Id > 0) ? x.Id != model.Id : true)) && model.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
            {
                return "Има избран председател";
            }

            return string.Empty;
        }

        [HttpPost]
        [DisableAudit]
        public JsonResult StornoLawUnit(int LawUnitId)
        {
            bool result = service.StornoCourtDepartment(LawUnitId);
            if (result)
            {
                var cdLawunit = service.GetById<CourtDepartmentLawUnit>(LawUnitId);
                var department = service.GetById<CourtDepartment>(cdLawunit.CourtDepartmentId);
                var luName = service.GetPropById<LawUnit, string>(x => x.Id == cdLawunit.LawUnitId, x => x.FullName);
                auditInfo(AuditConstants.Operations.Delete, department, $"Премахнат съдия: {luName}");
            }
            return Json(new { result = result });
        }

        /// <summary>
        /// Зареждане на съдебна структура за падащ списък
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        public async Task<IActionResult> GetCourtDepartmentCentralDistributionCCDDL(int courtId)
        {
            return Json(await service.Department_SelectDDLAsync(courtId, NomenclatureConstants.DepartmentType.Systav, null, NomenclatureConstants.CaseGroups.GrajdanskoDelo));
        }

        /// <summary>
        /// Зареждане на съдебна структура за падащ списък
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="lawUnitId">Идентификатор на лице</param>
        /// <returns></returns>
        public async Task<IActionResult> GetDDL_DepartmentByLawUnit(int courtId, int lawUnitId)
        {
            return Json(await service.GetDDL_DepartmentByLawUnit(courtId, lawUnitId, NomenclatureConstants.DepartmentType.Systav));
        }
    }
}