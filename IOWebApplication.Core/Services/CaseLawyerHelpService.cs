using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
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

namespace IOWebApplication.Core.Services
{
    public class CaseLawyerHelpService : BaseService, ICaseLawyerHelpService
    {
        public CaseLawyerHelpService(ILogger<CaseLawyerHelpService> _logger,
                                     IRepository _repo,
                                     IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
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
                                         LawyerHelpBaseId = x.LawyerHelpBaseId,
                                         LawyerHelpTypeId = x.LawyerHelpTypeId,
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
                                    NomenclatureConstants.PersonRole.ListForLawyerHelp_Person.Contains(x.PersonRoleId)) &&
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
            return repo.AllReadonly<CaseLawyerHelp>()
                       .Where(x => x.CaseId == CaseId &&
                                   x.DateExpired == null)
                       .Select(x => new CaseLawyerHelpVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CaseId = x.CaseId,
                           LawyerHelpBaseText = x.LawyerHelpBase.Label,
                           LawyerHelpTypeText = x.LawyerHelpType.Label
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

        public bool CaseLawyerHelp_SaveData(CaseLawyerHelpEditVM model)
        {
            model.CaseSessionToGoId = model.CaseSessionToGoId.NumberEmptyToNull();
            model.ActAppointmentId = model.ActAppointmentId.NumberEmptyToNull();
            model.LawyerHelpBasisAppointmentId = model.LawyerHelpBasisAppointmentId.NumberEmptyToNull();
            var modelSave = FillCaseLawyerHelp(model);
            var caseLawyerHelpOtherLawyers = new List<CaseLawyerHelpOtherLawyer>();

            try
            {
                if (modelSave.Id > 0)
                {
                    caseLawyerHelpOtherLawyers = repo.AllReadonly<CaseLawyerHelpOtherLawyer>()
                                                     .Where(x => x.CaseLawyerHelpId == model.Id)
                                                     .ToList();

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
                    repo.Add<CaseLawyerHelp>(modelSave);

                    if (model.CaseLawyerHelpPeople != null)
                    {
                        foreach (var checkListVM in model.CaseLawyerHelpPeople.Where(x => x.Checked))
                        {
                            var caseLawyerHelpPerson = new CaseLawyerHelpPerson()
                            {
                                CaseLawyerHelpId = modelSave.Id,
                                CasePersonId = int.Parse(checkListVM.Value)
                            };

                            repo.Add(caseLawyerHelpPerson);
                        }
                    }
                }

                if (model.CaseLawyerHelpOtherLawyers != null)
                {
                    foreach (var checkList in model.CaseLawyerHelpOtherLawyers)
                    {
                        var caseLawyerHelpOtherLawyer = caseLawyerHelpOtherLawyers.Where(x => x.CasePersonId == int.Parse(checkList.Value)).FirstOrDefault();

                        if ((checkList.Checked) && (caseLawyerHelpOtherLawyer == null))
                        {
                            var caseLawyerHelpOtherLawyerSave = new CaseLawyerHelpOtherLawyer()
                            {
                                CaseLawyerHelpId = modelSave.Id,
                                CasePersonId = int.Parse(checkList.Value),
                            };

                            repo.Add(caseLawyerHelpOtherLawyerSave);
                        }

                        if ((!checkList.Checked) && (caseLawyerHelpOtherLawyer != null))
                        {
                            repo.Delete(caseLawyerHelpOtherLawyer);
                        }
                    }
                }

                repo.SaveChanges();

                model.Id = modelSave.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Искане за Правна помощ Id={ modelSave.Id }");
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
                           AssignedLawyerText = x.AssignedLawyer.FullName
                       })
                       .AsQueryable();
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
                                                   NomenclatureConstants.PersonRole.ListForLawyerHelp_Person.Contains(x.PersonRoleId)) &&
                                                  ((x.DateTo ?? DateTime.Now.AddYears(1)) >= DateTime.Now) &&
                                                  (caseLawyerHelpPeople.Count > 0 ? !caseLawyerHelpPeople.Contains(x.Id) : true))
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.FullName + (x.PersonRoleId != null ? " (" + x.PersonRole.Label + ")" : string.Empty),
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

            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLawyerHelpPerson>(model.Id);
                    saved.CasePersonId = model.CasePersonId;
                    saved.AssignedLawyerId = model.AssignedLawyerId;
                    saved.SpecifiedLawyerLawUnitId = model.SpecifiedLawyerLawUnitId;
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
                logger.LogError(ex, $"Грешка при запис на лица, за които се иска правна помощ Id={ model.Id }");
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
                logger.LogError(ex, $"Грешка при запис на лица, за които се иска правна помощ Id={ model.CaseLawyerHelpId }");
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
    }
}
