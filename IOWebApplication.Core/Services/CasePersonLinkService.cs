using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
                            x.CaseSessionId == null &&
                            x.DateExpired == null)
                .Select(x => new CasePersonLinkListVM()
                {
                    Id = x.Id,
                    CasePersonName = x.CasePerson.FullName + "(" + (x.CasePerson.Uic ?? "") + ") - " + x.CasePerson.PersonRole.Label,
                    LinkDirectionName = x.LinkDirection.Label,
                    CasePersonRelName = x.CasePersonRel.FullName + "(" + (x.CasePersonRel.Uic ?? "") + ") - " + x.CasePersonRel.PersonRole.Label,
                    LinkDirectionSecondName = x.LinkDirectionSecond.Label,
                    CasePersonSecondRelName = (x.CasePersonSecondRelId > 0) ? x.CasePersonSecondRel.FullName + "(" + (x.CasePersonSecondRel.Uic ?? "") + ") - " + x.CasePersonSecondRel.PersonRole.Label : "",
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
                logger.LogError(ex, $"Грешка при запис на CasePersonLink Id={model.Id}");
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

        private List<CaseNotificationLinkVM> GetLinkForPersonList(int personId, int caseId, int? caseSessionId)
        {
            var casePersons = repo.AllReadonly<CasePerson>()
                                  .Where(x => x.CaseId == caseId &&
                                              x.CaseSessionId == caseSessionId);


            return repo.AllReadonly<CasePersonLink>()
                         .Where(x => x.CaseId == caseId &&
                                     x.DateExpired == null &&
                                     (x.CasePersonId == personId ||
                                      x.CasePersonRelId == personId ||
                                      x.CasePersonSecondRelId == personId))
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
                             PersonSecondRelId = casePersons.Where(c => c.CasePersonIdentificator == (x.CasePersonSecondRel.CasePersonIdentificator ?? "")).Select(c => c.Id).FirstOrDefault(),
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
                             LabelWithoutFirstPerson = "",
                             DateFrom = x.DateFrom,
                             DateTo = x.DateTo
                         })
                         .ToList();
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
                              .Where(x => x.CasePersonIdentificator == person.CasePersonIdentificator &&
                                          x.CaseSessionId == null)
                              .FirstOrDefault();

            if (personC == null)
                return result;

            result = GetLinkForPersonList(personC.Id, caseId, caseSessionId);

            if (filterPersonOnNotification)
            {
                var notificationList = repo.AllReadonly<CaseSessionNotificationList>()
                                           .Where(x => x.CaseId == caseId &&
                                                       x.CaseSessionId == caseSessionId &&
                                                       x.DateExpired == null &&
                                                       (x.NotificationListTypeId == notificationListTypeId ||
                                                         (notificationListTypeId == SourceTypeSelectVM.CaseSessionNotificationList && x.NotificationListTypeId == null))
                                           ).ToList();
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
        /// Извличане на Връзки по страни, може и втори представляващ по лице
        /// </summary>
        /// <param name="casePersonId"></param>
        /// <returns></returns>
        public List<CaseNotificationLinkVM> GetLinkForPersonMediation(int casePersonId, int mediationSessionId)
        {
            var result = new List<CaseNotificationLinkVM>();
            var person = repo.AllReadonly<CasePerson>()
                             .Where(x => x.Id == casePersonId)
                             .FirstOrDefault();

            if (person == null)
                return result;

            int caseId = person.CaseId;


            result = GetLinkForPersonList(person.Id, caseId, null);

            var notificationList = repo.AllReadonly<MediationCasePerson>()
                                       .Where(x => x.CaseId == caseId &&
                                                   x.MediationCaseSessionId == mediationSessionId &&
                                                   x.DateExpired == null
                                       ).ToList();
            result = result
                .Where(x => notificationList.Any(n => n.CasePersonId == x.PersonId) &&
                            notificationList.Any(n => n.CasePersonId == x.PersonRelId) &&
                            (x.PersonSecondRelId <= 0 || notificationList.Any(n => n.CasePersonId == x.PersonSecondRelId))
                )
                .ToList();
            result = result.Where(x => x.PersonId > 0).ToList();
            foreach (var item in result)
            {
                SetLabelFromTemplate(item);
            }
            return result;
        }
        /// <summary>
        /// Извличане на Връзки по страни, може и втори представляващ по лице за списък за множествено призоваване
        /// </summary>
        public async Task<List<CaseNotificationLinkVM>> GetLinkForPersonList(int[] casePersonIds, int caseId, int caseSessionId)
        {
            var result = new List<CaseNotificationLinkVM>();


            var casePersons = repo.AllReadonly<CasePerson>()
                                  .Where(x => x.CaseId == caseId &&
                                              x.CaseSessionId == caseSessionId);


            result = await repo.AllReadonly<CasePersonLink>()
                         .Where(x => x.CaseId == caseId &&
                                     x.DateExpired == null)
                         .Select(x => new CaseNotificationLinkVM()
                         {
                             Id = x.Id,
                             PersonCaseId = x.CasePersonId,
                             PersonCaseRelId = x.CasePersonRelId,
                             PersonCaseSecondRelId = x.CasePersonSecondRelId,
                             PersonGuid = x.CasePerson.CasePersonIdentificator,
                             PersonRelGuid = x.CasePersonRel.CasePersonIdentificator,
                             PersonId = casePersons.Where(c => c.CasePersonIdentificator != null && c.CasePersonIdentificator == x.CasePerson.CasePersonIdentificator).Select(c => c.Id).FirstOrDefault(),
                             PersonRelId = casePersons.Where(c => c.CasePersonIdentificator != null && c.CasePersonIdentificator == x.CasePersonRel.CasePersonIdentificator).Select(c => c.Id).FirstOrDefault(),
                             PersonSecondRelId = casePersons.Where(c => c.CasePersonIdentificator != null && c.CasePersonIdentificator == x.CasePersonSecondRel.CasePersonIdentificator).Select(c => c.Id).FirstOrDefault(),
                             LinkDirectionId = x.LinkDirectionId,
                             LinkDirectionSecondId = x.LinkDirectionSecondId,
                             PersonName = x.CasePerson.FullName,
                             PersonRelName = x.CasePersonRel.FullName,
                             PersonSecondRelName = x.CasePersonSecondRel.FullName,
                             PersonRole = x.CasePerson.PersonRole.Label,
                             PersonRelRole = x.CasePersonRel.PersonRole.Label,
                             PersonSecondRelRole = x.CasePersonSecondRel.PersonRole.Label,
                             LinkTemplate = x.LinkDirection.LinkTemplate,
                             LinkTemplateVks = x.LinkDirection.LinkTemplateVks,
                             SecondLinkTemplate = x.LinkDirectionSecond.LinkTemplate,
                             SecondLinkTemplateVks = x.LinkDirectionSecond.LinkTemplateVks,
                             DateFrom = x.DateFrom,
                             DateTo = x.DateTo
                         })
                         .ToListAsync();
            result = result
                    .Where(x => (casePersonIds.Any(n => n == x.PersonId) &&
                                 casePersonIds.Any(n => n == x.PersonRelId) &&
                                 (x.PersonSecondRelId <= 0 || casePersonIds.Any(n => n == x.PersonSecondRelId))
                                )
                    )
                    .ToList();
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
        public List<SelectListItem> ListForPersonToDropDown(List<CaseNotificationLinkVM> linkList, int casePersonId, bool addDefaultElement = true, bool addMulti = true)
        {
            var result = linkList.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Label
            }).ToList() ?? new List<SelectListItem>();
            var listBy = FilterPresentByList(linkList, casePersonId);
            if (addMulti && listBy.Count >= 1)
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

        private Expression<Func<PersonRoleLinkDirection, bool>> expressionIsForPersonZ(bool checkForTrue)
        {
            if (checkForTrue)
            {
                return x => EF.Functions.ILike(x.LinkDirection.LinkTemplate, "{Z}".ToPaternSearch());
            }
            else
            {
                return x => !EF.Functions.ILike(x.LinkDirection.LinkTemplate, "{Z}".ToPaternSearch());
            }
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
                 .Where(expressionIsForPersonZ(false))
                 //.Where(x => !IsForPersonZ(x.LinkDirection.LinkTemplate))
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
                    var linkDirections = repo.AllReadonly<LinkDirection>().ToList();
                    roleLinks = linkDirections
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
                                .Where(expressionIsForPersonZ(true));
            //.Where(x => IsForPersonZ(x.LinkDirection.LinkTemplate));

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
                                 .Where(x => !roleLinks.Any(z => x.Id == z.PersonRoleId));
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
                            .Where(expressionIsForPersonZ(true))
                            //.Where(x => IsForPersonZ(x.LinkDirection.LinkTemplate))
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
        /// <param name="casePersonLinkId">идентификатор на връзка</param>
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
                    PersonName = x.PersonRole.Label + " " + x.FullName
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
                }
                else
                {
                    casePersonLink.CasePersonId = model.CasePersonRelId;
                    casePersonLink.CasePersonRelId = personId;
                }
                repo.Add(casePersonLink);
            }
            repo.SaveChanges();
            return true;
        }
        public bool HaveSameLink(CasePersonLink model)
        {
            var casePersonSecondRelId = model.CasePersonSecondRelId.EmptyToNull();
            return repo.AllReadonly<CasePersonLink>()
                       .Where(x => x.Id != model.Id &&
                                   x.DateExpired == null &&
                                   x.CaseId == model.CaseId &&
                                   x.CasePersonId == model.CasePersonId &&
                                   x.CasePersonRelId == model.CasePersonRelId &&
                                   x.DateFrom <= model.DateFrom.Date &&
                                   model.DateFrom.Date <= (x.DateTo ?? DateTime.Now.Date) &&
                                   x.CasePersonSecondRelId == casePersonSecondRelId
                                   )
                       .Any();
        }

        public List<CaseNotificationLinkVM> FilterLinkOnSession(List<CaseNotificationLinkVM> links, int? caseSessionId, List<int> oldLinks)
        {
            if (caseSessionId == null || !links.Any())
                return links;
            var caseSession = repo.AllReadonly<CaseSession>()
                                  .Where(x => x.Id == caseSessionId)
                                  .FirstOrDefault();
            if (caseSession == null)
                return links;
            var dateEnd = DateTime.Now.AddYears(100);
            links = links.Where(x => (oldLinks != null && oldLinks.Any(o => x.Id == o)) ||
                                     (x.DateTo ?? dateEnd) >= caseSession.DateFrom.Date)
                         .ToList();
            return links;
        }

        public EpepSummonInfoVM GetEpepSummonInfo(CaseNotification model, bool chechAssignment = true)
        {
            if (model.CasePersonLinkId > 0 && model.CasePersonL1Id == null)
            {
                model.CasePersonL1Id = model.CasePersonId;
                model.CasePersonL2Id = null;
                model.CasePersonL3Id = null;

                CaseNotificationLinkVM casePersonLink = null;
                if (model.IsMultiLink != true && model.CasePersonLinkId > 0)
                {
                    var oldLinks = new List<int>() { model.CasePersonLinkId ?? 0 };
                    var casePersonLinks = GetLinkForPerson(model.CasePersonId ?? 0, NomenclatureConstants.FilterPersonOnNotification, model.NotificationTypeId ?? 0, oldLinks);
                    casePersonLinks = FilterLinkOnSession(casePersonLinks, model.CaseSessionId, oldLinks);
                    casePersonLink = casePersonLinks.Where(x => x.Id == model.CasePersonLinkId).FirstOrDefault();
                    if (casePersonLink != null)
                    {
                        model.CasePersonL1Id = casePersonLink.PersonId;
                        model.CasePersonL2Id = casePersonLink.PersonRelId;
                        if (!casePersonLink.isXFirst)
                        {
                            model.CasePersonL1Id = casePersonLink.PersonRelId;
                            model.CasePersonL2Id = casePersonLink.PersonId;
                        }
                        model.LinkDirectionId = casePersonLink.LinkDirectionId;
                        model.LinkDirectionSecondId = casePersonLink.LinkDirectionSecondId.EmptyToNull(0);
                        model.CasePersonL3Id = casePersonLink.PersonSecondRelId.EmptyToNull(0);
                    }
                }
            }


            int? casePersonId = null;

            #region Търси се последното валидно лице според връзката да има достъп, да види пролетчето

            //Лицето от призовката
            //int? linkPersonId = model.CasePersonId;
            //if (model.CasePersonLinkId > 0)
            //{
            //    //Ако има връзка - избира най-вътрешното валидно лице от връзката
            //    linkPersonId = model.CasePersonL3Id ?? (model.CasePersonL2Id ?? model.CasePersonL1Id);
            //}
            //casePersonId = getCasePersonIdFromCase(linkPersonId ?? 0, chechAssignment);
            #endregion

            #region Старата логика търсеше най-вътрешното лице според връзката, което има достъп, пак стана нова
            if (model.CasePersonL3Id > 0)
            {
                casePersonId = getCasePersonIdFromCase(model.CasePersonL3Id ?? 0, chechAssignment);
            }

            if (!casePersonId.HasValue && model.CasePersonL2Id > 0)
            {
                casePersonId = getCasePersonIdFromCase(model.CasePersonL2Id ?? 0, chechAssignment);
            }

            if (!casePersonId.HasValue && model.CasePersonL1Id > 0)
            {
                casePersonId = getCasePersonIdFromCase(model.CasePersonL1Id ?? 0, chechAssignment);
            }

            if (!casePersonId.HasValue && model.CasePersonId > 0)
            {
                casePersonId = getCasePersonIdFromCase(model.CasePersonId ?? 0, chechAssignment);
            }
            #endregion

            if (!casePersonId.HasValue)
            {
                return new EpepSummonInfoVM()
                {
                    CanSummonByEpep = false
                };
            }

            var result = new EpepSummonInfoVM()
            {
                CanSummonByEpep = casePersonId > 0,
                CasePersonId = casePersonId.Value,
                EpepUserId = repo.AllReadonly<EpepUserAssignment>()
                                    .Where(x => x.CasePersonId == casePersonId && x.DateExpired == null)
                                    .Where(x => x.CanSummon == true)
                                    .Select(x => x.EpepUserId)
                                    .FirstOrDefault()
            };
            if (model.CasePersonLinkId > 0)
            {
                var linkListVM = GetLinkForPerson(model.CasePersonId ?? 0, NomenclatureConstants.FilterPersonOnNotification, model.NotificationTypeId ?? 0, null);
                linkListVM = FilterLinkOnSession(linkListVM, model.CaseSessionId, null);
                var linkList = ListForPersonToDropDown(linkListVM.Where(x => x.Id == model.CasePersonLinkId).ToList(), model.CasePersonId ?? 0, false);
                if (linkList.Count > 0)
                {
                    result.AddresseeName = linkList.First().Text;
                }
            }
            else
            {
                result.AddresseeName = repo.GetPropById<CasePerson, string>(x => x.Id == model.CasePersonId, x => x.FullName);
            }

            return result;
        }

        private int? getCasePersonIdFromCase(int id, bool chechAssignment = true)
        {
            var _model = repo.AllReadonly<CasePerson>()
                        .Where(x => x.Id == id)
                        .Select(x => new { x.CaseSessionId, x.CasePersonIdentificator })
                        .FirstOrDefault();

            if (_model == null)
            {
                return null;
            }

            int casePersonId = 0;

            if (_model.CaseSessionId == null)
            {
                casePersonId = id;
            }
            else
            {
                casePersonId = repo.GetPropById<CasePerson, int>(x => x.CasePersonIdentificator == _model.CasePersonIdentificator && x.CaseSessionId == null, x => x.Id);
            }

            if (chechAssignment)
            {
                if (repo.AllReadonly<EpepUserAssignment>().Where(x => x.CasePersonId == casePersonId && x.DateExpired == null && x.CanSummon == true).Any())
                {
                    return casePersonId;
                }
            }
            else
            {
                return casePersonId;
            }

            return null;
        }
    }
}
