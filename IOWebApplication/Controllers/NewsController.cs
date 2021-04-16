using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using DnsClient.Internal;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

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

        public IActionResult SetAsRead(int id)
        {
            newsService.SetAsRead(id, userContext.UserId);

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

        [HttpGet]
        [Authorize(Roles = AccountConstants.Roles.GlobalAdministrator)]
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

            if (newsService.SaveNews(model, userContext.UserId))
            {
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
        public JsonResult ReadNews(NewsViewModel model)
        {
            newsService.SetAsRead(model.Id, userContext.UserId);
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
