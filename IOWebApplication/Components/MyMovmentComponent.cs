using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOWebApplication.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

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
                        var model = cmService.Select_ToDo();
                        return await Task.FromResult<IViewComponentResult>(View(view, model));
                    }
                case "MovementCount":
                    {
                        var model = cmService.Select_ToDoCount();
                        return await Task.FromResult<IViewComponentResult>(View(view, model));
                    }
                default:
                    return null;
            }
        }
    }
}
