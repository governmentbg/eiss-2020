// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Core.Extensions;

namespace IOWebApplication.Core.Services
{
    public class DocumentPersonLinkService : BaseService, IDocumentPersonLinkService
    {
        public DocumentPersonLinkService(ILogger<DocumentPersonLinkService> _logger,
            IRepository _repo, IUserContext _userContext)
        {
            logger = _logger;
            userContext = _userContext;
            repo = _repo;
        }
        /// <summary>
        /// Извличане на данни за Връзки по страни, може и втори представляващ
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public IQueryable<CasePersonLinkListVM> DocumentPersonLink_Select(long documentId)
        {
            return repo.AllReadonly<DocumentPersonLink>()
                .Where(x => x.DocumentId == documentId &&
                            x.DateExpired == null)
                .Select(x => new CasePersonLinkListVM()
                {
                    Id = x.Id,
                    CasePersonName = x.DocumentPerson.FullName + "(" + (x.DocumentPerson.Uic ?? "") + ") - " + x.DocumentPerson.PersonRole.Label,
                    LinkDirectionName = x.LinkDirection.Label,
                    CasePersonRelName = x.DocumentPersonRel.FullName + "(" + (x.DocumentPersonRel.Uic ?? "") + ") - " + x.DocumentPersonRel.PersonRole.Label,
                    LinkDirectionSecondName = x.LinkDirectionSecond.Label,
                    CasePersonSecondRelName = x.DocumentPersonSecondRel.FullName + "(" + (x.DocumentPersonSecondRel.Uic ?? "") + ") - " + x.DocumentPersonSecondRel.PersonRole.Label,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo
                }).AsQueryable();
        }
        /// <summary>
        /// Запис на Връзки по страни, може и втори представляващ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool DocumentPersonLink_SaveData(DocumentPersonLink model)
        {
            try
            {
                model.LinkDirectionSecondId = model.LinkDirectionSecondId.EmptyToNull();
                model.DocumentPersonSecondRelId = model.DocumentPersonSecondRelId <= 0 ? null : model.DocumentPersonSecondRelId;

                if (model.Id > 0)
                {
                    //Update
                    var docPersonLink = repo.GetById<DocumentPersonLink>(model.Id);
                    docPersonLink.DocumentPersonId = model.DocumentPersonId;
                    docPersonLink.LinkDirectionId = model.LinkDirectionId;
                    docPersonLink.DocumentPersonRelId = model.DocumentPersonRelId;
                    docPersonLink.LinkDirectionSecondId = model.LinkDirectionSecondId;
                    docPersonLink.DocumentPersonSecondRelId = model.DocumentPersonSecondRelId;
                    docPersonLink.DateFrom = model.DateFrom;
                    docPersonLink.DateTo = model.DateTo;

                    repo.Update(docPersonLink);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    repo.Add<DocumentPersonLink>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на DocumentPersonLink Id={ model.Id }");
                return false;
            }
        }
        /// <summary>
        /// Извличане на Връзки по страни, може и втори представляващ по лице
        /// </summary>
        /// <param name="documentPersonId"></param>
        /// <param name="notificationTypeId"></param>
        /// <param name="oldLinks"></param>
        /// <returns></returns>
        public List<DocumentNotificationLinkVM> GetLinkForPerson(long documentPersonId, int notificationTypeId, List<int> oldLinks)
        {
            var result = new List<DocumentNotificationLinkVM>();

            var person = repo.AllReadonly<DocumentPerson>()
                             .Where(x => x.Id == documentPersonId)
                             .FirstOrDefault();
            if (person == null)
                return result;
            var docPersons = repo.AllReadonly<DocumentPerson>()
                                  .Where(x => x.DocumentId == person.DocumentId);

            result = repo.AllReadonly<DocumentPersonLink>()
                .Where(x => x.DocumentId == person.DocumentId &&
                            x.DateExpired == null &&
                           (x.DocumentPersonId == person.Id || x.DocumentPersonRelId == person.Id || x.DocumentPersonSecondRelId == person.Id)
                )
                .Select(x => new DocumentNotificationLinkVM()
                {
                    Id = x.Id,
                    PersonId = person.Id,
                    PersonRelId = x.DocumentPersonRelId,
                    PersonSecondRelId = x.DocumentPersonSecondRelId,
                    LinkDirectionId = x.LinkDirectionId,
                    LinkDirectionSecondId = x.LinkDirectionSecondId,
                    PersonName = x.DocumentPerson.FullName,
                    PersonRelName = x.DocumentPersonRel.FullName ?? "",
                    PersonSecondRelName = x.DocumentPersonSecondRel.FullName ?? "",
                    PersonRole = x.DocumentPerson.PersonRole.Label,
                    PersonRelRole = x.DocumentPersonRel.PersonRole.Label ?? "",
                    PersonSecondRelRole = x.DocumentPersonSecondRel.PersonRole.Label ?? "",
                    LinkTemplate = x.LinkDirection.LinkTemplate,
                    SecondLinkTemplate = x.LinkDirectionSecond.LinkTemplate,
                    Label = ""
                }).ToList();
            result = result.Where(x => x.PersonId > 0).ToList();
            foreach (var item in result)
            {
                SetLabelFromTemplate(item);
            }
            return result;
        }
        /// <summary>
        /// Извличане на стринг за уведомления за Връзки по страни, може и втори представляващ
        /// </summary>
        /// <param name="model"></param>
        private void SetLabelFromTemplate(DocumentNotificationLinkVM model)
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

            if (model.LinkDirectionSecondId > 0)
                model.Label += model.SecondLinkTemplate
                               .Replace("{Z}", model.PersonSecondRelName)
                               .Replace("{RoleZ}", model.PersonSecondRelRole);
        }
        
        /// <summary>
        /// Извличане на данни за лица по дело/заседание за чекбокс
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public List<SelectListItem> DocumentPerson_SelectForDropDownList(long documentId)
        {
            var result = repo.AllReadonly<DocumentPerson>()
                .Include(x => x.PersonRole)
                .Include(x => x.PersonRole.RoleKind)
                .Where(x => x.DocumentId == documentId)
                .OrderBy(x => x.FullName)
                .Select(x => new SelectListItem()
                             {
                                Value = x.Id.ToString(),
                                Text = x.FullName + "(" + (x.Uic ?? "") + ") - " + x.PersonRole.Label
                             })
                .ToList();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }
        /// <summary>
        /// Извличане на данни за Ред на представляване
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="linkDirectionId"></param>
        /// <param name="defaultElementText"></param>
        /// <returns></returns>
        public List<SelectListItem> RelationalPersonDDL(long documentId, int linkDirectionId, string defaultElementText = null)
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
                casePerson = repo.AllReadonly<DocumentPerson>()
                     .Where(x => x.DocumentId == documentId &&
                                 roles.Any(r => r.Id == x.PersonRoleId))
                            .OrderBy(x => x.FullName)
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
        /// Извличане на данни за Ред на представляване
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="defaultElementText"></param>
        /// <returns></returns>
        public List<SelectListItem> SeccondRelationalPersonDDL(long documentId, string defaultElementText = null)
        {
            var roles = repo.AllReadonly<PersonRoleLinkDirection>()
                            .Where(x => IsForPersonZ(x.LinkDirection.LinkTemplate))
                            .Select(x => x.PersonRole);
            var casePerson = repo.AllReadonly<DocumentPerson>()
                     .Where(x => x.DocumentId == documentId &&
                                 roles.Any(r => r.Id == x.PersonRoleId))
                            .OrderBy(x => x.FullName)
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
        public List<SelectListItem> LinkDirectionForPersonDDL(long documentPersonId)
        {
            var documentPerson = repo.AllReadonly<DocumentPerson>()
                .Include(x => x.PersonRole)
                .Where(x => x.Id == documentPersonId)
                .FirstOrDefault();
            var roleLinks = new List<LinkDirection>();
            if (documentPerson != null)
            {
                roleLinks = repo.AllReadonly<PersonRoleLinkDirection>()
                 .Where(x => x.PersonRoleId == documentPerson.PersonRoleId)
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
        public List<SelectListItem> GetPersonDropDownList(long documentId, int? notificationTypeId, bool addDefaultElement = true, bool addAllElement = false)
        {
            int notificationListTypeId = NomenclatureConstants.NotificationType.ToListType(notificationTypeId);
            var result = repo.AllReadonly<DocumentPerson>()
                             .Where(x => x.DocumentId == documentId)
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.FullName + " (" + x.PersonRole.Label + ")",
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
        public List<SelectListItem> ListForPersonToDropDown(List<DocumentNotificationLinkVM> linkList, long documentPersonId, bool addDefaultElement = true)
        {
            var result = linkList.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.Label
            }).ToList() ?? new List<SelectListItem>();
            var listBy = FilterPresentByList(linkList, documentPersonId);
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
        private List<DocumentNotificationLinkVM> FilterPresentByList(List<DocumentNotificationLinkVM> linkList, long documentPersonId)
        {
            return linkList.Where(x => (!x.isXFirst && x.PersonId == documentPersonId) ||
                                       (x.isXFirst && x.PersonRelId == documentPersonId) ||
                                       (x.PersonSecondRelId == documentPersonId)
                                     ).ToList();
        }
        public List<SelectListItem> GetDDL_DocumentPersonAddress(long documentPersonId, int notificationDeliveryGroupId)
        {
            bool addTel = notificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnPhone;
            bool addMail = notificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.OnEMail;

            var result = repo.AllReadonly<DocumentPersonAddress>()
                .Include(x => x.Address)
                .Where(x => x.DocumentPersonId == documentPersonId)
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Address.FullAddressNotificationMailTel(addTel, addMail)
                })
                .ToList();

            if (result.Count == 0)
                result.Insert(0, new SelectListItem() { Text = "Няма данни", Value = "-1" });

            return result.OrderBy(x => x.Text).ToList();
        }
        public List<DocumentNotificationLinkVM> GetPresentByList(long documentPersonId, int notificationTypeId, List<int> oldLinks)
        {
            var listFrom = GetLinkForPerson(documentPersonId,  notificationTypeId, oldLinks);
            return FilterPresentByList(listFrom, documentPersonId);
        }
    }
}
