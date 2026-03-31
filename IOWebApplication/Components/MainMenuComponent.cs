using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    public class MainMenuComponent : ViewComponent
    {
        private readonly IUserContext userContext;
        public MainMenuComponent(IUserContext _userContext)
        {
            userContext = _userContext;
        }
        public Task<IViewComponentResult> InvokeAsync(string currentItem = "", bool rightBar = false, bool script = false)
        {
            if (rightBar)
            {
                return Task.FromResult<IViewComponentResult>(View("Horizontal_RightNavBar", currentItem));
            }

            if (userContext.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury)
            {
                return Task.FromResult<IViewComponentResult>(View("Horizontal_Jury", currentItem));
            }
            
            return Task.FromResult<IViewComponentResult>(View("Horizontal", currentItem));
        }
    }
}
