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
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;

namespace IOWebApplication.Core.Services
{
    public class CasePersonLinkService : BaseService, ICasePersonLinkService
    {
        public CasePersonLinkService(ILogger<CasePersonLinkService> _logger,
            IRepository _repo, IUserContext _userContext)
        {
            logger = _logger;
            userContext = _userContext;
            repo = _repo;
        }

        /// <summary>
        /// Извличане на данни за Връзки по страни, може и втори представляващ
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IQueryable<CasePersonLinkListVM> CasePersonLink_Select(int caseId)
        {
            return repo.AllReadonly<CasePersonLink>()
                .Where(x => x.CaseId == caseId && 
                            x.CaseSessionId  == null &&
                            x.DateExpired == null)
                .Select(x => new CasePersonLinkListVM()
                {
                    Id = x.Id,
                    CasePersonName = x.CasePerson.FullName + "(" + (x.CasePerson.Uic ?? "") + ") - " + x.CasePerson.PersonRole.Label,
                    LinkDirectionName = x.LinkDirection.Label,
                    CasePersonRelName = x.CasePersonRel.FullName + "(" + (x.CasePersonRel.Uic ?? "") + ") - " + x.CasePersonRel.PersonRole.Label,
                    LinkDirectionSecondName = x.LinkDirectionSecond.Label,
                    CasePersonSecondRelName = x.CasePersonSecondRel.FullName + "(" + (x.CasePersonSecondRel.Uic ?? "") + ") - " + x.CasePersonSecondRel.PersonRole.Label,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo
                }).AsQueryable();
        }

        /// <summary>
        /// Запис на Връзки по страни, може и втори представляващ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CasePersonLink_SaveData(CasePersonLink model)
        {
            try
            {
                model.LinkDirectionSecondId = (model.LinkDirectionSecondId ?? 0) <= 0 ? null : model.LinkDirectionSecondId;
                model.CasePersonSecondRelId = (model.CasePersonSecondRelId ?? 0) <= 0 ? null : model.CasePersonSecondRelId;

                if (model.Id > 0)
                {
                    //Update
                    var casePersonLink = repo.GetById<CasePersonLink>(model.Id);
                    casePersonLink.CasePersonId = model.CasePersonId;
                    casePersonLink.LinkDirectionId = model.LinkDirectionId;
                    casePersonLink.CasePersonRelId = model.CasePersonRelId;
                    casePersonLink.LinkDirectionSecondId = model.LinkDirectionSecondId;
                    casePersonLink.CasePersonSecondRelId = model.CasePersonSecondRelId;
                    casePersonLink.DateFrom = model.DateFrom;
                    casePersonLink.DateTo = model.DateTo;

                    repo.Update(casePersonLink);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    repo.Add<CasePersonLink>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CasePersonLink Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на стринг за уведомления за Връзки по страни, може и втори представляващ
        /// </summary>
        /// <param name="model"></param>
        private void SetLabelFromTemplate(CaseNotificationLinkVM model)
        {
            string personX = model.PersonName;
            string roleX = model.PersonRole;
            string personY = model.PersonRelName;
            string roleY = model.PersonRelRole;
            int posX = model.LinkTemplate.IndexOf("{X}");
            int posY = model.LinkTemplate.IndexOf("{Y}");
            model.isXFirst = true;
            if (posY < posX)
            {
                personX = model.PersonRelName;
                roleX = model.PersonRelRole;
                personY = model.PersonName;
                roleY = model.PersonRole;
                model.isXFirst = false;
            }
            model.Label = model.LinkTemplate
                               .Replace("{X}", personX)
                               .Replace("{RoleX}", roleX)
                               .Replace("{Y}", personY)
                               .Replace("{RoleY}", roleY);
            model.LabelWithoutSecondRel = model.Label;
            model.LabelWithoutFirstPerson = model.LinkTemplate
                                                 .Replace("{X}", string.Empty)
                                                 .Replace("{RoleX}", string.Empty)
                                                 .Replace("{Y}", personY)
                                                 .Replace("{RoleY}", roleY);
            model.LabelWithoutFirstPerson = model.LabelWithoutFirstPerson.Replace("()", "<br>");
            
            if (model.LinkDirectionSecondId > 0)
                model.Label += model.SecondLinkTemplate
                               .Replace("{Z}", model.PersonSecondRelName)
                               .Replace("{RoleZ}", model.PersonSecondRelRole);
        }

        /// <summary>
        /// Извличане на Връзки по страни, може и втори представляващ по лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public List<CaseNotificationLinkVM> GetLinkForPerson(int casePersonId, bool filterPersonOnNotification, int notificationTypeId, List<int> oldLinks)
        {
            var result = new List<CaseNotificationLinkVM>();
            int notificationListTypeId = NomenclatureConstants.NotificationType.ToListType(notificationTypeId);


            var person = repo.AllReadonly<CasePerson>()
                             .Where(x => x.Id == casePersonId)
                             .FirstOrDefault();
            if (person == null)
                return result;
            int caseId = person.CaseId;
            int? caseSessionId = person.CaseSessionId;
            var personC = repo.AllReadonly<CasePerson>()
                             .Where(x => x.CasePersonIdentificator == person.CasePersonIdentificator && x.CaseSessionId == null)
                             .FirstOrDefault();
            if (personC == null)
                return result;
            var casePersons = repo.AllReadonly<CasePerson>()
                                  .Where(x => x.CaseId == caseId && 
                                              x.CaseSessionId == caseSessionId );
            var notificationList = repo.AllReadonly<CaseSessionNotificationList>()
                                       .Where(x => x.CaseId == caseId && 
                                                   x.CaseSessionId == caseSessionId &&
                                                   x.DateExpired == null &&
                                                   (x.NotificationListTypeId == notificationListTypeId || (notificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList && x.NotificationListTypeId == null)));
            result = repo.AllReadonly<CasePersonLink>()
                .Where(x => x.CaseId == caseId && 
                            x.DateExpired == null &&
                           (x.CasePersonId == personC.Id || x.CasePersonRelId == personC.Id || x.CasePersonSecondRelId == personC.Id) 
                )
                .Select(x => new CaseNotificationLinkVM()
                {
                    Id = x.Id,
                    PersonCaseId = x.CasePersonId,
                    PersonCaseRelId = x.CasePersonRelId,
                    PersonCaseSecondRelId = x.CasePersonSecondRelId,
                    PersonGuid = x.CasePerson.CasePersonIdentificator,
                    PersonRelGuid = x.CasePersonRel.CasePersonIdentificator,
                    PersonId = casePersons.Where(c => c.CasePersonIdentificator == x.CasePerson.CasePersonIdentificator).Select(c => c.Id).FirstOrDefault(),
                    PersonRelId = casePersons.Where(c => c.CasePersonIdentificator == x.CasePersonRel.CasePersonIdentificator).Select(c => c.Id).FirstOrDefault(),
                    PersonSecondRelId =  casePersons.Where(c => c.CasePersonIdentificator == (x.CasePersonSecondRel.CasePersonIdentificator ?? "")).Select(c => c.Id).FirstOrDefault(),
                    LinkDirectionId = x.LinkDirectionId,
                    LinkDirectionSecondId = x.LinkDirectionSecondId,
                    PersonName = x.CasePerson.FullName,
                    PersonRelName = x.CasePersonRel.FullName ?? "",
                    PersonSecondRelName = x.CasePersonSecondRel.FullName ?? "",
                    PersonRole = x.CasePerson.PersonRole.Label,
                    PersonRelRole = x.CasePersonRel.PersonRole.Label ?? "",
                    PersonSecondRelRole = x.CasePersonSecondRel.PersonRole.Label ?? "",
                    LinkTemplate = x.LinkDirection.LinkTemplate,
                    LinkTemplateVks = x.LinkDirection.LinkTemplateVks,
                    SecondLinkTemplate = x.LinkDirectionSecond.LinkTemplate,
                    SecondLinkTemplateVks = x.LinkDirectionSecond.LinkTemplateVks,
                    Label = "",
                    LabelWithoutFirstPerson = ""
                }).ToList();
            if (filterPersonOnNotification)
            {
                result = result
                    .Where(x => (oldLinks != null && oldLinks.Any(o => o == x.Id)) || 
                                (
                                    notificationList.Any(n => n.CasePersonId == x.PersonId) &&
                                    notificationList.Any(n => n.CasePersonId == x.PersonRelId) &&
                                    (x.PersonSecondRelId <= 0 || notificationList.Any(n => n.CasePersonId == x.PersonSecondRelId))
                                )
                    )
                    .ToList();
            }
            result = result.Where(x => x.PersonId > 0).ToList();
            foreach (var item in result)
            {
                SetLabelFromTemplate(item);
            }
            return result;
        }

        /// <summary>
        /// Филтриране на списък с Връзки по страни, може и втори представляващ
        /// </summary>
        /// <param name="linkList"></param>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        private List<CaseNotificationLinkVM> FilterPresentByList(List<CaseNotificationLinkVM> linkList, int casePersonId)
        {
            return linkList.Where(x => (!x.isXFirst && x.PersonId == casePersonId) ||
                                       (x.isXFirst && x.PersonRelId == casePersonId) ||
                                       (x.PersonSecondRelId == casePersonId)
                                     ).ToList();
        }

        /// <summary>
        /// Филтриране на списък с Връзки по страни, може и втори представляващ
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public List<CaseNotificationLinkVM> GetPresentByList(int casePersonId, bool filterPersonOnNotification, int notificationTypeId, List<int> oldLinks)
        {
            var listFrom = GetLinkForPerson(casePersonId, filterPersonOnNotification, notificationTypeId, oldLinks);
            return FilterPresentByList(listFrom, casePersonId);
        }

        /// <summary>
        /// Извличане на данни за Връзки по страни, може и втори представляващ за комбо
        /// </summary>
        /// <param name="linkList"></param>
        /// <param name="casePersonId"></param>
        /// <param name="addDefaultElement"></param>
        /// <returns></returns>
        public List<SelectListItem> ListForPersonToDropDown(List<CaseNotificationLinkVM> linkList,int casePersonId, bool addDefaultElement = true)
        {
            var result = linkList.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Label
            }).ToList() ?? new List<SelectListItem>();
            var listBy = FilterPresentByList(linkList, casePersonId);
            if (listBy.Count >= 1)
            {
                result = result.Prepend(new SelectListItem() { Text = "Множествено уведомяване", Value = "-2" })
                              .ToList();
            }

            if (addDefaultElement)
            {
                result = result.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                              .ToList();
            }
            return result;
        }

        /// <summary>
        /// Метод за попълване на бланки
        /// </summary>
        /// <param name="linkTemplate"></param>
        /// <returns></returns>
        private bool isPersonXFirst(string linkTemplate)
        {
            int posX = linkTemplate.IndexOf("{X}");
            int posY = linkTemplate.IndexOf("{Y}");
            return (posY > posX);
        }

        /// <summary>
        /// Метод за попълване на бланки
        /// </summary>
        /// <param name="linkTemplate"></param>
        /// <returns></returns>
        private bool IsForPersonZ(string linkTemplate)
        {
            int posZ = linkTemplate.IndexOf("{Z}");
            return (posZ > 0);
        }

        /// <summary>
        /// Извличане на данни за Възможенo участие на роля във връзка между страни за комбо
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public List<SelectListItem> LinkDirectionForPersonDDL(int casePersonId)
        {
            var casePerson = repo.AllReadonly<CasePerson>()
                .Include(x => x.PersonRole)
                .Where(x => x.Id == casePersonId)
                .FirstOrDefault();
            var roleLinks = new List<LinkDirection>();
            if (casePerson != null)
            {
                roleLinks = repo.AllReadonly<PersonRoleLinkDirection>()
                 .Where(x => x.PersonRoleId == casePerson.PersonRoleId)
                 .Where(x => !IsForPersonZ(x.LinkDirection.LinkTemplate))
                 .Select(x => x.LinkDirection)
                 .ToList();
                if (roleLinks.Any())
                {
                    roleLinks = roleLinks.Where(x => !isPersonXFirst(x.LinkTemplate))
                                         .OrderBy(x => x.Id)
                                         .ToList();
                }
                else
                {
                    roleLinks = repo.AllReadonly<LinkDirection>()
                                    .Where(x => isPersonXFirst(x.LinkTemplate))
                                    .OrderBy(x => x.Id)
                                    .ToList();
                }
            }
            return roleLinks.AsQueryable().ToSelectList(true, false, true);
        }

        /// <summary>
        /// Извличане на данни за Ред на представляване
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> SecondLinkDirectionDDL()
        {
            var roleLinks = repo.AllReadonly<PersonRoleLinkDirection>()
                                .Where(x => IsForPersonZ(x.LinkDirection.LinkTemplate));

            return repo.AllReadonly<LinkDirection>()
                       .Where(x => roleLinks.Any(r => r.LinkDirectionId == x.Id))
                       .ToSelectList(true, false, true);
        }

        /// <summary>
        /// Извличане на данни за Ред на представляване
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="linkDirectionId"></param>
        /// <param name="defaultElementText"></param>
        /// <returns></returns>
        public List<SelectListItem> RelationalPersonDDL(int caseId, int linkDirectionId, string defaultElementText = null)
        {
            var casePerson = new List<SelectListItem>();
            var linkDirection = repo.AllReadonly<LinkDirection>()
                                    .Where(x => x.Id == linkDirectionId)
                                    .FirstOrDefault();
            if (linkDirection != null)
            {
                var roles = repo.AllReadonly<PersonRoleLinkDirection>()
                                 .Where(x => x.LinkDirectionId == linkDirectionId)
                                 .Select(x => x.PersonRole);
                if (!isPersonXFirst(linkDirection.LinkTemplate))
                {
                    var roleLinks = repo.AllReadonly<PersonRoleLinkDirection>();
                    roles = repo.AllReadonly<PersonRole>()
                                 .Where(x =>  !roleLinks.Any(z => x.Id == z.PersonRoleId));
                }
                casePerson = repo.AllReadonly<CasePerson>()
                     .Where(x => x.CaseId == caseId &&
                                 x.CaseSessionId == null &&
                                 x.DateExpired == null &&
                                 roles.Any(r => r.Id == x.PersonRoleId))
                            .OrderBy(x => x.RowNumber)
                            .Select(x => new SelectListItem()
                            {
                                Value = x.Id.ToString(),
                                Text = x.FullName + "(" + (x.Uic ?? "") + ") - " + x.PersonRole.Label
                            }).ToList();

            }
            if (string.IsNullOrEmpty(defaultElementText))
                defaultElementText = "Избери";
            casePerson.Insert(0, new SelectListItem() { Text = defaultElementText, Value = "-1" });
            return casePerson;
        }
        /// <summary>
        /// Извличане на данни за Ред на представляване
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="linkDirectionId"></param>
        /// <param name="defaultElementText"></param>
        /// <returns></returns>
        public List<SelectListItem> PersonYDDL(int caseId, int linkDirectionId, string defaultElementText = null)
        {
            var casePerson = new List<SelectListItem>();
            var linkDirection = repo.AllReadonly<LinkDirection>()
                                    .Where(x => x.Id == linkDirectionId)
                                    .FirstOrDefault();
            if (linkDirection != null)
            {
                var roles = repo.AllReadonly<PersonRoleLinkDirection>()
                                    .Where(x => x.LinkDirectionId == linkDirectionId)
                                    .Select(x => x.PersonRole);
                casePerson = repo.AllReadonly<CasePerson>()
                     .Where(x => x.CaseId == caseId &&
                                 x.CaseSessionId == null &&
                                 x.DateExpired == null &&
                                 roles.Any(r => r.Id == x.PersonRoleId))
                            .OrderBy(x => x.RowNumber)
                            .Select(x => new SelectListItem()
                            {
                                Value = x.Id.ToString(),
                                Text = x.FullName + "(" + (x.Uic ?? "") + ") - " + x.PersonRole.Label
                            }).ToList();
            }
            if (string.IsNullOrEmpty(defaultElementText))
                defaultElementText = "Избери";
            casePerson.Insert(0, new SelectListItem() { Text = defaultElementText, Value = "-1" });
            return casePerson;
        }

        /// <summary>
        /// Извличане на данни за Ред на представляване
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="defaultElementText"></param>
        /// <returns></returns>
        public List<SelectListItem> SeccondRelationalPersonDDL(int caseId, string defaultElementText = null)
        {
            var roles = repo.AllReadonly<PersonRoleLinkDirection>()
                            .Where(x => IsForPersonZ(x.LinkDirection.LinkTemplate))
                            .Select(x => x.PersonRole);
            var casePerson = repo.AllReadonly<CasePerson>()
                     .Where(x => x.CaseId == caseId &&
                                 x.CaseSessionId == null &&
                                 x.DateExpired == null &&
                                 roles.Any(r => r.Id == x.PersonRoleId))
                            .OrderBy(x => x.RowNumber)
                            .Select(x => new SelectListItem()
                            {
                                Value = x.Id.ToString(),
                                Text = x.FullName + "(" + (x.Uic ?? "") + ") - " + x.PersonRole.Label
                            }).ToList();


            if (string.IsNullOrEmpty(defaultElementText))
                defaultElementText = "Избери";
            casePerson.Insert(0, new SelectListItem() { Text = defaultElementText, Value = "-1" });
            return casePerson;
        }
        /// <summary>
        /// Проверка дали има призовка/съобщение/уведомление с тази връзка
        /// </summary>
        /// <param name="caseId"></param>
        /// идентификатор на дело
        /// <param name="casePersonLinkId"></param>
        /// идентификатор на връзка
        /// <returns></returns>
        public bool HaveCaseNotification(int casePersonLinkId)
        {
            int? caseId = repo.AllReadonly<CasePersonLink>()
                    .Where(x => x.Id == casePersonLinkId)
                    .Select(x => x.CaseId)
                    .FirstOrDefault();
            return repo.AllReadonly<CaseNotification>()
                             .Where(x => x.CaseId == caseId &&
                                x.DateExpired == null &&
                                (x.CasePersonLinkId == casePersonLinkId ||
                                  (x.IsMultiLink == true && x.CaseNotificationMLinks.Any(l => l.CasePersonLinkId == casePersonLinkId))
                                )
                              )
                              .Any();
        }
        /// <summary>
        /// Извличане на данни лява и дясна страна
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> RoleKindDDL()
        {
            var result = repo.AllReadonly<RoleKind>()
                              .Where(x => x.Id == NomenclatureConstants.RoleKind.RightSide ||  
                                          x.Id == NomenclatureConstants.RoleKind.LeftSide)
                              .Select(x => new SelectListItem()
                              {
                                  Value = x.Id.ToString(),
                                  Text = x.Label
                              }).ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        /// <summary>
        /// Изчитане на лица от дело
        /// </summary>
        /// <param name="caseId">дело</param>
        /// <param name="roleKindId">лява/дясна страна</param>
        /// <returns></returns>
        public List<CasePersonLinkSideItemVM> GetPersonXBySide(int caseId, int roleKindId)
        {
            return repo.AllReadonly<CasePerson>()
                .Where(x => x.CaseId == caseId &&
                            x.CaseSessionId == null &&
                            x.PersonRole.RoleKindId == roleKindId)
                .Select(x => new CasePersonLinkSideItemVM()
                {
                    Id = x.Id,
                    IsChecked = true,
                    PersonName = x.PersonRole.Label+" "+x.FullName
                })
                .ToList();
        }

        public bool Save_AddSide(CasePersonLinkSideVM model, List<int> personIds)
        {
            var linkDirection = repo.AllReadonly<LinkDirection>()
                              .Where(x => x.Id == model.LinkDirectionId)
                              .FirstOrDefault();
            foreach (var personId in personIds)
            {
                var casePersonLink = new CasePersonLink();

                casePersonLink.LinkDirectionId = model.LinkDirectionId;
                casePersonLink.DateFrom = model.DateFrom;
                casePersonLink.DateTo = model.DateTo;
                casePersonLink.CaseId = model.CaseId;
                casePersonLink.CourtId = model.CourtId;
                if (isPersonXFirst(linkDirection.LinkTemplate))
                {
                    casePersonLink.CasePersonId = personId;
                    casePersonLink.CasePersonRelId = model.CasePersonRelId;
                } else
                {
                    casePersonLink.CasePersonId = model.CasePersonRelId;
                    casePersonLink.CasePersonRelId = personId;
                }
                repo.Add(casePersonLink);
            }
            repo.SaveChanges();
            return true;
        }
    }
}
