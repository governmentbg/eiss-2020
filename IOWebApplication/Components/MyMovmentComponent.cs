using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOWebApplication.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IOWebApplication.Components
{
    [ViewComponent(Name = "MyMovementComponent")]
    public class MyMovementComponent : ViewComponent
    {
        private readonly ICaseMovementService cmService;

        public MyMovementComponent(ICaseMovementService _cmService)
        {
            cmService = _cmService;
        }
        public async Task<IViewComponentResult> InvokeAsync(string view = "MyMovement")
        {
            switch (view)
            {
                case "MyMovement":
                    {
                        var model = await cmService.Select_ToDoForComponent().Take(5).ToListAsync();
                        ViewBag.movementCount = await cmService.Select_ToDoForComponent().CountAsync();
                        return await Task.FromResult<IViewComponentResult>(View(view, model));
                    }
                case "MovementCount":
                    {
                        var model = await cmService.Select_ToDoForComponent().CountAsync();
                        return await Task.FromResult<IViewComponentResult>(View(view, model));
                    }
                default:
                    return null;
            }
        }
    }
}
