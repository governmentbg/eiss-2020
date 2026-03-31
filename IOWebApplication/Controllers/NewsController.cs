using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class NewsController : BaseController
    {
        private readonly INewsService newsService;

        private readonly ILogger<NewsController> logger;

        public NewsController(
            ILogger<NewsController> _logger,
            INewsService _newsService)
        {
            newsService = _newsService;
            logger = _logger;
        }

        [TitleAudit(Operation = AuditConstants.Operations.List)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [TitleAudit(Operation = AuditConstants.Operations.List)]
        [HttpGet]
        public IActionResult IndexUser()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetNews()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetNews(int id)
        {
            return View();
        }

        public async Task<IActionResult> SetAsRead(int id)
        {
            await newsService.SetAsRead(id, userContext.UserId);

            return Ok();
        }

        [HttpGet]
        public IActionResult LatestNews()
        {
            NewsViewModel model = newsService.GetLatest();

            return View();
        }

        public JsonResult GetData()
        {
            var model = newsService.GetLastNews(userContext.UserId);
            return Json(model);
        }

        void auditInfo(string operation, NewsViewModel model, string add = "")
        {
            if (model != null)
            {
                if (model.PublishDate.Year > 2000)
                {
                    AddAuditInfo(operation, $"Заглавие: {model.Title}  - непубликувана", add, "Новини");
                }
                else
                {
                    AddAuditInfo(operation, $"Заглавие: {model.Title} публикувана на: {model.PublishDate.ToString("dd.MM.yyyy")}", add, "Новини");
                }
            }
        }

        [HttpGet]
        [Authorize(Roles = AccountConstants.Roles.GlobalAdministrator)]
        [DisableAudit]
        public IActionResult Add()
        {
            var model = new NewsViewModel();

            return View("Edit", model);
        }

        [HttpGet]
        [Authorize(Roles = AccountConstants.Roles.GlobalAdministrator)]
        public IActionResult Edit(int id)
        {
            NewsViewModel model = newsService.GetById(id);
            auditInfo(AuditConstants.Operations.View, model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AccountConstants.Roles.GlobalAdministrator)]
        public IActionResult Edit(NewsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentId = model.Id;
            if (newsService.SaveNews(model, userContext.UserId))
            {
                this.SaveLogOperation(currentId == 0, model.Id);
                auditInfo(currentId == 0 ? AuditConstants.Operations.Append : AuditConstants.Operations.Update, model);
                TempData[MessageConstant.SuccessMessage] = MessageConstant.Values.SaveOK;
            }
            else
            {
                TempData[MessageConstant.ErrorMessage] = MessageConstant.Values.SaveFailed;
            }

            return View(model);
        }

        public IActionResult ReadNews(int id)
        {
            var newsViewModel = newsService.GetById(id);
            return PartialView(newsViewModel);
        }

        [HttpPost]
        public async Task<JsonResult> ReadNews(NewsViewModel model)
        {
            await newsService.SetAsRead(model.Id, userContext.UserId);
            var newsViewModel = newsService.GetById(model.Id);
            auditInfo(AuditConstants.Operations.Update, newsViewModel, "Маркиране като прочетена");
            return Json(new { result = 1 });
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request)
        {
            var data = newsService.News_Select();
            return request.GetResponse(data);
        }
    }
}
