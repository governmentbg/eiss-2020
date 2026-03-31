// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proxy.V2T.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Controllers
{
    public class Voice2TextController : BaseController
    {
        private readonly IProxyEissService voice2TextService;

        public Voice2TextController(IProxyEissService _voice2TextService)
        {
            voice2TextService = _voice2TextService;
        }

        public IActionResult Index()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> IsAuthenticated()
        {
            var v2tResponse = await voice2TextService.V2TIsAuthenticated();            
            return Json(v2tResponse);
        }
        

        [HttpPost]
        public async Task<IActionResult> List(string name)
        {
            var res = await voice2TextService.V2TList(name);
            var result = new SelectList(res, nameof(V2TFileList.Id), nameof(V2TFileList.Name));
            if (result.Count() > 1)
            {
                return Json(result.AddAllItem());
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> FileContent(string fileId)
        {
            var content = await voice2TextService.V2TContent(fileId);
            if (content != null)
            {
                content = content.Replace("\\n", "<br/>");
            }
            return Content(content);
        }
    }
}
