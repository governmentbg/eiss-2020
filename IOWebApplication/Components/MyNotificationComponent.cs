using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                        int modelCnt = await workNotificationService.SelectWorkNotifications(filter).CountAsync();
                        return await Task.FromResult<IViewComponentResult>(View(view, modelCnt));
                    }
                case "FPnotifications":
                    {
                        var model = await workNotificationService.SelectWorkNotifications(new WorkNotificationFilterVM()
                                                                  {
                                                                      CourtId = userContext.CourtId,
                                                                      UserId = userContext.UserId,
                                                                      DateCreate = DateTime.Now,
                                                                      ReadTypeId = WorkNotificationFilterVM.ReadTypeUnRead,
                                                                      NotificationKind = NomenclatureConstants.NotificationKinds.FastProcess
                                                                  })
                                                                  .Select(x => new
                                                                  {
                                                                      x.WorkNotificationTypeId,
                                                                      x.WorkNotificationTypeLabel
                                                                  })
                                                                  .GroupBy(x => new { x.WorkNotificationTypeId, x.WorkNotificationTypeLabel })
                                                                  .Select(x => new WorkNotificationWidgetVM
                                                                  {
                                                                      NotificationTypeId = x.Key.WorkNotificationTypeId,
                                                                      NotificationTypeName = x.Key.WorkNotificationTypeLabel,
                                                                      Count = x.Count()
                                                                  })
                                                                  .ToListAsync();
                        return await Task.FromResult<IViewComponentResult>(View(view, model));
                    }
                default:
                    return null;
            }
        }
    }
}
