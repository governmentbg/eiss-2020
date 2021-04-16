using DataTables.AspNet.Core;
using IOWebApplication.Components;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Account;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.AccountConstants;

namespace IOWebApplication.Controllers
{

    public class AccountController : BaseController
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly ILogger<AccountController> logger;
        private readonly ICommonService commonService;
        private readonly IStringLocalizer localizer;
        private readonly IConfiguration config;

        public AccountController(
            SignInManager<ApplicationUser> _signInManager,
            UserManager<ApplicationUser> _userManager,
            RoleManager<ApplicationRole> _roleManager,
            ILogger<AccountController> _logger,
            ICommonService _commonService,
            IConfiguration _config,
            IStringLocalizer<AccountController> _localizer
            )
        {
            signInManager = _signInManager;
            userManager = _userManager;
            roleManager = _roleManager;
            logger = _logger;
            commonService = _commonService;
            localizer = _localizer;
            config = _config;
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return RedirectToAction(nameof(HomeController.AccessDenied), HomeController.ControlerName);
        }

        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        public IActionResult Index()
        {
            ViewBag.breadcrumbs = commonService.Breadcrumbs_Account().DeleteOrDisableLast();
            SetHelpFile(HelpFileValues.Nom4);

            return View();
        }

        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = commonService.Users_Select(userContext.CourtId, null, null, true);

            return request.GetResponse(data);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login_Cert_Error(string error)
        {
            logger.LogError(error);

            return RedirectToAction(nameof(Login), new { error = "Моля изберете валиден сертификат." });
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null, string error = null, string showPassword = null)
        {
            var model = new LoginVM
            {
                ReturnUrl = returnUrl
            };

            model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.errorMessage = error;
            }
            if (showPassword == "yes" && config.GetValue<bool>("PasswordLoginEnabled"))
            {
                model.LoginWithPassword = true;
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM model)
        {
            var user = await this.userManager.FindByEmailAsync(model.Email);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("Password", "Невалиден потребител и/или парола");
                return View(model);
            }
            else
            {
                if (!await userManager.CheckPasswordAsync(user, model.Password))
                {
                    ModelState.AddModelError("Password", "Невалиден потребител и/или парола");
                }
            }
            var lawUnit = commonService.GetById<LawUnit>(user.LawUnitId);
            if (lawUnit.DateTo.HasValue && lawUnit.DateTo < DateTime.Now)
            {
                ModelState.AddModelError("Password", "Невалиден потребител и/или парола");
            }
            if (!ModelState.IsValid)
            {
                model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                return View(model);
            }

            user.PasswordLogin = true;
            await this.signInManager.SignInAsync(user, isPersistent: false);

            if (!string.IsNullOrEmpty(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }
            return this.RedirectToAction("Index", "Home");
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();

            return LocalRedirect("/");
        }


        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                logger.LogError($"Error from external provider: {remoteError}");

                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            var info = await signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                logger.LogError("Error loading external login information.");

                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }


            ApplicationUser user = null;
            if (info.LoginProvider == "IdStampIT")
            {
                string userId = commonService.Users_GetByLawUnitUIC(info.ProviderKey);
                if (!string.IsNullOrEmpty(userId))
                {
                    user = await userManager.FindByIdAsync(userId);
                }
                if (user == null)
                {
                    SetErrorMessage("Невалиден потребител.");
                    return RedirectToAction("Login", new { ReturnUrl = returnUrl });
                }
            }
            if (user == null)
            {
                user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            }
            if (user != null)
            {
                var claims = await userManager.GetClaimsAsync(user);
                var certNoClaim = claims.FirstOrDefault(c => c.Type == CustomClaimType.IdStampit.CertificateNumber);
                var currentCertNoClaim = info.Principal.Claims.FirstOrDefault(c => c.Type == CustomClaimType.IdStampit.CertificateNumber);

                if (currentCertNoClaim != null)
                {
                    if (certNoClaim != null)
                    {
                        await userManager.ReplaceClaimAsync(user, certNoClaim, currentCertNoClaim);
                    }
                    else
                    {
                        await userManager.AddClaimAsync(user, currentCertNoClaim);
                    }
                }

                await signInManager.SignInAsync(user, isPersistent: false);

                return LocalRedirect(returnUrl);
            }


            return RedirectToAction("AccessDenied");
        }


        public void SetBreadcrums(string id)
        {
            if (!string.IsNullOrEmpty(id))
                ViewBag.breadcrumbs = commonService.Breadcrumbs_AccountEdit(id).DeleteOrDisableLast();
            else
                ViewBag.breadcrumbs = commonService.Breadcrumbs_AccountAdd().DeleteOrDisableLast();

            SetHelpFile(HelpFileValues.Nom4);
        }

        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        public IActionResult Register()
        {
            SetBreadcrums("");
            UserProfileRegisterVM model = new UserProfileRegisterVM();
            return View(model);
        }

        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        [HttpPost]
        public async Task<IActionResult> Register(UserProfileRegisterVM model)
        {
            SetBreadcrums("");
            ValidateRegister(model);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!commonService.Users_CheckUserByEmail(model.Id, model.Email))
            {
                SetErrorMessage("Вече съществува потребител с този мейл");
                return View(model);
            }

            model.CourtId = userContext.CourtId;
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                LawUnitId = model.LawUnitId,
                CourtId = userContext.CourtId,
                WorkNotificationToMail = model.WorkNotificationToMail,
                IsActive = true
            };

            IdentityResult res = null;
            if (AccountConstants.PasswordLoginEnabled)
            {
                res = await userManager.CreateAsync(user, model.Password);
            }
            else
            {
                res = await userManager.CreateAsync(user);
            }

            if (res.Succeeded)
            {
                commonService.Users_GenerateEissId(user.Id);
                this.SaveLogOperation(true, user.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(model);
            }
        }
        void ValidateRegister(UserProfileRegisterVM model)
        {
            if (AccountConstants.PasswordLoginEnabled)
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError(nameof(UserProfileRegisterVM.Password), "Въведете 'Парола'.");
                }
                if (model.Password != model.Password2)
                {
                    ModelState.AddModelError(nameof(UserProfileRegisterVM.Password), "Двете копия на паролата не съвпадат.");
                }
            }
            if (model.LawUnitId <= 0)
            {
                ModelState.AddModelError(nameof(UserProfileRegisterVM.LawUnitId), "Изберете 'Служител.'");
            }
            string userVal = commonService.Users_ValidateEmailLawUnit(null, model.LawUnitId);
            if (!string.IsNullOrEmpty(userVal))
            {
                ModelState.AddModelError(nameof(UserProfileRegisterVM.LawUnitId), userVal);
            }
        }

        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        public async Task<IActionResult> Edit(string id)
        {
            SetBreadcrums(id);
            var user = await userManager.FindByIdAsync(id);
            var model = new UserProfileVM()
            {
                Id = user.Id,
                LawUnitId = user.LawUnitId,
                Email = user.Email,
                CourtId = user.CourtId,
                EissId = user.EissId,
                WorkNotificationToMail = user.WorkNotificationToMail == true,
                IsActive = user.IsActive
            };
            model.Roles = roleManager.Roles.Select(x => new CheckListVM
            {
                Value = x.Name,
                Label = localizer[x.Name].ToString(),
                Checked = false
            }).OrderBy(x => x.Label).ToList();

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in model.Roles)
            {
                if (userRoles.Any(x => x == role.Value))
                {
                    role.Checked = true;
                }
            }
            bool canChange = true;
            if (!userContext.IsUserInRole(Roles.GlobalAdministrator))
            {
                if (model.Roles.Any(x => x.Value == Roles.GlobalAdministrator && x.Checked))
                {
                    canChange = false;
                }
                for (int i = model.Roles.Count() - 1; i > 0; i--)
                {
                    if (model.Roles[i].Value == Roles.GlobalAdministrator)
                    {
                        model.Roles.RemoveAt(i);
                        break;
                    }
                }
            }
            ViewBag.canChange = canChange;
            return View(model);
        }
        [Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        [HttpPost]
        public async Task<IActionResult> Edit(UserProfileVM model)
        {
            if (model.IsActive)
            {
                if (!commonService.Users_CheckUserByLawUnit(model.Id, model.LawUnitId))
                {
                    SetErrorMessage("Вече съществува друг активен потребител за избрания служител");
                    return RedirectToAction(nameof(Edit), new { id = model.Id });
                }
            }

            SetBreadcrums(model.Id);

            //Редактиране на потребител
            var user = await userManager.FindByIdAsync(model.Id);

            //Задаване на роли/групи
            var userRoles = await userManager.GetRolesAsync(user);
            var res = await userManager.RemoveFromRolesAsync(user, userRoles);
            if (res.Succeeded)
            {
                res = await userManager.AddToRolesAsync(user, model.Roles.Where(x => x.Checked).Select(x => x.Value));

                user.CourtId = model.CourtId;
                user.LawUnitId = model.LawUnitId;
                user.WorkNotificationToMail = model.WorkNotificationToMail;
                user.IsActive = model.IsActive;
                await userManager.UpdateAsync(user);
                this.SaveLogOperation(false, user.Id);
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }

            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        [HttpGet]
        public IActionResult SearchUser(string query)
        {
            var model = commonService.Users_Select(userContext.CourtId, query.SafeLower(), null)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id,
                                Label = $"{x.FullName} ({x.Email}, {x.LawUnitTypeName})"
                            });
            return new JsonResult(model);
        }
        [HttpGet]
        public IActionResult GetUser(string id)
        {
            var model = commonService.Users_Select(userContext.CourtId, null, id)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id,
                                Label = $"{x.FullName} ({x.Email}, {x.LawUnitTypeName})"
                            }).FirstOrDefault();

            if (model == null)
            {
                return BadRequest();
            }

            return new JsonResult(model);
        }

        public IActionResult SelectCourt()
        {

            object court = userContext.CourtId.ToString();
            ViewBag.courts = commonService.CourtSelect_ByUser(userContext.UserId).ToSelectList(x => x.Value, x => x.Label).SetSelected(court);
            var model = new SelectCourtVM()
            {
                CourtId = userContext.CourtId,
                IsAdmin = userContext.IsUserInRole(Infrastructure.Constants.AccountConstants.Roles.GlobalAdministrator)
            };
            return PartialView(model);
        }

        [HttpPost]
        public async Task<JsonResult> SelectCourt(SelectCourtVM model)
        {
            var user = await userManager.FindByIdAsync(userContext.UserId);
            user.CourtId = model.CourtId;
            await userManager.UpdateAsync(user);
            await this.signInManager.SignOutAsync();
            await this.signInManager.SignInAsync(user, isPersistent: false);
            return Json(new { result = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserSetting(string setting, string value)
        {
            if (await commonService.Users_UpdateSetting(setting, value))
            {
                return Content("ok");
            }
            else
            {
                return Content("failed");
            }
        }

        public async Task<IActionResult> UserSetting()
        {
            var model = await userContext.Settings();
            return View(model);
        }
        [HttpPost]
        public IActionResult UserSetting(UserSettingsModel model)
        {
            if (commonService.Users_UpdateSetting(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);

            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            return View(model);
        }

        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            PasswordResetVM model = new PasswordResetVM()
            {
                UserId = userId
            };
            return PartialView(model);
        }

        [HttpPost]
        public async Task<JsonResult> ResetPassword(PasswordResetVM model)
        {
            if (model.Password != model.Password2)
            {
                return Json(new { isok = false, error = "Двете копия на паролата не съвпадат." });
            }
            var user = await userManager.FindByIdAsync(model.UserId);
            var userRoles = await userManager.GetRolesAsync(user);
            if (!userContext.IsUserInRole(Roles.GlobalAdministrator))
            {
                if (userRoles.Contains(Roles.GlobalAdministrator))
                {
                    return Json(new { isok = false, error = "Не можете да задавате парола на Администратор на инфраструктурата!" });
                }
            }
            string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            IdentityResult resetResult = await userManager.ResetPasswordAsync(user, resetToken, model.Password);
            return Json(new { isok = resetResult.Succeeded, error = string.Join(",", resetResult.Errors.Select(x => x.Description)) });
        }

        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.FindByIdAsync(userContext.UserId);
            ChangePasswordVM model = new ChangePasswordVM()
            {
                FullName = userContext.FullName
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            var user = await userManager.FindByIdAsync(userContext.UserId);
            if (!await userManager.CheckPasswordAsync(user, model.OldPassword))
            {
                ModelState.AddModelError("Password", "Невалидна съществуваща парола парола");
            }

            if (model.Password != model.Password2)
            {
                ModelState.AddModelError("Password", "Двете копия на паролата не съвпадат.");
            }
            string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            IdentityResult resetResult = await userManager.ResetPasswordAsync(user, resetToken, model.Password);
            if (!resetResult.Succeeded)
            {
                ModelState.AddModelError("", string.Join(", ", resetResult.Errors.Select(x => x.Description)));

            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            SetSuccessMessage("Вашата парола е сменена успешно.");
            return RedirectToAction("Index", "Home");
        }

        //[Authorize(Policy = AdminOnlyPolicyRequirement.Name)]
        //public IActionResult manage()
        //{
        //    var result = roleManager.CreateAsync(new ApplicationRole() { Name = "HR_ADMIN" }).Result;

        //    return Json(result);
        //}
    }
}