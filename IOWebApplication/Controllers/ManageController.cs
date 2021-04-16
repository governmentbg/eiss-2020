using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace IOWebApplication.Controllers
{
    public class ManageController : BaseController
    {
        public IActionResult ExpiredInfo(int id, long longId, string stringId, string fileContainer, string submitUrl, string returnUrl, bool otherBool)
        {
            var model = new ExpiredInfoVM()
            {
                Id = id,
                LongId = longId,
                StringId = stringId,
                ExpireSubmitUrl = submitUrl,
                FileContainerName = fileContainer,
                ReturnUrl = returnUrl,
                OtherBool = otherBool
            };
            return PartialView(model);
        }
    }
}