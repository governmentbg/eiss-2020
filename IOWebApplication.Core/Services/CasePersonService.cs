using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using IOWebApplication.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using System.Linq.Expressions;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Documents;

namespace IOWebApplication.Core.Services
{
    public class CasePersonService : BaseService, ICasePersonService
    {
        private readonly INomenclatureService nomenclatureService;
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly IMQEpepService mqEpepService;
        private readonly ICommonService commonService;
        private readonly IEisppService eisppService;

        public CasePersonService(
            ILogger<CasePersonService> _logger,
            INomenclatureService _nomenclatureService,
            ICasePersonLinkService _casePersonLinkService,
            AutoMapper.IMapper _mapper,
            IMQEpepService _mqEpepService,
            IRepository _repo,
            IUserContext _userContext,
            ICommonService _commonService,
            IEisppService _eisppService)
        {
            logger = _logger;
            mapper = _mapper;
            nomenclatureService = _nomenclatureService;
            repo = _repo;
            mqEpepService = _mqEpepService;
            userContext = _userContext;
            casePersonLinkService = _casePersonLinkService;
            commonService = _commonService;
            eisppService = _eisppService;
        }

        /// <summary>
        /// Извличане на данни за лица по дело/заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="checkSessionDate"></param>
        /// <param name="showExpired"></param>
        /// <param name="setRowNumberFromCase">Ако е за заседание и е true да вземе rownumber от делото за този идентификатор</param>
        /// <returns></returns>
        public IQueryable<CasePersonListVM> CasePerson_Select(int caseId, int? caseSessionId, bool checkSessionDate, bool showExpired, bool setRowNumberFromCase)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            Expression<Func<CasePerson, bool>> checkDateWhere = x => true;
            if (checkSessionDate == true && (caseSessionId ?? 0) > 0)
                checkDateWhere = x => ((x.DateTo ?? dateEnd) >= x.CaseSession.DateFrom);

            bool setRowNumber = false;
            List<CasePerson> casePersons = null;
            if (setRowNumberFromCase && (caseSessionId ?? 0) > 0)
            {
                setRowNumber = true;
                casePersons = repo.AllReadonly<CasePerson>()
                                .Where(x => x.CaseId == caseId && x.CaseSessionId == null)
                                .ToList();
            }

            var casePersonLists = repo.AllReadonly<CasePerson>()
                                       .Include(x => x.Case)
                                       .ThenInclude(x => x.Document)
                                       .Include(x => x.Case)
                                       .ThenInclude(x => x.CaseType)
                                       .Include(x => x.PersonRole)
                                       .Include(x => x.CaseSession)
                                       .ThenInclude(x => x.SessionType)
                                       .Include(x => x.Addresses)
                                       .Include(x => x.UicType)
                                       .Where(x => x.CaseId == caseId &&
                                                   ((caseSessionId == null) ? true : (x.CaseSessionId ?? 0) == (caseSessionId ?? 0)))
                                       .Where(checkDateWhere)
                                       .Where(this.FilterExpireInfo<CasePerson>(showExpired))
                                       .Select(x => new CasePersonListVM()
                                       {
                                           Id = x.Id,
                                           CaseId = x.CaseId,
                                           CaseSessionId = x.CaseSessionId,
                                           Uic = x.Uic,
                                           UicTypeLabel = (x.UicType != null) ? x.UicType.Label : string.Empty,
                                           FullName = x.FullName,
                                           RoleName = x.PersonRole.Label,
                                           PersonRoleId = x.PersonRole.Id,
                                           PersonRoleLabel = x.PersonRole.Label,
                                           RoleKindId = x.PersonRole.RoleKindId,
                                           DateFrom = x.DateFrom,
                                           DateTo = x.DateTo,
                                           RowNumber = setRowNumber == false ? x.RowNumber :
                                                       casePersons.Where(a => x.CasePersonIdentificator == a.CasePersonIdentificator)
                                                       .Select(a => a.RowNumber)
                                                       .FirstOrDefault(),
                                           ForNotification = x.ForNotification,
                                           NotificationNumber = x.NotificationNumber,
                                           CaseSessionLabel = ((x.CaseSessionId != null) ? (x.CaseSession.SessionType.Label + " " + x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm")) : (string.Empty)),
                                           CasePersonIdentificator = x.CasePersonIdentificator,
                                           AddressString = (x.Addresses.Count > 0) ? ((x.Addresses.Any(a => ((a.ForNotification ?? false) == true)) ? x.Addresses.Where(c => c.ForNotification == true).FirstOrDefault().Address.FullAddress : x.Addresses.FirstOrDefault().Address.FullAddress)) : string.Empty,
                                           CurrentAddressString = (x.Addresses.Count > 0) ? x.Addresses.Where(a => a.Address.AddressTypeId == NomenclatureConstants.AddressType.Current).FirstOrDefault().Address.FullAddress : string.Empty,
                                           WorkAddressString = (x.Addresses.Count > 0) ? x.Addresses.Where(a => a.Address.AddressTypeId == NomenclatureConstants.AddressType.Work).FirstOrDefault().Address.FullAddress : string.Empty,
                                           IsViewPersonSentence = ((x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.RightSide) && (x.CaseSessionId == null) && (x.Case.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo) && ((x.Case.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.FirstInstance) || (x.Case.CaseType.CaseInstanceId == NomenclatureConstants.CaseInstanceType.SecondInstance))),
                                           IsIndividual = ((x.UicTypeId == NomenclatureConstants.UicTypes.BirthDate || x.UicTypeId == NomenclatureConstants.UicTypes.EGN || x.UicTypeId == NomenclatureConstants.UicTypes.LNCh) && (x.CaseSessionId == null)),
                                           IsViewPersonInheritance = ((x.CaseSessionId == null) && (x.PersonRoleId == NomenclatureConstants.PersonRole.Inheritor) && ((x.Case.Document.DocumentTypeId == NomenclatureConstants.DocumentType.Request51LawInheritance) || (x.Case.Document.DocumentTypeId == NomenclatureConstants.DocumentType.RequestAcceptanceInheritance) || (x.Case.Document.DocumentTypeId == NomenclatureConstants.DocumentType.RequestRefusalInheritance))),
                                           IsArrested = x.IsArrested ?? false,
                                       }).OrderBy(x => x.RoleKindId).ToList();

            foreach (var casePerson in casePersonLists)
            {
                var linkListVM = casePersonLinkService.GetLinkForPerson(casePerson.Id, false, 0, null);
                casePerson.LinkForPersonString = string.Empty;
                if (linkListVM != null)
                {
                    casePerson.LinkForPersonString = string.Join(", ", linkListVM.Select(x => x.Label));
                }
            }

            return casePersonLists.OrderBy(x => x.RoleKindId).AsQueryable();
        }

        /// <summary>
        /// Запис на лица по дело/заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CasePerson_SaveData(CasePersonVM model)
        {
            try
            {
                //Ако човека е с дата преди делото да стане датата на делото + 1 сек. заради електронната папка на делото и да се направи проверка за датите
                var caseData = GetById<Case>(model.CaseId);
                if (model.DateFrom <= caseData.RegDate)
                    model.DateFrom = caseData.RegDate.AddSeconds(1);

                if (model.DateTo != null)
                {
                    if (((DateTime)model.DateTo).Date < model.DateFrom.Date)
                    {
                        return (result: false, errorMessage: "От дата не може да е по-голяма от До дата");
                    }
                }

                //Ако не е наказателно първа инстанция и не е роля за задържан Isarrested = false
                if (NomenclatureConstants.CaseTypes.CaseTypeArrested.Contains(caseData.CaseTypeId))
                {
                    var isSideForArrested = repo.AllReadonly<PersonRoleGrouping>()
                         .Where(x => x.PersonRoleId == model.PersonRoleId &&
                         x.PersonRoleGroup == NomenclatureConstants.PersonRoleGroupings.RoleArrested).Any();
                    if (isSideForArrested == false)
                        model.IsArrested = false;
                }
                else
                {
                    model.IsArrested = false;
                }

                CasePerson casePerson = null;
                if (model.Id > 0)
                {
                    casePerson = repo.GetById<CasePerson>(model.Id);

                }
                else
                {
                    casePerson = new CasePerson();
                    casePerson.CourtId = userContext.CourtId;
                    casePerson.CaseId = model.CaseId;
                    casePerson.CaseSessionId = model.CaseSessionId;
                    casePerson.RowNumber = repo.AllReadonly<CasePerson>().Where(a => a.CaseId == casePerson.CaseId &&
                                             (a.CaseSessionId ?? 0) == (casePerson.CaseSessionId ?? 0))
                                                .Select(x => x.RowNumber)
                                                .DefaultIfEmpty(0)
                                                .Max() + 1;

                }

                casePerson.PersonRoleId = model.PersonRoleId;
                casePerson.PersonMaturityId = model.PersonMaturityId.EmptyToNull();
                casePerson.MilitaryRangId = model.MilitaryRangId.EmptyToNull();
                casePerson.IsInitialPerson = false; //засега не го искат, но казаха, че по късно пак може да го поискат casePerson.CaseSessionId != null ? false : model.IsInitialPerson; //Ако е заседание FALSE
                casePerson.DateFrom = model.DateFrom;
                casePerson.DateTo = model.DateTo;
                casePerson.IsArrested = model.IsArrested;
                casePerson.CompanyTypeId = model.CompanyTypeId.EmptyToNull();
                casePerson.TaxNumber = model.TaxNumber;
                casePerson.ReRegisterDate = model.ReRegisterDate;
                casePerson.IsDeceased = model.IsDeceased;
                casePerson.UserId = userContext.UserId;
                casePerson.DateWrt = DateTime.Now;

                casePerson.CopyFrom(model);
                PersonNamesBase_SaveData(casePerson);

                if (model.Id > 0)
                {
                    CreateHistory<CasePerson, CasePersonH>(casePerson);

                    //Update
                    repo.Update(casePerson);
                    repo.SaveChanges();

                    mqEpepService.AppendCasePerson(casePerson, EpepConstants.ServiceMethod.Update);
                }
                else
                {
                    casePerson.CasePersonIdentificator = Guid.NewGuid().ToString().ToLower();
                    CreateHistory<CasePerson, CasePersonH>(casePerson);

                    if (model.FromPersonId > 0)
                    {
                        casePerson.Addresses = repo.AllReadonly<CasePersonAddress>().Include(x => x.Address).Where(x => x.CasePersonId == model.FromPersonId).ToList();
                        foreach (var itemAddress in casePerson.Addresses)
                        {
                            itemAddress.Id = 0;
                            itemAddress.CourtId = casePerson.CourtId;
                            itemAddress.CaseId = casePerson.CaseId;
                            itemAddress.AddressId = 0;
                            itemAddress.CasePersonId = 0;
                            itemAddress.Address.Id = 0;
                            itemAddress.UserId = userContext.UserId;
                            itemAddress.DateWrt = DateTime.Now;
                            itemAddress.CasePersonAddressIdentificator = Guid.NewGuid().ToString().ToLower();

                            CreateHistory<CasePersonAddress, CasePersonAddressH>(itemAddress);
                        }
                    }
                    else
                    {
                        if ((model.Person_SourceType == SourceTypeSelectVM.Instutution || model.Person_SourceType == SourceTypeSelectVM.Court)
                            && (model.Person_SourceId ?? 0) > 0)
                        {
                            var instAddress = commonService.SelectEntity_SelectAddress(model.Person_SourceType ?? 0, model.Person_SourceId ?? 0);
                            foreach (var adr in instAddress)
                            {
                                CasePersonAddress itemAddress = new CasePersonAddress();
                                itemAddress.CaseId = casePerson.CaseId;
                                itemAddress.CourtId = userContext.CourtId;
                                itemAddress.Address = new Address();
                                itemAddress.Address.CopyFrom(adr);
                                itemAddress.UserId = userContext.UserId;
                                itemAddress.DateWrt = DateTime.Now;
                                itemAddress.CasePersonAddressIdentificator = Guid.NewGuid().ToString().ToLower();
                                casePerson.Addresses.Add(itemAddress);

                                CreateHistory<CasePersonAddress, CasePersonAddressH>(itemAddress);
                            }
                        }
                    }
                    //Insert
                    repo.Add<CasePerson>(casePerson);
                    repo.SaveChanges();

                    mqEpepService.AppendCasePerson(casePerson, EpepConstants.ServiceMethod.Add);
                }
                model.Id = casePerson.Id;
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CasePerson Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за лица по дело/заседание по ид
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CasePersonVM CasePerson_GetById(int id)
        {
            var casePerson = repo.AllReadonly<CasePerson>()
                               .Include(x => x.Case)
                               .Where(a => a.Id == id).FirstOrDefault();
            var casePersonVM = new CasePersonVM();
            casePersonVM.Id = casePerson.Id;
            casePersonVM.CourtId = casePerson.CourtId;
            casePersonVM.CaseId = casePerson.CaseId;
            casePersonVM.CaseTypeId = casePerson.Case.CaseTypeId;
            casePersonVM.CaseGroupId = casePerson.Case.CaseGroupId;
            casePersonVM.CaseSessionId = casePerson.CaseSessionId;
            casePersonVM.PersonRoleId = casePerson.PersonRoleId;
            casePersonVM.PersonMaturityId = casePerson.PersonMaturityId;
            casePersonVM.MilitaryRangId = casePerson.MilitaryRangId;
            //casePersonVM.IsInitialPerson = casePerson.IsInitialPerson;
            casePersonVM.DateFrom = casePerson.DateFrom;
            casePersonVM.DateTo = casePerson.DateTo;
            casePersonVM.IsExpired = casePerson.DateExpired != null;
            casePersonVM.IsArrested = casePerson.IsArrested ?? false;
            casePersonVM.IsDeceased = casePerson.IsDeceased ?? false;
            casePersonVM.CompanyTypeId = casePerson.CompanyTypeId;
            casePersonVM.ReRegisterDate = casePerson.ReRegisterDate;
            casePersonVM.TaxNumber = casePerson.TaxNumber;
            casePersonVM.CopyFrom(casePerson);

            return casePersonVM;
        }

        /// <summary>
        /// Извличане на адреси на лица по дело/заседание
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public IQueryable<CasePersonAddressListVM> CasePersonAddress_Select(int casePersonId)
        {
            return repo.AllReadonly<CasePersonAddress>()
                .Include(x => x.Address)
                .Include(x => x.Address.AddressType)
                .Where(x => x.CasePersonId == casePersonId)
                .Where(FilterExpireInfo<CasePersonAddress>(false))
                .Select(x => new CasePersonAddressListVM()
                {
                    Id = x.Id,
                    FullAddress = x.Address.FullAddress,
                    ForNotification = x.ForNotification ?? false,
                    AddressTypeName = x.Address.AddressType.Label
                }).AsQueryable();
        }

        /// <summary>
        /// Запис на адрес на лица по дело/заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CasePersonAddress_SaveData(CasePersonAddress model)
        {
            try
            {
                if (model.Address.AddressTypeId == NomenclatureConstants.AddressType.Permanent)
                {
                    var existsPermanentAddress = repo.AllReadonly<CasePersonAddress>().Where(x => x.CasePersonId == model.CasePersonId &&
                                    x.Address.AddressTypeId == NomenclatureConstants.AddressType.Permanent &&
                                    x.Id != model.Id).Any();
                    if (existsPermanentAddress == true)
                    {
                        return (result: false, errorMessage: "Не може да има повече от един постоянен адрес. При необходимост коригирайте данните във вече въведения постоянен адрес.");
                    }
                }

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.All<CasePersonAddress>().Include(x => x.Address).Where(x => x.Id == model.Id).FirstOrDefault();
                    saved.Address.CopyFrom(model.Address);
                    nomenclatureService.SetFullAddress(saved.Address);
                    saved.ForNotification = model.ForNotification;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;

                    CreateHistory<CasePersonAddress, CasePersonAddressH>(saved);

                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    nomenclatureService.SetFullAddress(model.Address);

                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    model.CasePersonAddressIdentificator = Guid.NewGuid().ToString().ToLower();

                    CreateHistory<CasePersonAddress, CasePersonAddressH>(model);

                    repo.Add<CasePersonAddress>(model);
                    repo.SaveChanges();
                }
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CasePersonAddress Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за адрес на лица по дело/заседание
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CasePersonAddress CasePersonAddress_GetById(int id)
        {
            return repo.AllReadonly<CasePersonAddress>().Include(x => x.Address).Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Извличане на данни за комбо на лица по дело/заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_Case_CaseSession_ForPersonCopy(int caseId, int caseSessionId)
        {
            //var result = repo.AllReadonly<CaseSession>()
            //     .Include(x => x.SessionType)
            //     .Where(x => x.CaseId == caseId && x.Id < caseSessionId)
            //     .OrderByDescending(x => x.Id)
            //                     .Select(x => new SelectListItem()
            //                     {
            //                         Value = x.Id.ToString(),
            //                         Text = x.SessionType.Label + " от " + x.DateFrom.ToString("dd.MM.yyyy HH:mm")
            //                     }).ToList();

            List<SelectListItem> result = new List<SelectListItem>();
            result.Insert(result.Count(), new SelectListItem { Value = "0", Text = "От делото" });
            return result;
        }

        /// <summary>
        /// Връща списък с елементите за чекбокса
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        private IList<CheckListVM> FillCheckListVMs(int caseId, int? caseSessionId)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            var casePerson = CasePerson_Select(caseId, caseSessionId, false, false, false);

            foreach (var person in casePerson)
                checkListVMs.Add(new CheckListVM() { Checked = true, Value = person.Id.ToString(), Label = person.FullName + "(" + (person.Uic ?? "") + ") - " + person.RoleName });

            return checkListVMs.OrderBy(x => x.Label).ToList();
        }

        /// <summary>
        /// Връща списък с елементите за чекбокса
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        private IList<CheckListVM> FillCheckListVMs_SelectForCheck(int caseId, int? caseSessionId, int realCaseSessionId)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            var casePerson = CasePerson_Select(caseId, caseSessionId, false, false, false);
            var casePersonRealSession = repo.AllReadonly<CasePerson>().Where(x => x.CaseId == caseId && x.CaseSessionId == realCaseSessionId).ToList();
            var caseSession = GetById<CaseSession>(realCaseSessionId);

            DateTime dateEnd = DateTime.Now.AddYears(100);
            foreach (var person in casePerson.Where(x => ((x.DateTo ?? dateEnd) >= caseSession.DateFrom)))
            {
                bool check = casePersonRealSession.Where(x => (((x.DateTo ?? dateEnd) >= caseSession.DateFrom)) && (x.CasePersonIdentificator == person.CasePersonIdentificator)).Any();
                checkListVMs.Add(new CheckListVM() { Checked = check, Value = person.Id.ToString(), Label = person.FullName + "(" + (person.Uic ?? "") + ") - " + person.RoleName });
            }

            return checkListVMs.OrderBy(x => x.Label).ToList();
        }

        /// <summary>
        /// Извличане на данни за лица по дело/заседание за чекбокс
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="realCaseSessionId"></param>
        /// <returns></returns>
        public CheckListViewVM CasePerson_SelectForCheck(int caseId, int caseSessionId, int realCaseSessionId)
        {
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = caseId,
                ObjectId = realCaseSessionId,
                Label = "Изберете страни",
                checkListVMs = FillCheckListVMs_SelectForCheck(caseId, caseSessionId, realCaseSessionId)
            };

            return checkListViewVM;
        }

        /// <summary>
        /// Извличане на данни за лица по дело/заседание за чекбокс по дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public CheckListViewVM CasePersonPrint_SelectForCheck(int caseId)
        {
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = caseId,
                ObjectId = 0,
                Label = "Изберете страни, които да се включат в списъка на лицата по делото",
                ButtonLabel = "Потвърди",
                checkListVMs = FillCheckListVMs(caseId, 0)
            };

            return checkListViewVM;
        }

        /// <summary>
        /// Извличане на лица по дело/заседание за чекбокс за уведонление
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        private IList<CheckListVM> FillCheckListVMs_ForNotification(int caseId, int caseSessionId)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            var casePerson = CasePerson_Select(caseId, caseSessionId, false, false, false);

            foreach (var person in casePerson)
                checkListVMs.Add(new CheckListVM() { Checked = (person.ForNotification == true), Value = person.Id.ToString(), Label = person.FullName + "(" + (person.Uic ?? "") + ") - " + person.RoleName });

            return checkListVMs.OrderBy(x => x.Label).ToList();
        }

        /// <summary>
        /// Извличане на лица по дело/заседание за чекбокс за уведонление
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public CheckListViewVM CasePersonNotification_SelectForCheck(int caseId, int caseSessionId)
        {
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = caseId,
                ObjectId = caseSessionId,
                Label = "Изберете страни",
                checkListVMs = FillCheckListVMs_ForNotification(caseId, caseSessionId)
            };

            return checkListViewVM;
        }

        /// <summary>
        /// Добавяне премахване на лица по заседание от дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseOldSession"></param>
        /// <param name="caseNewSessionId"></param>
        /// <param name="casePersonList"></param>
        /// <param name="caseNewSessionDateFrom"></param>
        public void SetCasePersonDataForCopySession(int caseId, int? caseOldSession, int caseNewSessionId, List<CasePerson> casePersonList, DateTime caseNewSessionDateFrom)
        {
            var casePersonCurrentSession = repo.AllReadonly<CasePerson>()
                                     .Include(x => x.Addresses)
                                     .ThenInclude(x => x.Address)
                                     .Where(x => x.CaseId == caseId &&
                                         (x.CaseSessionId ?? 0) == caseNewSessionId).ToList();

            //Всички, които ги има в заседанието и са активни, но не са чекнати да ги затърка
            DateTime dateEnd = DateTime.Now.AddYears(100);
            CaseSession caseNewSession = null;
            if (caseNewSessionId > 0)
                caseNewSession = GetById<CaseSession>(caseNewSessionId);
            foreach (var item in casePersonCurrentSession)
            {
                if ((item.DateTo ?? dateEnd) >= caseNewSession.DateFrom && casePersonList.Where(x => x.CasePersonIdentificator == item.CasePersonIdentificator).Any() == false)
                {
                    item.DateTo = caseNewSession.DateFrom.AddDays(-1);
                    CreateHistory<CasePerson, CasePersonH>(item);
                    repo.Update(item);
                }
            }

            int rownumber = casePersonCurrentSession.Select(x => x.RowNumber).DefaultIfEmpty(0).Max() + 1;
            foreach (var item in casePersonList)
            {
                //Ако го има и има DateTo преди заседанието да му я смени и да ъпдейтне данните, а ако го няма да го добави
                var personCurrentSession = casePersonCurrentSession.Where(x => x.CasePersonIdentificator == item.CasePersonIdentificator).FirstOrDefault();
                if (personCurrentSession != null)
                {
                    if ((personCurrentSession.DateTo ?? dateEnd) < caseNewSession.DateFrom)
                    {
                        ReloadPersonData_Item(personCurrentSession, item);
                    }
                }
                else
                {
                    int idOld = item.Id;
                    item.Id = 0;
                    item.CaseSessionId = caseNewSessionId;
                    item.RowNumber = rownumber;
                    item.IsInitialPerson = false; //за заседание това поле винаги е FALSE
                    item.UserId = userContext.UserId;
                    item.DateWrt = DateTime.Now;
                    item.DateFrom = (caseNewSession != null) ? caseNewSession.DateFrom : caseNewSessionDateFrom;

                    rownumber++;
                    foreach (var itemAddress in item.Addresses)
                    {
                        itemAddress.Id = 0;
                        itemAddress.AddressId = 0;
                        itemAddress.CasePersonId = 0;
                        itemAddress.Address.Id = 0;
                        itemAddress.UserId = userContext.UserId;
                        itemAddress.DateWrt = DateTime.Now;

                        CreateHistory<CasePersonAddress, CasePersonAddressH>(itemAddress);
                    }

                    CreateHistory<CasePerson, CasePersonH>(item);
                    repo.Add<CasePerson>(item);
                }
            }
        }

        /// <summary>
        /// запис на копирани на лица по дело/заседание
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="caseId"></param>
        /// <param name="caseNewSessionId"></param>
        /// <returns></returns>
        public bool CasePerson_CopyCasePerson(string ids, int caseId, int caseNewSessionId)
        {
            try
            {
                List<string> id = ids.Split(',').ToList();
                var casePersonList = repo.AllReadonly<CasePerson>().Include(x => x.Addresses).ThenInclude(x => x.Address)
                                         .Where(x => id.Contains(x.Id.ToString())).OrderBy(x => x.RowNumber).ToList();
                var caseOldSession = casePersonList.Select(x => x.CaseSessionId).FirstOrDefault();

                SetCasePersonDataForCopySession(caseId, caseOldSession, caseNewSessionId, casePersonList, DateTime.Now);

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CasePerson_Copy CaseId={ caseId }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за лица по дело/заседание за чекбокс
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="roleKindIds"></param>
        /// <param name="defaultElementText"></param>
        /// <returns></returns>
        public List<SelectListItem> CasePerson_SelectForDropDownList(int caseId, int? caseSessionId, string roleKindIds = "", string defaultElementText = "")
        {
            Expression<Func<CasePerson, bool>> roleKindWhere = x => true;
            if (string.IsNullOrEmpty(roleKindIds) == false)
            {
                string[] roles = roleKindIds.Split(',');
                roleKindWhere = x => roles.Contains(x.PersonRole.RoleKindId.ToString());
            }

            var result = repo.AllReadonly<CasePerson>()
                .Include(x => x.PersonRole)
                .Include(x => x.PersonRole.RoleKind)
                .Where(x => x.CaseId == caseId &&
                            (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) &&
                            x.DateExpired == null)
                .Where(roleKindWhere)
                 .OrderBy(x => x.RowNumber)
                                 .Select(x => new SelectListItem()
                                 {
                                     Value = x.Id.ToString(),
                                     Text = x.FullName + "(" + (x.Uic ?? "") + ") - " + x.PersonRole.Label
                                 }).ToList();

            if (string.IsNullOrEmpty(defaultElementText))
                defaultElementText = "Избери";
            result.Insert(0, new SelectListItem() { Text = defaultElementText, Value = "-1" });

            return result;
        }


        /// <summary>
        /// Извличане на данни за лица по дело/заседание за комбобокс
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="addLinkName3"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList(int caseId, int? caseSessionId, bool addLinkName3, int? notificationTypeId, int? casePersonId, bool filterPersonOnNotification, bool addDefaultElement = true, bool addAllElement = false)
        {
            int notificationListTypeId = NomenclatureConstants.NotificationType.ToListType(notificationTypeId);
            var caseSessionNotificationLists = repo.AllReadonly<CaseSessionNotificationList>()
                                                   .Where(x => x.CaseSessionId == caseSessionId &&
                                                               x.DateExpired == null &&
                                                               x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson &&
                                                               (
                                                                    notificationListTypeId <= 0 ||
                                                                    x.NotificationListTypeId == notificationListTypeId ||
                                                                    (x.NotificationListTypeId == null && notificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList)
                                                               )
                                                   )
                                                   .ToList();
            var casePersons = repo.AllReadonly<CasePerson>()
                                 .Where(x => x.CaseId == caseId &&
                                             x.CaseSessionId == null);

            var result = repo.AllReadonly<CasePerson>()
                  .Where(x => x.CaseId == caseId &&
                           (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) &&
                           (x.CaseSessionId == null || casePersons.Any(p => p.CasePersonIdentificator == x.CasePersonIdentificator && p.DateExpired == null)) &&
                           (!filterPersonOnNotification || caseSessionNotificationLists.Any(l => l.CasePersonId == x.Id) || x.Id == casePersonId)
                      )
                .Select(x => new SelectListItem()
                {
                    Text = x.FullName + " (" + x.PersonRole.Label + ")" +
                           (caseSessionNotificationLists.Any(y => y.CasePersonId == x.Id) ? (" (Призован номер " + caseSessionNotificationLists.Where(y => y.CasePersonId == x.Id).FirstOrDefault().RowNumber + ")") : string.Empty)
                          ,
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
            }

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Извличане на данни за лица по дело/заседание само дясна част за комбобокс
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_RightSide(int caseId, int? caseSessionId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseSessionNotificationLists = repo.AllReadonly<CaseSessionNotificationList>()
                                                   .Where(x => x.CaseSessionId == caseSessionId && x.DateExpired == null && x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CasePerson).ToList();
            var result = repo.All<CasePerson>()
                .Include(x => x.PersonRole)
                .Where(x => x.CaseId == caseId && (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) && (x.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide))
                .Select(x => new SelectListItem()
                {
                    Text = x.FullName + " (" + x.PersonRole.Label + ")" + (caseSessionNotificationLists.Any(y => y.CasePersonId == x.Id) ? (" (Призован номер " + caseSessionNotificationLists.Where(y => y.CasePersonId == x.Id).FirstOrDefault().RowNumber + ")") : string.Empty),
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (result.Count > 0)
            {
                result = result.OrderBy(x => x.Text).ToList();
            }

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Запис на данни за лица по дело/заседание свързани с уведомление
        /// </summary>
        /// <param name="checkListViewVM"></param>
        /// <returns></returns>
        public bool CasePerson_SaveNotification(CheckListViewVM checkListViewVM)
        {
            var casePersons = repo.All<CasePerson>()
                .Where(x => x.CaseId == checkListViewVM.CourtId && (x.CaseSessionId == checkListViewVM.ObjectId))
                .OrderBy(x => x.NotificationNumber)
                .ToList();

            try
            {
                foreach (var person in casePersons)
                {
                    person.ForNotification = checkListViewVM.checkListVMs.Where(p => int.Parse(p.Value) == person.Id).Select(p => p.Checked).FirstOrDefault();

                    if (person.ForNotification ?? false)
                    {
                        if (person.NotificationNumber == null)
                            person.NotificationNumber = (casePersons.Max(x => (int?)x.NotificationNumber) ?? 0) + 1;
                        else
                            person.NotificationNumber = (person.NotificationNumber == 0) ? casePersons.Max(x => x.NotificationNumber) + 1 : person.NotificationNumber;
                    }
                    else
                        person.NotificationNumber = 0;
                }

                int _num = 0;

                foreach (var person in casePersons.Where(x => x.NotificationNumber > 0).OrderBy(x => x.NotificationNumber))
                {
                    _num++;
                    person.NotificationNumber = _num;
                    repo.Update(person);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseNotification CaseId={ checkListViewVM.CourtId }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за лица по дело/заседание за справка
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="uic"></param>
        /// <param name="fullName"></param>
        /// <param name="caseRegnumber"></param>
        /// <returns></returns>
        public IQueryable<CasePersonReportVM> CasePerson_SelectForReport(int courtId, string uic, string fullName, string caseRegnumber, DateTime? DateFrom, DateTime? DateTo, DateTime? FinalDateFrom, DateTime? FinalDateTo, DateTime? WithoutFinalDateTo)
        {
            var resultFinish = repo.AllReadonly<SessionResultGrouping>()
                                   .Where(x => x.SessionResultGroup == NomenclatureConstants.SessionResultGroupings.CaseWithoutFinalAct_Result)
                                   .Select(x => x.SessionResultId)
                                   .ToList();

            DateFrom = NomenclatureExtensions.ForceStartDate(DateFrom);
            DateTo = NomenclatureExtensions.ForceEndDate(DateTo);
            FinalDateFrom = NomenclatureExtensions.ForceStartDate(FinalDateFrom);
            FinalDateTo = NomenclatureExtensions.ForceEndDate(FinalDateTo);
            WithoutFinalDateTo = NomenclatureExtensions.ForceEndDate(WithoutFinalDateTo);

            uic = uic?.ToLower();
            fullName = fullName?.ToLower();
            caseRegnumber = caseRegnumber?.ToLower();

            Expression<Func<CasePerson, bool>> uicSearch = x => true;
            if (!string.IsNullOrEmpty(uic))
                uicSearch = x => x.Uic.ToLower() == uic;

            Expression<Func<CasePerson, bool>> nameSearch = x => true;
            if (!string.IsNullOrEmpty(fullName))
                nameSearch = x => EF.Functions.ILike(x.FullName, fullName.ToPaternSearch());

            Expression<Func<CasePerson, bool>> caseNumberSearch = x => true;
            if (!string.IsNullOrEmpty(caseRegnumber))
                caseNumberSearch = x => x.Case.RegNumber.ToLower().EndsWith(caseRegnumber.ToShortCaseNumber());

            Expression<Func<CasePerson, bool>> caseRegDateSearch = x => true;
            if ((DateFrom != null) && (DateTo != null))
                caseRegDateSearch = x => x.Case.RegDate >= DateFrom && x.Case.RegDate <= DateTo;

            Expression<Func<CasePerson, bool>> withFinalActSearch = x => true;
            if ((FinalDateFrom != null) && (FinalDateTo != null))
                withFinalActSearch = x => ((x.Case.CaseSessions.Any(a => a.DateExpired == null &&
                                                                         a.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                    (b.ActDeclaredDate >= FinalDateFrom &&
                                                                                                    b.ActDeclaredDate <= FinalDateTo) && b.IsFinalDoc &&
                                                                                                    (b.ActStateId != NomenclatureConstants.SessionActState.Project && b.ActStateId != NomenclatureConstants.SessionActState.Registered) &&
                                                                                                    b.CaseSession.CaseSessionResults.Any(r => r.DateExpired == null && resultFinish.Contains(r.SessionResultId))))));

            Expression<Func<CasePerson, bool>> withoutFinalActSearch = x => true;
            if (WithoutFinalDateTo != null)
                withoutFinalActSearch = x => ((x.Case.RegDate <= WithoutFinalDateTo) && ((!x.Case.CaseSessions.Any(a => a.DateExpired == null &&
                                                                                                                        a.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                                                                                                   (b.ActDeclaredDate != null &&
                                                                                                                                                   b.ActDeclaredDate <= WithoutFinalDateTo) && b.IsFinalDoc &&
                                                                                                                                                   (b.ActStateId != NomenclatureConstants.SessionActState.Project && b.ActStateId != NomenclatureConstants.SessionActState.Registered) &&
                                                                                                                                                   b.CaseSession.CaseSessionResults.Any(r => r.DateExpired == null && resultFinish.Contains(r.SessionResultId)))))));

            return repo.AllReadonly<CasePerson>()
                       .Include(x => x.PersonRole)
                       .Include(x => x.Case)
                       .Include(x => x.Case.CaseState)
                       .Where(x => x.Case.CourtId == courtId && x.CaseSessionId == null)
                       .Where(x => x.DateExpired == null)
                       .Where(uicSearch)
                       .Where(nameSearch)
                       .Where(caseNumberSearch)
                       .Where(caseRegDateSearch)
                       .Where(withFinalActSearch)
                       .Where(withoutFinalActSearch)
                       .Select(x => new CasePersonReportVM()
                       {
                           CaseId = x.CaseId,
                           CaseNumber = x.Case.RegNumber,
                           CaseDate = x.Case.RegDate,
                           Uic = x.Uic,
                           FullName = x.FullName,
                           RoleName = x.PersonRole.Label,
                           CaseStateLabel = x.Case.CaseState.Label
                       }).AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за адреси за лица по дело/заседание за комбо бокс
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CasePersonAddress(int casePersonId, int notificationDeliveryGroupId)
        {
            bool addTel = notificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnPhone;
            bool addMail = notificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnEMail;

            var result = repo.AllReadonly<CasePersonAddress>()
                .Include(x => x.Address)
                .Where(x => x.CasePersonId == casePersonId)
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = ((x.ForNotification ?? false) ? " " : "") + x.Address.FullAddressNotificationMailTel(addTel, addMail)
                })
                .ToList();

            if (result.Count == 0)
                result.Insert(0, new SelectListItem() { Text = "Няма данни", Value = "-1" });

            return result.OrderBy(x => x.Text).ToList();
        }

        /// <summary>
        /// Извличане на адреси за комбобокс
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_AddressByCasePersonAddress(int casePersonId)
        {
            var result = repo.AllReadonly<CasePersonAddress>()
                .Include(x => x.Address)
                .Where(x => x.CasePersonId == casePersonId)
                .Select(x => new SelectListItem()
                {
                    Value = x.Address.Id.ToString(),
                    Text = ((x.ForNotification ?? false) ? " " : "") + x.Address.FullAddress
                })
                .ToList();

            if (result.Count == 0)
                result.Insert(0, new SelectListItem() { Text = "Няма данни", Value = "-1" });

            return result.OrderBy(x => x.Text).ToList();
        }

        /// <summary>
        /// Извличане на данни за адреси за комбобокс
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public List<CasePersonAddress> Get_CasePersonAddress(int casePersonId)
        {
            return repo.AllReadonly<CasePersonAddress>()
                .Include(x => x.Address)
                .Where(x => x.CasePersonId == casePersonId)
                .ToList();
        }

        /// <summary>
        /// Опресняване на данни за лица по дело/заседание
        /// </summary>
        /// <param name="toObj"></param>
        /// <param name="fromObj"></param>
        private void ReloadPersonData_Item(CasePerson toObj, CasePerson fromObj)
        {
            toObj.CopyFrom(fromObj);
            toObj.PersonRoleId = fromObj.PersonRoleId;
            toObj.MilitaryRangId = fromObj.MilitaryRangId;
            toObj.IsArrested = fromObj.IsArrested;
            toObj.IsDeceased = fromObj.IsDeceased;
            toObj.PersonMaturityId = fromObj.PersonMaturityId;
            toObj.DateFrom = fromObj.DateFrom;
            toObj.DateTo = fromObj.DateTo;
            foreach (var itemAddressCase in fromObj.Addresses)
            {
                if (itemAddressCase.CasePersonAddressIdentificator == null) continue;
                var itemAddressCurrent = toObj.Addresses.Where(x => x.CasePersonAddressIdentificator == itemAddressCase.CasePersonAddressIdentificator).FirstOrDefault();
                if (itemAddressCurrent == null)
                {
                    //Insert
                    itemAddressCase.Id = 0;
                    itemAddressCase.AddressId = 0;
                    itemAddressCase.CasePersonId = toObj.Id;
                    itemAddressCase.Address.Id = 0;
                    itemAddressCase.UserId = userContext.UserId;
                    itemAddressCase.DateWrt = DateTime.Now;
                    toObj.Addresses.Add(itemAddressCase);
                    CreateHistory<CasePersonAddress, CasePersonAddressH>(itemAddressCase);
                }
                else
                {
                    //Update
                    itemAddressCurrent.Address.CopyFrom(itemAddressCase.Address);
                    itemAddressCurrent.Address.FullAddress = itemAddressCase.Address.FullAddress;
                    itemAddressCurrent.ForNotification = itemAddressCase.ForNotification;
                    CreateHistory<CasePersonAddress, CasePersonAddressH>(itemAddressCase);
                }
            }
            CreateHistory<CasePerson, CasePersonH>(toObj);
            repo.Update(toObj);
        }

        /// <summary>
        /// Опресняване на данни за лица по дело/заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) ReloadPersonData(int caseId, int caseSessionId)
        {
            try
            {
                var caseSession = repo.AllReadonly<CaseSession>().Where(x => x.Id == caseSessionId).FirstOrDefault();
                //if (!caseSessionService.IsLastConductedSession(caseSessionId))
                //{
                //    return (result: false, errorMessage: "Заседанието вече е започнало. Не може да бъде извършено обновяване на данните.");
                //}

                var casePersonCurrent = repo.AllReadonly<CasePerson>().Include(x => x.Addresses).ThenInclude(x => x.Address)
                                                         .Where(x => x.CaseId == caseId && x.CaseSessionId == caseSessionId).ToList();
                var casePersonCase = repo.AllReadonly<CasePerson>().Include(x => x.Addresses).ThenInclude(x => x.Address)
                                                         .Where(x => x.CaseId == caseId && x.CaseSessionId == null && x.DateExpired == null).ToList();

                var caseNotification = repo.AllReadonly<CaseSessionNotificationList>().Where(x => x.CaseSessionId == caseSessionId && x.DateExpired == null).ToList();

                foreach (var item in casePersonCurrent)
                {
                    if (item.CasePersonIdentificator == null) continue;
                    var itemCasePersonCase = casePersonCase.Where(x => x.CasePersonIdentificator == item.CasePersonIdentificator).FirstOrDefault();
                    if (itemCasePersonCase == null) continue;

                    ReloadPersonData_Item(item, itemCasePersonCase);

                    if (item.Addresses.Any())
                    {
                        var itemNotification = caseNotification.Where(x => x.CasePersonId == item.Id).FirstOrDefault();
                        if (itemNotification != null && itemNotification.NotificationAddressId == null)
                        {
                            long addressId = item.Addresses.Where(x => x.ForNotification == true).Select(x => x.AddressId).DefaultIfEmpty(0).FirstOrDefault();
                            if (addressId == 0)
                                addressId = item.Addresses.Select(x => x.AddressId).FirstOrDefault();

                            itemNotification.NotificationAddressId = addressId;
                            repo.Update(itemNotification);
                        }
                    }
                }

                repo.SaveChanges();
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на ReloadPersonData CaseId={ caseId }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }

        }

        /// <summary>
        /// Извличане на списък за призоваване на принтиране
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionNotificationListVM> PersonListForPrint_Select(CheckListViewVM model)
        {
            List<CaseSessionNotificationListVM> result = new List<CaseSessionNotificationListVM>();
            var casePersonLists = CasePerson_Select(model.CourtId, 0, false, false, false).ToList();
            int maxnum = 0;

            foreach (var item in casePersonLists.OrderBy(x => x.RoleKindId))
            {
                var check = model.checkListVMs.Where(x => int.Parse(x.Value) == item.Id).FirstOrDefault();

                if (check != null)
                {
                    if (check.Checked)
                    {
                        var linkListVM = casePersonLinkService.GetLinkForPerson(item.Id, false, 0, null);

                        maxnum++;
                        var casePersonAddressLists = CasePersonAddress_Select(item.Id).ToList();

                        var caseSessionNotificationListVM = new CaseSessionNotificationListVM()
                        {
                            Id = item.Id,
                            CaseSessionId = 0,
                            PersonName = item.FullName,
                            PersonRole = item.PersonRoleLabel,
                            PersonId = item.Id,
                            RowNumber = maxnum,
                            NotificationPersonType = 0,
                            AddressString = ((casePersonAddressLists.Count > 0) ? ((casePersonAddressLists.Any(x => x.ForNotification == true)) ? (casePersonAddressLists.Where(x => x.ForNotification == true).FirstOrDefault().FullAddress) : (casePersonAddressLists.FirstOrDefault().FullAddress)) : string.Empty),
                            LinkForPersonString = (linkListVM != null) ? string.Join(", ", linkListVM.Select(x => x.Label)) : string.Empty
                        };

                        result.Add(caseSessionNotificationListVM);
                    }
                }
            }

            return result.AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за връзка между лица по дело/заседание за комбобокс
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public (List<SelectListItem> person_ddl, List<SelectListItem> linkDirection_ddl) CasePersonForLinkRel_SelectForDropDownList(int casePersonId)
        {
            var casePerson = repo.AllReadonly<CasePerson>()
                .Include(x => x.PersonRole)
                .Where(x => x.Id == casePersonId)
                .FirstOrDefault();

            string roleKinds = "";
            List<SelectListItem> linkDirectionRead = null;
            if (casePerson.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide || casePerson.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
            {
                roleKinds = NomenclatureConstants.PersonKinds.Represent.ToString();
                linkDirectionRead = repo.AllReadonly<LinkDirection>().Where(x => x.Id == NomenclatureConstants.LinkDirectionType.RepresentBy)
                                      .ToSelectList(true, false, true);

            }
            else if (casePerson.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.Represent)
            {
                roleKinds = NomenclatureConstants.PersonKinds.LeftSide.ToString() + "," + NomenclatureConstants.PersonKinds.RightSide.ToString();
                linkDirectionRead = repo.AllReadonly<LinkDirection>().Where(x => x.Id == NomenclatureConstants.LinkDirectionType.Represent)
                                      .ToSelectList(true, false, true);
            }


            var personRead = CasePerson_SelectForDropDownList(casePerson.CaseId, casePerson.CaseSessionId, roleKinds);
            if (linkDirectionRead == null)
                linkDirectionRead = nomenclatureService.GetDropDownList<LinkDirection>();
            return (person_ddl: personRead, linkDirection_ddl: linkDirectionRead);
        }

        /// <summary>
        /// Извличане на данни за лице от акт
        /// </summary>
        /// <param name="actId"></param>
        /// <returns></returns>
        private IQueryable<CasePerson> GetCasePersonByActId(int actId)
        {
            int[] roles = { NomenclatureConstants.PersonKinds.LeftSide, NomenclatureConstants.PersonKinds.RightSide };
            var caseId = repo.AllReadonly<CaseSessionAct>().Where(x => x.Id == actId).Select(x => x.CaseSession.CaseId).DefaultIfEmpty(0).FirstOrDefault();
            return repo.AllReadonly<CasePerson>()
                .Where(x => x.CaseId == caseId && x.CaseSessionId == null)
                .Where(x => roles.Contains(x.PersonRole.RoleKindId))
                .AsQueryable();
        }

        /// <summary>
        /// Извличане на лица за разводи
        /// </summary>
        /// <param name="actId"></param>
        /// <returns></returns>
        public (List<SelectListItem> men, List<SelectListItem> women, List<PersonDataVM> personData) GetCasePersonForDivorce(int actId)
        {
            List<SelectListItem> resultMen = new List<SelectListItem>();
            List<SelectListItem> resultWomen = new List<SelectListItem>();
            List<PersonDataVM> resultPersonData = new List<PersonDataVM>();

            var casePerson = GetCasePersonByActId(actId).OrderBy(x => x.FullName).ToList();

            foreach (var item in casePerson)
            {
                bool addItem = false;
                if (item.UicTypeId == NomenclatureConstants.UicTypes.EGN && Utils.Validation.IsEGN(item.Uic) == true)
                {
                    var sex = item.Uic.Substring(8, 1);
                    int sexInt = 0;
                    if (int.TryParse(sex, out sexInt) == true)
                    {
                        DateTime? birthDay = Utils.Validation.GetBirthDayFromEgn(item.Uic);
                        resultPersonData.Add(new PersonDataVM() { Id = item.Id, FullName = item.FullName, BirthDay = birthDay });
                        if (sexInt % 2 == 0)
                        {
                            resultMen.Add(new SelectListItem() { Text = item.FullName, Value = item.Id.ToString() });
                        }
                        else
                        {
                            resultWomen.Add(new SelectListItem() { Text = item.FullName, Value = item.Id.ToString() });
                        }
                        addItem = true;
                    }
                }
                if (addItem == false)
                {
                    resultMen.Add(new SelectListItem() { Text = item.FullName, Value = item.Id.ToString() });
                    resultWomen.Add(new SelectListItem() { Text = item.FullName, Value = item.Id.ToString() });
                }
            }

            return (men: resultMen, women: resultWomen, personData: resultPersonData);
        }

        /// <summary>
        /// Търсене на адрес по лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <param name="addressId"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CasePersonAddress_AddFromSearch(int casePersonId, int addressId)
        {
            var casePerson = repo.GetById<CasePerson>(casePersonId);
            CasePersonAddress model = new CasePersonAddress();
            model.CasePersonId = casePersonId;
            model.CaseId = casePerson.CaseId;
            model.CourtId = userContext.CourtId;
            model.Address = repo.AllReadonly<Address>().Where(x => x.Id == addressId).FirstOrDefault();
            model.Address.Id = 0;
            return CasePersonAddress_SaveData(model);
        }

        /// <summary>
        /// Извличане на данни за Наследство по лице в дело
        /// </summary>
        /// <param name="CasePersonId"></param>
        /// <returns></returns>
        public IQueryable<CasePersonInheritanceVM> CasePersonInheritance_Select(int CasePersonId)
        {
            return repo.AllReadonly<CasePersonInheritance>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.DecreedCourt)
                       .Include(x => x.CasePersonInheritanceResult)
                       .Include(x => x.CaseSessionAct)
                       .ThenInclude(x => x.ActType)
                       .Where(x => x.CasePersonId == CasePersonId)
                       .Select(x => new CasePersonInheritanceVM()
                       {
                           Id = x.Id,
                           CaseSessionActLabel = x.CaseSessionAct != null ? x.CaseSessionAct.ActType.Label + " " + (x.CaseSessionAct.RegNumber ?? string.Empty) + "/" + (x.CaseSessionAct.RegDate != null ? (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) : string.Empty,
                           CourtLabel = x.DecreedCourt.Label,
                           CasePersonInheritanceResultLabel = x.CasePersonInheritanceResult.Label,
                           IsActiveText = (x.IsActive ?? false) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Наследство по лице в дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CasePersonInheritance_SaveData(CasePersonInheritance model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CasePersonInheritance>(model.Id);
                    saved.DecreedCourtId = model.DecreedCourtId;
                    saved.CasePersonId = model.CasePersonId;
                    saved.CaseSessionActId = model.CaseSessionActId;
                    saved.CasePersonInheritanceResultId = model.CasePersonInheritanceResultId;
                    saved.IsActive = model.IsActive;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CasePersonInheritance>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Наследство по лице в дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Мерки към лица по НД по дело
        /// </summary>
        /// <param name="CasePersonId"></param>
        /// <param name="showExpired"></param>
        /// <returns></returns>
        public IQueryable<CasePersonMeasureVM> CasePersonMeasure_Select(int CasePersonId, bool showExpired = false)
        {
            return repo.AllReadonly<CasePersonMeasure>()
                       .Include(x => x.MeasureInstitution)
                       .Include(x => x.MeasureCourt)
                       .Where(x => x.CasePersonId == CasePersonId &&
                                   (!showExpired ? x.DateExpired == null : true))
                       .Select(x => new CasePersonMeasureVM()
                       {
                           Id = x.Id,
                           MeasureCourtLabel = (x.MeasureCourt != null) ? x.MeasureCourt.Label : string.Empty,
                           MeasureInstitutionLabel = (x.MeasureInstitution != null) ? x.MeasureInstitution.FullName : string.Empty,
                           MeasureTypeLabel = x.MeasureTypeLabel,
                           MeasureStatusDate = x.MeasureStatusDate,
                           BailAmount = x.BailAmount,
                           MeasureStatusLabel = x.MeasureStatusLabel
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за Лични документи на лица по дело
        /// </summary>
        /// <param name="CasePersonId"></param>
        /// <param name="showExpired"></param>
        /// <returns></returns>
        public IQueryable<CasePersonDocumentVM> CasePersonDocument_Select(int CasePersonId, bool showExpired = false)
        {
            return repo.AllReadonly<CasePersonDocument>()
                       .Where(x => x.CasePersonId == CasePersonId &&
                                   (!showExpired ? x.DateExpired == null : true))
                       .Select(x => new CasePersonDocumentVM()
                       {
                           Id = x.Id,
                           IssuerCountryName = x.IssuerCountryName,
                           PersonalDocumentTypeLabel = x.PersonalDocumentTypeLabel,
                           DocumentNumber = x.DocumentNumber,
                           DocumentDate = x.DocumentDate,
                           IssuerName = x.IssuerName
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Попълване на осносвен обект за Мерки към лица по НД по дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private CasePersonMeasure FillCasePersonMeasure(CasePersonMeasureEditVM model)
        {
            return new CasePersonMeasure()
            {
                Id = model.Id,
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                ParentId = model.ParentId,
                MeasureCourtId = model.MeasureCourtId,
                CasePersonId = model.CasePersonId,
                MeasureInstitutionId = model.MeasureInstitutionId,
                MeasureType = model.MeasureType,
                MeasureTypeLabel = eisppService.GetByCode(model.MeasureType).Label,
                MeasureStatusDate = model.MeasureStatusDate,
                BailAmount = model.BailAmount,
                MeasureStatus = model.MeasureStatus,
                MeasureStatusLabel = eisppService.GetByCode(model.MeasureStatus).Label
            };
        }

        /// <summary>
        /// Попълване на обект за редакция за Мерки към лица по НД по дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private CasePersonMeasureEditVM FillCasePersonMeasureEditVM(CasePersonMeasure model)
        {
            return new CasePersonMeasureEditVM()
            {
                Id = model.Id,
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                ParentId = model.ParentId,
                MeasureCourtId = model.MeasureCourtId,
                CasePersonId = model.CasePersonId,
                MeasureInstitutionId = model.MeasureInstitutionId,
                MeasureInstitutionTypeId = (model.MeasureInstitutionId != null) ? (repo.AllReadonly<Institution>().Where(x => x.Id == model.MeasureInstitutionId).FirstOrDefault()).InstitutionTypeId : (int?)null,
                MeasureType = model.MeasureType,
                MeasureStatusDate = model.MeasureStatusDate,
                BailAmount = model.BailAmount,
                MeasureStatus = model.MeasureStatus,
                DateExpired = model.DateExpired
            };
        }

        /// <summary>
        /// Запис на Мерки към лица по НД по дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CasePersonMeasure_SaveData(CasePersonMeasureEditVM model)
        {
            try
            {
                var modelSave = FillCasePersonMeasure(model);

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CasePersonMeasure>(modelSave.Id);
                    saved.MeasureInstitutionId = modelSave.MeasureInstitutionId;
                    saved.MeasureType = modelSave.MeasureType;
                    saved.MeasureTypeLabel = modelSave.MeasureTypeLabel;
                    saved.MeasureStatusDate = modelSave.MeasureStatusDate;
                    saved.BailAmount = modelSave.BailAmount;
                    saved.MeasureStatus = modelSave.MeasureStatus;
                    saved.MeasureStatusLabel = modelSave.MeasureStatusLabel;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    modelSave.DateWrt = DateTime.Now;
                    modelSave.UserId = userContext.UserId;
                    repo.Add<CasePersonMeasure>(modelSave);
                    repo.SaveChanges();
                    model.Id = modelSave.Id;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Мерки към лица по НД Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Запис на Лични документи на лица по дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CasePersonDocument_SaveData(CasePersonDocument model)
        {
            try
            {
                model.IssuerCountryName = (repo.AllReadonly<EkCountry>().Where(x => x.Code == model.IssuerCountryCode).FirstOrDefault()).Name;
                model.PersonalDocumentTypeLabel = eisppService.GetByCode(model.PersonalDocumentTypeId).Label;

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CasePersonDocument>(model.Id);
                    saved.IssuerCountryCode = model.IssuerCountryCode;
                    saved.IssuerCountryName = model.IssuerCountryName;
                    saved.PersonalDocumentTypeId = model.PersonalDocumentTypeId;
                    saved.PersonalDocumentTypeLabel = model.PersonalDocumentTypeLabel;
                    saved.DocumentNumber = model.DocumentNumber;
                    saved.DocumentDate = model.DocumentDate;
                    saved.DocumentDateTo = model.DocumentDateTo;
                    saved.IssuerName = model.IssuerName;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CasePersonDocument>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Мерки към лица по НД Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за мерки на лице по ид на мярката
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CasePersonMeasureEditVM CasePersonMeasure_GetById(int id)
        {
            return FillCasePersonMeasureEditVM(repo.GetById<CasePersonMeasure>(id));
        }


        public List<SelectListItem> GetForEispp(int caseId)
        {
            var result = repo.AllReadonly<CasePerson>()
                             .Where(x => x.CaseId == caseId &&
                                         x.CaseSessionId == null &&
                                         x.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide)
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = x.FullName + "(" + (x.Uic ?? "") + ") - " + x.PersonRole.Label
                             }).ToList();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public (bool result, string errorMessage) CheckCasePersonExpired(CasePerson model)
        {
            //Тъй като са с 3 или-та гледам и делото
            bool checkLink = repo.AllReadonly<CasePersonLink>()
                              .Where(x => x.CaseId == model.CaseId && x.DateExpired == null &&
                                 (x.CasePersonId == model.Id || x.CasePersonRelId == model.Id ||
                                 x.CasePersonSecondRelId == model.Id))
                              .Any();
            if (checkLink)
                return (result: false, errorMessage: "Има активна връзка за лицето");

            //Тъй като са с 3 или-та гледам и делото
            bool checkNotification = repo.AllReadonly<CaseNotification>()
                              .Where(x => x.CaseId == model.CaseId && x.DateExpired == null &&
                                 x.CasePerson.CasePersonIdentificator == model.CasePersonIdentificator)
                              .Any();
            if (checkNotification)
                return (result: false, errorMessage: "Има създадена призовка/съобщение за лицето");

            return (result: true, errorMessage: "");
        }
        public bool IsPersonDead(int casePersonId)
        {
            var casePersonIdentificator = repo.AllReadonly<CasePerson>()
                                              .Where(x => x.Id == casePersonId)
                                              .Select(x => x.CasePersonIdentificator)
                                              .FirstOrDefault();
            if (!string.IsNullOrEmpty(casePersonIdentificator))
            {
                return repo.AllReadonly<CasePerson>()
                            .Where(x => x.CasePersonIdentificator == casePersonIdentificator &&
                                        x.CaseSessionId == null)
                            .Select(x => x.IsDeceased)
                            .FirstOrDefault() ?? false;
            }
            else
            {
                return false;
            }

        }

        public bool CasePerson_SaveExpiredPlus(ExpiredInfoVM model)
        {
            try
            {
                var expireObject = repo.GetById<CasePerson>(model.Id);
                model.DateExpired = DateTime.Now;

                expireObject.DateExpired = model.DateExpired;
                expireObject.UserExpiredId = userContext.UserId;
                expireObject.DescriptionExpired = model.DescriptionExpired;
                repo.Update(expireObject);

                var casePeople = repo.AllReadonly<CasePerson>()
                                     .Where(x => x.CasePersonIdentificator == expireObject.CasePersonIdentificator &&
                                                 x.CaseSession.DateFrom >= model.DateExpired &&
                                                 x.Id != model.Id)
                                     .ToList();

                foreach (var casePerson in casePeople)
                {
                    casePerson.DateExpired = model.DateExpired;
                    casePerson.UserExpiredId = userContext.UserId;
                    casePerson.DescriptionExpired = model.DescriptionExpired;
                    repo.Update(casePerson);
                }

                var caseNotifications = repo.AllReadonly<CaseNotification>()
                                            .Where(x => x.CasePerson.CasePersonIdentificator == expireObject.CasePersonIdentificator &&
                                                        x.CaseSession.DateFrom >= model.DateExpired)
                                            .ToList();

                foreach (var caseNotification in caseNotifications)
                {
                    caseNotification.DateExpired = model.DateExpired;
                    caseNotification.UserExpiredId = userContext.UserId;
                    caseNotification.DescriptionExpired = model.DescriptionExpired;
                    repo.Update(caseNotification);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при премахване на лице с Id={ model.Id }");
                return false;
            }
        }

        public List<SelectListItem> GetAddressByCasePerson_DropDown(int casePersonId)
        {
            var result = repo.AllReadonly<CasePersonAddress>()
                .Where(x => x.CasePersonId == casePersonId)
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Address.FullAddress
                })
                .OrderBy(x => x.Text)
                .ToList();

            result.Insert(0, new SelectListItem() { Text = "Изберете", Value = "-1" });

            return result;
        }

        public SaveResultVM CasePersonAddress_IsUsed(CasePersonAddress model)
        {
            if (repo.AllReadonly<CaseNotification>()
                            .Include(x => x.CasePersonAddress)
                            .Select(x => x.CasePersonAddress)
                            .Where(x => x != null)
                            .Where(x => x.CasePersonAddressIdentificator == model.CasePersonAddressIdentificator).Any())
            {
                return new SaveResultVM(true, "Има изготвено уведомление. Не можете да деактивирате адреса!");
            }
            if (repo.AllReadonly<DocumentTemplate>().Where(x => x.CasePersonAddressId == model.Id).Any())
            {
                return new SaveResultVM(true, "За избрания адрес има издадени изходящи писма");
            }
            return new SaveResultVM(false);
        }
    }
}
