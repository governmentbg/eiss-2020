using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Controllers
{
    public class PriceController : BaseController
    {
        private readonly IPriceService service;

        public PriceController(IPriceService _service)
        {
            service = _service;
        }

        public IActionResult TestPrice(string key = "TEST1", decimal data = 7, string rowKey = null)
        {
            var result = service.GetPriceValue(null, key, data, null, 0, 0, rowKey);
            return Content(result.ToString());
        }

        public IActionResult PriceDesc()
        {
            ViewBag.courtId = null;// userContext.CourtId;
            return View();
        }
        public IActionResult PriceDesc_LoadData(IDataTablesRequest request, int? courtId)
        {
            var data = service.PriceDesc_Select(courtId, null);
            return request.GetResponse(data);
        }

        public IActionResult PriceDesc_Edit(int id = 0)
        {
            PriceDesc model = (id > 0) ? service.GetById<PriceDesc>(id) : new PriceDesc();
            return View(model);
        }
        [HttpPost]
        public IActionResult PriceDesc_Edit(PriceDesc model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (service.PriceDesc_SaveData(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(PriceDesc_Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                return View(model);
            }
        }

        public IActionResult PriceCol(int priceDesc)
        {
            ViewBag.price = service.GetById<PriceDesc>(priceDesc);
            return View();
        }
        public IActionResult PriceCol_LoadData(IDataTablesRequest request, int priceDesc)
        {
            var data = service.PriceCol_Select(priceDesc);
            return request.GetResponse(data);
        }
        void SetViewBag_PriceCol()
        {
            ViewBag.ColType_ddl = new SelectList(PriceColTypes.Names, "Key", "Value").ToList();
        }
        public IActionResult PriceCol_Edit(int id = 0, int priceDesc = 0)
        {
            PriceCol model = (id > 0) ? service.GetById<PriceCol>(id) : new PriceCol() { PriceDescId = priceDesc, Active = true };
            SetViewBag_PriceCol();
            return View(model);
        }
        [HttpPost]
        public IActionResult PriceCol_Edit(PriceCol model)
        {
            if (!ModelState.IsValid)
            {
                SetViewBag_PriceCol();
                return View(model);
            }
            if (service.PriceCol_SaveData(model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(PriceCol), new { priceDesc = model.PriceDescId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
                SetViewBag_PriceCol();
                return View(model);
            }
        }

        public IActionResult PriceVal(int priceDesc, int showRows = 0, int insertAt = -1, int removeAt = -1)
        {
            var vals = service.PriceVal_Select(priceDesc);
            var cols = service.PriceCol_Select(priceDesc).ToList();

            var maxRow = 1;
            if (vals.Count() > 0)
            {
                maxRow = vals.Max(x => x.RowNo);
            }

            if (showRows > 0)
            {
                maxRow = showRows;
            }
            if (insertAt >= 0)
            {
                maxRow++;
            }
            if (removeAt >= 0)
            {
                maxRow--;
            }

            List<PriceVal> model = new List<PriceVal>();
            for (int row = 1; row <= maxRow; row++)
            {
                var currentRow = row;
                if (insertAt > 0 && insertAt < row)
                {
                    currentRow--;
                }
                if (removeAt > 0 && removeAt <= row)
                {
                    currentRow++;
                }
                foreach (var col in cols)
                {
                    var _val = new PriceVal()
                    {
                        PriceDescId = priceDesc,
                        RowNo = row,
                        PriceColId = col.Id
                    };

                    if (insertAt != row)
                    {
                        var saved = vals.FirstOrDefault(x => x.RowNo == currentRow && x.PriceColId == col.Id);
                        if (saved != null)
                        {
                            _val.Value = saved.Value;
                            _val.Text = saved.Text;
                        }
                    }
                    model.Add(_val);
                }
            }

            if (insertAt > 0 || removeAt > 0)
            {
                service.PriceVal_SaveData(priceDesc, model);
                return RedirectToAction(this.ActionName, new { pricedesc_id = priceDesc });
            }

            var price = service.GetById<PriceDesc>(priceDesc);
            ViewBag.price = price;
            ViewBag.cols = cols;
            ViewBag.maxRow = maxRow;
            List<BreadcrumbsVM> breadcrumbs = new List<BreadcrumbsVM>()
            {
                new BreadcrumbsVM(){
                    Title = price.Name,
                    Href=Url.Action(nameof(PriceDesc),new{ courtId = price.CourtId})
                },
                new BreadcrumbsVM(){
                    Title = "Колони",
                    Href=Url.Action(nameof(PriceCol),new{ priceDesc = price.Id})
                }
            };
            ViewBag.breadcrumbs = breadcrumbs;
            return View(model);
        }
        [HttpPost]
        public IActionResult PriceVal(List<PriceVal> model, int priceDesc)
        {
            if (service.PriceVal_SaveData(priceDesc, model))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
            }
            return RedirectToAction(this.ActionName, new { priceDesc = priceDesc });
        }
    }
}