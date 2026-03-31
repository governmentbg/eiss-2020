using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    [ViewComponent(Name = "DocumentWarningComponent")]
    public class DocumentWarningComponent : ViewComponent
    {
        private readonly IUserContext userContext;
        private readonly ICdnService cdnService;
        public DocumentWarningComponent(
            IUserContext _userContext, ICdnService cdnService)
        {
            userContext = _userContext;
            this.cdnService = cdnService;
        }
        public async Task<IViewComponentResult> InvokeAsync(long documentId, string warningType)
        {
            switch (warningType)
            {
                case "hasDocumentRequest":
                    //Приложимо за документ от ЦР
                    bool hasRequest = await cdnService.Select(SourceTypeSelectVM.DocumentRequest, documentId.ToString()).AnyAsync();
                    if (!hasRequest)
                    {
                        ViewBag.ShowWarning = "Няма въведени данни за заявление";
                    }
                    break;
                default:
                    ViewBag.ShowWarning = null;
                    break;
            }
            return await Task.FromResult<IViewComponentResult>(View());
        }
    }
}
