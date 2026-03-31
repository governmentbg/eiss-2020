using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace IOWebApplication.Controllers
{
    public class FastProcessSelectionCourtController : BaseController
    {
        private readonly IFastProcessSelectionCourtService service;
        private readonly INomenclatureService nomService;
        private readonly ICaseService caseService;


        public FastProcessSelectionCourtController(IFastProcessSelectionCourtService _service, INomenclatureService _nomService, ICaseService _caseService)
        {
            service = _service;
            nomService = _nomService;
            caseService = _caseService;
        }

        //public async Task<IActionResult> Index()
        //{

        //    //var res = await service.GetSelectedAvailableCourtForFastSelection(0, 3);
        //    //string dateString = "2024-02-23";
        //    //DateTime date = DateTime.Parse(dateString);

        //    //var model = await service.GetFastSelectioForToday();


        //    // bool b = await service.CreateInitializationSelectionForCourtByDate(date, "1");
        //    //var t = service.CreateSelectionForCourtByDate(157, data,"1");

        //    // bool r=service.CheckIsCreatedSelectionDay(data,"0").Result;



        //    // var res = await service.FastProcessCreateSelectionProtocol(11243);
        //    //  res = 2;
        //    // var res = await service.FastProcessLawUnit_LoadJudge(45, 1);
        //    // testovo razpredelenie
        //    //for (int i = 1; i < 29; i++)
        //    //{
        //    //    if (i != 5 && i != 6 && i != 12 && i != 13 && i != 19 && i != 20 && i != 26 && i != 27 ) { 
        //    //    DateTime data = new DateTime(2024, 2, i);
        //    //    Random random = new Random();
        //    //    int dayCases = random.Next(650, 700);
        //    //    dayCases = 684;

        //    //    for (int j = 0; j < dayCases; j++)

        //    //    {

        //    //        bool r = service.CheckIsCreatedSelectionDay(data, "0").Result;
        //    //        bool rf = service.AddAfterZeroCourtForSelectionDat(data, "0").Result;
        //    //        var t = await service.GetSelectedCourtForSelectionDate(data, "0");

        //    //    }
        //    //    }

        //    //}
        //    // testovo razpredelenie

        //    //var fil = new FastProcessSelectionCourtReportFilterVM();
        //    //fil.YearId = 2024;
        //    //fil.MonthId = 2;

        //    //var d = await service.FastProcessSelectionCorut_SelectForReport(fil);

        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Index(FastProcessSelectionMonthsVM model)

        //{
        //    for (int j = 0; j < model.SelectionCount; j++)

        //    {
        //        var available_courts = await service.GetRegionalCourts();
        //        bool r = service.CheckIsCreatedSelectionDay(model.SelectionDate, "0").Result;
        //        bool rf = service.AddAfterZeroCourtForSelectionDat(model.SelectionDate, available_courts, "0").Result;
        //        var t = await service.GetSelectedCourtForSelectionDate(model.SelectionDate, available_courts, "0");

        //    }
        //    model = await service.GetFastSelectioForToday();

        //    //return View(model);
        //    return RedirectToAction(nameof(Index), model);
        //}







        public async Task<IActionResult> PreviewFastProtocol(int id)
        {

            var model = await service.FastProcessSelectionProtokol_Preview(id);


            return View("Preview", model);
        }

        public async Task<IActionResult> GenerateProtocol(int id)
        {

            var model = await service.FastProcessCreateSelectionProtocol(id);


            return View("Preview", model);
        }

        //[HttpPost]
        ////public IActionResult ListDataReport(IDataTablesRequest request, CaseSelectionProtokolFilterVM model)
        //public async Task<IActionResult> ListDataReport(IDataTablesRequest request, FastProcessSelectionCourtReportFilterVM model)
        //{
        //    var data = await service.FastProcessSelectionCorut_SelectForReport(model);
        //    ViewBag.DaysToShow=data.listOfDates.ToArray();
        //    return request.GetResponse(data.rows.AsQueryable());
        //}

        [HttpPost]
        public async Task<IActionResult> GetDataReport(FastProcessSelectionCourtReportFilterVM model)
        {
            var data = await service.FastProcessSelectionCorut_SelectForReport(model);
            return Json(data);
        }

        [Authorize(Roles = AccountConstants.Roles.GlobalAdministrator)]
        public async Task<IActionResult> FastProcessMonthReport()
        {

            FastProcessSelectionCourtReportFilterVM model = new FastProcessSelectionCourtReportFilterVM();
            model.YearId = DateTime.Now.Year;
            model.MonthId = DateTime.Now.Month;


            ViewBag.YearId_ddl = await service.SelectionYears_ForDropDownList();
            ViewBag.MonthId_ddl = service.SelectionMonths_ForDropDownList(model.YearId);

            return View(model);
        }

        [HttpGet]
        public IActionResult GetDDL_SelectionMonths(int year)
        {
            var model = service.SelectionMonths_ForDropDownList(year);

            return Json(model);
        }

        [Authorize(Roles = AccountConstants.Roles.GlobalAdministrator)]
        public async Task<IActionResult> TestRandomization()
        {

            var res = await service.GenerateRandomForDateAndCount("01.07.2025", 666);
            //res = await service.GenerateRandomForDateAndCount("2025-6-3", 857);
            //res = await service.GenerateRandomForDateAndCount("2025-6-4", 771);
            //res = await service.GenerateRandomForDateAndCount("2025-6-5", 754);
            //res = await service.GenerateRandomForDateAndCount("2025-6-6", 557);
            //res = await service.GenerateRandomForDateAndCount("2025-6-9", 681);
            //res = await service.GenerateRandomForDateAndCount("2025-6-10", 783);
            //res = await service.GenerateRandomForDateAndCount("2025-6-11", 989);
            //res = await service.GenerateRandomForDateAndCount("2025-6-12", 700);
            //res = await service.GenerateRandomForDateAndCount("2025-6-13", 1016);

            //res = await service.GenerateRandomForDateAndCount("2025-6-16", 1037);
            //res = await service.GenerateRandomForDateAndCount("2025-6-16", 737);
            //res = await service.GenerateRandomForDateAndCount("2025-6-18", 857);
            //res = await service.GenerateRandomForDateAndCount("2025-6-19", 831);
            //res = await service.GenerateRandomForDateAndCount("2025-6-20", 624);

            //res = await service.GenerateRandomForDateAndCount("2025-6-23", 641);
            //res = await service.GenerateRandomForDateAndCount("2025-6-24", 781);
            //res = await service.GenerateRandomForDateAndCount("2025-6-25", 1563);
            //res = await service.GenerateRandomForDateAndCount("2025-6-26", 1252);
            //res = await service.GenerateRandomForDateAndCount("2025-6-27", 1272);

            ////res = await service.GenerateRandomForDateAndCount("2025-4-22", 797);
            ////res = await service.GenerateRandomForDateAndCount("2025-4-23", 804);
            ////res = await service.GenerateRandomForDateAndCount("2025-4-24", 794);
            ////res = await service.GenerateRandomForDateAndCount("2025-4-25", 787);

            ////res = await service.GenerateRandomForDateAndCount("2025-4-29", 795);
            ////res = await service.GenerateRandomForDateAndCount("2025-4-30", 792);

            return RedirectToAction("FastProcessMonthReport", "FastProcessSelectionCourt");
        }
        public async Task<IActionResult> InitRandomization()
        {
            DateTime date = Utils.SafeParseDate("01.07.2025") ?? DateTime.Now;
            bool b = await service.CreateInitializationSelectionForCourtByDate(date, "0");


            return RedirectToAction("FastProcessMonthReport", "FastProcessSelectionCourt");
        }

    }
}