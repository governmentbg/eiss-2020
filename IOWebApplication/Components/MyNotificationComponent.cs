using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    [ViewComponent(Name = "MyNotificationComponent")]
    public class MyNotificationComponent : ViewComponent
    {
        private readonly IWorkNotificationService workNotificationService;
        private readonly IUserContext userContext;
        public MyNotificationComponent(IWorkNotificationService _workNotificationService, IUserContext _userContext)
        {
            workNotificationService = _workNotificationService;
            userContext = _userContext;
        }

        public int IWorkNotification { get; private set; }

        public async Task<IViewComponentResult> InvokeAsync(string view, WorkNotificationFilterVM filter)
        {
            ViewBag.userId = userContext.UserId;
            if (filter == null)
            {
                filter = workNotificationService.MakeDefaultFilter();
            }
            switch (view)
            {
                case "MyNotifications":
                    {
                        return await Task.FromResult<IViewComponentResult>(View(view, filter));
                    }
                case "NotificationCount":
                    {
                        var notifications = workNotificationService.SelectWorkNotifications(filter).ToList();
                        int modelCnt = notifications.Count();
                        return await Task.FromResult<IViewComponentResult>(View(view, modelCnt));
                    }
                default:
                    return null;
            }
        }
    }
}
