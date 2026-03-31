using IOWebApplication.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IOWebApplication.Controllers
{
  public class ReportViewerController : BaseController
  {
    private readonly IReportViewerService service;
    private readonly INomenclatureService nomService;
    private readonly ICourtDepartmentService departmentService;
    private readonly ICommonService commonService;
    private readonly IConfiguration configuration;

    public ReportViewerController(IReportViewerService _service, INomenclatureService _nomService,
             ICourtDepartmentService _departmentService,
             ICommonService _commonService,
             IConfiguration _configuration)
    {
      service = _service;
      nomService = _nomService;
      departmentService = _departmentService;
      commonService = _commonService;
      configuration = _configuration;
    }

    public IActionResult Index()
    {
      var model = service.Report_Select(null);
      return View(model);
    }
    public IActionResult View(int reportId)
    {
      var redirectGuid=service.ReportRequest_Insert(reportId);
      var reportviewerUrl = configuration.GetValue<string>("Reports:ReportViewerUrl");
      return Redirect(reportviewerUrl + redirectGuid);
    }

  }
}