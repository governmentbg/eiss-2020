using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    [ViewComponent(Name = "CaseWarningComponent")]
    public class CaseWarningComponent : ViewComponent
    {
        private readonly IUserContext userContext;
        private readonly ICaseLawUnitService service;
        public CaseWarningComponent(IUserContext _userContext, ICaseLawUnitService _service)
        {
            userContext = _userContext;
            service = _service;
        }
        public async Task<IViewComponentResult> InvokeAsync(int caseId)
        {
            if (userContext.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge)
            {
                var judgeReporter = service.CaseLawUnit_Select(caseId, null)
                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                    .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                    .FirstOrDefault();

                if (judgeReporter != null)
                {
                    if (judgeReporter.LawUnitId != userContext.LawUnitId)
                    {
                        ViewBag.otherJudge = true;
                    }
                }
            }
            return await Task.FromResult<IViewComponentResult>(View());
        }
    }
}
