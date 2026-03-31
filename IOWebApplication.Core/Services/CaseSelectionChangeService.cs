// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

namespace IOWebApplication.Core.Services
{
    public class CaseSelectionChangeService : BaseService, ICaseSelectionChangeService
    {

        private readonly ICdnService cdnService;
        private readonly ICaseLawUnitService lawunitService;
        private readonly ICaseSelectionProtokolService protokolService;
        private readonly ICourtLawUnitService courtLawUnitService;
        private readonly IMQEpepService mqEpepService;
        public CaseSelectionChangeService(
            IRepository _repo,
            ILogger<CaseSelectionChangeService> _logger,
            IUserContext _userContext,
            ICdnService _cdnService,
            ICaseLawUnitService _lawunitService,
            ICaseSelectionProtokolService _protokolService,
            ICourtLawUnitService _courtLawUnitService,
            IMQEpepService _mqEpepService)
        {
            repo = _repo;
            logger = _logger;
            userContext = _userContext;
            cdnService = _cdnService;
            lawunitService = _lawunitService;
            protokolService = _protokolService;
            courtLawUnitService = _courtLawUnitService;
            mqEpepService = _mqEpepService;
        }

        public IQueryable<CaseSelectionChangeVM> Select(CaseSelectionChangeFilterVM model)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo.MakeEndDate();
            DateTime dateNow = DateTime.Now;

            Expression<Func<CaseSelectionChange, bool>> changeSearch = x => true;
            if (model.ChangeTypeId > 0)
            {
                changeSearch = x => x.ChangeTypeId == model.ChangeTypeId;
            }

            Expression<Func<CaseSelectionChange, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.DateWrt >= dateFromSearch && x.DateWrt <= dateToSearch;

            Expression<Func<CaseSelectionChange, bool>> courtGroupSearch = x => true;
            if (model.CourtGroupId > 0)
            {
                courtGroupSearch = x => x.CourtGroupId == model.CourtGroupId;
            }

            Expression<Func<CaseSelectionChange, bool>> fromLawunitSearch = x => true;
            if (model.FromLawunitId > 0)
            {
                fromLawunitSearch = x => x.FromLawunitId == model.FromLawunitId;
            }
            Expression<Func<CaseSelectionChange, bool>> toLawunitSearch = x => true;
            if (model.ToLawunitId > 0)
            {
                toLawunitSearch = x => x.ToLawunitId == model.ToLawunitId;
            }

            Expression<Func<CaseSelectionChange, bool>> toLawunitDepartmentSearch = x => true;
            if (model.ToLawunitDepartmentId > 0)
            {
                toLawunitDepartmentSearch = x => x.ToLawunitDepartmentId == model.ToLawunitDepartmentId;
            }

            return repo.AllReadonly<CaseSelectionChange>()
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(dateSearch)
                            .Where(changeSearch)
                            .Where(courtGroupSearch)
                            .Where(fromLawunitSearch)
                            .Where(toLawunitSearch)
                            .Where(toLawunitDepartmentSearch)
                            .Select(x => new CaseSelectionChangeVM
                            {
                                Id = x.Id,
                                ChangeTypeId = x.ChangeTypeId,
                                ChangeTypeLabel = x.ChangeType.Label,
                                CourtGroupLabel = x.CourtGroup.Label,
                                FromLawunitName = (x.FromLawunit != null) ? x.FromLawunit.FullName : "",
                                ToLawunitName = (x.ToLawunit != null) ? x.ToLawunit.FullName : "",
                                ToLawunitDepartmentName = (x.ToLawunitDepartment != null) ? x.ToLawunitDepartment.Label : "",
                                StateName = x.ChangeState.Label,
                                DateWrt = x.DateWrt,
                                UserName = x.User.LawUnit.FullName
                            }).AsQueryable();
        }


        public IQueryable<CaseSelectionChangeListVM> Select_Cases(CaseSelectionChangeFilterVM model)
        {

            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo.MakeEndDate();
            DateTime dateNow = DateTime.Now;

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate >= dateFromSearch && x.RegDate <= dateToSearch;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
            {
                var listGroupIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
                    listGroupIds = model.CaseGroupIds_text.Split(',').Select(Int32.Parse).ToList();
                caseGroupWhere = x => listGroupIds.Contains(x.CaseGroupId);
            }

            //Expression<Func<Case, bool>> courtGroupSearch = x => true;
            //if (model.CourtGroupId > 0)
            //{
            //    courtGroupSearch = x => x.CourtGroupId == model.CourtGroupId;
            //}
            Expression<Func<Case, bool>> fromLawunitSearch = x => true;
            if (model.FromLawunitId > 0)
                fromLawunitSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
               (a.DateTo ?? dateEnd).Date != dateNow.Date &&
                (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.FromLawunitId && a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<Case, bool>> toLawunitSearch = x => true;
            if (model.ToLawunitId > 0)
                toLawunitSearch = x => !x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.ToLawunitId && a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();


            Expression<Func<Case, bool>> filterByChangeType = x => true;
            switch (model.ChangeTypeId)
            {
                case NomenclatureConstants.CaseSelectionChangeTypes.Prerazpredelenie:

                    filterByChangeType = x => !NomenclatureConstants.CaseState.CantChangeSelection.Contains(x.CaseStateId);
                    break;
            }

            return repo.AllReadonly<Case>()
                        .Where(x => x.CourtId == userContext.CourtId)
                        .Where(x => x.CourtGroupId == model.CourtGroupId)
                        .Where(dateSearch)
                        //.Where(courtGroupSearch)
                        //.Where(caseGroupWhere)
                        .Where(fromLawunitSearch)
                        .Where(toLawunitSearch)
                        .Where(filterByChangeType)
                        .Select(x => new CaseSelectionChangeListVM
                        {
                            CaseId = x.Id,
                            RegDate = x.RegDate,
                            RegNumber = x.RegNumber,
                            Lawunits = x.CaseLawUnits.Where(l => l.CaseSessionId == null && (l.DateTo ?? dateEnd) >= dateNow
                                                         //&& l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter
                                                         && NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(l.JudgeRoleId)
                                                         )
                                                        .OrderBy(c => c.JudgeRoleId)
                                                        .Select(c => $"{c.LawUnit.FullName} ({c.JudgeRole.Label})")
                                                        .ToArray(),
                            CaseTypeLabel = x.CaseType.Code,
                            CaseCodeLabel = (x.CaseCode != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
                            CaseStateLabel = (x.CaseState != null) ? x.CaseState.Label : string.Empty,
                            ProcessPriorityLabel = (x.ProcessPriority != null) ? x.ProcessPriority.Label : string.Empty,
                            DepartmentOtdelenieText = (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? x.Otdelenie.Label : string.Empty) +
                                               (x.JudicalComposition != null ? (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? " / " + x.JudicalComposition.Label : x.JudicalComposition.Label) : string.Empty)

                        }).AsQueryable();
        }
        public IQueryable<CaseSelectionChangeListVM> Select_CasesJudge(CaseSelectionChangeFilterVM model)
        {

            DateTime dateEnd = DateTime.Now.AddYears(100);
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo.MakeEndDate();
            DateTime dateNow = DateTime.Now;

            Expression<Func<Case, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate >= dateFromSearch && x.RegDate <= dateToSearch;

            Expression<Func<Case, bool>> caseGroupWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
            {
                var listGroupIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
                    listGroupIds = model.CaseGroupIds_text.Split(',').Select(Int32.Parse).ToList();
                caseGroupWhere = x => listGroupIds.Contains(x.CaseGroupId);
            }

            //Expression<Func<Case, bool>> courtGroupSearch = x => true;
            //if (model.CourtGroupId > 0)
            //{
            //    courtGroupSearch = x => x.CourtGroupId == model.CourtGroupId;
            //}
            Expression<Func<Case, bool>> fromLawunitSearch = x => true;
            if (model.FromLawunitId > 0)
                fromLawunitSearch = x => x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.FromLawunitId && ((a.JudgeRoleId == NomenclatureConstants.JudgeRole.Judge) || (a.JudgeRoleId == NomenclatureConstants.JudgeRole.ReserveJudge) || (a.JudgeRoleId == NomenclatureConstants.JudgeRole.ExtJudge))).Any();

            Expression<Func<Case, bool>> toLawunitSearch = x => true;
            if (model.ToLawunitId > 0)
                toLawunitSearch = x => !x.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                      (a.DateTo ?? dateEnd).Date >= dateNow.Date && a.LawUnitId == model.ToLawunitId && ((a.JudgeRoleId == NomenclatureConstants.JudgeRole.Judge) || (a.JudgeRoleId == NomenclatureConstants.JudgeRole.ReserveJudge) || (a.JudgeRoleId == NomenclatureConstants.JudgeRole.ExtJudge))).Any();


            Expression<Func<Case, bool>> filterByChangeType = x => true;
            switch (model.ChangeTypeId)
            {
                case NomenclatureConstants.CaseSelectionChangeTypes.Prerazpredelenie:

                    filterByChangeType = x => !NomenclatureConstants.CaseState.CantChangeSelection.Contains(x.CaseStateId);
                    break;
            }

            return repo.AllReadonly<Case>()
                        .Where(x => x.CourtId == userContext.CourtId)
                        .Where(x => x.CourtGroupId == model.CourtGroupId)
                        .Where(dateSearch)
                        //.Where(courtGroupSearch)
                        //.Where(caseGroupWhere)
                        .Where(fromLawunitSearch)
                        .Where(toLawunitSearch)
                        .Where(filterByChangeType)
                        .Where(x => x.CaseLawUnits.Where(a => (a.DateTo == null) && (a.CaseSessionId == null) && ((a.JudgeRoleId == NomenclatureConstants.JudgeRole.Judge) || (a.JudgeRoleId == NomenclatureConstants.JudgeRole.ReserveJudge) || (a.JudgeRoleId == NomenclatureConstants.JudgeRole.ExtJudge))).Count() > 0)
                        .Select(x => new CaseSelectionChangeListVM
                        {
                            CaseId = x.Id,
                            RegDate = x.RegDate,
                            RegNumber = x.RegNumber,
                            Lawunits = x.CaseLawUnits.Where(l => l.CaseSessionId == null && (l.DateTo ?? dateEnd) >= dateNow
                                                         //&& l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter
                                                         && NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(l.JudgeRoleId)
                                                         )
                                                        .OrderBy(c => c.JudgeRoleId)
                                                        .Select(c => $"{c.LawUnit.FullName} ({c.JudgeRole.Label})")
                                                        .ToArray(),
                            CaseTypeLabel = x.CaseType.Code,
                            CaseCodeLabel = (x.CaseCode != null) ? x.CaseCode.Code + " " + x.CaseCode.Label : string.Empty,
                            CaseStateLabel = (x.CaseState != null) ? x.CaseState.Label : string.Empty,
                            ProcessPriorityLabel = (x.ProcessPriority != null) ? x.ProcessPriority.Label : string.Empty,
                            DepartmentOtdelenieText = (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? x.Otdelenie.Label : string.Empty) +
                                               (x.JudicalComposition != null ? (x.Otdelenie != null && x.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? " / " + x.JudicalComposition.Label : x.JudicalComposition.Label) : string.Empty)

                        }).AsQueryable();
        }

        public async Task<SaveResultVM> ReplaceJudge_SaveData(CaseSelectionChangeFilterVM model)
        {
            var forSave = new CaseSelectionChange()
            {
                ChangeTypeId = model.ChangeTypeId,
                CourtId = userContext.CourtId,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
                Desription = model.Description,
                CourtGroupId = model.CourtGroupId,
                FromLawunitId = model.FromLawunitId,
                ToLawunitId = model.ToLawunitId,
                ToLawunitDepartmentId = model.ToLawunitDepartmentId,
                ChangeStateId = NomenclatureConstants.CaseSelectionChangeStates.New
            };

            var caseIds = model.CaseList.ToIntArray();
            foreach (var item in caseIds)
            {
                var newCase = new CaseSelectionChangeList()
                {
                    CaseId = item
                };
                forSave.CaseList.Add(newCase);
            }

            repo.Add(forSave);
            await repo.SaveChangesAsync().ConfigureAwait(false);


            return new SaveResultVM(true)
            {
                ObjectId = forSave.Id,
                AuditInfo = getAuditInfo(forSave.Id)
            };

        }

        private string getAuditInfo(int id)
        {
            var info = repo.AllReadonly<CaseSelectionChange>()
                               .Where(x => x.Id == id)
                               .Select(x => new
                               {
                                   ChangeType = x.ChangeState.Label,
                                   CourtGroupName = x.CourtGroup.Label,
                                   ToLawunit = x.ToLawunit.FullName
                               }).FirstOrDefault();
            return $"{info.ChangeType}, група:{info.CourtGroupName}, избран съдия: {info.ToLawunit}";
        }

        public CaseSelectionChangePrintVM GetModelForProtokol(int id)
        {
            var result = repo.AllReadonly<CaseSelectionChange>()
                                .Where(x => x.Id == id)
                                .Select(x => new CaseSelectionChangePrintVM
                                {
                                    Id = x.Id,
                                    DateWrt = x.DateWrt,
                                    UserName = x.User.LawUnit.FullName,
                                    Description = x.Desription,
                                    CourtName = x.Court.Label,
                                    CourtGroupName = x.CourtGroup.Label,
                                    ChangeTypeId = x.ChangeTypeId,
                                    ChangeTypeLabel = x.ChangeType.Label,
                                    ChangeStateId = x.ChangeStateId,
                                    FromLawunitName = (x.FromLawunit != null) ? x.FromLawunit.FullName : "",
                                    ToLawunitName = (x.ToLawunit != null) ? x.ToLawunit.FullName : "",
                                    ToLawunitDepartmentName = (x.ToLawunitDepartment != null) ? x.ToLawunitDepartment.Label : "",
                                }).FirstOrDefault();

            result.CaseList = repo.AllReadonly<CaseSelectionChangeList>()
                                    .Where(x => x.CaseSelectionChangeId == id)
                                    .Select(x => new CaseSelectionChangePrintListVM
                                    {
                                        Id = x.Id,
                                        CaseId = x.CaseId,
                                        CaseTypeLabel = x.Case.CaseType.Code,
                                        CaseCodeLabel = x.Case.CaseCode.Code,
                                        LoadGroupLabel = x.Case.LoadGroupLink.LoadGroup.Label,
                                        RegNumber = x.Case.RegNumber,
                                        DocumentNumber = x.Case.Document.DocumentNumber,
                                        DocumentDate = x.Case.Document.DocumentDate,
                                        FromLawunitName = (x.FromLawunit != null) ? x.FromLawunit.FullName : "",
                                        ToLawunitName = (x.ToLawunit != null) ? x.ToLawunit.FullName : ""
                                    }).ToList();

            return result;
        }

        public async Task<SaveResultVM> DeclareChange(int id)
        {
            try
            {
                var model = await repo.All<CaseSelectionChange>()
                                    .Where(x => x.Id == id)
                                    .FirstOrDefaultAsync().ConfigureAwait(false);

                model.DeclaredDate = DateTime.Now;
                model.ChangeStateId = NomenclatureConstants.CaseSelectionChangeStates.Declared;
                await repo.SaveChangesAsync().ConfigureAwait(false);

                return new SaveResultVM(true)
                {
                    AuditInfo = getAuditInfo(id)
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"DeclareChange id={id}", ex);
                return new SaveResultVM(false);
            }
        }

        public async Task<SaveResultVM> EnforceChange(int id)
        {


            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.CaseSelectionChangeProtokol
            }).ConfigureAwait(false);


            try
            {
                bool isOk = false;
                //using (var ts = repo.BeginTransaction())
                //{
                var model = await repo.All<CaseSelectionChange>()
                                .Include(x => x.CaseList)
                                .Where(x => x.Id == id)
                                .FirstOrDefaultAsync().ConfigureAwait(false);
                isOk = model.CaseList.Any();

                foreach (var _case in model.CaseList)
                {
                    if ((model.FromLawunitId > 0) && (_case.CaseLawunitDismisalId == null))
                    {
                        var res = lawunitService.CreateAutomaticDismissalForJudgeReporter(_case.CaseId, model.Desription, model.DeclaredDate.Value);

                        if (!res.Result || res.ObjectId == null)
                        {
                            continue;
                        }

                        _case.CaseLawunitDismisalId = (int)res.ObjectId;
                    }

                    if (_case.CaseSelectionProtokolId > 0)
                    {
                        continue;
                    }

                    var protRes = protokolService.SaveAutomaticSelectionProtocol(_case.CaseId, _case.CaseLawunitDismisalId.Value,
                                            model.ToLawunitId.Value, model.Desription, model.ToLawunitDepartmentId);

                    if (!protRes.Result || protRes.ObjectId == null)
                    {
                        continue;
                    }
                    _case.CaseSelectionProtokolId = (int)protRes.ObjectId;

                    if (_case.CaseSelectionProtokolId > 0)
                    {
                        var selectionProtokol = repo.GetById<CaseSelectionProtokol>((int)_case.CaseSelectionProtokolId);
                        var newJudgeCaseLawunitId = repo.AllReadonly<CaseLawUnit>()
                                                            .Where(x => x.CaseId == _case.CaseId
                                                            && x.LawUnitId == x.LawUnitId
                                                            && x.CaseSessionId == null
                                                            && x.DateTo == null)
                                                            .OrderByDescending(x => x.Id)
                                                            .Select(x => x.Id).FirstOrDefault();

                        var protRequest = new CdnUploadRequest()
                        {
                            SourceId = _case.CaseSelectionProtokolId.ToString(),
                            SourceType = SourceTypeSelectVM.CaseSelectionProtokol,
                            Title = fileModel.FileTitle,
                            FileName = fileModel.FileName,
                            ContentType = fileModel.ContentType,
                            FileContentBase64 = fileModel.FileContentBase64
                        };

                        var uplResult = await cdnService.MongoCdn_UploadFile(protRequest).ConfigureAwait(false);
                        isOk &= uplResult.Succeded;

                        /*
                        if (uplResult.Succeded && newJudgeCaseLawunitId > 0)
                        {
                            //Интеграциите се създават в protokolService.CaseSelectionProtokol_UpdateBeforeAfterSign
                            //mqEpepService.AppendJudgeReporter(newJudgeCaseLawunitId, EpepConstants.ServiceMethod.Add);
                            //mqEpepService.AppendCaseSelectionProtocol(selectionProtokol, EpepConstants.ServiceMethod.Add);

                            courtLawUnitService.CourtDepartmentUnitOrder_ActualizeForCase(_case.CaseId);
                        }
                        */
                    }
                    else
                    {
                        continue;
                    }


                    isOk &= await protokolService.CaseSelectionProtokol_UpdateBeforeAfterSign(_case.CaseSelectionProtokolId.Value);

                    if (model.ToLawunitDepartmentId > 0)
                    {
                        lawunitService.GetCaseLawUnitChangeDepRol_Save(new CaseLawUnitChangeDepRolVM()
                        {
                            CaseId = _case.CaseId,
                            DepartmentId = model.ToLawunitDepartmentId.Value
                        });
                    }
                }

                if (isOk && !model.CaseList.Where(x => x.CaseSelectionProtokolId == null).Any())
                {
                    model.ChangeStateId = NomenclatureConstants.CaseSelectionChangeStates.Saved;
                    await repo.SaveChangesAsync().ConfigureAwait(false);

                    //    scope.Complete();
                    //}
                }

                return new SaveResultVM(isOk);
            }
            catch (Exception ex)
            {
                logger.LogError($"EnforceChange id={id}", ex);
                return new SaveResultVM(false);
            }
        }
        public async Task<SaveResultVM> EnforceChangeJudge(int id)
        {


            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect()
            {
                SourceId = id.ToString(),
                SourceType = SourceTypeSelectVM.CaseSelectionChangeProtokol
            }).ConfigureAwait(false);


            try
            {
                bool isOk = false;
                //using (var ts = repo.BeginTransaction())
                //{
                var model = await repo.All<CaseSelectionChange>()
                                .Include(x => x.CaseList)
                                .Where(x => x.Id == id)
                                .FirstOrDefaultAsync().ConfigureAwait(false);
                isOk = model.CaseList.Any();

                foreach (var _case in model.CaseList)
                {
                    var judgeRoleIdInCase = lawunitService.GetJudgeRoleChlenSastav(model.FromLawunitId.Value, _case.CaseId);
                    if ((model.FromLawunitId > 0) && (_case.CaseLawunitDismisalId == null))
                    {

                        var res = lawunitService.CreateAutomaticDismissalForJudgeChlenSastav(model.FromLawunitId.Value, _case.CaseId, model.Desription, model.DeclaredDate.Value);

                        if (!res.Result || res.ObjectId == null)
                        {
                            continue;
                        }

                        _case.CaseLawunitDismisalId = (int)res.ObjectId;
                    }

                    if (_case.CaseSelectionProtokolId > 0)
                    {
                        continue;
                    }

                    var protRes = protokolService.SaveAutomaticSelectionProtocolChlenSastav(_case.CaseId, _case.CaseLawunitDismisalId.Value, model.ToLawunitId.Value, judgeRoleIdInCase, model.Desription, model.ToLawunitDepartmentId);

                    if (!protRes.Result || protRes.ObjectId == null)
                    {
                        continue;
                    }
                    _case.CaseSelectionProtokolId = (int)protRes.ObjectId;

                    if (_case.CaseSelectionProtokolId > 0)
                    {
                        var selectionProtokol = repo.GetById<CaseSelectionProtokol>((int)_case.CaseSelectionProtokolId);
                        var newJudgeCaseLawunitId = repo.AllReadonly<CaseLawUnit>()
                                                            .Where(x => x.CaseId == _case.CaseId
                                                            && x.LawUnitId == x.LawUnitId
                                                            && x.CaseSessionId == null
                                                            && x.DateTo == null)
                                                            .OrderByDescending(x => x.Id)
                                                            .Select(x => x.Id).FirstOrDefault();

                        var protRequest = new CdnUploadRequest()
                        {
                            SourceId = _case.CaseSelectionProtokolId.ToString(),
                            SourceType = SourceTypeSelectVM.CaseSelectionProtokol,
                            Title = fileModel.FileTitle,
                            FileName = fileModel.FileName,
                            ContentType = fileModel.ContentType,
                            FileContentBase64 = fileModel.FileContentBase64
                        };

                        var uplResult = await cdnService.MongoCdn_UploadFile(protRequest).ConfigureAwait(false);
                        isOk &= uplResult.Succeded;

                        /*
                        if (uplResult.Succeded && newJudgeCaseLawunitId > 0)
                        {
                            //Интеграциите се създават в protokolService.CaseSelectionProtokol_UpdateBeforeAfterSign
                            //mqEpepService.AppendJudgeReporter(newJudgeCaseLawunitId, EpepConstants.ServiceMethod.Add);
                            //mqEpepService.AppendCaseSelectionProtocol(selectionProtokol, EpepConstants.ServiceMethod.Add);

                            courtLawUnitService.CourtDepartmentUnitOrder_ActualizeForCase(_case.CaseId);
                        }
                        */
                    }
                    else
                    {
                        continue;
                    }


                    isOk &= await protokolService.CaseSelectionProtokol_UpdateBeforeAfterSign(_case.CaseSelectionProtokolId.Value);

                    if (model.ToLawunitDepartmentId > 0)
                    {
                        lawunitService.GetCaseLawUnitChangeDepRol_Save(new CaseLawUnitChangeDepRolVM()
                        {
                            CaseId = _case.CaseId,
                            DepartmentId = model.ToLawunitDepartmentId.Value
                        });
                    }
                }

                if (isOk && !model.CaseList.Where(x => x.CaseSelectionProtokolId == null).Any())
                {
                    model.ChangeStateId = NomenclatureConstants.CaseSelectionChangeStates.Saved;
                    await repo.SaveChangesAsync().ConfigureAwait(false);

                    //    scope.Complete();
                    //}
                }

                return new SaveResultVM(isOk);
            }
            catch (Exception ex)
            {
                logger.LogError($"EnforceChange id={id}", ex);
                return new SaveResultVM(false);
            }
        }

    }
}
