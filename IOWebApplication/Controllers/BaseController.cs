using IO.LogOperation.Models;
using IO.LogOperation.Service;
using IOWebApplication.Components;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace IOWebApplication.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private IUserContext _userContext;
        protected IDBUserContext dbUserContext;
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

        protected void SaveLogOperation(bool isInsert, object objectKey, object masterKey = null, string actionName = null, string controllerName = null)
        {
            SaveLogOperation((isInsert) ? OperationTypes.Insert : OperationTypes.Update, objectKey, masterKey, actionName, controllerName);
        }
        protected void SaveLogOperation(OperationTypes operation, object objectKey, object masterKey = null, string actionName = null, string controllerName = null)
        {
            if (Request.Form["hfContainer"].FirstOrDefault() != null)
            {
                var html = Request.Form["hfContainer"].FirstOrDefault();
                SaveLogOperation(controllerName ?? this.ControllerName?.ToLower(), actionName ?? this.ActionName, html, operation, objectKey, masterKey);
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

        protected void ClearCheckDoublePostback(string actionKey)
        {
            var tempDataKey = $"CDP{actionKey}";

            TempData.Remove(tempDataKey);
        }
        protected bool CheckDoublePostback(string actionKey = "", double millisecondsAfterLastCheck = 3000)
        {
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CheckDoublePostback))
            {
                return false;
            }

            bool result = false;
            var tsFormat = "yyMMddHHmmssfff";
            var tempDataKey = $"CDP{actionKey}";

            var stored = TempData.Peek<string>(tempDataKey);
            if (string.IsNullOrEmpty(stored))
            {
                TempData[tempDataKey] = DateTime.Now.ToString(tsFormat);
                return false;
            }

            try
            {
                var storedDate = Utils.SafeParseDate(stored, tsFormat);
                if ((DateTime.Now - storedDate.Value).TotalMilliseconds < millisecondsAfterLastCheck)
                {
                    return true;
                }
                TempData[tempDataKey] = DateTime.Now.ToString(tsFormat);
            }
            catch (Exception ex)
            {
                return false;
            }

            return result;
        }

        protected IActionResult NotFoundError(string message)
        {
            return RedirectToAction("NotFound", "Home", new { message });
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            try
            {
                if (filterContext.ActionArguments.Values != null)
                {
                    foreach (var obj in filterContext.ActionArguments.Values.Where(v => v != null && v.GetType()?.IsClass == true))
                    {

                        encodeStringProperties(obj);
                    }
                }
            }
            catch { }

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

            if (string.IsNullOrEmpty(ProgramExtensions.MachineName))
                ProgramExtensions.MachineName = System.Net.Dns.GetHostName();
        }

        private void encodeStringProperties(object obj)
        {
            if (obj == null)
            {
                return;
            }
            Type objType = obj.GetType();
            var objProperties = objType.GetProperties();
            foreach (var memberInfo in objProperties)
            {
                if (!memberInfo.CanWrite)
                {
                    continue;
                }
                var memName = memberInfo.Name;
                var isString = memberInfo.PropertyType.FullName.ToLower().Contains("string");
                if (isString)
                {
                    bool forEncode = true;
                    if (memName.ToLower().Contains("url") ||
                        memName.ToLower().Contains("html") ||
                        memName.ToLower().Contains("json"))
                    {
                        forEncode = false;
                    }
                    else
                    {
                        var attribs = memberInfo.GetCustomAttributes(true);
                        foreach (var attr in attribs)
                        {
                            if (attr.GetType().FullName == typeof(AllowHtmlAttribute).FullName)
                            {
                                forEncode = false;
                            }
                        }
                    }
                    if (forEncode)
                    {
                        var stringValue = memberInfo.GetValue(obj);
                        var stringValueEncoded = HttpUtility.HtmlEncode(stringValue);
                        memberInfo.SetValue(obj, stringValueEncoded);
                    }
                }
                else
                {
                    if (memberInfo.PropertyType.FullName.StartsWith("System"))
                    {
                        continue;
                    }
                    Type propertyType = memberInfo.GetType();
                    if (!propertyType.IsSubclassOf(typeof(System.ValueType)))
                    {
                        if (memberInfo.PropertyType.IsClass && propertyType.GetProperties().Any())
                        {
                            if (!memberInfo.PropertyType.IsGenericType &&
                                !memberInfo.PropertyType.IsGenericTypeDefinition &&
                                !memberInfo.PropertyType.IsAbstract &&
                                !memberInfo.PropertyType.IsArray)
                            {
                                try
                                {
                                    encodeStringProperties(memberInfo.GetValue(obj));
                                }
                                catch { }
                            }

                        }
                    }
                }

            }
        }

        #region Лог на операциите

        public IActionResult Redirect_Denied(string message = null)
        {
            return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName, new { message = message });
        }

        public void SetDataKey(object key, string tempDataKey = null)
        {
            var tdKey = tempDataKey ?? $"DataKey{userContext.UserId}";
            TempData[tdKey] = key.ToString();
        }
        public bool CheckDataKey(object key, bool setModelStateError = true, string tempDataKey = null)
        {
            var tdKey = tempDataKey ?? $"DataKey{userContext.UserId}";
            var storedKey = TempData.Peek(tdKey);
            var result = storedKey?.ToString().Equals(key?.ToString()) ?? false;
            if (setModelStateError && !result)
            {
                ModelState.AddModelError("", "Промяната на ключови данни не е разрешена!");
            }
            return result;
        }
        public IActionResult Redirect_DataChange()
        {
            return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName, new { message = "Промяната на ключови данни не е разрешена!" });
        }
        public void SetAuditContext(IBaseService service, int sourceType, long? sourceId, bool isInsert)
        {
            CheckAccess(service, sourceType, sourceId, (isInsert) ? AuditConstants.Operations.Append : AuditConstants.Operations.Update);
        }
        public Task SetAuditContextAsync(IBaseService service, int sourceType, long? sourceId, bool isInsert)
        {
            return CheckAccessAsync(service, sourceType, sourceId, (isInsert) ? AuditConstants.Operations.Append : AuditConstants.Operations.Update);
        }
        public void SetAuditContextDelete(IBaseService service, int sourceType, long? sourceId)
        {
            CheckAccess(service, sourceType, sourceId, AuditConstants.Operations.Delete);
            var _context = CurrentContext;
            if (_context.IsRead)
            {
                AddAuditInfo(AuditConstants.Operations.Delete, _context.Info.BaseObject, _context.Info.ObjectInfo, sourceType);
            }
        }
        public bool CheckAccess(IBaseService service, int sourceType, long? sourceId, string operation = "", object parentId = null)
        {
            return CurrentContext_Set(service.GetCurrentContext(sourceType, sourceId, operation, parentId));
        }
        public async Task<bool> CheckAccessAsync(IBaseService service, int sourceType, long? sourceId, string operation = "", object parentId = null)
        {
            return CurrentContext_Set(await service.GetCurrentContextAsync(sourceType, sourceId, operation, parentId));
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

        public void CurrentContext_SetObjectInfo(string objectInfo, bool appendMode = false)
        {
            CurrentContextModel context = TempData.Peek<CurrentContextModel>(TempData_CurrentContext);
            if (context != null)
            {
                if (appendMode)
                {
                    objectInfo = (context.Info.ObjectInfo ?? "") + objectInfo;
                }
                context.Info.ObjectInfo = objectInfo;

                CurrentContext_Set(context);
            }
        }
        public string CurrentContext_GetObjectInfo()
        {
            CurrentContextModel context = TempData.Peek<CurrentContextModel>(TempData_CurrentContext);
            if (context != null)
            {
                return context.Info.ObjectInfo ?? "";
            }
            return "";
        }
        #endregion        

        
        protected void AddAuditInfo(string operation, string baseInfo, string addInfo, string objectType, bool alwaysSetUrl = false, string userId = null, int? courtId = null)
        {
            var auditService = (IAuditLogService)HttpContext.RequestServices.GetService(typeof(IAuditLogService));
            var auditLog = new Infrastructure.Data.Models.Audit.AuditLog()
            {
                CourtId = courtId ?? userContext.CourtId,
                Operation = operation,
                BaseObject = baseInfo,
                ObjectInfo = addInfo,
                InsertedDate = DateTime.Now,
                UserId = userId ?? userContext.UserId,
                ObjectType = objectType
            };
            if (Request.Method == "GET" || alwaysSetUrl)
            {
                string requestUrl = Request.Path;
                if (!string.IsNullOrEmpty(Request.QueryString.Value))
                {
                    requestUrl += Request.QueryString.Value;
                }

                auditLog.RequestUrl = requestUrl;
            }
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var currentIp))
            {
                string ip = currentIp;
                auditLog.ClientIP = ip;
            }
            if (auditService.SaveLog(auditLog))
            {
                auditManualySaved = true;
            }
        }
        protected void AddAuditInfo(string operation, string baseInfo, string addInfo = null, int sourceType = 0, bool alwaysSetUrl = false)
        {
            string objType = null;
            if (sourceType > 0)
            {
                objType = SourceTypeSelectVM.GetSourceTypeName(sourceType);
            }
            AddAuditInfo(operation, baseInfo, addInfo, objType, alwaysSetUrl);
        }


        protected void DisableAudit()
        {
            auditManualySaved = true;
        }

        bool auditManualySaved = false;
        ActionExecutedContext lastContext;
        string lastClientIP;
        protected override void Dispose(bool disposing)
        {

            if (lastContext != null && !lastContext.IsHiddenActionResult() && !auditManualySaved)
            {
                ControllerActionDescriptor controllerActionDescriptor = lastContext.ActionDescriptor as ControllerActionDescriptor;
                if (controllerActionDescriptor != null)
                {
                    var disableAuditOnController = controllerActionDescriptor
                                                        .ControllerTypeInfo
                                                       .CustomAttributes
                                                       .Where(a => a.AttributeType == typeof(DisableAuditAttribute))
                                                       .Any();

                    var disableAuditOnAction = controllerActionDescriptor
                                                        .MethodInfo
                                                        .CustomAttributes
                                                        .Where(a => a.AttributeType == typeof(DisableAuditAttribute))
                                                        .Any();
                    var titleAuditOperation = controllerActionDescriptor
                                                        .MethodInfo
                                                        .CustomAttributes
                                                        .Where(a => a.AttributeType == typeof(TitleAuditAttribute))
                                                        .Select(x => x.NamedArguments
                                                                    .Where(n => n.MemberName == nameof(TitleAuditAttribute.Operation))
                                                                    .Select(n => n.TypedValue.Value.ToString())
                                                                    .FirstOrDefault())
                                                        .FirstOrDefault();

                    if (!disableAuditOnController && !disableAuditOnAction)
                    {
                        var auditLog = new Infrastructure.Data.Models.Audit.AuditLog()
                        {
                            CourtId = userContext.CourtId,
                            UserId = userContext.UserId,
                            Method = Request?.Method,
                            ClientIP = lastClientIP
                        };
                        var _context = CurrentContext;
                        if (_context.IsRead && _context.LastController == this.ControllerName)
                        {
                            auditLog.ObjectType = _context.Info.ObjectType;
                            auditLog.BaseObject = _context.Info.BaseObject;
                            auditLog.ObjectInfo = _context.Info.ObjectInfo;
                            auditLog.Operation = _context.Info.Operation;
                            if (Request?.Method == "GET")
                            {
                                if (auditLog.Operation == AuditConstants.Operations.Append && string.IsNullOrEmpty(auditLog.ObjectInfo))
                                {
                                    auditLog.ObjectInfo = "Създаване на нов обект";
                                }
                                auditLog.Operation = AuditConstants.Operations.View;
                            }

                        }
                        else
                        {

                            if (!string.IsNullOrEmpty(titleAuditOperation))
                            {
                                //ако няма контекст - се взема само title на страницата - зададен е в layout.cshtml
                                var pageTitle = TempData["PageTitle"];
                                auditLog.Operation = titleAuditOperation;
                                auditLog.BaseObject = (string)pageTitle;
                            }
                        }
                        //if (Request.Headers.TryGetValue("X-Forwarded-For", out var currentIp))
                        //{
                        //    string ip = currentIp;
                        //    auditLog.ClientIP = ip;
                        //}
                        string requestUrl = lastContext.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(lastContext.HttpContext.Request.QueryString.Value))
                        {
                            requestUrl += lastContext.HttpContext.Request.QueryString.Value;
                        }
                        auditLog.RequestUrl = requestUrl;
                        var auditService = (IAuditLogService)HttpContext.RequestServices.GetService(typeof(IAuditLogService));
                        auditService.SaveLog(auditLog);
                    }
                }
            }

            base.Dispose(disposing);
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            lastContext = context;
            lastClientIP = null;
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var currentIp))
            {
                lastClientIP = currentIp;
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
        public void SetWarningMessage(string message)
        {
            TempData[MessageConstant.WarningMessage] = message;
        }

        public void SetHelpFile(string helpFile)
        {
            TempData["HelpFile"] = helpFile;
        }

        public void ClearHelpFile()
        {
            TempData.Remove("HelpFile");
        }

        public void SetSourceKey(int sourceType, object sourceId, string tempDataKey = null)
        {
            SetDataKey($"{sourceType}|{sourceId.ToString()}", tempDataKey);
        }

        public bool CheckSourceKey(int sourceType, object sourceId, string tempDataKey = null)
        {
            return CheckDataKey($"{sourceType}|{sourceId.ToString()}", false, tempDataKey);
        }

        public IActionResult SourceKeyExpireJsonError()
        {
            return Json(new { result = false, message = "Грешен или променен идентификатор." });
        }

        public IActionResult SourceKeyExpireJsonErrorDescription()
        {
            return Json(new { result = false, message = "Въведете Причина за премахването." });
        }

        public IActionResult SourceTypeAction(int sourceType, long sourceId)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.DocumentDecision:
                    return RedirectToAction("EditDocumentDecision", "Document", new { id = sourceId });
                case SourceTypeSelectVM.DocumentNotification:
                    return RedirectToAction("Edit", "DocumentNotification", new { id = sourceId });
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
                case SourceTypeSelectVM.CaseLawyerHelp:
                    return RedirectToAction("Edit", "CaseLawyerHelp", new { id = sourceId });
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        public string GetFooterInfoUrl(int courtId)
        {

            var config = (IConfiguration)HttpContext
                                     .RequestServices
                                     .GetService(typeof(IConfiguration));

            var basePath = config.GetValue<string>("EissUrl");
            if (!string.IsNullOrEmpty(basePath))
            {
                return new Uri(new Uri(basePath), Url.Action("GetBlankFooter", "Ajax", new { id = courtId })).AbsoluteUri;
            }
            else
            {
                return Url.Action("GetBlankFooter", "Ajax", new { id = courtId }, this.ControllerContext.HttpContext.Request.Scheme);

            }

            //return null;
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
