using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    /// <summary>
    /// Document Scanner component
    /// </summary>
    public class ScanComponent : ViewComponent
    {
        private readonly ICommonService commonService;
        public ScanComponent(ICommonService commonService)
        {
            this.commonService = commonService;
        }

        public async Task<IViewComponentResult> InvokeAsync(ScanInfoViewModel info, string viewName = "")
        {
            var scanConfiguration = commonService.SystemParam_SelectValue(NomenclatureConstants.SystemParamName.ScannerConfiguration);
            if (string.IsNullOrEmpty(scanConfiguration))
            {
                scanConfiguration = "190/200";
            }

            try
            {
                var values = scanConfiguration.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
                info.ScanDPI = values[0];
                info.ScanBlackThreshold = values[1];
            }
            catch (Exception ex) { }

            return await Task.FromResult(View(viewName, info));
        }
    }
}
