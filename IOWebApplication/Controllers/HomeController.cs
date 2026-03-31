using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    //[Authorize]
    [DisableAudit]
    public class HomeController : BaseController
    {
        public static string ControlerName = "Home";
        private readonly ILogger<HomeController> logger;
        private readonly INewsService newsService;

        public HomeController(
            ILogger<HomeController> _logger,
            INewsService _newsService,
            IDBUserContext dbUserContext)
        {
            logger = _logger;
            newsService = _newsService;
            this.dbUserContext = dbUserContext;
        }

        public async Task<IActionResult> Index()
        {
            SetHelpFile(HelpFileValues.HomeDashboard);
            ViewBag.userSettings = await dbUserContext.Settings();
            return View();
        }
        public IActionResult AccessDenied(string message = null)
        {
            ViewBag.message = message ?? "Нямате достъп до избрания от Вас ресурс или функционалност.";
            return View();
        }

        public IActionResult NotFound(string message = null)
        {
            var errorModel = new ErrorViewModel
            {
                Title = "Ненамерен ресурс",
                Message = message
            };

            return View(nameof(Error), errorModel);
        }


        [AllowAnonymous]
        public IActionResult Error(string message = null)
        {
            var feature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();
            string errorTitle = "Грешка";
            string errorMessage = "Възникна неочаквана грешка. Моля, опитайте по-късно.";
            string innerMessage = "";

            var error = feature?.Error.Message;
            if (error != null)
            {
                errorMessage = feature.Error.Message;
                innerMessage = feature.Error.InnerException?.Message;
                if (feature.Error is NotFoundException)
                {
                    errorTitle = "Ненамерен ресурс";
                }
                else
                {
                    logger.LogError(feature.Error, $"EissWeb; {message}");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                {
                    innerMessage = message;

                    if (message.Trim().ToLower().Replace(" ", "").Contains("authstate"))
                    {
                        return RedirectToAction("Login", "Account", new
                        {
                            error = "Моля изберете валиден сертификат."
                        });
                    }
                }
            }
            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Title = errorTitle,
                Message = errorMessage,
                InnerException = innerMessage
            };

            return View(errorModel);
        }
    }
}
