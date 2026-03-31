using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CaseLawyerHelpService : BaseService, ICaseLawyerHelpService
    {

        private readonly IMQEpepService mqEpepService;
        private readonly ICasePersonService casePersonService;

        public CaseLawyerHelpService(ILogger<CaseLawyerHelpService> _logger,
                                     IRepository _repo,
                                     IUserContext _userContext,
                                     IMQEpepService _mqEpepService,
                                     ICasePersonService _casePerson)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            mqEpepService = _mqEpepService;
            casePersonService = _casePerson;
        }

        public CaseLawyerHelpEditVM CaseLawyerHelp_GetById(int Id)
        {
            var caseLawyerHelp = repo.AllReadonly<CaseLawyerHelp>()
                                     .Where(x => x.Id == Id)
                                     .Select(x => new CaseLawyerHelpEditVM()
                                     {
                                         Id = x.Id,
                                         CourtId = x.CourtId,
                                         CaseId = x.CaseId,
                                         CaseName = x.Case.CaseType.Code + " " + x.Case.ShortNumber + "/" + x.Case.RegDate.ToString("yyyy"),
                                         LawyerHelpBaseId = x.LawyerHelpBaseId,
                                         LawyerHelpBaseLabel = x.LawyerHelpBase.Label,
                                         LawyerHelpTypeId = x.LawyerHelpTypeId,
                                         LawyerHelpTypeLabel = x.LawyerHelpType.Label,
                                         CaseSessionActId = x.CaseSessionActId,
                                         HasInterestConflict = x.HasInterestConflict,
                                         PrevDefenderName = x.PrevDefenderName,
                                         Description = x.Description,
                                         CaseSessionToGoId = x.CaseSessionToGoId,
                                         ActAppointmentId = x.ActAppointmentId,
                                         LawyerHelpBasisAppointmentId = x.LawyerHelpBasisAppointmentId
                                     })
                                     .FirstOrDefault();

            caseLawyerHelp.CaseLawyerHelpOtherLawyers = FillCaseLawyerHelpOtherLawyers(caseLawyerHelp.Id, caseLawyerHelp.CaseId);

            return caseLawyerHelp;
        }

        public List<CheckListVM> FillCaseLawyerHelpOtherLawyers(int? CaseLawyerHelpId, int CaseId)
        {
            var datetimeNow = DateTime.Now;

            var caseLawyerHelpOtherLawyers = CaseLawyerHelpId == null ? new List<CaseLawyerHelpOtherLawyer>() :
                                                                        repo.AllReadonly<CaseLawyerHelpOtherLawyer>()
                                                                            .Where(x => x.CaseLawyerHelpId == CaseLawyerHelpId)
                                                                            .ToList();

            var casePeople = repo.AllReadonly<CasePerson>()
                                 .Include(x => x.PersonRole)
                                 .Where(x => x.CaseId == CaseId &&
                                             x.CaseSessionId == null &&
                                             NomenclatureConstants.PersonRole.ListForLawyerHelp_Lawyer.Contains(x.PersonRoleId) &&
                                             (x.DateTo ?? datetimeNow.AddYears(100)) >= datetimeNow &&
                                             x.DateExpired == null)
                                 .Select(x => new CheckListVM()
                                 {
                                     Value = x.Id.ToString(),
                                     Label = x.FullName + " (" + x.PersonRole.Label + ")"
                                 })
                                 .ToList();

            foreach (var check in casePeople)
                check.Checked = caseLawyerHelpOtherLawyers.Any(c => c.CasePersonId == int.Parse(check.Value));

            return casePeople;
        }

        public List<CheckListVM> FillLeftRightSide(int CaseId)
        {
            var dateTime = DateTime.Now;

            return repo.AllReadonly<CasePerson>()
                       .Where(x => (x.CaseId == CaseId) &&
                                   (x.CaseSessionId == null) &&
                                   (x.DateExpired == null) &&
                                   (NomenclatureConstants.PersonKinds.ListLeftRightSide.Contains(x.PersonRole.RoleKindId) ||
                                    x.PersonRole.ForLawyerHelp) &&
                                   ((x.DateTo ?? DateTime.Now.AddYears(1)) >= DateTime.Now))
                       .Select(x => new CheckListVM()
                       {
                           Value = x.Id.ToString(),
                           Label = x.FullName,
                           Checked = false
                       })
                       .OrderBy(x => x.Label)
                       .ToList();
        }

        public IQueryable<CaseLawyerHelpVM> CaseLawyerHelp_Select(int CaseId)
        {
            var documentTemplateQuery = repo.AllReadonly<DocumentTemplate>()
                                            .Where(d => d.SourceType == SourceTypeSelectVM.CaseLawyerHelp &&
                                                        d.DateExpired == null);

            return repo.AllReadonly<CaseLawyerHelp>()
                       .Where(x => x.CaseId == CaseId &&
                                   x.DateExpired == null)
                       .Select(x => new CaseLawyerHelpVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CaseId = x.CaseId,
                           LawyerHelpBaseText = x.LawyerHelpBase.Label,
                           LawyerHelpTypeText = x.LawyerHelpType.Label,
                           DocumentDateFromDb = documentTemplateQuery.Where(d => d.SourceId == x.Id)
                                                                     .Select(d => d.Document.DocumentDate)
                                                                     .OrderByDescending(d => d)
                                                                     .FirstOrDefault()
                       })
                       .AsQueryable();
        }

        private CaseLawyerHelp FillCaseLawyerHelp(CaseLawyerHelpEditVM model)
        {
            return new CaseLawyerHelp()
            {
                Id = model.Id,
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                LawyerHelpBaseId = model.LawyerHelpBaseId,
                LawyerHelpTypeId = model.LawyerHelpTypeId,
                CaseSessionActId = model.CaseSessionActId,
                HasInterestConflict = model.HasInterestConflict,
                PrevDefenderName = model.PrevDefenderName,
                Description = model.Description,
                CaseSessionToGoId = model.CaseSessionToGoId,
                ActAppointmentId = model.ActAppointmentId,
                LawyerHelpBasisAppointmentId = model.LawyerHelpBasisAppointmentId
            };
        }

        public async Task<bool> CaseLawyerHelp_SaveData(CaseLawyerHelpEditVM model)
        {
            model.CaseSessionToGoId = model.CaseSessionToGoId.NumberEmptyToNull();
            model.ActAppointmentId = model.ActAppointmentId.NumberEmptyToNull();
            model.LawyerHelpBasisAppointmentId = model.LawyerHelpBasisAppointmentId.NumberEmptyToNull();
            var modelSave = FillCaseLawyerHelp(model);

            try
            {
                if (modelSave.Id > 0)
                {
                    if (model.CaseLawyerHelpOtherLawyers != null)
                    {
                        List<CaseLawyerHelpOtherLawyer> caseLawyerHelpOtherLawyers = await repo.AllReadonly<CaseLawyerHelpOtherLawyer>()
                                                                                               .Where(x => x.CaseLawyerHelpId == model.Id)
                                                                                               .ToListAsync();

                        foreach (var checkList in model.CaseLawyerHelpOtherLawyers)
                        {
                            var caseLawyerHelpOtherLawyer = caseLawyerHelpOtherLawyers.Where(x => x.CasePersonId == int.Parse(checkList.Value))
                                                                                      .FirstOrDefault();

                            if (checkList.Checked && caseLawyerHelpOtherLawyer == null)
                            {
                                repo.Add(new CaseLawyerHelpOtherLawyer()
                                {
                                    CaseLawyerHelpId = modelSave.Id,
                                    CasePersonId = int.Parse(checkList.Value),
                                });
                            }

                            if (!checkList.Checked && caseLawyerHelpOtherLawyer != null)
                                repo.Delete(caseLawyerHelpOtherLawyer);
                        }
                    }

                    //Update
                    var saved = repo.GetById<CaseLawyerHelp>(modelSave.Id);
                    saved.LawyerHelpBaseId = modelSave.LawyerHelpBaseId;
                    saved.CaseSessionActId = modelSave.CaseSessionActId;
                    saved.HasInterestConflict = modelSave.HasInterestConflict;
                    saved.PrevDefenderName = modelSave.PrevDefenderName;
                    saved.Description = modelSave.Description;
                    saved.LawyerHelpTypeId = modelSave.LawyerHelpTypeId;
                    saved.CaseSessionToGoId = modelSave.CaseSessionToGoId;
                    saved.ActAppointmentId = modelSave.ActAppointmentId;
                    saved.LawyerHelpBasisAppointmentId = modelSave.LawyerHelpBasisAppointmentId;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    modelSave.DateWrt = DateTime.Now;
                    modelSave.UserId = userContext.UserId;

                    if (model.CaseLawyerHelpPeople != null && model.CaseLawyerHelpPeople.Any(x => x.Checked))
                    {
                        foreach (var person in model.CaseLawyerHelpPeople.Where(x => x.Checked))
                        {
                            modelSave.CaseLawyerHelpPersons.Add(new CaseLawyerHelpPerson()
                            {
                                CasePersonId = int.Parse(person.Value),
                                CasePersonAddressId = await repo.AllReadonly<CasePersonAddress>()
                                                                 .Where(p => p.CasePersonId == int.Parse(person.Value) &&
                                                                             p.DateExpired == null)
                                                                 .OrderBy(p => p.Address.AddressTypeId)
                                                                 .Select(p => (int?)p.Id)
                                                                 .FirstOrDefaultAsync()
                            });
                        }
                    }

                    repo.Add(modelSave);
                }

                await repo.SaveChangesAsync();

                model.Id = modelSave.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Искане за Правна помощ Id={modelSave.Id}");
                return false;
            }
        }

        public IQueryable<CaseLawyerHelpPersonVM> CaseLawyerHelpPerson_Select(int CaseLawyerHelpId)
        {
            return repo.AllReadonly<CaseLawyerHelpPerson>()
                       .Where(x => x.CaseLawyerHelpId == CaseLawyerHelpId &&
                                   x.DateExpired == null)
                       .Select(x => new CaseLawyerHelpPersonVM()
                       {
                           Id = x.Id,
                           CasePersonText = x.CasePerson.FullName + " (" + x.CasePerson.PersonRole.Label + ")",
                           AssignedLawyerText = x.AssignedLawyer.FullName,
                           SpecifiedLawyerLawUnitLabel = x.SpecifiedLawyerLawUnit.FullName,
                           DescriptionExpired = (x.EesppPersonStateId == NomenclatureConstants.EesppPersonState.Declined && x.DescriptionExpired != null && x.DateExpired == null) ? x.DescriptionExpired : (string)null
                       })
                       .AsQueryable();
        }

        public CaseLawyerHelpPersonVM CaseLawyerHelpPerson_GetById(int Id)
        {
            return repo.AllReadonly<CaseLawyerHelpPerson>()
                       .Where(x => x.Id == Id)
                       .Select(x => new CaseLawyerHelpPersonVM()
                       {
                           Id = x.Id,
                           CasePersonText = x.CasePerson.FullName + " (" + x.CasePerson.PersonRole.Label + ")",
                           AssignedLawyerText = x.AssignedLawyer.FullName,
                           SpecifiedLawyerLawUnitLabel = x.SpecifiedLawyerLawUnit.FullName,
                           CaseName = x.CaseLawyerHelp.Case.CaseType.Code + " " + x.CaseLawyerHelp.Case.ShortNumber + "/" + x.CaseLawyerHelp.Case.RegDate.ToString("yyyy")
                       })
                       .FirstOrDefault();
        }

        public List<SelectListItem> GetDDL_LeftRightSide(int CaseLawyerHelpId, int? CasePersonId, bool addDefaultElement = true)
        {
            var caseLawyerHelp = repo.GetById<CaseLawyerHelp>(CaseLawyerHelpId);
            var caseLawyerHelpPeople = repo.AllReadonly<CaseLawyerHelpPerson>()
                                           .Where(x => x.CaseLawyerHelpId == CaseLawyerHelpId &&
                                                       (CasePersonId != null ? x.CasePersonId != CasePersonId : true) &&
                                                       x.DateExpired == null)
                                           .Select(x => x.CasePersonId)
                                           .ToList() ?? new List<int>();

            var selectListItems = repo.AllReadonly<CasePerson>()
                                      .Where(x => x.CaseId == caseLawyerHelp.CaseId &&
                                                  x.CaseSessionId == null &&
                                                  x.DateExpired == null &&
                                                  (NomenclatureConstants.PersonKinds.ListLeftRightSide.Contains(x.PersonRole.RoleKindId) ||
                                                   x.PersonRole.ForLawyerHelp) &&
                                                  ((x.DateTo ?? DateTime.Now.AddYears(1)) >= DateTime.Now) &&
                                                  (caseLawyerHelpPeople.Count > 0 ? !caseLawyerHelpPeople.Contains(x.Id) : true))
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.FullName + " (" + x.PersonRole.Label + ")",
                                          Value = x.Id.ToString()
                                      })
                                      .ToList();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_Lawyer(int CaseLawyerHelpId, bool addDefaultElement = true)
        {
            var datetimeNow = DateTime.Now;
            var caseLawyerHelp = repo.GetById<CaseLawyerHelp>(CaseLawyerHelpId);

            var selectListItems = repo.AllReadonly<CasePerson>()
                                      .Include(x => x.PersonRole)
                                      .Where(x => x.CaseId == caseLawyerHelp.CaseId &&
                                                  x.CaseSessionId == null &&
                                                  NomenclatureConstants.PersonRole.ListForLawyerHelp_Lawyer.Contains(x.PersonRoleId) &&
                                                  (x.DateTo ?? datetimeNow.AddYears(100)) >= datetimeNow &&
                                                  x.DateExpired == null)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.FullName + " (" + x.PersonRole.Label + ")",
                                          Value = x.Id.ToString()
                                      })
                                      .ToList();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        public bool CaseLawyerHelpPerson_SaveData(CaseLawyerHelpPerson model)
        {
            model.AssignedLawyerId = model.AssignedLawyerId.NumberEmptyToNull();
            model.SpecifiedLawyerLawUnitId = model.SpecifiedLawyerLawUnitId.NumberEmptyToNull();
            model.CasePersonAddressId = model.CasePersonAddressId.NumberEmptyToNull();

            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLawyerHelpPerson>(model.Id);
                    saved.CasePersonId = model.CasePersonId;
                    saved.AssignedLawyerId = model.AssignedLawyerId;
                    saved.SpecifiedLawyerLawUnitId = model.SpecifiedLawyerLawUnitId;
                    saved.CasePersonAddressId = model.CasePersonAddressId;
                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    repo.Add<CaseLawyerHelpPerson>(model);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на лица, за които се иска правна помощ Id={model.Id}");
                return false;
            }
        }

        public List<SelectListItem> GetDDL_LawyerHelpBase(int CaseId, bool addDefaultElement = true)
        {
            var caseCase = repo.GetById<Case>(CaseId);

            var selectListItems = repo.AllReadonly<LawyerHelpBaseCaseGroup>()
                                      .Where(x => x.CaseGroupId == caseCase.CaseGroupId &&
                                                  x.LawyerHelpBase.IsActive)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.LawyerHelpBase.Label,
                                          Value = x.LawyerHelpBase.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        public CaseLawyerHelpPersonMultiEditVM CaseLawyerHelpPersonMultiEdit_Get(int CaseLawyerHelpId)
        {
            var caseLawyerHelpPeople = repo.AllReadonly<CaseLawyerHelpPerson>()
                                           .Include(x => x.CasePerson)
                                           .Where(x => x.CaseLawyerHelpId == CaseLawyerHelpId &&
                                                       x.DateExpired == null)
                                           .ToList() ?? new List<CaseLawyerHelpPerson>();

            return new CaseLawyerHelpPersonMultiEditVM()
            {
                CaseLawyerHelpId = CaseLawyerHelpId,
                AssignedLawyerId = caseLawyerHelpPeople.FirstOrDefault()?.AssignedLawyerId,
                SpecifiedLawyerLawUnitId = caseLawyerHelpPeople.FirstOrDefault()?.SpecifiedLawyerLawUnitId,
                CaseLawyerHelpPeople = caseLawyerHelpPeople.Select(x => new CheckListVM()
                {
                    Value = x.Id.ToString(),
                    Label = x.CasePerson.FullName,
                    Checked = true
                }).ToList()
            };
        }

        public bool CaseLawyerHelpPersonMulti_UpdateData(CaseLawyerHelpPersonMultiEditVM model)
        {
            model.AssignedLawyerId = model.AssignedLawyerId.NumberEmptyToNull();
            model.SpecifiedLawyerLawUnitId = model.SpecifiedLawyerLawUnitId.NumberEmptyToNull();

            try
            {
                foreach (var checkList in model.CaseLawyerHelpPeople.Where(x => x.Checked))
                {
                    var saved = repo.GetById<CaseLawyerHelpPerson>(int.Parse(checkList.Value));
                    saved.AssignedLawyerId = model.AssignedLawyerId;
                    saved.SpecifiedLawyerLawUnitId = model.SpecifiedLawyerLawUnitId;
                    repo.Update(saved);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на лица, за които се иска правна помощ Id={model.CaseLawyerHelpId}");
                return false;
            }
        }

        public bool IsExistPerson_CaseLawyerHelp(int Id)
        {
            return repo.AllReadonly<CaseLawyerHelpPerson>()
                       .Any(x => x.CaseLawyerHelpId == Id &&
                                 x.DateExpired == null);
        }

        public bool IsExistDocumentTemplate_CaseLawyerHelp(int Id)
        {
            return repo.AllReadonly<DocumentTemplate>()
                       .Any(x => x.SourceType == SourceTypeSelectVM.CaseLawyerHelp &&
                                 x.SourceId == Id &&
                                 x.Document.DateExpired == null &&
                                 x.DateExpired == null);
        }

        public IQueryable<CaseLawyerHelpAssignedLawyerVM> CaseLawyerHelpAssignedLawyer_Select(int CaseLawyerHelpId)
        {
            var lawyersQuery = repo.AllReadonly<LawUnit>().Where(l => l.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Lawyer);
            return repo.AllReadonly<CaseLawyerHelpAssignedLawyer>()
                       .Where(x => x.CaseLawyerHelpId == CaseLawyerHelpId)
                       .Select(x => new CaseLawyerHelpAssignedLawyerVM()
                       {
                           Id = x.Id,
                           Lawyer = x.LawyerNumber + " " + x.LawyerName + (lawyersQuery.Any(l => l.Code == x.LawyerNumber) ? "" : " (Не е намерен в списъка в ЕИСС)"),
                           People = string.Join(", ", x.Persons.Select(p => p.CaseLawyerHelpPerson.CasePerson.FullName + " (" + p.CaseLawyerHelpPerson.CasePerson.PersonRole.Label + ")")),
                           LawyerStateLabel = x.LawyerState.Label,
                           IsEdit = x.LawyerStateDate == null,
                           IsFinish = (x.LawyerStateDate == null && x.LawyerStateId != NomenclatureConstants.EesppLawyerState.Assigned),
                           CaseName = x.CaseLawyerHelp.Case.CaseType.Code + " " + x.CaseLawyerHelp.Case.ShortNumber + "/" + x.CaseLawyerHelp.Case.RegDate.ToString("yyyy"),
                           CaseId = x.CaseLawyerHelp.CaseId
                       })
                       .AsQueryable();
        }

        private CasePersonVM FillCasePerson(int CaseId, LawUnit lawUnit, DateTime dateFrom)
        {
            CasePersonVM model = new CasePersonVM();
            model.CaseId = CaseId;
            model.CourtId = userContext.CourtId;
            model.DateFrom = dateFrom;
            model.Person_SourceType = SourceTypeSelectVM.LawUnit;
            model.Person_SourceId = lawUnit.Id;
            model.PersonRoleId = NomenclatureConstants.PersonRole.OfficialDefender;
            model.Uic = lawUnit.Uic;
            model.UicTypeId = lawUnit.UicTypeId;
            model.FirstName = lawUnit.FirstName;
            model.MiddleName = lawUnit.MiddleName;
            model.FamilyName = lawUnit.FamilyName;
            model.Family2Name = lawUnit.Family2Name;
            model.FullName = lawUnit.FullName;
            return model;
        }

        public async Task<bool> CaseLawyerHelpAssignedLawyer_ChangeState(int CaseId, int Id, int? LawyerStateId, DateTime? dateTime)
        {
            try
            {
                //Update
                var saved = repo.GetById<CaseLawyerHelpAssignedLawyer>(Id);
                saved.LawyerStateId = (LawyerStateId ?? saved.LawyerStateId);

                if (dateTime != null)
                {
                    int? laweyrId = null;

                    var lawUnits = repo.AllReadonly<LawUnit>()
                                       .Where(x => x.Code == saved.LawyerNumber &&
                                                   x.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Lawyer)
                                       .FirstOrDefault();

                    var casePerson = lawUnits != null ? repo.AllReadonly<CasePerson>()
                                                            .Where(x => x.Person_SourceType == SourceTypeSelectVM.LawUnit &&
                                                                        x.Person_SourceId == lawUnits.Id)
                                                            .Where(x => x.CaseId == CaseId && x.CaseSessionId == null)
                                                            .FirstOrDefault() : null;

                    var caseSessionAct = repo.AllReadonly<CaseSessionAct>()
                                             .Where(x => x.Id == saved.CaseSessionActAssignedId)
                                             .FirstOrDefault();

                    saved.LawyerStateDate = caseSessionAct.ActDeclaredDate ?? DateTime.Now;

                    if (saved.LawyerStateId == NomenclatureConstants.EesppLawyerState.Confirmed)
                    {
                        if ((lawUnits != null) && (casePerson == null))
                        {
                            var casePersonVM = FillCasePerson(CaseId, lawUnits, caseSessionAct.ActDeclaredDate ?? DateTime.Now);
                            (bool result, string errorMessage) = await casePersonService.CasePerson_SaveData(casePersonVM);
                            if (!result)
                            {
                                return false;
                            }
                            laweyrId = casePersonVM.Id;
                        }

                        if (casePerson != null)
                        {
                            laweyrId = casePerson.Id;
                        }
                    }

                    var caseLawyerHelpPeople = repo.AllReadonly<CaseLawyerHelpAssignedLawyerPerson>()
                                                                 .Include(x => x.CaseLawyerHelpPerson)
                                                                 .Where(x => x.CaseLawyerAssignedLawyerId == saved.Id)
                                                                 .Select(x => x.CaseLawyerHelpPerson)
                                                                 .ToList();

                    foreach (var _person in caseLawyerHelpPeople)
                    {
                        _person.EesppPersonStateId = (saved.LawyerStateId == NomenclatureConstants.EesppLawyerState.Confirmed) ? NomenclatureConstants.EesppPersonState.Confirmed : NomenclatureConstants.EesppPersonState.Sent;
                        _person.AssignedLawyerId = laweyrId;
                        repo.Update(_person);
                    }
                }

                repo.SaveChanges();
                if (saved.LawyerStateId == NomenclatureConstants.EesppLawyerState.Confirmed)
                {
                    mqEpepService.EESPP_AppendLawyerAssignment(saved.Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при сетване на статус на Върнати адвокати по заявка за правна помощ от ЕЕСПП Id={Id}");
                return false;
            }
        }

        public CaseLawyerHelpAssignedLawyerVM CaseLawyerHelpAssignedLawyerVM_GetById(int Id)
        {
            return repo.AllReadonly<CaseLawyerHelpAssignedLawyer>()
                       .Where(x => x.Id == Id)
                       .Select(x => new CaseLawyerHelpAssignedLawyerVM()
                       {
                           Id = x.Id,
                           Lawyer = x.LawyerNumber + " " + x.LawyerName + (repo.AllReadonly<LawUnit>().Any(l => l.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Lawyer && l.Code == x.LawyerNumber) ? "" : " (Не е намерен в списъка в ЕИСС)"),
                           People = string.Join(", ", x.Persons.Select(p => p.CaseLawyerHelpPerson.CasePerson.FullName + " (" + p.CaseLawyerHelpPerson.CasePerson.PersonRole.Label + ")")),
                           LawyerStateLabel = x.LawyerState.Label,
                           IsEdit = x.LawyerStateDate == null,
                           IsFinish = (x.LawyerStateDate == null && x.LawyerStateId != NomenclatureConstants.EesppLawyerState.Assigned),
                           CaseName = x.CaseLawyerHelp.Case.CaseType.Code + " " + x.CaseLawyerHelp.Case.ShortNumber + "/" + x.CaseLawyerHelp.Case.RegDate.ToString("yyyy"),
                           CaseId = x.CaseLawyerHelp.CaseId
                       })
                       .FirstOrDefault();


        }

        public CaseLawyerHelpAssignedLawyerEditVM CaseLawyerHelpAssignedLawyerEditVM_GetById(int Id)
        {
            return repo.AllReadonly<CaseLawyerHelpAssignedLawyer>()
                       .Where(x => x.Id == Id)
                       .Select(x => new CaseLawyerHelpAssignedLawyerEditVM()
                       {
                           Id = x.Id,
                           CaseLawyerHelpId = x.CaseLawyerHelpId,
                           CaseSessionActAssignedId = x.CaseSessionActAssignedId,
                           LawyerStateId = x.LawyerStateId,
                           CaseId = x.CaseLawyerHelp.CaseId
                       })
                       .FirstOrDefault();
        }

        public bool CaseLawyerHelpAssignedLawyer_SaveData(CaseLawyerHelpAssignedLawyerEditVM model)
        {
            try
            {
                //Update
                var saved = repo.GetById<CaseLawyerHelpAssignedLawyer>(model.Id);
                saved.LawyerStateId = model.LawyerStateId;
                saved.CaseSessionActAssignedId = model.CaseSessionActAssignedId;

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при сетване на статус на Върнати адвокати по заявка за правна помощ от ЕЕСПП Id={model.Id}");
                return false;
            }
        }
    }
}
