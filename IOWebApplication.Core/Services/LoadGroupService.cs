using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class LoadGroupService : BaseService, ILoadGroupService
    {
        public LoadGroupService(ILogger<LoadGroupService> _logger,
            IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
        }
        public IQueryable<LoadGroup> LoadGroup_Select()
        {
            return repo.AllReadonly<LoadGroup>().AsQueryable();
        }

        public bool LoadGroup_SaveData(LoadGroup model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<LoadGroup>(model.Id);
                    saved.Code = model.Code;
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.IsActive = model.IsActive;
                    saved.DateStart = model.DateStart;
                    saved.DateEnd = model.DateEnd;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    int maxOrderNumber = repo.AllReadonly<LoadGroup>()
                        .Select(x => x.OrderNumber)
                        .DefaultIfEmpty(0)
                        .Max();

                    model.OrderNumber = maxOrderNumber + 1;


                    repo.Add<LoadGroup>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на LoadGroup Id={ model.Id }");
                return false;
            }
        }

        public IQueryable<LoadGroupLinkVM> LoadGroupLink_Select(int loadGroupId)
        {
            return repo.AllReadonly<LoadGroupLink>()
                .Include(x => x.CourtType)
                .Include(x => x.CaseInstance)
                .Where(x => x.LoadGroupId == loadGroupId)
                .Select(x => new LoadGroupLinkVM()
                {
                    Id = x.Id,
                    CourtTypeName = x.CourtType.Label,
                    CaseInstanceName = x.CaseInstance.Label,
                    LoadIndex = x.LoadIndex
                }).AsQueryable();
        }

        public bool LoadGroupLink_SaveData(LoadGroupLink model, List<int> caseCodes, ref string errorMessgae)
        {
            try
            {
                model.CaseInstanceId = (model.CaseInstanceId ?? 0) <= 0 ? null : model.CaseInstanceId;

                var linkExist = repo.AllReadonly<LoadGroupLink>().Where(x => x.Id != model.Id && x.LoadGroupId == model.LoadGroupId
                                && x.CourtTypeId == model.CourtTypeId && (x.CaseInstanceId ?? 0) == (model.CaseInstanceId ?? 0)).Any();
                if (linkExist == true)
                {
                    errorMessgae = "За тази Група/Вид съд/Инстанция вече има въведен процент";
                    return false;
                }

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<LoadGroupLink>(model.Id);
                    saved.CourtTypeId = model.CourtTypeId;
                    saved.CaseInstanceId = model.CaseInstanceId;
                    saved.LoadIndex = model.LoadIndex;
                    repo.Update(saved);

                    //Взима всичко за това ид и го трие
                    var loadGroupLinkCode = repo.AllReadonly<LoadGroupLinkCode>().Where(a => a.LoadGroupLinkId == model.Id).ToList();
                    foreach (var item in loadGroupLinkCode)
                    {
                        repo.Delete<LoadGroupLinkCode>(item);
                    }
                }
                else
                {
                    //Insert
                    repo.Add<LoadGroupLink>(model);
                }

                //записва листа със кодовете за loadgrouplinkid
                foreach (var code in caseCodes)
                {
                    LoadGroupLinkCode newLoadLinkCode = new LoadGroupLinkCode();
                    newLoadLinkCode.LoadGroupLinkId = model.Id;
                    newLoadLinkCode.CaseCodeId = code;
                    repo.Add<LoadGroupLinkCode>(newLoadLinkCode);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на LoadGroupLink Id={ model.Id }");
                return false;
            }
        }

        public IQueryable<MultiSelectTransferVM> LoadGroupLinkCode_Select(int loadGroupLinkId)
        {
            return repo.AllReadonly<LoadGroupLinkCode>()
            .Include(x => x.CaseCode)
            .Where(x => x.LoadGroupLinkId == loadGroupLinkId)
            .Select(x => new MultiSelectTransferVM()
            {
                Id = x.CaseCode.Id,
                Order = x.CaseCode.OrderNumber,
                Text = $"{x.CaseCode.Code} {x.CaseCode.Label}"
            })
            .GroupBy(x => x.Id)
            .Select(g => g.FirstOrDefault())
            .AsQueryable();
        }
    }
}
