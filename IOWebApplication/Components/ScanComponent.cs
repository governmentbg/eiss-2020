using IOWebApplication.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    /// <summary>
    /// Document Scanner component
    /// </summary>
    public class ScanComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(ScanInfoViewModel info, string viewName = "")
        {
            return await Task.FromResult(View(viewName, info));
        }
    }
}
