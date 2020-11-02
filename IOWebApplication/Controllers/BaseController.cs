// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Audit.Mvc;
using IO.LogOperation.Models;
using IO.LogOperation.Service;
using IOWebApplication.Components;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Net.Http;

namespace IOWebApplication.Controllers
{
    [Authorize]
    [Audit(EventTypeName = "{controller}/{action} ({verb})",
        IncludeHeaders = false,
        IncludeModel = true,
        IncludeRequestBody = false,
        IncludeResponseBody = false)]
    public class BaseController : Controller
    {
        private IUserContext _userContext;
        protected const string TempData_CurrentContext = "CurrentContext";

        protected IUserContext userContext
        {
            get
            {
                if (_userContext == null)
                {
                    _userContext = (IUserContext)HttpContext
                         .RequestServices
                         .GetService(typeof(IUserContext));
                }



                return _userContext;
            }
        }

        public string LanguageCode = "bg";

        protected void SaveLogOperation(bool isInsert, object objectKey, object masterKey = null, string actionName = null)
        {
            SaveLogOperation((isInsert) ? OperationTypes.Insert : OperationTypes.Update, objectKey, masterKey, actionName);
        }
        protected void SaveLogOperation(OperationTypes operation, object objectKey, object masterKey = null, string actionName = null)
        {
            if (Request.Form["hfContainer"].FirstOrDefault() != null)
            {
                var html = Request.Form["hfContainer"].FirstOrDefault();
                SaveLogOperation(this.ControllerName?.ToLower(), actionName ?? this.ActionName, html, operation, objectKey, masterKey);
            }
        }

        protected void SaveLogOperation(string controllerName, string actionName, string html, OperationTypes operation, object objectKey, object masterKey = null)
        {
            ILogOperationService<ApplicationDbContext> logOperation =
                (ILogOperationService<ApplicationDbContext>)HttpContext
                .RequestServices
                .GetService(typeof(ILogOperationService<ApplicationDbContext>));

            logOperation.Save(controllerName.ToLower(), logOperation.MakeActionName(actionName, ActionTransformType.AddToEdit)?.ToLower(), objectKey.ToString(), operation, html, userContext.LogName, userContext.UserId, masterKey?.ToString());
        }

        public string ActionName { get; set; }
        public string ControllerName { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            Audit.Core.Configuration.DataProvider = (IOAuditLogDataProvider)HttpContext
                .RequestServices.GetService(typeof(IOAuditLogDataProvider));
            Audit.Core.Configuration.CreationPolicy = Audit.Core.EventCreationPolicy.Manual;

            /*
             *      Управление на активния елемент на менюто
             *      ViewBag.MenuItemValue съдържа ключовата дума, отговорна за отварянето на менюто
             *      Ако не намери атрибут на action-а MenuItem("{keyword}"), се използва името на action-а
             *      Ако action-а е от вида List_Edit се подава list (отрязва до последния символ долна подчертавка)
             */
            ControllerActionDescriptor controllerActionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;

            if (controllerActionDescriptor != null)
            {
                ActionName = controllerActionDescriptor.ActionName;
                ControllerName = controllerActionDescriptor.ControllerName;
                object currentMenuItem = null;
                var menuAttrib = controllerActionDescriptor
                                    .MethodInfo
                                    .CustomAttributes
                                    .FirstOrDefault(a => a.AttributeType == typeof(MenuItemAttribute));
                if (menuAttrib != null)
                {
                    currentMenuItem = menuAttrib.ConstructorArguments[0].Value;
                }
                if (currentMenuItem == null)
                {
                    var actionName = controllerActionDescriptor.ActionName;
                    if (actionName.Contains('_'))
                    {
                        currentMenuItem = actionName.Substring(0, actionName.LastIndexOf('_')).ToLower();
                    }
                    else
                    {
                        currentMenuItem = actionName.ToLower();
                    }
                }
                ViewBag.MenuItemValue = currentMenuItem;
                ViewBag.ActionName = ActionName;
            }
            // ---------Управление на активния елемент на менюто, край

            var cultureInfo = HttpContext.Features.Get<IRequestCultureFeature>();
            LanguageCode = cultureInfo?.RequestCulture?.UICulture.TwoLetterISOLanguageName ?? "bg";
            CurrentContextModel currentContext = TempData.Peek<CurrentContextModel>(TempData_CurrentContext);
            if (currentContext != null)
            {
                ViewBag.AccessControl = (IAccessControl)currentContext;
            }
        }

        #region Лог на операциите

        public IActionResult Redirect_Denied(string message = null)
        {
            return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName, new { message = message });
        }
        public void SetAuditContext(IBaseService service, int sourceType, long? sourceId, bool isInsert)
        {
            CheckAccess(service, sourceType, sourceId, (isInsert) ? AuditConstants.Operations.Append : AuditConstants.Operations.Update);
        }
        public void SetAuditContextDelete(IBaseService service, int sourceType, long? sourceId)
        {
            CheckAccess(service, sourceType, sourceId, AuditConstants.Operations.Delete);
        }
        public bool CheckAccess(IBaseService service, int sourceType, long? sourceId, string operation = "", object parentId = null)
        {
            return CurrentContext_Set(service.GetCurrentContext(sourceType, sourceId, operation, parentId));
        }


        public bool CurrentContext_IsSame(int sourceType, object sourceId)
        {
            CurrentContextModel context = TempData.Peek<CurrentContextModel>(TempData_CurrentContext);
            if (context != null)
            {
                if (context.Info.SourceType == sourceType && context.Info.SourceId == sourceId.ToString())
                {
                    return true;
                }
            }
            return false;
        }


        private CurrentContextModel _currentContext;
        public CurrentContextModel CurrentContext
        {
            get
            {
                if (_currentContext == null)
                {
                    //_currentContext = TempData.Get<CurrentContextModel>(TempData_CurrentContext);
                    _currentContext = TempData.Peek<CurrentContextModel>(TempData_CurrentContext);

                }
                return _currentContext ?? new CurrentContextModel();
            }
            set
            {
                _currentContext = value;
                TempData.Put<CurrentContextModel>(TempData_CurrentContext, _currentContext);
            }
        }
        /// <summary>
        /// Добавя информация за текущия обект и правата върху него
        /// </summary>
        /// <param name="model"></param>
        /// <returns>true, ако потребителя има право да достъп върху него (model.CanAccess)</returns>
        public bool CurrentContext_Set(CurrentContextModel model)
        {
            model.LastController = this.ControllerName;
            CurrentContext = model;
            if (CurrentContext.IsRead)
                ViewBag.AccessControl = (IAccessControl)CurrentContext;
            return CurrentContext.CanAccess;
        }
        public void CurrentContext_SetOperation(string operation)
        {
            CurrentContextModel context = TempData.Peek<CurrentContextModel>(TempData_CurrentContext);
            if (context != null)
            {
                context.Info.Operation = operation;
                CurrentContext_Set(context);
            }
        }

        #endregion

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            ControllerActionDescriptor controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {

                var auditScope = this.GetCurrentAuditScope();

                if (HttpContext.Request.Method != HttpMethod.Get.Method)
                {

                    var hasDisableAuditAttribute = controllerActionDescriptor
                                                        .MethodInfo
                                                        .CustomAttributes
                                                        .Where(a => a.AttributeType == typeof(DisableAuditAttribute))
                                                        .Any();

                    if (!hasDisableAuditAttribute)
                    {
                        var _context = CurrentContext;
                        if (_context.IsRead)
                        {

                            auditScope.SetCustomField("currentContext", _context.Info, true);
                        }
                    }
                }

                if (Request.Headers.TryGetValue("X-Forwarded-For", out var currentIp))
                {
                    string ip = currentIp;

                    auditScope.SetCustomField("currentIp", ip, true);
                }

                var auditAction = auditScope?.Event?.GetMvcAuditAction();

                if (auditAction?.ActionParameters != null)
                {
                    CdnUploadRequest fileRequest = (CdnUploadRequest)auditAction.ActionParameters
                        .Select(x => x.Value)
                        .FirstOrDefault(x => x?.GetType() == typeof(CdnUploadRequest));

                    if (fileRequest != null)
                    {
                        fileRequest.FileContentBase64 = string.Empty;
                        fileRequest.FileContent = null;
                    }
                }

                auditScope.SaveAsync();
            }
        }

        public void SetSuccessMessage(string message)
        {
            TempData[MessageConstant.SuccessMessage] = message;
        }
        public void SetErrorMessage(string message)
        {
            TempData[MessageConstant.ErrorMessage] = message;
        }

        public void SetHelpFile(string helpFile)
        {
            TempData["HelpFile"] = helpFile;
        }

        public void ClearHelpFile()
        {
            TempData.Remove("HelpFile");
        }

        public IActionResult SourceTypeAction(int sourceType, long sourceId)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.DocumentDecision:
                    return RedirectToAction("EditDocumentDecision", "Document", new { id = sourceId });
                case SourceTypeSelectVM.Case:
                    return RedirectToAction("CasePreview", "Case", new { id = sourceId });
                case SourceTypeSelectVM.CaseSessionAct:
                    return RedirectToAction("Edit", "CaseSessionAct", new { id = sourceId });
                case SourceTypeSelectVM.CaseNotification:
                    return RedirectToAction("Edit", "CaseNotification", new { id = sourceId });
                case SourceTypeSelectVM.CaseMigration:
                    return RedirectToAction("Edit", "CaseMigration", new { id = sourceId });
                case SourceTypeSelectVM.CaseSessionActDivorce:
                    return RedirectToAction("EditDivorce", "CaseSessionAct", new { id = sourceId });
                case SourceTypeSelectVM.CasePersonBulletin:
                    return RedirectToAction("EditBulletin", "CasePersonSentence", new { id = sourceId });
                case SourceTypeSelectVM.ExchangeDoc:
                    return RedirectToAction("EditExchangeDoc", "Money", new { id = sourceId });
                case SourceTypeSelectVM.ExecList:
                    return RedirectToAction("EditExecList", "Money", new { id = sourceId });
                case SourceTypeSelectVM.CasePersonSentence:
                    return RedirectToAction("Edit", "CasePersonSentence", new { id = sourceId });
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        public string GetFooterInfoUrl(int courtId)
        {
            return null;
            //return Url.Action("GetBlankFooter", "Ajax", new { id = courtId }, this.ControllerContext.HttpContext.Request.Scheme);
        }
        
    }

    public class AdminBaseController : BaseController
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (CurrentContext != null)
            {
                if (CurrentContext.LastController != this.ControllerName)
                {
                    TempData.Remove(TempData_CurrentContext);
                }
            }
            var hasAllowAnonymousAttribute = false;
            ControllerActionDescriptor controllerActionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                hasAllowAnonymousAttribute = controllerActionDescriptor
                                                    .MethodInfo
                                                    .CustomAttributes
                                                    .Where(a => a.AttributeType == typeof(AllowAnonymousAttribute))
                                                    .Any();
            }

            if (!hasAllowAnonymousAttribute)
            {

                if (!userContext.IsUserInRole(AccountConstants.Roles.Administrator))
                {
                    filterContext.Result = RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
                }
            }
        }
    }

    [Authorize(Roles = AccountConstants.Roles.GlobalAdministrator)]
    public class GlobalAdminBaseController : AdminBaseController
    {

    }
}
