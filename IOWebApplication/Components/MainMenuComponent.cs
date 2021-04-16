﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    public class MainMenuComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string currentItem = "")
        {
            return await Task.FromResult<IViewComponentResult>(View("Horizontal", currentItem));
        }
    }
}
