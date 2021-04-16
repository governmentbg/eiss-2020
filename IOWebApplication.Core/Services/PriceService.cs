using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class PriceService : BaseService, IPriceService
    {
        public PriceService(
            ILogger<PriceService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }
        public IQueryable<PriceDesc> PriceDesc_Select(int? courtId, string name)
        {
            try
            {

                if (string.IsNullOrEmpty(name))
                {
                    name = null;
                }

                return repo.AllReadonly<PriceDesc>()
                        .Include(x => x.Court)
                        .Where(x => x.CourtId == (courtId ?? x.CourtId)
                                && x.Name.Contains(name ?? x.Name))
                        .OrderBy(x => x.Name).ThenBy(x => x.DateFrom)
                        .AsQueryable();

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"PriceService PriceDesc_Select");
                return null;
            }
        }
        public bool PriceDesc_Clone(int id, int? courtId)
        {
            try
            {
                var sourcePrice = repo.AllReadonly<PriceDesc>().Where(x => x.Id == id).Include(x => x.PriceCols).Include(x => x.PriceCols.Select(v => v.PriceVals)).FirstOrDefault();
                var price = new PriceDesc()
                {
                    CourtId = courtId,
                    Keyword = sourcePrice.Keyword,
                    Name = sourcePrice.Name,
                    DateFrom = sourcePrice.DateFrom,
                    DateTo = sourcePrice.DateTo
                };

                foreach (var col in sourcePrice.PriceCols.OrderBy(x => x.OrderBy))
                {
                    var _col = new PriceCol()
                    {
                        Name = col.Name,
                        ColType = col.ColType,
                        ColNo = col.ColNo,
                        Active = col.Active
                    };

                    foreach (var val in col.PriceVals)
                    {
                        var _val = new PriceVal()
                        {
                            RowNo = val.RowNo,
                            Value = val.Value,
                            Text = val.Text
                        };
                        _col.PriceVals.Add(_val);
                    }

                    price.PriceCols.Add(_col);
                }


                repo.Add<PriceDesc>(price);
                repo.SaveChanges();
                foreach (var col in price.PriceCols)
                {
                    col.OrderBy = col.Id;
                    repo.Update<PriceCol>(col);
                    repo.SaveChanges();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool PriceDesc_Delete(int id)
        {
            try
            {
                repo.DeleteRange<PriceVal>(x => x.PriceDescId == id);
                repo.DeleteRange<PriceCol>(x => x.PriceDescId == id);
                repo.Delete<PriceDesc>(id);
                repo.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool PriceDesc_SaveData(PriceDesc model)
        {
            try
            {
                if (model.Id > 0)
                {
                    repo.Update(model);
                }
                else
                {
                    repo.Add(model);
                }
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при запис на PriceDesc_SaveData");
                return false;
            }
        }

        public IQueryable<PriceColVM> PriceCol_Select(int pricedesc_id)
        {
            return repo.AllReadonly<PriceCol>()
                            .Where(x => x.PriceDescId == pricedesc_id)
                            .OrderBy(x => x.OrderBy)
                            .ToList()
                            .Select(x => new PriceColVM
                            {
                                Id = x.Id,
                                PriceDescId = x.PriceDescId,
                                Name = x.Name,
                                ColType = x.ColType,
                                ColTypeName = PriceColTypes.Names.First(c => c.Key == x.ColType).Value
                            })
                            .AsQueryable();
        }

        public bool PriceCol_SaveData(PriceCol model)
        {
            try
            {
                if (model.Id > 0)
                {
                    repo.Update(model);
                }
                else
                {
                    repo.Add(model);
                    repo.SaveChanges();
                    model.OrderBy = model.Id;
                }
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при запис на PriceCol_SaveData");
                return false;
            }
        }

        public bool PriceCol_Delete(int id)
        {
            repo.Delete<PriceCol>(id);
            repo.SaveChanges();
            return true;
        }

        public IEnumerable<PriceVal> PriceVal_Select(int pricedesc_id)
        {
            return repo.AllReadonly<PriceVal>()
                        .Include(x => x.Col)
                        .Where(x => x.PriceDescId == pricedesc_id)
                        .OrderBy(x => x.RowNo)
                        .ThenBy(x => x.Col.OrderBy).ToList();
        }

        public bool PriceVal_SaveData(int pricedesc_id, List<PriceVal> model)
        {
            try
            {
                repo.DeleteRange<PriceVal>(x => x.PriceDescId == pricedesc_id);
                foreach (var item in model)
                {
                    item.PriceDescId = pricedesc_id;
                    repo.Add(item);
                }
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при запис на PriceVal_SaveData");
                return false;
            }
        }

        public decimal GetPriceValue(int? courtId, string keyword, decimal mainData = 0M, DateTime? dateNow = null, decimal baseValue = 0M, int colNumber = 0, string rowKeyword = null)
        {
            decimal result = 0M;
            decimal bfN = 99900999999M;

            dateNow = dateNow ?? DateTime.Now;

            var price = repo.AllReadonly<PriceDesc>()
                    .Include(x => x.PriceCols)
                    .Include(x => x.PriceVals)
                    .ThenInclude(x => x.Col)
                    .Where(x => x.CourtId == (courtId ?? x.CourtId)
                            && x.Keyword == keyword
                            && x.DateFrom <= dateNow
                            && (x.DateTo ?? dateNow) >= dateNow)

                    .FirstOrDefault();

            if (price == null || !price.PriceVals.Any())
            {
                return result;
            }

            int rowNo = -1;
            if (price.PriceCols.Any(x => x.ColType == PriceColTypes.DataFrom))
            {
                var maxRowNo = price.PriceVals.Select(x => x.RowNo).Max();
                for (int i = 1; i <= maxRowNo; i++)
                {
                    var dataFrom = 0M;
                    var dataTo = bfN;
                    var currentRowKeyword = string.Empty;
                    var hasRange = false;
                    foreach (var rowItem in price.PriceVals.Where(x => x.RowNo == i))
                    {
                        //var colType = price.PriceCols.FirstOrDefault(x => x.ID == rowItem.PriceColID).ColType;
                        switch (rowItem.Col.ColType)
                        {
                            case PriceColTypes.RowKeyword:
                                currentRowKeyword = rowItem.Text;
                                break;
                            case PriceColTypes.DataFrom:
                                dataFrom = rowItem.Value;
                                hasRange = true;
                                break;
                            case PriceColTypes.DataTo:
                                dataTo = rowItem.Value;
                                hasRange = true;
                                break;
                        }
                    }
                    bool isRowOK = false;
                    if (hasRange && string.IsNullOrEmpty(currentRowKeyword))
                    {
                        if (dataTo < dataFrom || dataTo == 0M)
                        {
                            dataTo = bfN;
                        }
                        if (dataFrom == 0M)
                        {
                            dataFrom = -bfN;
                        }

                        isRowOK = (dataFrom <= mainData && dataTo >= mainData);
                    }
                    else
                    {
                        isRowOK = true;
                    }

                    if (!string.IsNullOrEmpty(currentRowKeyword) && !string.IsNullOrEmpty(rowKeyword))
                    {
                        isRowOK &= (currentRowKeyword.Trim().ToLower() == rowKeyword.Trim().ToLower());
                    }

                    if (isRowOK)
                    {
                        rowNo = i;
                        // result = tmpResult;
                        break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(rowKeyword) && price.PriceCols.Any(x => x.ColType == PriceColTypes.RowKeyword))
                {
                    var maxRowNo = price.PriceVals.Select(x => x.RowNo).Max();
                    bool isRowOK = false;
                    for (int i = 1; i <= maxRowNo; i++)
                    {
                        foreach (var rowItem in price.PriceVals.Where(x => x.RowNo == i))
                        {
                            //var colType = price.PriceCols.FirstOrDefault(x => x.ID == rowItem.PriceColID).ColType;
                            switch (rowItem.Col.ColType)
                            {
                                case PriceColTypes.RowKeyword:
                                    if (rowItem.Text?.Trim()?.ToLower() == rowKeyword.Trim().ToLower())
                                    {
                                        rowNo = i;
                                        isRowOK = true;
                                    }
                                    break;
                            }
                        }

                        if (isRowOK)
                        {   
                            break;
                        }
                    }
                }

                else
                {
                    rowNo = 1;
                }
            }
            if (rowNo >= 0)
            {
                if (colNumber > 0)
                {
                    var priceFromColumn = price.PriceVals.FirstOrDefault(x => x.RowNo == rowNo && x.Col.ColNo == colNumber);
                    if (priceFromColumn != null)
                    {
                        switch (priceFromColumn.Col.ColType)
                        {
                            case PriceColTypes.Procent:
                                result = baseValue * priceFromColumn.Value / 100;
                                break;
                            default:
                                result = priceFromColumn.Value;
                                break;
                        }
                    }
                }
                else
                {
                    var priceWithProcent = price.PriceVals.FirstOrDefault(x => x.RowNo == rowNo && x.Col.ColType == PriceColTypes.Procent);
                    if (priceWithProcent != null)
                    {
                        result = baseValue * priceWithProcent.Value / 100; ;
                    }
                    else
                    {
                        result = price.PriceVals.FirstOrDefault(x => x.RowNo == rowNo && x.Col.ColType == PriceColTypes.Value).Value;
                    }
                }
            }

            return result;
        }
    }
}
