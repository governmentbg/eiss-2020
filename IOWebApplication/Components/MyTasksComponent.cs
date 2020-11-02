// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    [ViewComponent(Name = "MyTasksComponent")]
    public class MyTasksComponent : ViewComponent
    {
        private readonly IWorkTaskService workTaskService;
        private readonly IUserContext userContext;
        public MyTasksComponent(IWorkTaskService _workTaskService, IUserContext _userContext)
        {
            workTaskService = _workTaskService;
            userContext = _userContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(string view = "MyTasks")
        {
            ViewBag.userId = userContext.UserId;
            switch (view)
            {
                case "MyTasks":
                    {
                        var model = workTaskService.Select_ToDo(5);
                        ViewBag.taskCount = workTaskService.Select_ToDoCount();
                        return await Task.FromResult<IViewComponentResult>(View(view, model));
                    }
                case "TaskCount":
                    {
                        var model = workTaskService.Select_ToDoCount();
                        return await Task.FromResult<IViewComponentResult>(View(view, model));
                    }
                default:
                    return null;
            }
        }
    }
}
