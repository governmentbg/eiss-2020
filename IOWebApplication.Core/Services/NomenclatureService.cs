using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.EISPP;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
//using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class NomenclatureService : BaseService, INomenclatureService
    {
        public NomenclatureService(
            ILogger<NomenclatureService> _logger,
            IUserContext _userContext,
            IRepository _repo)
        {
            logger = _logger;
            userContext = _userContext;
            repo = _repo;
        }

        public List<SelectListItem> GetDropDownList<T>(bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true) where T : class, ICommonNomenclature
        {
            var result = repo.AllReadonly<T>()
                        .Where(x => x.IsActive)
                        .ToSelectList(addDefaultElement, addAllElement, orderByNumber);

            return result;
        }

        public async Task<List<SelectListItem>> GetDropDownListAsync<T>(bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true) where T : class, ICommonNomenclature
        {
            var result = await repo.AllReadonly<T>()
                        .Where(x => x.IsActive)
                        .ToSelectListAsync(addDefaultElement, addAllElement, orderByNumber);

            return result;
        }




        public List<SelectListItem> GetDropDownListDescription<T>(bool addDefaultElement = true, bool addAllElement = false) where T : class, ICommonNomenclature
        {
            var result = repo.AllReadonly<T>()
                        .ToSelectListDescription(addDefaultElement, addAllElement);

            return result;
        }

        public List<SelectListItem> GetDropDownListCodeDescription<T>(bool addDefaultElement = true, bool addAllElement = false) where T : class, ICommonNomenclature
        {
            var result = repo.AllReadonly<T>()
                        .ToSelectListCodeDescription(addDefaultElement, addAllElement);

            return result;
        }

        public List<SelectListItem> GetDropDownOrderedList<T>(bool addDefaultElement = true, bool addAllElement = false) where T : class, ICommonNomenclature
        {
            var result = repo.AllReadonly<T>()
                        .OrderBy(x => x.OrderNumber)
                        .ToSelectList(addDefaultElement, addAllElement);

            return result;
        }

        public T GetItem<T>(int id) where T : class, ICommonNomenclature
        {

            var item = repo.GetById<T>(id);

            return item;
        }

        public IQueryable<CommonNomenclatureListItem> GetList<T>() where T : class, ICommonNomenclature
        {
            return repo.AllReadonly<T>()
                .Select(x => new CommonNomenclatureListItem()
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    Label = x.Label,
                    OrderNumber = x.OrderNumber
                }).OrderBy(x => x.OrderNumber);

        }

        public bool SaveItem<T>(T entity) where T : class, ICommonNomenclature
        {
            bool result = false;

            try
            {
                if (entity.Id > 0)
                {
                    repo.Update(entity);
                }
                else
                {
                    int maxOrderNumber = repo.All<T>()
                        .Select(x => x.OrderNumber)
                        .DefaultIfEmpty(0)
                        .Max();

                    entity.OrderNumber = maxOrderNumber + 1;
                    repo.Add(entity);
                }

                repo.SaveChanges();

                result = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на номенклатура ({typeof(T).ToString()})");
            }

            return result;
        }

        public HierarchicalNomenclatureDisplayModel GetEkatte(string query)
        {
            var result = new HierarchicalNomenclatureDisplayModel();
            query = query?.ToLower();

            var ekatte = repo.AllReadonly<EkEkatte>()
                .Include(e => e.Munincipality)
                .Include(e => e.District)
                .Where(e => EF.Functions.ILike(e.Name, query.ToPaternSearch()))
                .Select(e => new HierarchicalNomenclatureDisplayItem()
                {
                    Id = e.Ekatte,
                    Label = String.Format("{0} {1}", e.TVM, e.Name),
                    Category = String.Format("общ. {0}, обл. {1}", e.Munincipality.Name, e.District.Name)
                });

            result.Data.AddRange(ekatte);

            var sobr = repo.AllReadonly<EkSobr>()
                .Where(s => EF.Functions.ILike(s.Name, query.ToPaternSearch()))
                .Select(s => new HierarchicalNomenclatureDisplayItem()
                {
                    Id = s.Ekatte,
                    Label = s.Name,
                    Category = s.Area1 != null ? s.Area1.Substring(s.Area1.IndexOf(')', StringComparison.InvariantCultureIgnoreCase) + 1).Trim() : "Селищни образования"
                });

            result.Data.AddRange(sobr);

            result.Data = result.Data
                .OrderBy(d => d.Category)
                .ToList();

            return result;
        }

        public HierarchicalNomenclatureDisplayModel GetEkatteEispp(string query)
        {
            var result = new HierarchicalNomenclatureDisplayModel();
            query = query?.ToLower();

            var ekatte = repo.AllReadonly<EisppEktteCode>()
                .Where(e => EF.Functions.ILike(e.Name, query.ToPaternSearch()))
                .Select(e => new HierarchicalNomenclatureDisplayItem()
                {
                    Id = e.Code,
                    Label = String.Format("{0} {1}", e.TypeNM, e.Name),
                    Category = String.Format("общ. {0}, обл. {1}", e.Municipality, e.District)
                });

            result.Data.AddRange(ekatte);

            result.Data = result.Data
                .OrderBy(d => d.Category)
                .ToList();

            return result;
        }

        public HierarchicalNomenclatureDisplayItem GetEkatteById(string id)
        {
            var result = repo.AllReadonly<EkEkatte>()
                .Where(e => e.Ekatte == id)
                .Select(e => new HierarchicalNomenclatureDisplayItem()
                {
                    Id = e.Ekatte,
                    Label = String.Format("{0} {1}", e.TVM, e.Name),
                    Category = String.Format("общ. {0}, обл. {1}", e.Munincipality.Name, e.District.Name)
                })
                .FirstOrDefault();

            if (result == null)
            {
                result = repo.AllReadonly<EkSobr>()
                .Where(s => s.Ekatte == id)
                .Select(s => new HierarchicalNomenclatureDisplayItem()
                {
                    Id = s.Ekatte,
                    Label = s.Name,
                    Category = s.Area1 != null ? s.Area1.Substring(s.Area1.IndexOf(')', StringComparison.InvariantCultureIgnoreCase) + 1).Trim() : "Селищни образования"
                })
                .FirstOrDefault();
            }

            return result;
        }

        public HierarchicalNomenclatureDisplayItem GetEkatteByEisppCodeCategory(string eisppCode)
        {
            var result = repo.AllReadonly<EisppEktteCode>()
                .Where(e => e.Code == eisppCode)
                .Select(e => new HierarchicalNomenclatureDisplayItem()
                {
                    Id = e.Code,
                    Label = String.Format("{0} {1}", e.TypeNM, e.Name),
                    Category = String.Format("общ. {0}, обл. {1}", e.Municipality, e.District)
                })
                .FirstOrDefault();
            return result;
        }

        public bool ChangeOrder<T>(ChangeOrderModel model) where T : class, ICommonNomenclature
        {
            bool result = false;

            try
            {
                var nomList = repo.All<T>()
                    .ToList();

                int maxOrderNumber = nomList
                    .Max(x => x.OrderNumber);
                int minOrderNumber = nomList
                    .Min(x => x.OrderNumber);
                var currentElement = nomList
                    .Where(x => x.Id == model.Id)
                    .FirstOrDefault();

                if (currentElement != null)
                {
                    if (model.Direction == "up" && currentElement.OrderNumber > minOrderNumber)
                    {
                        var previousElement = nomList
                            .Where(x => x.OrderNumber == currentElement.OrderNumber - 1)
                            .FirstOrDefault();

                        if (previousElement != null)
                        {
                            previousElement.OrderNumber = currentElement.OrderNumber;
                        }

                        currentElement.OrderNumber -= 1;
                    }

                    if (model.Direction == "down" && currentElement.OrderNumber < maxOrderNumber)
                    {
                        var nextElement = nomList
                            .Where(x => x.OrderNumber == currentElement.OrderNumber + 1)
                            .FirstOrDefault();

                        if (nextElement != null)
                        {
                            nextElement.OrderNumber = currentElement.OrderNumber;
                        }

                        currentElement.OrderNumber += 1;
                    }

                    repo.SaveChanges();
                }

                result = true;
            }
            catch (Exception ex)
            {
                logger.LogError("ChangeOrder", ex);
            }

            return result;
        }

        public List<SelectListItem> GetDDL_DocumentGroup(int documentKindId)
        {
            return repo.AllReadonly<DocumentGroup>(x => x.DocumentKindId == documentKindId)
                .OrderBy(x => x.Label)
                .ToSelectList(true);
        }

        public async Task<List<SelectListItem>> GetDDL_DocumentGroupAsync(int documentKindId)
        {
            return await repo.AllReadonly<DocumentGroup>(x => x.DocumentKindId == documentKindId)
                             .OrderBy(x => x.Label)
                             .ToSelectListAsync(true);
        }

        public List<SelectListItem> GetDDL_DocumentGroupByCourt(int documentKindId, int? courtOrganizationId = null)
        {
            if (documentKindId != DocumentConstants.DocumentKind.InitialDocument)
            {
                return GetDDL_DocumentGroup(documentKindId);
            }

            Expression<Func<DocumentTypeCourtType, bool>> courtOrgSelect = x => true;
            if (courtOrganizationId > 0)
            {
                int[] caseGroupsByOrganization = repo.AllReadonly<CourtOrganizationCaseGroup>()
                                                        .Where(x => x.CourtOrganizationId == courtOrganizationId)
                                                        .Select(x => x.CaseGroupId).ToArray();

                int[] docTypesByGroups = repo.AllReadonly<DocumentTypeCaseType>()
                                            .Include(x => x.CaseType)
                                            .Where(x => caseGroupsByOrganization.Contains(x.CaseType.CaseGroupId))
                                            .Select(x => x.DocumentTypeId).ToArray();

                courtOrgSelect = x => docTypesByGroups.Contains(x.DocumentTypeId);
            }

            int[] caseGroups = Get_CaseGroupsByCourtType(userContext.CourtTypeId);
            int[] docTypesByCourtCaseGroups = repo.AllReadonly<DocumentTypeCaseType>()
                                                .Include(x => x.CaseType)
                                                .Where(x => caseGroups.Contains(x.CaseType.CaseGroupId))
                                                .Select(x => x.DocumentTypeId)
                                                .ToArray();
            Expression<Func<DocumentTypeCourtType, bool>> courtTypeCaseGroupSelect = x => docTypesByCourtCaseGroups.Contains(x.DocumentTypeId);


            var courtType = userContext.CourtTypeId;
            var docGroupsByCourt = repo.AllReadonly<DocumentTypeCourtType>()
                                    .Include(x => x.DocumentType)
                                    .Where(x => x.CourtTypeId == courtType)
                                    .Where(courtOrgSelect)
                                    .Where(courtTypeCaseGroupSelect)
                                    .Select(x => x.DocumentType.DocumentGroupId)
                                    .Distinct()
                                    .ToArray();

            return repo.AllReadonly<DocumentGroup>(x => x.DocumentKindId == documentKindId && docGroupsByCourt.Contains(x.Id))
                .OrderBy(x => x.Label)
                .ToSelectList(true);
        }
        public List<SelectListItem> GetDDL_DocumentType(int documentGroupId, bool addDefaultElement = false)
        {
            return repo.AllReadonly<DocumentType>(x => x.DocumentGroupId == documentGroupId)
                    .OrderBy(x => x.Label)
                    .ToSelectList(addDefaultElement);
        }

        /// <summary>
        /// Метод зареждащ видове документи за падащ списък
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <param name="documentKind">Вид документ</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_DocumentTypeSortByName(bool addDefaultElement = true, int? documentKind = null)
        {
            Expression<Func<DocumentType, bool>> documentKindWhere = x => true;
            if (documentKind != null && documentKind > 0)
                documentKindWhere = x => x.DocumentGroup.DocumentKindId == documentKind;

            var selectListItems = await repo.AllReadonly<DocumentType>()
                                            .Where(x => x.IsActive)
                                            .Where(documentKindWhere)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.Label,
                                                Value = x.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Метод зареждащ видове документи за падащ списък
        /// </summary>
        /// <param name="caseTypeId">Тип дело</param>
        /// <param name="documentKind">Вид документ</param>
        /// <param name="addDefaultElement">Дали да добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_DocumentTypeByCaseType(int caseTypeId, int? documentKind = null, bool addDefaultElement = true)
        {
            Expression<Func<DocumentTypeCaseType, bool>> caseTypeWhere = x => x.CaseTypeId == caseTypeId;

            Expression<Func<DocumentTypeCaseType, bool>> documentKindWhere = x => true;
            if (documentKind != null && documentKind > 0)
                documentKindWhere = x => x.DocumentType.DocumentGroup.DocumentKindId == documentKind;

            var selectListItems = await repo.AllReadonly<DocumentTypeCaseType>()
                                            .Where(x => x.DocumentType.IsActive)
                                            .Where(caseTypeWhere)
                                            .Where(documentKindWhere)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.DocumentType.Label,
                                                Value = x.DocumentType.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync()
                                            .ConfigureAwait(false);

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        private List<SelectListItem> AddDeffAllValue(List<SelectListItem> model, bool addDefaultElement, bool addAllElement)
        {
            if (addDefaultElement)
            {
                model = model
                    .Prepend(new SelectListItem() { Text = "Избери", Value = string.Empty })
                    .ToList();
            }

            if (addAllElement)
            {
                model = model
                    .Prepend(new SelectListItem() { Text = "Всички", Value = string.Empty })
                    .ToList();
            }

            return model;
        }

        public List<SelectListItem> GetDDL_DocumentTypeByCourt(int documentGroupId, bool addDefaultElement = false, bool addAllElement = false, int? courtOrganizationId = null)
        {
            var docGroup = repo.GetById<DocumentGroup>(documentGroupId);
            if (docGroup == null)
            {
                return AddDeffAllValue(new List<SelectListItem>(), addDefaultElement, addAllElement);
            }
            if (docGroup.DocumentKindId != DocumentConstants.DocumentKind.InitialDocument)
            {
                return GetDDL_DocumentType(documentGroupId, addDefaultElement);
            }

            Expression<Func<DocumentTypeCourtType, bool>> courtOrgSelect = x => true;
            if (courtOrganizationId > 0)
            {
                int[] caseGroupsByOrganization = repo.AllReadonly<CourtOrganizationCaseGroup>()
                                                        .Where(x => x.CourtOrganizationId == courtOrganizationId)
                                                        .Select(x => x.CaseGroupId).ToArray();

                int[] docTypesByGroups = repo.AllReadonly<DocumentTypeCaseType>()
                                            .Include(x => x.CaseType)
                                            .Where(x => caseGroupsByOrganization.Contains(x.CaseType.CaseGroupId))
                                            .Select(x => x.DocumentTypeId).ToArray();

                courtOrgSelect = x => docTypesByGroups.Contains(x.DocumentTypeId);
            }

            int[] caseGroups = Get_CaseGroupsByCourtType(userContext.CourtTypeId);
            int[] docTypesByCourtCaseGroups = repo.AllReadonly<DocumentTypeCaseType>()
                                                .Include(x => x.CaseType)
                                                .Where(x => caseGroups.Contains(x.CaseType.CaseGroupId))
                                                .Select(x => x.DocumentTypeId)
                                                .ToArray();
            Expression<Func<DocumentTypeCourtType, bool>> courtTypeCaseGroupSelect = x => docTypesByCourtCaseGroups.Contains(x.DocumentTypeId);


            var courtType = userContext.CourtTypeId;
            var docTypesByCourt = repo.AllReadonly<DocumentTypeCourtType>(x => x.CourtTypeId == courtType)
                                    .Where(courtOrgSelect)
                                    .Where(courtTypeCaseGroupSelect)
                                    .Select(x => x.DocumentTypeId)
                                    .ToArray();

            var selectListItems = repo.AllReadonly<DocumentType>(x => x.DocumentGroupId == documentGroupId && docTypesByCourt.Contains(x.Id))
                                      .OrderBy(x => x.Label)
                                      .ToSelectList();

            return AddDeffAllValue(selectListItems, addDefaultElement, addAllElement);
        }

        public int[] Get_CaseGroupsByCourtType(int courtTypeId)
        {
            var caseGroupsText = this.GetPropById<CourtType, string>(x => x.Id == userContext.CourtTypeId, x => x.CaseGroupList);
            return caseGroupsText.ToIntArray();
        }

        /// <summary>
        /// Зарежда списък с държави
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Изберете"</param>
        /// <returns></returns>
        public List<SelectListItem> GetCountries(bool addDefaultElement = false)
        {
            List<SelectListItem> selectListItems = new();

            var countries = repo.AllReadonly<EkCountry>()
                                .Where(x => x.IsActive)
                                .ToList();

            selectListItems.AddRange(countries.Where(x => x.Code == NomenclatureConstants.CountryBG)
                                              .Select(x => new SelectListItem()
                                              {
                                                  Text = x.Name,
                                                  Value = x.Code
                                              }));

            selectListItems.AddRange(countries.Where(x => x.Code != NomenclatureConstants.CountryBG)
                                              .OrderBy(x => x.Name)
                                              .Select(x => new SelectListItem()
                                              {
                                                  Text = x.Name,
                                                  Value = x.Code
                                              }));

            if (addDefaultElement)
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();

            return selectListItems;
        }

        /// <summary>
        /// Зарежда списък с държави
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Изберете"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetCountriesAsync(bool addDefaultElement = false)
        {
            List<SelectListItem> selectListItems = new();

            var countries = await repo.AllReadonly<EkCountry>()
                                      .Where(x => x.IsActive)
                                      .ToListAsync();

            selectListItems.AddRange(countries.Where(x => x.Code == NomenclatureConstants.CountryBG)
                                              .Select(x => new SelectListItem()
                                              {
                                                  Text = x.Name,
                                                  Value = x.Code
                                              }));

            selectListItems.AddRange(countries.Where(x => x.Code != NomenclatureConstants.CountryBG)
                                              .OrderBy(x => x.Name)
                                              .Select(x => new SelectListItem()
                                              {
                                                  Text = x.Name,
                                                  Value = x.Code
                                              }));

            if (addDefaultElement)
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();

            return selectListItems;
        }

        /// <summary>
        /// Зарежда списък с държави
        /// </summary>
        /// <param name="addDefaultElement">Дали да добави елемент "Изберете"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_Countries(bool addDefaultElement = false)
        {
            List<SelectListItem> selectListItems = new();

            var countries = await repo.AllReadonly<EkCountry>()
                                      .Where(x => x.IsActive)
                                      .ToListAsync();

            selectListItems.AddRange(countries.Where(x => x.Code == NomenclatureConstants.CountryBG)
                                              .Select(x => new SelectListItem()
                                              {
                                                  Text = x.Name,
                                                  Value = x.CountryId.ToString()
                                              }));

            selectListItems.AddRange(countries.Where(x => x.Code != NomenclatureConstants.CountryBG)
                                              .OrderBy(x => x.Name)
                                              .Select(x => new SelectListItem()
                                              {
                                                  Text = x.Name,
                                                  Value = x.CountryId.ToString()
                                              }));

            if (addDefaultElement)
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();

            return selectListItems;
        }

        public List<SelectListItem> GetCourts()
        {
            return repo.AllReadonly<Court>()
                   .OrderBy(x => x.Label)
                   .ToSelectList(x => x.Id, x => x.Label);
        }

        public async Task<List<SelectListItem>> GetCourtsAsync()
        {
            return await repo.AllReadonly<Court>()
                             .OrderBy(x => x.Label)
                             .ToSelectListAsync();
        }

        public void InitEkStreets(IEnumerable<EkStreet> model)
        {
            repo.AddRange<EkStreet>(model);
            repo.SaveChanges();
        }

        public List<SelectListItem> GetDDL_CaseType(int caseGroupId, bool addDefaultElement = true)
        {
            return repo.AllReadonly<CaseType>(x => x.CaseGroupId == caseGroupId)
                       .OrderBy(x => x.Label)
                       .ToSelectList(addDefaultElement);
        }

        public List<SelectListItem> GetDDL_CaseTypes(string caseGroupIds, bool addDefaultElement = true)
        {
            var listIds = new List<int>();
            if (!string.IsNullOrEmpty(caseGroupIds))
                listIds = caseGroupIds.Split(',').Select(Int32.Parse).ToList();
            return repo.AllReadonly<CaseType>(x => (listIds.Count > 0 ? listIds.Contains(x.CaseGroupId) : false))
                       .OrderBy(x => x.Label)
                       .ToSelectList(addDefaultElement);
        }

        public List<SelectListItem> GetDDL_CaseCodeDropDown(int[] caseTypeIds)
        {
            return GetDDL_CaseCode(caseTypeIds).Select(x => new SelectListItem()
            {
                Text = x.Label,
                Value = x.Value
            })
                                               .ToList();
        }

        public List<LabelValueVM> GetDDL_CaseCode(int[] caseTypeIds, string search = null, int? caseCodeId = null, bool byLoadGroup = false)
        {
            var isSingleAndEmpty = false;
            if (caseTypeIds == null || (caseTypeIds?.Length == 0 && caseTypeIds?.FirstOrDefault() == 0))
            {
                isSingleAndEmpty = true;
            }
            if (isSingleAndEmpty && (caseCodeId ?? 0) == 0)
            {
                return new List<LabelValueVM>();
            }
            DateTime today = DateTime.Today;
            Expression<Func<CaseTypeCode, bool>> whereLink = x => caseTypeIds.Contains(x.CaseTypeId);

            if (!string.IsNullOrEmpty(search))
            {
                whereLink = x => caseTypeIds.Contains(x.CaseTypeId) &&
                //(x.CaseCode.Code.Contains(search, StringComparison.InvariantCultureIgnoreCase) || x.CaseCode.Label.Contains(search, StringComparison.InvariantCultureIgnoreCase) || (x.CaseCode.LawBaseDescription ?? "").Contains(search, StringComparison.InvariantCultureIgnoreCase));
                (
                    EF.Functions.ILike(x.CaseCode.Code, search.ToPaternSearch()) ||
                    EF.Functions.ILike(x.CaseCode.Label, search.ToPaternSearch()) ||
                    EF.Functions.ILike(x.CaseCode.LawBaseDescription ?? "", search.ToPaternSearch())
                );
            }

            Expression<Func<CaseCode, bool>> whereCode = x => true;
            if (caseCodeId > 0)
            {
                whereLink = x => x.CaseCodeId == caseCodeId;
                whereCode = x => x.Id == caseCodeId;
            }

            Expression<Func<CaseTypeCode, bool>> whereLoadGroup = x => true;
            if (byLoadGroup && (caseCodeId ?? 0) == 0)
            {
                var typeInstances = repo.AllReadonly<CaseType>(x => caseTypeIds.Contains(x.Id))
                                    .Select(x => (int?)x.CaseInstanceId).ToArray();
                var codes = repo.AllReadonly<LoadGroupLinkCode>()
                                    .Include(x => x.LoadGroupLink)
                                    .Where(x => x.LoadGroupLink.CourtTypeId == userContext.CourtTypeId
                                        && typeInstances.Contains(x.LoadGroupLink.CaseInstanceId)
                                        )
                                    .Select(x => x.CaseCodeId)
                                    .ToArray();

                whereLoadGroup = x => codes.Contains(x.CaseCodeId);
            }


            return repo.AllReadonly<CaseTypeCode>()
                            .Include(x => x.CaseCode)
                            .Where(whereLink)
                            .Where(whereLoadGroup)
                            .Select(x => x.CaseCode)
                            .Where(whereCode)
                            .Where(x => x.IsActive)
                            .Where(x => x.DateStart <= today)
                            .Where(x => (x.DateEnd ?? today) >= today)
                            .OrderBy(x => x.Label)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = x.Code + " " + x.Label,
                                Description = x.LawBaseDescription
                            }).ToList();

            //return Get_CaseCode(caseTypeId, search, caseCodeId, byLoadGroup)
            //            .Select(x => new LabelValueVM
            //            {
            //                Value = x.Id.ToString(),
            //                Label = x.Code + " " + x.Label,
            //                Description = x.LawBaseDescription
            //            }).ToList();
        }

        private IQueryable<CaseCode> Get_CaseCode(int caseTypeId, string search = null, int? caseCodeId = null, bool byLoadGroup = false)
        {
            if (caseTypeId <= 0 && (caseCodeId ?? 0) == 0)
            {
                return new List<CaseCode>().AsQueryable();
            }
            DateTime today = DateTime.Today;
            Expression<Func<CaseTypeCode, bool>> whereLink = x => x.CaseTypeId == caseTypeId;

            if (!string.IsNullOrEmpty(search))
            {
                whereLink = x => (x.CaseTypeId == caseTypeId) &&
                (x.CaseCode.Code.Contains(search, StringComparison.InvariantCultureIgnoreCase) || x.CaseCode.Label.Contains(search, StringComparison.InvariantCultureIgnoreCase) || (x.CaseCode.LawBaseDescription ?? "").Contains(search, StringComparison.InvariantCultureIgnoreCase));
            }

            Expression<Func<CaseCode, bool>> whereCode = x => true;
            if (caseCodeId > 0)
            {
                whereLink = x => x.CaseCodeId == caseCodeId;
                whereCode = x => x.Id == caseCodeId;
            }

            Expression<Func<CaseTypeCode, bool>> whereLoadGroup = x => true;
            if (byLoadGroup && (caseCodeId ?? 0) == 0)
            {
                var caseInfo = repo.AllReadonly<CaseType>(x => x.Id == caseTypeId)
                                    .Select(x => new
                                    {
                                        caseInstance = x.CaseInstanceId,
                                        caseGroup = x.CaseGroupId
                                    }).FirstOrDefault();
                var codes = repo.AllReadonly<LoadGroupLinkCode>()
                                    .Include(x => x.LoadGroupLink)
                                    .Where(x => x.LoadGroupLink.CourtTypeId == userContext.CourtTypeId
                                        && x.LoadGroupLink.CaseInstanceId == caseInfo.caseInstance
                                        )
                                    .Select(x => x.CaseCodeId)
                                    .ToArray();

                whereLoadGroup = x => codes.Contains(x.CaseCodeId);
            }

            return repo.AllReadonly<CaseTypeCode>()
                            .Include(x => x.CaseCode)
                            .Where(whereLink)
                            .Where(whereLoadGroup)
                            .Select(x => x.CaseCode)
                            .Where(whereCode)
                            .Where(x => x.IsActive)
                            .Where(x => x.DateStart <= today)
                            .Where(x => (x.DateEnd ?? today) >= today)
                            .OrderBy(x => x.Label);

        }

        public IEnumerable<LabelValueVM> GetStreet(string ekatte, string query, int? streetType = null)
        {
            return repo.AllReadonly<EkStreet>()
                        .Where(x => x.Ekatte == ekatte)
                        .Where(x => EF.Functions.ILike(x.Name, query.ToPaternSearch()))
                        .Where(x => x.StreetType == (streetType ?? x.StreetType))
                        .OrderBy(x => x.Name)
                        .Select(x => new LabelValueVM
                        {
                            Value = x.Code,
                            Label = x.Name
                        });
        }

        public LabelValueVM GetStreetByCode(string ekatte, string code, int? streetType = null)
        {
            return repo.AllReadonly<EkStreet>().Where(x => x.Ekatte == ekatte && x.Code == code)
                        .Where(x => x.StreetType == (streetType ?? x.StreetType))
                        .OrderBy(x => x.Name)
                        .Select(x => new LabelValueVM
                        {
                            Value = x.Code,
                            Label = x.Name
                        }).FirstOrDefault();
        }
        public LabelValueVM GetEkatteByEisppCode(string eisppCode)
        {
            return repo.AllReadonly<EisppEktteCode>().Where(x => x.Code == eisppCode)
                        .Select(x => new LabelValueVM
                        {
                            Value = x.EktteCode,
                            Label = x.Name
                        }).FirstOrDefault();
        }
        public List<SelectListItem> GetDDL_DeliveryType(int deliveryGroupId)
        {
            return repo.AllReadonly<DeliveryType>(x => x.DeliveryGroupId == deliveryGroupId)
                .OrderBy(x => x.Label)
                .ToSelectList(true);
        }

        public List<SelectListItem> GetDDL_DeliveryNumberType()
        {
            return repo.AllReadonly<DeliveryNumberType>()
                .Where(x => x.IsActive && (x.DateEnd ?? DateTime.Now) >= DateTime.Now.Date)
                .OrderBy(x => x.OrderNumber)
                .ToSelectList(true);
        }
        public List<SelectListItem> GetDDL_CaseTypeByDocType(int documentTypeId, int? caseCharacter = null, int? courtOrganizationId = null)
        {
            var today = DateTime.Now;
            int[] currentInstances = userContext.CourtInstances;
            Expression<Func<DocumentTypeCaseType, bool>> characterSelect = x => true;
            if (caseCharacter > 0)
            {
                int[] caseTypesByCharacter = repo.AllReadonly<CaseTypeCharacter>()
                                                        .Where(x => x.CaseCharacterId == caseCharacter)
                                                        .Select(x => x.CaseTypeId)
                                                        .ToArray();

                characterSelect = x => caseTypesByCharacter.Contains(x.CaseTypeId);
            }
            Expression<Func<DocumentTypeCaseType, bool>> courtOrgSelect = x => true;
            if (courtOrganizationId > 0)
            {
                int[] caseGroupsByOrganization = repo.AllReadonly<CourtOrganizationCaseGroup>()
                                                        .Where(x => x.CourtOrganizationId == courtOrganizationId)
                                                        .Select(x => x.CaseGroupId).ToArray();

                courtOrgSelect = x => caseGroupsByOrganization.Contains(x.CaseType.CaseGroupId);
            }

            int[] caseGroups = Get_CaseGroupsByCourtType(userContext.CourtTypeId);
            Expression<Func<DocumentTypeCaseType, bool>> courtCaseGroupsSelect = x => caseGroups.Contains(x.CaseType.CaseGroupId);

            return (repo.AllReadonly<DocumentTypeCaseType>()
                .Where(x => x.DocumentType.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument)
                .Where(x => x.DocumentTypeId == documentTypeId)
                .Where(x => x.CaseType.IsActive)
                .Where(x => x.CaseType.DateStart <= today)
                .Where(x => (x.CaseType.DateEnd ?? today) >= today)
                .Where(x => currentInstances.Contains(x.CaseType.CaseInstanceId))
                .Where(characterSelect)
                .Where(courtOrgSelect)
                .Where(courtCaseGroupsSelect)
                .OrderBy(x => x.CaseType.Label)
                .Select(x => new SelectListItem()
                {
                    Text = $"{x.CaseType.Label} ({x.CaseType.CaseGroup.Label})",
                    Value = x.CaseType.Id.ToString()
                }).ToList() ?? new List<SelectListItem>());
        }



        public List<SelectListItem> GetCaseYears()
        {
            List<string> years = new List<string>();
            for (int i = DateTime.Now.Year; i >= 2000; i--)
            {
                years.Add(i.ToString());
            }
            return years.Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
        }
        public IQueryable<CaseType> CaseTypeNow(int courtId)
        {
            int courtTypeId = -1;
            Court court = repo.AllReadonly<Court>().Where(x => x.Id == courtId).FirstOrDefault();
            courtTypeId = court?.CourtTypeId ?? -1;
            var typesForCourt = repo.AllReadonly<CourtTypeCaseType>().Where(x => x.CourtTypeId == courtTypeId);

            return repo.AllReadonly<CaseType>()
              .Where(x => (x.DateEnd ?? DateTime.Now) >= DateTime.Now.Date &&
                          typesForCourt.Any(t => t.CaseTypeId == x.Id));
        }

        public void SetFullAddress(Address model)
        {
            model.FullAddress = GetFullAddress(model, true, false, false);
        }
        public string MakeVksCase(string streetKvartal)
        {
            var specialPrefix = new List<string>()
            {
                "Ул.",
                "Пл.",
                "Бул.",
                "Жк.",
                "Жк ",
                "Ж.К.",
                "Кв.",
                "Квартал",
                "Местн",
                "Кк.",
                "К.К.",
            };
            if (string.IsNullOrEmpty(streetKvartal))
                return streetKvartal;
            TextInfo myTI = new CultureInfo("bg-BG", false).TextInfo;
            streetKvartal = myTI.ToTitleCase(streetKvartal.ToLower());
            foreach (var prefix in specialPrefix)
            {
                if (streetKvartal.StartsWith(prefix))
                {
                    streetKvartal = streetKvartal.Replace(prefix, prefix.ToLower());
                    break;
                }
            }

            return streetKvartal;
        }

        /// <summary>
        /// Създава адрес до ниво улица/квартал, апартамент, без населено място и данни за контакт
        /// </summary>
        /// <param name="model"></param>
        /// <param name="vksCase"></param>
        /// <returns></returns>
        public string GetStreetAddress(Address model, bool vksCase)
        {
            string result = "";
            if (!string.IsNullOrEmpty(model.ResidentionAreaCode))
            {
                var resAreaInfo = repo.AllReadonly<EkStreet>().FirstOrDefault(x => x.Ekatte == model.CityCode && x.Code == model.ResidentionAreaCode);
                if (resAreaInfo != null)
                {
                    if (vksCase)
                    {
                        result += $"{MakeVksCase(resAreaInfo.Name)}";
                    }
                    else
                    {
                        result += $"{resAreaInfo.Name}";
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.StreetCode))
            {
                var streetInfo = repo.AllReadonly<EkStreet>().FirstOrDefault(x => x.Ekatte == model.CityCode && x.Code == model.StreetCode);
                if (streetInfo != null)
                {
                    if (vksCase)
                    {
                        result += $"{MakeVksCase(streetInfo.Name)}";
                    }
                    else
                    {
                        result += $"{streetInfo.Name}";
                    }
                }
            }
            if (model.StreetNumber.HasValue)
            {
                result += $" {model.StreetNumber}{model.SubNumber}";
            }
            if (model.Block.HasValue || string.IsNullOrEmpty(model.SubBlock) == false)
            {
                result += $", бл.";
                if (model.Block.HasValue)
                    result += $"{model.Block}";
                if (string.IsNullOrEmpty(model.SubBlock) == false)
                    result += $"{model.SubBlock}";
            }
            if (!string.IsNullOrEmpty(model.Entrance))
            {
                result += $", вх.{model.Entrance}";
            }
            if (!string.IsNullOrEmpty(model.Floor))
            {
                result += $", ет.{model.Floor}";
            }
            if (!string.IsNullOrEmpty(model.Appartment))
            {
                result += $", ап.{model.Appartment}";
            }
            return result;
        }

        public string GetFullAddress(Address model, bool setContactData, bool munAreaNameFirst, bool vksCase)
        {
            string result = "";
            string munAreaName = "";
            if (model.CountryCode == NomenclatureConstants.CountryBG)
            {
                var ekkateInfo = GetEkatteById(model.CityCode);
                if (ekkateInfo != null)
                {
                    munAreaName = ekkateInfo.Category;
                    if (!string.IsNullOrEmpty(munAreaName) && munAreaNameFirst)
                    {
                        result += $"{munAreaName}, {ekkateInfo.Label}";
                    }
                    else
                    {
                        result = ekkateInfo.Label;
                    }
                }
                result += $", {GetStreetAddress(model, vksCase)}";
                //if (!string.IsNullOrEmpty(model.ResidentionAreaCode))
                //{
                //    var resAreaInfo = repo.AllReadonly<EkStreet>().FirstOrDefault(x => x.Ekatte == model.CityCode && x.Code == model.ResidentionAreaCode);
                //    if (resAreaInfo != null)
                //    {
                //        if (vksCase)
                //        {
                //            result += $", {MakeVksCase(resAreaInfo.Name)}";
                //        }
                //        else
                //        {
                //            result += $", {resAreaInfo.Name}";
                //        }
                //    }
                //}
                //if (!string.IsNullOrEmpty(model.StreetCode))
                //{
                //    var streetInfo = repo.AllReadonly<EkStreet>().FirstOrDefault(x => x.Ekatte == model.CityCode && x.Code == model.StreetCode);
                //    if (streetInfo != null)
                //    {
                //        if (vksCase)
                //        {
                //            result += $", {MakeVksCase(streetInfo.Name)}";
                //        }
                //        else
                //        {
                //            result += $", {streetInfo.Name}";
                //        }
                //    }
                //}
                //if (model.StreetNumber.HasValue)
                //{
                //    result += $" {model.StreetNumber}{model.SubNumber}";
                //}
                //if (model.Block.HasValue || string.IsNullOrEmpty(model.SubBlock) == false)
                //{
                //    result += $", бл.";
                //    if (model.Block.HasValue)
                //        result += $"{model.Block}";
                //    if (string.IsNullOrEmpty(model.SubBlock) == false)
                //        result += $"{model.SubBlock}";
                //}
                //if (!string.IsNullOrEmpty(model.Entrance))
                //{
                //    result += $", вх.{model.Entrance}";
                //}
                //if (!string.IsNullOrEmpty(model.Floor))
                //{
                //    result += $", ет.{model.Floor}";
                //}
                //if (!string.IsNullOrEmpty(model.Appartment))
                //{
                //    result += $", ап.{model.Appartment}";
                //}

            }
            else
            {
                var country = repo.AllReadonly<EkCountry>().FirstOrDefault(x => x.Code == model.CountryCode);
                if (country != null)
                {
                    result = $"{country.Name}, {model.ForeignAddress}";
                }
            }

            if (setContactData == true)
            {
                if (!string.IsNullOrEmpty(model.Phone))
                {
                    result += $", тел: {model.Phone}";
                }
                if (!string.IsNullOrEmpty(model.Fax))
                {
                    result += $", факс: {model.Fax}";
                }
                if (!string.IsNullOrEmpty(model.Email))
                {
                    result += $", e-mail: {model.Email}";
                }
            }

            if (!string.IsNullOrEmpty(model.Description))
            {
                result += $", {model.Description}";
            }
            if (!string.IsNullOrEmpty(munAreaName) && !munAreaNameFirst)
            {
                result += $", {munAreaName}";
            }

            return result;
        }

        public List<SelectListItem> GetDDL_LoadGroupLink(int courtTypeId, int caseTypeId, int caseCodeId = NomenclatureConstants.NullVal)
        {
            var caseInstanceId = repo.AllReadonly<CaseType>()
                                    .Where(x => x.Id == caseTypeId)
                                    .Select(x => x.CaseInstanceId)
                                    .FirstOrValue(0);

            var today = DateTime.Now;

            Expression<Func<LoadGroupLink, bool>> codeFilter = x => true;
            if (caseCodeId > NomenclatureConstants.NullVal)
            {
                codeFilter = x => x.GroupCodes.Any(cc => cc.CaseCodeId == caseCodeId);
            }

            var result = repo.AllReadonly<LoadGroupLink>()
                .Where(x => x.CourtTypeId == courtTypeId && x.CaseInstanceId == caseInstanceId)
                //Визуализират се групите по натовареност само с въведен базов индекс #41096
                .Where(x => x.LoadIndex > 0M)
                .Where(codeFilter)
                .Where(x => x.LoadGroup.IsActive)
                .Where(x => x.LoadGroup.DateStart <= today)
                .Where(x => (x.LoadGroup.DateEnd ?? today) >= today)
                .OrderBy(x => x.LoadGroup.Label)
                .Select(x => new SelectListItem()
                {
                    Text = $"{x.LoadGroup.Code}. {x.LoadGroup.Label}",
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            return result;
        }

        public List<HtmlTemplateDdlVM> GetDDL_HtmlTemplate(int notificationTypeId, int caseId, int? htmlTemplateId, bool addDefaultElement = true)
        {

            var notificationType = repo.GetPropById<NotificationType, int>(x => x.Id == notificationTypeId, x => x.HtmlTemplateTypeId);
            int htmlTemplateTypeId = (notificationType > 0) ? notificationType : notificationTypeId;

            //var caseCase = repo.GetById<Case>(caseId);
            //var court = repo.GetById<Court>(caseCase.CourtId);
            //var courtType = court.CourtTypeId;
            //var caseGroupe = caseCase.CaseGroupId;
            var caseInfo = repo.AllReadonly<Case>()
                                .Where(x => x.Id == caseId)
                                .Select(x => new
                                {
                                    x.Court.CourtTypeId,
                                    x.CaseGroupId
                                }).FirstOrDefault();

            var list = repo.AllReadonly<HtmlTemplate>()
                .Include(x => x.HtmlTemplateLinks)
                .Where(x => (x.HtmlTemplateTypeId == htmlTemplateTypeId) &&
                            (x.HtmlTemplateLinks.Any(p => (p.CourtTypeId ?? caseInfo.CourtTypeId) == caseInfo.CourtTypeId && (p.CaseGroupId ?? caseInfo.CaseGroupId) == caseInfo.CaseGroupId)))
                .Where(x => x.DateTo == null || x.DateTo > DateTime.Now || x.Id == htmlTemplateId)
                .ToList();
            if (!list.Any())
                list = repo.AllReadonly<HtmlTemplate>()
                .Include(x => x.HtmlTemplateLinks)
                .Where(x => (x.HtmlTemplateTypeId == htmlTemplateTypeId) &&
                            (!x.HtmlTemplateLinks.Any()))
                .Where(x => x.DateTo == null || x.DateTo > DateTime.Now || x.Id == htmlTemplateId)
                .ToList();
            var result = list.Select(x => new HtmlTemplateDdlVM()
            {
                Value = x.Id.ToString(),
                Alias = x.Alias,
                Text = x.Label,
                HaveExpertReport = x.HaveExpertReport ?? false,
                HaveSessionAct = x.HaveSessionAct ?? false,
                HaveSessionActComplain = x.HaveSessionActComplain ?? false,
                RequiredSessionActComplain = x.RequiredSessionActComplain ?? false,
                HaveMultiActComplain = x.HaveMultiActComplain ?? false,
                HaveDocumentSenderPerson = x.HaveDocumentSenderPerson ?? false,
                HaveMoneyObligation = x.HaveMoneyObligation ?? false,
                HaveInstitutionDocument = x.HaveInstitutionDocument ?? false,
                HaveNotificationIspnReason = x.HaveNotificationIspnReason ?? false,
                HaveDocuments = x.HaveDocuments ?? false,
                HaveMongoFiles = x.HaveMongoFiles ?? false,
                HaveSessionMultiAct = x.HaveSessionMultiAct ?? false,
            }).ToList();
            if (notificationTypeId != NomenclatureConstants.HtmlTemplateTypes.All3Notification)
                result.AddRange(GetDDL_HtmlTemplate(NomenclatureConstants.HtmlTemplateTypes.All3Notification, caseId, htmlTemplateId, false));
            result = result.OrderBy(x => x.Text).ToList();
            if (addDefaultElement)
            {
                result = result
                    .Prepend(new HtmlTemplateDdlVM() { Text = "Избери", Value = "-1" })
                    .ToList();
            }


            return result;
        }
        public List<HtmlTemplateDdlVM> GetDDL_HtmlTemplateDocument(int notificationTypeId, bool addDefaultElement = true)
        {

            // var notificationType = repo.GetById<NotificationType>(notificationTypeId);
            // int htmlTemplateTypeId = notificationType?.HtmlTemplateTypeId ?? notificationTypeId;
            int htmlTemplateTypeId = notificationTypeId == NomenclatureConstants.NotificationType.Message ?
                                     NomenclatureConstants.HtmlTemplateTypes.MessageResolution :
                                     NomenclatureConstants.HtmlTemplateTypes.NotificationResolution;
            var dateTimeNow = DateTime.Now;
            var dateTimeAddOneYear = DateTime.Now.AddYears(1);
            var result = repo.AllReadonly<HtmlTemplate>()
                             .Where(x => (x.DateFrom <= dateTimeNow && dateTimeNow <= (x.DateTo ?? dateTimeAddOneYear)) &&
                                        x.HtmlTemplateTypeId == htmlTemplateTypeId)
                             .Select(x => new HtmlTemplateDdlVM()
                             {
                                 Value = x.Id.ToString(),
                                 Alias = x.Alias,
                                 Text = x.Label,
                                 HaveExpertReport = x.HaveExpertReport ?? false,
                                 HaveSessionAct = x.HaveSessionAct ?? false,
                                 HaveSessionActComplain = x.HaveSessionActComplain ?? false,
                                 RequiredSessionActComplain = x.RequiredSessionActComplain ?? false,
                                 HaveMultiActComplain = x.HaveMultiActComplain ?? false,
                                 HaveDocumentSenderPerson = x.HaveDocumentSenderPerson ?? false,
                                 HaveMoneyObligation = x.HaveMoneyObligation ?? false,
                                 HaveInstitutionDocument = x.HaveInstitutionDocument ?? false,
                                 HaveNotificationIspnReason = x.HaveNotificationIspnReason ?? false
                             }).ToList();
            result = result.OrderBy(x => x.Text).ToList();
            if (addDefaultElement)
            {
                result = result
                    .Prepend(new HtmlTemplateDdlVM() { Text = "Избери", Value = "-1" })
                    .ToList();
            }


            return result;
        }

        public List<HtmlTemplateDdlVM> GetDDL_HtmlTemplateMediation(int notificationTypeId, bool addDefaultElement = true)
        {

            var dateTimeNow = DateTime.Now;
            var dateTimeAddOneYear = DateTime.Now.AddYears(1);
            var result = repo.AllReadonly<HtmlTemplate>()
                             .Where(x => (x.DateFrom <= dateTimeNow && dateTimeNow <= (x.DateTo ?? dateTimeAddOneYear)) &&
                                        x.HtmlTemplateTypeId == NomenclatureConstants.HtmlTemplateTypes.SubpoenaMediation)
                             .Select(x => new HtmlTemplateDdlVM()
                             {
                                 Value = x.Id.ToString(),
                                 Alias = x.Alias,
                                 Text = x.Label,
                                 HaveExpertReport = x.HaveExpertReport ?? false,
                                 HaveSessionAct = x.HaveSessionAct ?? false,
                                 HaveSessionActComplain = x.HaveSessionActComplain ?? false,
                                 RequiredSessionActComplain = x.RequiredSessionActComplain ?? false,
                                 HaveMultiActComplain = x.HaveMultiActComplain ?? false,
                                 HaveDocumentSenderPerson = x.HaveDocumentSenderPerson ?? false,
                                 HaveMoneyObligation = x.HaveMoneyObligation ?? false,
                                 HaveInstitutionDocument = x.HaveInstitutionDocument ?? false,
                                 HaveNotificationIspnReason = x.HaveNotificationIspnReason ?? false
                             }).ToList();
            result = result.OrderBy(x => x.Text).ToList();
            if (addDefaultElement)
            {
                result = result
                    .Prepend(new HtmlTemplateDdlVM() { Text = "Избери", Value = "-1" })
                    .ToList();
            }


            return result;
        }

        public List<SelectListItem> GetDDL_DocumentKind(int documentDirectionId, bool addDefaultElement = false, bool addAllElement = false)
        {
            return repo.AllReadonly<DocumentKind>(x => x.DocumentDirectionId == documentDirectionId)
                           .OrderBy(x => x.OrderNumber)
                           .ToSelectList(addDefaultElement, addAllElement);
        }

        public async Task<List<SelectListItem>> GetDDL_DocumentKindAsync(int documentDirectionId, bool addDefaultElement = false, bool addAllElement = false)
        {
            return await repo.AllReadonly<DocumentKind>(x => x.DocumentDirectionId == documentDirectionId)
                             .OrderBy(x => x.OrderNumber)
                             .ToSelectListAsync(addDefaultElement, addAllElement);
        }

        public IQueryable<MultiSelectTransferVM> CaseCodeForSelect_Select(int caseGroupId)
        {
            Expression<Func<CourtGroupCode, bool>> groupWhere = x => true;
            if (caseGroupId > 0)
                groupWhere = x => x.CourtGroup.CaseGroupId == caseGroupId;

            DateTime dateEnd = DateTime.Now.AddDays(1);
            var result = repo.AllReadonly<CourtGroupCode>()
                .Where(x => x.CourtGroup.CourtId == userContext.CourtId)
                .Where(x => (x.DateTo ?? dateEnd) >= DateTime.Now)
                .Where(x => (x.CourtGroup.DateTo ?? dateEnd).Date >= DateTime.Now.Date)
                .Where(groupWhere)
            .Select(x => new MultiSelectTransferVM()
            {
                Id = x.CaseCodeId,
                //OrderInt = x.CaseCode.OrderNumber,
                Text = (x.CaseCode.Code ?? "") + " " + (x.CaseCode.Label ?? "")
            })
            .GroupBy(x => x.Id)
            .Select(g => g.FirstOrDefault())
            .AsQueryable();

            //var sql = result.ToSql();
            return result;
        }

        public IQueryable<MultiSelectTransferVM> CaseCodeForSelect_SelectAll(int caseGroupId, int courtTypeId, int caseInstanceId)
        {
            Expression<Func<CaseTypeCode, bool>> groupWhere = x => true;
            if (caseGroupId > 0)
                groupWhere = x => x.CaseType.CaseGroupId == caseGroupId;

            int[] typesForCourt = repo.AllReadonly<CourtTypeCaseType>().Where(x => x.CourtTypeId == courtTypeId).Select(x => x.CaseTypeId).ToArray();
            DateTime dateEnd = DateTime.Now.AddDays(1);
            var result = repo.AllReadonly<CaseTypeCode>()
                .Where(x => x.CaseType.CaseInstanceId == caseInstanceId)
                .Where(x => typesForCourt.Contains(x.CaseTypeId))
                .Where(groupWhere)
                .Select(x => new
                {
                    x.CaseCodeId,
                    OrderInt = x.CaseCode.OrderNumber,
                    Text = (x.CaseCode.Code ?? "") + " " + (x.CaseCode.Label ?? "")
                })
                .ToList()
            .Select(x => new MultiSelectTransferVM()
            {
                Id = x.CaseCodeId,
                //За да сортира по текста
                OrderInt = 0,
                Text = x.Text
            })
            .GroupBy(x => x.Id)
            .Select(g => g.FirstOrDefault())
            .OrderBy(x => x.Text)
            .AsQueryable();

            //var sql = result.ToSql();
            return result;
        }


        public List<SelectListItem> GetDDL_NotificationStateFromDeliveryGroup(int deliveryGroupId, int notificationStateId, bool addDefaultElement = true, bool addAllElement = false)
        {
            int[] lastStates = NomenclatureConstants.NotificationState.NotificationEndStateAndUdeliveredMail();
            var notificationStates = repo.AllReadonly<NotificationDeliveryGroupState>()
                                        .Where(x => x.NotificationDeliveryGroupId == deliveryGroupId &&
                                               x.NotificationState.IsActive)
                                        .OrderBy(x => x.NotificationState.OrderNumber)
                                        .Select(x => x.NotificationState)
                                        .ToList();
            int countToAdd = notificationStates.Count;
            if (!notificationStates.Any(x => x.Id == notificationStateId))
            {
                if (notificationStateId != -1)
                    countToAdd = 1;
            }
            else if (deliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithSummons)
            {
                var notificationState = notificationStates.FirstOrDefault(x => x.Id == notificationStateId);
                if (notificationState != null)
                {
                    var pos = notificationStates.IndexOf(notificationState);
                    if (!notificationStates.Take(pos + 1).Any(x => lastStates.Any(l => l == x.Id)))
                        countToAdd = pos + 1;
                    if (!notificationStates.Take(pos + 2).Any(x => lastStates.Any(l => l == x.Id)))
                        countToAdd = pos + 2;
                }
            }
            notificationStates = notificationStates.OrderBy(x => x.OrderNumber).Take(countToAdd).ToList();
            var result = notificationStates.Select(x => new SelectListItem()
            {
                Text = x.Label,
                Value = x.Id.ToString()
            }).ToList() ?? new List<SelectListItem>();

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

        public List<SelectListItem> GetDDL_NotificationDeliveryType(int deliveryGroupId, bool addDefaultElement = true)
        {
            var notificationDeliveryTypes = repo.AllReadonly<NotificationDeliveryType>()
                                        .Where(x => x.NotificationDeliveryGroupId == deliveryGroupId)
                                        .OrderBy(x => x.OrderNumber)
                                        .ToList();
            var result = notificationDeliveryTypes.Select(x => new SelectListItem()
            {
                Text = x.Label,
                Value = x.Id.ToString()
            }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return result;
        }

        public List<SelectListItem> GetDDL_SessionResult(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<SessionResult>()
                                      .Where(x => x.IsActive)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label,
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDDL_SessionResultAsync(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = await repo.AllReadonly<SessionResult>()
                                            .Where(x => x.IsActive)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.Label,
                                                Value = x.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Метод връщащ списък с основания към резултати
        /// </summary>
        /// <param name="sessionResultId">Идентификатор на резултат</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <param name="addAllElement">Флаг за добавяне на елемент "Всички"</param>
        /// <param name="withDateCheck">Флаг указващ метода дали да гледа датите</param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_SessionResultBase(int sessionResultId, bool addDefaultElement = true, bool addAllElement = false, bool withDateCheck = false, int? caseId = null, int? caseSessionId = null)
        {
            DateTime dateNow = DateTime.Now;

            Expression<Func<SessionResultBase, bool>> withDateCheckWhere = x => true;
            if (withDateCheck)
                withDateCheckWhere = x => x.DateStart.Date <= dateNow.Date && (x.DateEnd ?? dateNow.AddYears(10)) >= dateNow.Date;

            Expression<Func<SessionResultBase, bool>> sessionResultGroupIdWhere = x => x.SessionResultGroupId == 0;
            if (sessionResultId > 0)
            {
                var sessionResultGroup = repo.AllReadonly<SessionResult>().Where(r => r.Id == sessionResultId).Select(r => r.SessionResultGroupId ?? 0).FirstOrDefault();
                sessionResultGroupIdWhere = x => x.SessionResultGroupId == sessionResultGroup;
            }
            Expression<Func<SessionResultBase, bool>> caseSessionWhere = x => true;
            if (caseId > 0)
            {
                int caseGroupId = repo.AllReadonly<Case>().Where(x => x.Id == caseId).Select(x => x.CaseGroupId).FirstOrDefault();
                DateTime dtNow = DateTime.Now;
                if (caseSessionId > 0)
                {
                    dtNow = repo.AllReadonly<CaseSession>().Where(x => x.Id == caseSessionId).Select(x => x.DateFrom).FirstOrDefault();
                }
                caseSessionWhere = x => x.ResultBaseRules.Any(b => b.CaseGroupId == caseGroupId && x.DateStart <= dtNow && (x.DateEnd ?? DateTime.MaxValue) > dtNow);
            }

            List<SelectListItem> selectListItems = repo.AllReadonly<SessionResultBase>()
                                                       .Where(sessionResultGroupIdWhere)
                                                       .Where(withDateCheckWhere)
                                                       .Where(caseSessionWhere)
                                                       .Where(x => x.IsActive)
                                                       .Select(x => new SelectListItem()
                                                       {
                                                           Text = x.Label,
                                                           Value = x.Id.ToString()
                                                       })
                                                       .OrderBy(x => x.Text)
                                                       .ToList();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return selectListItems;
        }

        public EkDistrict GetEkDistrictByEkatte(string Ekatte)
        {
            return repo.AllReadonly<EkDistrict>()
            .Where(x => x.Ekatte == Ekatte)
            .FirstOrDefault();
        }


        public List<SelectListItem> GetDDL_EkDistrict(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<EkDistrict>()
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.Name,
                                            Value = x.Ekatte.ToString()
                                        }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = string.Empty })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = string.Empty })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_EkMunincipality(string EkatteDistrict, bool addDefaultElement = true, bool addAllElement = false)
        {
            var ekDistrict = GetEkDistrictByEkatte(EkatteDistrict);

            var selectListItems = repo.AllReadonly<EkMunincipality>()
                                        .Where(x => x.DistrictId == ekDistrict.DistrictId)
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.Name,
                                            Value = x.Municipality.ToString()
                                        }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = string.Empty })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = string.Empty })
                    .ToList();
            }

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDDL_SessionDuration(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = (await repo.AllReadonly<SessionDuration>()
                                            .OrderBy(x => x.Minutes)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.Label,
                                                Value = x.Minutes.ToString()
                                            })
                                            .ToListAsync()) ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_CaseCharacter(int caseTypeId, int? caseCharacterId = null)
        {
            return repo.AllReadonly<CaseTypeCharacter>()
                            .Include(x => x.CaseCharacter)
                            .Where(x => x.CaseTypeId == caseTypeId && x.CaseCharacterId == (caseCharacterId ?? x.CaseCharacterId))
                            .Select(x => x.CaseCharacter)
                            .ToSelectList(true, false, false);

        }

        public async Task<List<SelectListItem>> GetDDL_SessionTypesByCase(int caseId, int? oldSessionTypeId, bool addDefaultElement = true, bool addAllElement = false, DateTime? dtNow = null)
        {
            Expression<Func<CourtTypeSessionType, bool>> whereDate = x => true;
            if (dtNow.HasValue)
            {
                whereDate = x => (x.DateStart ?? DateTime.MinValue) <= dtNow.Value.MakeEndDate()
                && (x.DateEnd ?? DateTime.MaxValue) >= dtNow;
            }

            var caseInfo = await repo.AllReadonly<Case>()
                                     .Where(x => x.Id == caseId)
                                     .Select(x => new { courtType = x.Court.CourtTypeId, caseType = x.CaseTypeId })
                                     .FirstOrDefaultAsync();

            return await repo.AllReadonly<CourtTypeSessionType>()
                             .Where(x => x.CourtTypeId == caseInfo.courtType && x.CaseTypeId == caseInfo.caseType && x.SessionType.IsActive)
                             .Where(x => NomenclatureConstants.SessionType.OpenSessionsForPastSessions.Contains(oldSessionTypeId ?? 0) ? NomenclatureConstants.SessionType.OpenSessionsForPastSessions.Contains(x.SessionTypeId) : true)
                             .Where(whereDate)
                             .Select(x => x.SessionType)
                             .ToSelectListAsync(addDefaultElement, addAllElement);
        }

        public async Task<List<SelectListItem>> GetDDL_SessionTypesByCaseByGroupe(int caseId, int SessionTypeGroupId, bool addDefaultElement = true, bool addAllElement = false, DateTime? dtNow = null)
        {
            Expression<Func<CourtTypeSessionType, bool>> whereDate = x => true;
            if (dtNow.HasValue)
            {
                whereDate = x => (x.DateStart ?? DateTime.MinValue) <= dtNow.Value.MakeEndDate()
                && (x.DateEnd ?? DateTime.MaxValue) >= dtNow;
            }

            var caseInfo = await repo.AllReadonly<Case>()
                                     .Where(x => x.Id == caseId)
                                     .Select(x => new { courtType = x.Court.CourtTypeId, caseType = x.CaseTypeId })
                                     .FirstOrDefaultAsync();

            return await repo.AllReadonly<CourtTypeSessionType>()
                             .Where(x => x.CourtTypeId == caseInfo.courtType &&
                                         x.CaseTypeId == caseInfo.caseType &&
                                         x.SessionType.IsActive &&
                                         x.SessionType.SessionTypeGroup == SessionTypeGroupId)
                             .Select(x => x.SessionType)
                             .ToSelectListAsync(addDefaultElement, addAllElement);
        }

        public List<SelectListItem> GetDDL_CaseTypeForCourt(int courtId)
        {
            int courtTypeId = -1;
            Court court = repo.AllReadonly<Court>().Where(x => x.Id == courtId).FirstOrDefault();
            courtTypeId = court?.CourtTypeId ?? -1;
            var typesForCourt = repo.AllReadonly<CourtTypeCaseType>().Where(x => x.CourtTypeId == courtTypeId);

            var selectListItems = repo.AllReadonly<CaseType>()
                                       .Where(x => typesForCourt.Any(t => t.CaseTypeId == x.Id))
                                       .OrderBy(x => x.OrderNumber)
                                       .Select(x => new SelectListItem()
                                       {
                                           Text = x.Label,
                                           Value = x.Id.ToString()
                                       }).ToList() ?? new List<SelectListItem>();

            selectListItems = selectListItems
                   .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                   .ToList();


            return selectListItems;
        }

        public List<SelectListItem> GetDDL_CaseStateHand(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = "Активни", Value = NomenclatureConstants.CaseState.WithoutArchive.ToString() });
            selectListItems.Add(new SelectListItem() { Text = "Архивирани", Value = NomenclatureConstants.CaseState.WithArchive.ToString() });

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetSelectionLawUnitState(int selectionMode, bool disabledOnly = false)


        {
            List<SelectListItem> selectionLawUnitStates = new List<SelectListItem>();

            if (disabledOnly)
            {
                return new List<SelectListItem>() { new SelectListItem("Не участва", NomenclatureConstants.SelectionProtokolLawUnitState.Exclude.ToString()) };
            }


            if (selectionMode == 3)
            //Ръчно и по дежурство имет еднакъва номенклатура на статуси
            { selectionMode = 1; }

            string sSelectionMode = selectionMode.ToString();



            var result = repo.AllReadonly<SelectionLawUnitState>()
                     .Where(x => x.Code == sSelectionMode)
                     .Select(x => new SelectListItem()
                     {
                         Text = x.Label,
                         Value = x.Id.ToString()
                     }).ToList() ?? new List<SelectListItem>();

            return result;
        }

        public List<SelectListItem> GetDDL_MoneyClaimType(int moneyClaimGroupId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<CaseMoneyClaimType>()
                                        .Where(x => x.CaseMoneyClaimGroupId == moneyClaimGroupId)
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.Label,
                                            Value = x.Id.ToString()
                                        }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_MoneyCollectionType(int moneyCollectionGroupId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<CaseMoneyCollectionType>()
                                        .Where(x => x.CaseMoneyCollectionGroupId == moneyCollectionGroupId)
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.Label,
                                            Value = x.Id.ToString()
                                        }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_MoneyCollectionKind(int moneyCollectionGroupId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<CaseMoneyCollectionKind>()
                                        .Where(x => x.CaseMoneyCollectionGroupId == moneyCollectionGroupId)
                                        .OrderBy(x => x.OrderNumber)
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.Label,
                                            Value = x.Id.ToString()
                                        })
                                        .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_CaseReason(int caseTypeId)
        {
            var caseType = GetById<CaseType>(caseTypeId);
            return repo.AllReadonly<CaseReason>()
                             .Where(x => x.CaseGroupId == caseType.CaseGroupId)
                             .ToSelectList(true, false, false);
        }


        public List<SelectListItem> GetDismisalTypes_SelectForDropDownList(int CaseLawUnitId)
        {
            List<SelectListItem> result = null;
            var caseLawUnit = repo.AllReadonly<CaseLawUnit>()
                          .Where(x => x.Id == CaseLawUnitId)
                .Select(x => x).FirstOrDefault();




            if (caseLawUnit != null)
            {
                int dismisalKindId;

                if (NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(caseLawUnit.JudgeRoleId))

                { dismisalKindId = NomenclatureConstants.LawUnitTypes.Judge; }
                else
                { dismisalKindId = NomenclatureConstants.LawUnitTypes.Jury; }

                result = repo.AllReadonly<DismisalType>()

                       .Where(x => x.DismisalKindId == dismisalKindId)
                       .Select(x => new SelectListItem()
                       {
                           Value = x.Id.ToString(),
                           Text = x.Label
                       })
                       .OrderBy(x => x.Text)
                       .ToList();

                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            }


            return result;
        }

        public async Task<List<SelectListItem>> GetDismisalTypes_SelectForDropDownListAsync(int CaseLawUnitId)
        {
            List<SelectListItem> result = null;
            var caseLawUnit = await repo.AllReadonly<CaseLawUnit>()
                                        .Where(x => x.Id == CaseLawUnitId)
                                        .FirstOrDefaultAsync();

            if (caseLawUnit != null)
            {
                int dismisalKindId = NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(caseLawUnit.JudgeRoleId) ? NomenclatureConstants.LawUnitTypes.Judge :
                                                                                                                        NomenclatureConstants.LawUnitTypes.Jury;

                result = await repo.AllReadonly<DismisalType>()
                                   .Where(x => x.DismisalKindId == dismisalKindId)
                                   .Select(x => new SelectListItem()
                                   {
                                       Value = x.Id.ToString(),
                                       Text = x.Label
                                   })
                                   .OrderBy(x => x.Text)
                                   .ToListAsync();

                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            }

            return result;
        }

        public List<SelectListItem> GetDDL_SpecialityForFilter(int? lawUnitTypeId = null)
        {
            var selectListItems = repo.AllReadonly<Speciality>()
                                        .Where(x => x.IsActive && (x.DateEnd ?? DateTime.Now) >= DateTime.Now.Date)
                                        .Where(x => (x.LawUnitTypeID ?? 0) == (lawUnitTypeId ?? (x.LawUnitTypeID ?? 0)))
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.Label,
                                            Value = x.Id.ToString()
                                        }).ToList() ?? new List<SelectListItem>();


            selectListItems.Insert(0, new SelectListItem() { Text = "Всички", Value = "-1" });
            selectListItems.Insert(1, new SelectListItem() { Text = "Без специалност", Value = "0" });


            return selectListItems;
        }

        public bool CaseCodeGroup_Check(string alias, int caseCodeId)
        {
            return repo.AllReadonly<CaseCodeGroup>().Any(x => x.GroupAlias == alias && x.CaseCodeId == caseCodeId);
        }

        public List<SelectListItem> GetDDL_MoneyFeeType(int documentGroupId)
        {
            var selectListItems = repo.AllReadonly<MoneyFeeType>()
                                        .Where(x => x.IsActive && (x.DateEnd ?? DateTime.Now) >= DateTime.Now.Date)
                                        .Where(x => repo.AllReadonly<MoneyFeeDocumentGroup>().Where(d => d.DocumentGroupId == documentGroupId && d.MoneyFeeTypeId == x.Id).Any())
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.Label,
                                            Value = x.Id.ToString()
                                        }).ToList() ?? new List<SelectListItem>();


            selectListItems.Insert(0, new SelectListItem() { Text = "Други", Value = "-1" });

            return selectListItems;
        }

        public Bank GetBankByCodeSearch(string codeSearch)
        {
            if (string.IsNullOrEmpty(codeSearch) == true) return null;

            return repo.AllReadonly<Bank>().Where(x => x.CodeForSearch.ToUpper() == codeSearch.ToUpper()).FirstOrDefault();
        }

        public List<SelectListItem> GetDDL_CaseTypeGroupInstance(int caseGroupId, int caseInstanceId, string caseInstanceIds)
        {
            Expression<Func<CaseType, bool>> caseInstanceWhere = x => true;
            if (caseInstanceId > 0)
                caseInstanceWhere = x => x.CaseInstanceId == caseInstanceId;

            Expression<Func<CaseType, bool>> caseInstanceContainsWhere = x => true;
            if (string.IsNullOrEmpty(caseInstanceIds) == false)
            {
                string[] instances = caseInstanceIds.Split(",").ToArray();
                caseInstanceContainsWhere = x => instances.Contains(x.CaseInstanceId.ToString());
            }

            return repo.AllReadonly<CaseType>()
                    .Where(x => x.CaseGroupId == caseGroupId)
                    .Where(caseInstanceWhere)
                    .Where(caseInstanceContainsWhere)
                   .OrderBy(x => x.Label)
                   .ToSelectList(true);
        }

        public List<SelectListItem> GetDDL_HtmlTemplateByDocType(int documentTypeId, int caseId, int sourceType, int courtTypeId, bool setDefault)
        {
            var info = repo.AllReadonly<Case>()
                                .Include(x => x.Court)
                                .Where(x => x.Id == caseId)
                                .Select(x => new
                                {
                                    RegDate = x.RegDate,
                                    CaseGroupId = x.CaseGroupId,
                                    CourtTypeId = x.Court.CourtTypeId
                                }).FirstOrDefault();

            var regDate = info == null ? DateTime.Now : info.RegDate;
            var caseGroupId = info == null ? 0 : info.CaseGroupId;
            var documentType = repo.AllReadonly<DocumentType>()
                                        .Where(x => x.Id == documentTypeId)
                                        .FirstOrDefault();

            int htmlTemplateTypeId = documentType == null ? 0 : documentType.HtmlTemplateTypeId ?? 0;
            int defaultHtmlTemplateId = documentType == null ? 0 : documentType.DefaultHtmlTemplateId ?? 0;

            Expression<Func<HtmlTemplate, bool>> linkWhere = x => x.HtmlTemplateLinks
                         .Where(a => (a.IsActive ?? true) == true &&
                                     (a.CaseGroupId ?? caseGroupId) == caseGroupId &&
                                     (a.SourceType ?? sourceType) == sourceType &&
                                     (a.CourtTypeId ?? courtTypeId) == courtTypeId).Any();

            var dateNow = DateTime.Now;
            var list = repo.AllReadonly<HtmlTemplate>()
                               .Where(x => x.HtmlTemplateTypeId == htmlTemplateTypeId && (x.DateTo ?? dateNow.AddYears(100)) >= dateNow)
                               .Where(linkWhere)
                               .Select(x => new { x.Id, x.Label })
                               .ToList();


            //var list = repo.All<HtmlTemplate>()
            //                    .Include(x => x.HtmlTemplateLinks)
            //                    .Where(x => x.HtmlTemplateTypeId == templateType &&
            //                    (x.HtmlTemplateLinks.Any(p => p.CourtTypeId == info.CourtTypeId && p.CaseGroupId == info.CaseGroupId)))
            //                    .Select(x => new { x.Id, x.Label })
            //                    .ToList();
            //if (!list.Any())
            //    list = repo.All<HtmlTemplate>()
            //                .Include(x => x.HtmlTemplateLinks)
            //                .Where(x => (x.HtmlTemplateTypeId == templateType) &&
            //                            (!x.HtmlTemplateLinks.Any()))
            //                .Select(x => new { x.Id, x.Label })
            //                .ToList();

            var result = list.OrderBy(x => x.Label).Select(x => new SelectListItem()
            {
                Text = x.Label,
                Value = x.Id.ToString(),
                Selected = setDefault == false ? false : (x.Id == defaultHtmlTemplateId ? true : false)
            }).ToList();
            result = result.Prepend(new SelectListItem() { Text = "Общ формуляр", Value = "-1" }).ToList();

            //Общ формуляр остава само за всичко различно от Удостоверение от HtmlTemplate - втъщаме го            
            //if (templateType != NomenclatureConstants.HtmlTemplateTypes.Certificate)
            //{
            //    result = result.Prepend(new SelectListItem() { Text = "Общ формуляр", Value = "-1" }).ToList();
            //}


            return result;
        }

        public List<SelectListItem> GetDDL_DecisionType(int documentTypeId)
        {
            DateTime today = DateTime.Today;
            var result = repo.AllReadonly<DocumentTypeDecisionType>()
                   .Where(x => x.DocumentTypeId == documentTypeId)
                   .Where(x => x.DecisionType.IsActive)
                   .Where(x => x.DecisionType.DateStart <= today)
                   .Where(x => (x.DecisionType.DateEnd ?? today) >= today)
                   .OrderBy(x => x.DecisionType.OrderNumber)
                   .Select(x => new SelectListItem()
                   {
                       Text = x.DecisionType.Label,
                       Value = x.DecisionType.Id.ToString()
                   }).ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public int[] GetCaseCodeGroupingByGroup(int groupId)
        {
            return repo.AllReadonly<CaseCodeGrouping>()
                   .Where(x => x.CaseCodeGroup == groupId)
                   .Select(x => x.CaseCodeId).ToArray();
        }

        public List<SelectListItem> GetCountriesWitoutBG_DDL()
        {
            var countries = repo.AllReadonly<EkCountry>();
            var result = countries.Where(x => x.Code != NomenclatureConstants.CountryBG)
                    .OrderBy(x => x.Name)
                    .ToSelectList(x => x.Code, x => x.Name);

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "" });

            return result;
        }

        public List<SelectListItem> GetDDL_LawUnitPosition(int LawUnitTypeId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<LawUnitTypePosition>()
                                      .Include(x => x.LawUnitPosition)
                                      .Where(x => x.LawUnitTypeId == LawUnitTypeId)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.LawUnitPosition.Label,
                                          Value = x.LawUnitPosition.Id.ToString()
                                      }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_ActComplainResult(int CaseTypeId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<ActComplainResultCaseType>()
                                      .Include(x => x.ActComplainResult)
                                      .Where(x => x.CaseTypeId == CaseTypeId && x.ActComplainResult.IsActive)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.ActComplainResult.Label,
                                          Value = x.ActComplainResult.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (selectListItems.Count < 1)
            {
                selectListItems.AddRange(repo.AllReadonly<ActComplainResult>()
                                             .Where(x => x.IsActive)
                                             .Select(x => new SelectListItem()
                                             {
                                                 Text = x.Label,
                                                 Value = x.Id.ToString()
                                             })
                                             .OrderBy(x => x.Text)
                                             .ToList() ?? new List<SelectListItem>());
            }

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDDL_ActComplainResultAsync(int CaseTypeId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = await repo.AllReadonly<ActComplainResultCaseType>()
                                            .Where(x => x.CaseTypeId == CaseTypeId && x.ActComplainResult.IsActive)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.ActComplainResult.Label,
                                                Value = x.ActComplainResult.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync() ?? new List<SelectListItem>();

            if (selectListItems.Count < 1)
            {
                selectListItems.AddRange(await repo.AllReadonly<ActComplainResult>()
                                                   .Where(x => x.IsActive)
                                                   .Select(x => new SelectListItem()
                                                   {
                                                       Text = x.Label,
                                                       Value = x.Id.ToString()
                                                   })
                                                   .OrderBy(x => x.Text)
                                                   .ToListAsync() ?? new List<SelectListItem>());
            }

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDDL_ActComplainResult(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = await repo.AllReadonly<ActComplainResult>()
                                            .Where(x => x.IsActive)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.Label,
                                                Value = x.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_ActComplainIndex(int CaseId, int CaseSessionActId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseCase = repo.AllReadonly<Case>()
                               .Include(x => x.Court)
                               .Where(x => x.Id == CaseId)
                               .FirstOrDefault();

            Expression<Func<ActComplainIndexCourtTypeCaseGroup, bool>> whereFilterReq7 = x => true;
            if (CaseSessionActId > 0)
            {
                //К.Борисов: Взема най-старата дата на резултат от обжалване на акта
                var dtResult = repo.AllReadonly<CaseSessionActComplainResult>()
                                    .Where(x => x.CaseSessionActComplain.CaseSessionActId == CaseSessionActId)
                                    .Where(x => x.CaseSessionActComplain.DateExpired == null)
                                    .OrderBy(x => x.DateResult)
                                    .Select(x => x.DateResult)
                                    .FirstOrDefault();

                dtResult = dtResult ?? DateTime.Now;

                whereFilterReq7 = x => x.DateStart <= dtResult && (x.DateEnd ?? DateTime.MaxValue) > dtResult;
            }


            var selectListItems = repo.AllReadonly<ActComplainIndexCourtTypeCaseGroup>()
                                      .Where(x => x.CourtTypeId == caseCase.Court.CourtTypeId &&
                                                  x.CaseGroupId == caseCase.CaseGroupId &&
                                                  x.ActComplainIndex.IsActive)
                                      .Where(whereFilterReq7)
                                      .OrderBy(x => x.ActComplainIndex.OrderNumber)
                                      .ThenBy(x => x.ActComplainIndex.Code)
                                      .ThenBy(x => x.ActComplainIndex.Label)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = (!string.IsNullOrEmpty(x.ActComplainIndex.Code) ? x.ActComplainIndex.Code + " " : string.Empty) + x.ActComplainIndex.Label,
                                          Value = x.ActComplainIndex.Id.ToString()
                                      }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        private List<SelectListItem> GetActResultGrouping(int fromCaseInstanceId, int toCaseInstanceId, int caseGroupId, int documentTypeId)
        {
            return repo.AllReadonly<ActResultGrouping>()
                       .Where(x => x.FromCaseInstanceId == fromCaseInstanceId &&
                                   x.ToCaseInstanceId == toCaseInstanceId &&
                                   x.CaseGroupId == caseGroupId &&
                                   x.DocumentTypeId == documentTypeId)
                       .Select(x => new SelectListItem()
                       {
                           Text = x.ActResult.Label,
                           Value = x.ActResult.Id.ToString()
                       }).OrderBy(x => x.Text).ToList() ?? new List<SelectListItem>();
        }

        public List<SelectListItem> GetDDL_ActResult(int CaseFromId, int CaseSessionActComplainId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseSessionActComplain = repo.AllReadonly<CaseSessionActComplain>()
                                             .Include(x => x.ComplainDocument)
                                             .Include(x => x.CaseSessionAct)
                                             .ThenInclude(x => x.CaseSession)
                                             .ThenInclude(x => x.Case)
                                             .ThenInclude(x => x.CaseType)
                                             .Where(x => x.Id == CaseSessionActComplainId)
                                             .FirstOrDefault();

            var caseFrom = repo.AllReadonly<Case>()
                               .Include(x => x.CaseType)
                               .Where(x => x.Id == CaseFromId)
                               .FirstOrDefault();

            var selectListItems = GetActResultGrouping(((caseFrom != null) ? caseFrom.CaseType.CaseInstanceId : -1),
                                                       caseSessionActComplain.CaseSessionAct.CaseSession.Case.CaseType.CaseInstanceId,
                                                       caseSessionActComplain.CaseSessionAct.CaseSession.Case.CaseGroupId,
                                                       caseSessionActComplain.ComplainDocument.DocumentTypeId);

            if (selectListItems.Count != 1)
            {
                if (addDefaultElement)
                {
                    selectListItems = selectListItems
                        .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                        .ToList();
                }

                if (addAllElement)
                {
                    selectListItems = selectListItems
                        .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                        .ToList();
                }
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_ActResultOtherCase(string CaseRegNumberOtherSystem, int CaseSessionActComplainId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var instance = -1;
            var caseNumberDecoded = DecodeCaseRegNumber(CaseRegNumberOtherSystem);
            if (!caseNumberDecoded.IsValid)
            {
                return (new List<SelectListItem>()).Prepend(new SelectListItem() { Text = "Избери", Value = "0" }).ToList();
            }
            else
            {
                var court = repo.AllReadonly<Court>()
                                .Include(x => x.CourtType)
                                .Where(x => x.Id == caseNumberDecoded.CourtId)
                                .FirstOrDefault();

                if (court == null)
                    return (new List<SelectListItem>()).Prepend(new SelectListItem() { Text = "Избери", Value = "0" }).ToList();

                var listCourt = court.CourtType.InstanceList.Split(',').Select(Int32.Parse).ToList();
                instance = listCourt.Max(x => x);
            }

            var caseSessionActComplain = repo.AllReadonly<CaseSessionActComplain>()
                                             .Include(x => x.ComplainDocument)
                                             .Include(x => x.CaseSessionAct)
                                             .ThenInclude(x => x.CaseSession)
                                             .ThenInclude(x => x.Case)
                                             .ThenInclude(x => x.CaseType)
                                             .Where(x => x.Id == CaseSessionActComplainId)
                                             .FirstOrDefault();

            var selectListItems = GetActResultGrouping(instance,
                                                       caseSessionActComplain.CaseSessionAct.CaseSession.Case.CaseType.CaseInstanceId,
                                                       caseSessionActComplain.CaseSessionAct.CaseSession.Case.CaseGroupId,
                                                       caseSessionActComplain.ComplainDocument.DocumentTypeId);

            if (selectListItems.Count != 1)
            {
                if (addDefaultElement)
                {
                    selectListItems = selectListItems
                        .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                        .ToList();
                }

                if (addAllElement)
                {
                    selectListItems = selectListItems
                        .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                        .ToList();
                }
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_ExecListLawBase(int caseGroupId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<ExecListLawBaseCaseGroup>()
                                      .Where(x => x.CaseGroupId == caseGroupId)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.ExecListLawBase.Label,
                                          Value = x.ExecListLawBaseId.ToString()
                                      }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Четене на ид-та за PersonRole по група
        /// </summary>
        /// <param name="personRoleGroup"></param>
        /// <returns></returns>
        public async Task<int[]> GetPersonRoleIdsByGroup(int personRoleGroup)
        {
            return await repo.AllReadonly<PersonRoleGrouping>()
                             .Where(x => x.PersonRoleGroup == personRoleGroup)
                             .Select(x => x.PersonRoleId)
                             .Distinct()
                             .ToArrayAsync();
        }

        public async Task<List<SelectListItem>> GetDDL_DocumentGroupByDirection(int documentDirectionId)
        {
            var selectListItems = await repo.AllReadonly<DocumentGroup>()
                                            .Where(x => x.DocumentKind.DocumentDirectionId == documentDirectionId)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.DocumentKind.Label + " - " + x.Label,
                                                Value = x.Id.ToString()
                                            })
                                            .ToListAsync() ?? new List<SelectListItem>();

            selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                                             .ToList();

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDDL_CaseState(bool InitialOnly, bool HideInitialStates)
        {
            Expression<Func<CaseState, bool>> whereInitial = x => true;
            if (InitialOnly)
            {
                whereInitial = x => (x.IsInitialState ?? true) == true;
            }
            if (HideInitialStates)
            {
                whereInitial = x => ((x.IsInitialState ?? false) == false)
                                && !NomenclatureConstants.CaseState.AutomatedStates.Contains(x.Id);
            }
            return await repo.AllReadonly<CaseState>()
                             .Where(whereInitial)
                             .ToSelectListAsync();
        }

        public List<SelectListItem> GetDDL_CaseSessionState(bool InitialOnly)
        {
            Expression<Func<SessionState, bool>> whereInitial = x => true;
            if (InitialOnly)
            {
                whereInitial = x => x.IsInitialState == true;
            }
            return repo.AllReadonly<SessionState>()
                                      .Where(whereInitial)
                                      .ToSelectList();
        }

        public List<SelectListItem> GetDDL_CaseSessionActState(bool InitialOnly, bool HideInitialStates, bool actIsDeclared)
        {
            Expression<Func<ActState, bool>> whereInitial = x => true;
            if (InitialOnly)
            {
                whereInitial = x => x.IsInitialState == true;
            }
            if (HideInitialStates)
            {
                whereInitial = x => x.IsInitialState == false;
            }
            Expression<Func<ActState, bool>> whereDeclared = x => true;
            if (actIsDeclared)
            {
                int[] disabledStatesAfterDeclaration = { NomenclatureConstants.SessionActState.Project, NomenclatureConstants.SessionActState.Coordinated, NomenclatureConstants.SessionActState.Registered };
                whereDeclared = x => !disabledStatesAfterDeclaration.Contains(x.Id);
            }
            return repo.AllReadonly<ActState>()
                                      .Where(whereInitial)
                                      .Where(whereDeclared)
                                      .ToSelectList();
        }

        public List<SelectListItem> GetDDL_JudgeRoleManualRoles(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<JudgeRole>()
                                      .Where(x => (NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.Id)))
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label,
                                          Value = x.Id.ToString()
                                      }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems; ;
        }

        public async Task<List<SelectListItem>> GetDDL_JudgeRoleManualRolesAsync(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = await repo.AllReadonly<JudgeRole>()
                                            .Where(x => (NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.Id)))
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.Label,
                                                Value = x.Id.ToString()
                                            })
                                            .ToListAsync() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems; ;
        }

        public List<SelectListItem> GetDDL_ByCourtTypeInstanceList(int[] courtTypeInstanceList, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<CaseInstance>()
                .Where(x => courtTypeInstanceList.Contains(x.Id))
                .Select(x => new SelectListItem()
                {
                    Text = x.Label,
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return result;
        }

        public List<SelectListItem> GetDDL_SessionState(bool InitialOnly)
        {
            return repo.AllReadonly<SessionState>()
                       .Where(x => x.IsActive &&
                                   (InitialOnly ? x.IsInitialState == true : true))
                       .ToSelectList();
        }
        public List<SelectListItem> GetDDL_SessionStateFiltered(int currentStateId)
        {
            Expression<Func<SessionState, bool>> stateWhere = x => x.Id == currentStateId;
            switch (currentStateId)
            {
                case 0:
                    stateWhere = x => x.IsInitialState == true;
                    break;
                case NomenclatureConstants.SessionState.Nasrocheno:
                    stateWhere = x => true;
                    break;
            }

            return repo.AllReadonly<SessionState>()
                       .Where(stateWhere)
                       .Where(x => x.IsActive)
                       .ToSelectList();
        }

        public async Task<List<SelectListItem>> GetDDL_SessionStateRoute(int currentStateId)
        {
            return await repo.AllReadonly<SessionStateRoute>()
                             .Where(x => x.SessionStateFromId == currentStateId)
                             .Select(x => x.SessionStateTo)
                             .ToSelectListAsync();
        }

        public List<SelectListItem> GetDDL_Specyality_ByLowUnit_Type(int lawunitTypeId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<Speciality>()
                .Where(x => x.LawUnitTypeID == lawunitTypeId && (x.DateEnd ?? DateTime.Now.AddDays(1)).Date > DateTime.Now.Date)
                .OrderBy(x => x.OrderNumber)
                .Select(x => new SelectListItem()
                {
                    Text = ((x.Code != null) ? x.Code + " " : "") + x.Label,
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

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
        public CodeMapping GetInnerCodeFromCodeMappingStr(string alias, string outerCode)
        {
            return repo.AllReadonly<CodeMapping>()
                                .Where(x => x.Alias == alias &&
                                            x.OuterCode == outerCode)
                                .FirstOrDefault();
        }

        public int GetInnerCodeFromCodeMapping(string alias, string outerCode)
        {
            return int.Parse(repo.AllReadonly<CodeMapping>()
                                 .Where(x => x.Alias == alias &&
                                             x.OuterCode == outerCode)
                                 .Select(x => x.InnerCode)
                                 .FirstOrValue("0"));
        }

        public string GetOuterCodeFromCodeMapping(string alias, string innerCode)
        {
            return repo.AllReadonly<CodeMapping>()
                               .Where(x => x.Alias == alias &&
                                           x.InnerCode == innerCode)
                               .Select(x => x.OuterCode)
                               .FirstOrDefault();
        }


        public List<SelectListItem> GetDDL_CaseSessionResult(bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<SessionResult>()
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.Label,
                                 Value = x.Id.ToString()
                             })
                             .OrderBy(x => x.Text)
                             .ToList() ?? new List<SelectListItem>();

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

        public List<SelectListItem> GetDDL_CaseTypeByCourtType(int caseGroupId, int courtTypeId, bool addDefaultElement)
        {
            var typesForCourt = repo.AllReadonly<CourtTypeCaseType>().Where(x => x.CourtTypeId == courtTypeId);

            var selectListItems = repo.AllReadonly<CaseType>()
                                       .Where(x => typesForCourt.Any(t => t.CaseTypeId == x.Id))
                                       .Where(x => x.CaseGroupId == caseGroupId)
                                       .OrderBy(x => x.OrderNumber)
                                       .Select(x => new SelectListItem()
                                       {
                                           Text = x.Label,
                                           Value = x.Id.ToString()
                                       }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                       .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                       .ToList();
            }


            return selectListItems;
        }

        public List<SelectListItem> GetDDL_CaseTypeFromCourtType(int caseGroupId, string caseInstanceIds, bool addDefaultElement = true)
        {
            Expression<Func<CourtTypeCaseType, bool>> caseInstanceContainsWhere = x => true;
            if (string.IsNullOrEmpty(caseInstanceIds) == false)
            {
                string[] instances = caseInstanceIds.Split(",").ToArray();
                caseInstanceContainsWhere = x => instances.Contains(x.CaseType.CaseInstanceId.ToString());
            }

            var selectListItems = (userContext.CourtId == NomenclatureConstants.Courts.VSS) ? repo.AllReadonly<CaseType>()
                                                                                                  .Where(x => x.CaseGroupId == caseGroupId)
                                                                                                  .Select(x => new SelectListItem()
                                                                                                  {
                                                                                                      Text = x.Label,
                                                                                                      Value = x.Id.ToString()
                                                                                                  })
                                                                                                  .ToList() :
                                                                                              repo.AllReadonly<CourtTypeCaseType>()
                                                                                                  .Where(x => x.CourtTypeId == userContext.CourtTypeId &&
                                                                                                              x.CaseType.CaseGroupId == caseGroupId)
                                                                                                  .Where(caseInstanceContainsWhere)
                                                                                                  .Select(x => new SelectListItem()
                                                                                                  {
                                                                                                      Text = x.CaseType.Label,
                                                                                                      Value = x.CaseType.Id.ToString()
                                                                                                  })
                                                                                                  .OrderBy(x => x.Text)
                                                                                                  .ToList();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                       .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                       .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_ActComplainIndexByCourtType(bool addDefaultElement = true, bool addAllElement = false)
        {
            var dtNow = DateTime.Now;
            var selectListItems = repo.AllReadonly<ActComplainIndexCourtTypeCaseGroup>()
                                      .Where(x => x.CourtTypeId == userContext.CourtTypeId && x.ActComplainIndex.IsActive)
                                      .Select(x => new
                                      {
                                          x.ActComplainIndex.OrderNumber,
                                          Text = (!string.IsNullOrEmpty(x.ActComplainIndex.Code) ? x.ActComplainIndex.Code + " " : string.Empty) + x.ActComplainIndex.Label,
                                          Value = x.ActComplainIndex.Id.ToString(),
                                          IsActive = ((x.DateEnd ?? DateTime.MaxValue) > dtNow) && ((x.ActComplainIndex.DateEnd ?? DateTime.MaxValue) > dtNow)
                                      })
                                      .Distinct()
                                      .ToList()
                                      .OrderBy(x => x.OrderNumber)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = (!x.IsActive) ? "(Неакт.) " + x.Text : x.Text,
                                          Value = x.Value
                                      })
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_ActResult(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<ActResult>()
                                      .Where(x => x.IsActive)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label,
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_MoneyFineType(int caseGroupId, bool addDefaultElement = true)
        {
            var selectListItems = repo.AllReadonly<MoneyFineType>()
                                      .Where(x => x.IsActive)
                                      .Where(x => x.MoneyFineCaseGroups.Where(a => a.CaseGroupId == caseGroupId).Any())
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label,
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_SessionTypeWithoutClosedSession(bool addDefaultElement = true)
        {
            var selectListItems = repo.AllReadonly<SessionType>()
                                       .Where(x => x.Id != NomenclatureConstants.SessionType.ClosedSession)
                                       .OrderBy(x => x.Label)
                                       .Select(x => new SelectListItem()
                                       {
                                           Text = x.Label,
                                           Value = x.Id.ToString()
                                       }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                       .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                       .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_CourtGroup(int courtId, bool addDefaultElement = true)
        {
            var selectListItems = repo.AllReadonly<CourtGroup>()
                                      .Where(x => x.CourtId == courtId)
                                      .Where(x => x.GroupKind == NomenclatureConstants.CourtGroupKinds.JudgeSelection)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label,
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_CourtGroup_Substitution(int courtId, bool addDefaultElement = true)
        {
            var selectListItems = repo.AllReadonly<CourtGroup>()
                                      .Where(x => x.CourtId == courtId)
                                      .Where(x => x.GroupKind == NomenclatureConstants.CourtGroupKinds.Replacement)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label,
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_LoadGroupLink(bool addDefaultElement = true)
        {
            var selectListItems = repo.AllReadonly<LoadGroupLink>()
                                      .Where(x => x.CourtTypeId == userContext.CourtTypeId)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.LoadGroup.Label,
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        private List<SelectListItem> GetListResultFromRule(int? SessionTypeGroupId, int CaseGroupId, int CourtTypeId, bool addDefaultElement = true, DateTime? dtNow = null)
        {
            Expression<Func<SessionResultFilterRule, bool>> whereDate = x => true;
            if (dtNow.HasValue)
            {
                whereDate = x => (x.DateStart ?? DateTime.MinValue) <= dtNow.Value.MakeEndDate()
                && (x.DateEnd ?? DateTime.MaxValue) >= dtNow;
            }


            var selectListItems = repo.AllReadonly<SessionResultFilterRule>()
                                     .Where(x => x.SessionTypeGroupId == SessionTypeGroupId &&
                                                 x.CaseGroupId == CaseGroupId &&
                                                 x.CourtTypeId == CourtTypeId &&
                                                 x.IsActive)
                                     .Where(whereDate)
                                     .Select(x => new SelectListItem()
                                     {
                                         Text = x.SessionResult.Label,
                                         Value = x.SessionResultId.ToString()
                                     })
                                     .OrderBy(x => x.Text)
                                     .ToList() ?? new List<SelectListItem>();

            if (selectListItems.Count < 1)
            {
                selectListItems.AddRange(repo.AllReadonly<SessionResult>()
                                             .Where(x => x.IsActive)
                                             .Select(x => new SelectListItem()
                                             {
                                                 Text = x.Label,
                                                 Value = x.Id.ToString()
                                             })
                                             .OrderBy(x => x.Text)
                                             .ToList() ?? new List<SelectListItem>());
            }

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        private async Task<List<SelectListItem>> GetListResultFromRuleAsync(int? SessionTypeGroupId, int CaseGroupId, int CourtTypeId, bool addDefaultElement = true, DateTime? dtNow = null)
        {
            Expression<Func<SessionResultFilterRule, bool>> whereDate = x => true;
            if (dtNow.HasValue)
            {
                whereDate = x => (x.DateStart ?? DateTime.MinValue) <= dtNow.Value.MakeEndDate()
                && (x.DateEnd ?? DateTime.MaxValue) >= dtNow;
            }


            var selectListItems = await repo.AllReadonly<SessionResultFilterRule>()
                                            .Where(x => x.SessionTypeGroupId == SessionTypeGroupId &&
                                                        x.CaseGroupId == CaseGroupId &&
                                                        x.CourtTypeId == CourtTypeId &&
                                                        x.IsActive)
                                            .Where(whereDate)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.SessionResult.Label,
                                                Value = x.SessionResultId.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync() ?? new List<SelectListItem>();

            if (selectListItems.Count < 1)
            {
                selectListItems.AddRange(await repo.AllReadonly<SessionResult>()
                                                   .Where(x => x.IsActive)
                                                   .Select(x => new SelectListItem()
                                                   {
                                                       Text = x.Label,
                                                       Value = x.Id.ToString()
                                                   })
                                                   .OrderBy(x => x.Text)
                                                   .ToListAsync() ?? new List<SelectListItem>());
            }

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_SessionResultFromRulesByCaseLoadElementTypeAndSessionType(int CaseLoadElementTypeId, int SessionTypeId, bool addDefaultElement = true)
        {
            var caseLoadElementType = repo.AllReadonly<CaseLoadElementType>()
                                           .Include(x => x.CaseLoadElementGroup)
                                           .ThenInclude(x => x.CaseType)
                                           .Where(x => x.Id == CaseLoadElementTypeId)
                                           .FirstOrDefault();

            var sessionType = repo.GetById<SessionType>(SessionTypeId);

            var selectListItems = GetListResultFromRule((sessionType != null ? sessionType.SessionTypeGroup : 0), (caseLoadElementType.CaseLoadElementGroup.CaseType != null ? caseLoadElementType.CaseLoadElementGroup.CaseType.CaseGroupId : 0), userContext.CourtTypeId, addDefaultElement);

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDDL_SessionResultFromRulesByCaseId(int CaseId, bool addDefaultElement = true)
        {
            var caseCase = await repo.AllReadonly<Case>()
                                     .Where(x => x.Id == CaseId)
                                     .FirstOrDefaultAsync();

            var selectListItems = await GetListResultFromRuleAsync(NomenclatureConstants.SessionTypeGroupe.ClosedSession, caseCase.CaseGroupId, userContext.CourtTypeId, addDefaultElement, DateTime.Now);

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_SessionResultFromRules(int CaseSessionId, bool addDefaultElement = true)
        {
            var caseSession = repo.AllReadonly<CaseSession>()
                                  .Include(x => x.Case)
                                  .Include(x => x.SessionType)
                                  .Where(x => x.Id == CaseSessionId)
                                  .FirstOrDefault();

            var selectListItems = GetListResultFromRule(caseSession.SessionType.SessionTypeGroup, caseSession.Case.CaseGroupId, userContext.CourtTypeId, addDefaultElement, caseSession.DateFrom);

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDDL_SessionResultFromRulesAsync(int CaseSessionId, bool addDefaultElement = true)
        {
            var caseSession = await repo.AllReadonly<CaseSession>()
                                        .Include(x => x.Case)
                                        .Include(x => x.SessionType)
                                        .Where(x => x.Id == CaseSessionId)
                                        .FirstOrDefaultAsync();

            var selectListItems = await GetListResultFromRuleAsync(caseSession.SessionType.SessionTypeGroup, caseSession.Case.CaseGroupId, userContext.CourtTypeId, addDefaultElement, caseSession.DateFrom);

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDDL_CaseMigrationType(int directionId, bool addDefaultElement = true)
        {
            var selectListItems = await repo.AllReadonly<CaseMigrationType>()
                                            .Where(x => x.MigrationDirection == directionId)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.Description ?? x.Label,
                                                Value = x.Id.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        public IQueryable<MoneyType> Get_MoneyType()
        {
            return repo.AllReadonly<MoneyType>()
                .AsQueryable();
        }

        public List<SelectListItem> GetDDL_StreetType(bool addDefaultElement = true)
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = "Квартал", Value = "2" });
            selectListItems.Add(new SelectListItem() { Text = "Булевард/Улица/Площад", Value = "1" });

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        public IQueryable<EkStreetVM> EkStreet_Select(EkStreetFilterVM model)
        {
            var ekStreets = repo.AllReadonly<EkStreet>()
                                .Where(x => (!string.IsNullOrEmpty(model.CityCode) ? x.Ekatte == model.CityCode : true) &&
                                            (model.StreetTipeId > 0 ? x.StreetType == model.StreetTipeId : true) &&
                                            (!string.IsNullOrEmpty(model.ElementName) ? (x.Name ?? string.Empty).Contains((model.ElementName ?? string.Empty).ToUpper()) : true))
                                .ToList() ?? new List<EkStreet>();

            var result = new List<EkStreetVM>();
            if (ekStreets.Count > 0)
            {
                var ekEkattes = repo.AllReadonly<EkEkatte>().ToList();

                foreach (var ekStreet in ekStreets)
                {
                    result.Add(new EkStreetVM()
                    {
                        Id = ekStreet.Id,
                        Name = ekStreet.Name,
                        City = ekEkattes.Where(x => x.Ekatte == ekStreet.Ekatte).Select(x => x.Name).FirstOrDefault(),
                        StreetTypeLabel = (((ekStreet.StreetType ?? 0) == 1) ? "Булевард/Улица/Площад" : "Квартал")
                    });
                }
            }

            return result.Where(x => !string.IsNullOrEmpty(x.City)).AsQueryable();
        }

        public bool EkStreet_SaveData(EkStreet model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<EkStreet>(model.Id);
                    saved.Code = model.Code;
                    saved.Ekatte = model.Ekatte;
                    saved.Name = model.Name.ToUpper();
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.StreetType = model.StreetType;
                    saved.DateWrt = DateTime.Now;
                    repo.SaveChanges();
                }
                else
                {

                    //Insert
                    model.Id = 0;
                    model.Name = model.Name.ToUpper();

                    var lastManualCode = repo.AllReadonly<EkStreet>()
                                             .Where(x => x.Ekatte == model.Ekatte &&
                                                         EF.Functions.ILike(x.Code, "M%"))
                                             .OrderByDescending(x => x.Id)
                                             .Select(x => x.Code)
                                             .FirstOrDefault();

                    int manualCounter = 0;
                    if (!string.IsNullOrEmpty(lastManualCode))
                    {
                        lastManualCode = lastManualCode.Replace("M", "", StringComparison.InvariantCultureIgnoreCase);
                        if (!int.TryParse(lastManualCode, out manualCounter))
                        {
                            return false;
                        }
                    }
                    manualCounter += 1;
                    model.Code = $"M{manualCounter}";
                    model.DateWrt = DateTime.Now;

                    repo.Add<EkStreet>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на адрес Id={model.Id}");
                return false;
            }
        }

        public List<SelectListItem> GetDDL_SessionToDate(bool addDefaultElement = true)
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.SessionToDateLabel.SessionTo1MonthLabel, Value = NomenclatureConstants.SessionToDateValue.SessionTo1MonthValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.SessionToDateLabel.SessionTo2MonthLabel, Value = NomenclatureConstants.SessionToDateValue.SessionTo2MonthValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.SessionToDateLabel.SessionTo3MonthLabel, Value = NomenclatureConstants.SessionToDateValue.SessionTo3MonthValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.SessionToDateLabel.SessionUp3MonthLabel, Value = NomenclatureConstants.SessionToDateValue.SessionUp3MonthValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.SessionToDateLabel.NoSessionLabel, Value = NomenclatureConstants.SessionToDateValue.NoSessionValue.ToString() });

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Зареждане на списък с разлика между дата на заседание и дата на обявяване на акта
        /// </summary>
        /// <param name="addDefaultElement">Добавя елемент "Избери"</param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_ActToDate(bool addDefaultElement = true)
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.ActToDateLabel.ActDateTo1MonthLabel, Value = NomenclatureConstants.ActToDateValue.ActDateTo1MonthValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.ActToDateLabel.ActDateTo2MonthLabel, Value = NomenclatureConstants.ActToDateValue.ActDateTo2MonthValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.ActToDateLabel.SessionTo3MonthLabel, Value = NomenclatureConstants.ActToDateValue.ActDateTo3MonthValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.ActToDateLabel.ActDateTo1YeаrLabel, Value = NomenclatureConstants.ActToDateValue.ActDateTo1YeаrValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.ActToDateLabel.ActDateUo1YeаrLabel, Value = NomenclatureConstants.ActToDateValue.ActDateUo1YeаrValue.ToString() });

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_ActMotiveToDate(bool addDefaultElement = true)
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.ActMotiveToDateLabel.ActMotiveDateTo15DayLabel, Value = NomenclatureConstants.ActMotiveToDateValue.ActMotiveDateTo15DayValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.ActMotiveToDateLabel.ActMotiveDateTo60DayLabel, Value = NomenclatureConstants.ActMotiveToDateValue.ActMotiveDateTo60DayValue.ToString() });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.ActMotiveToDateLabel.ActMotiveDateToUp60DayLabel, Value = NomenclatureConstants.ActMotiveToDateValue.ActMotiveDateToUp60DayValue.ToString() });

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_ComplexIndex(bool addDefaultElement = true)
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = "1", Value = "1" });
            selectListItems.Add(new SelectListItem() { Text = "2", Value = "2" });
            selectListItems.Add(new SelectListItem() { Text = "3", Value = "3" });
            selectListItems.Add(new SelectListItem() { Text = "4", Value = "4" });
            selectListItems.Add(new SelectListItem() { Text = "5", Value = "5" });

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Извличане на данни от SessionResultGrouping за DropDown
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_SessionResultGrouping(int groupId)
        {
            var selectListItems = await repo.AllReadonly<SessionResultGrouping>()
                                            .Where(x => x.SessionResultGroup == groupId)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.SessionResult.Label,
                                                Value = x.SessionResultId.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync() ?? new List<SelectListItem>();

            selectListItems = selectListItems
                .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                .ToList();

            return selectListItems;
        }

        /// <summary>
        /// Източник на постъпване за дело
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseCreateFroms(int instanceId)
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = "Избери", Value = "-1" });
            selectListItems.Add(new SelectListItem() { Text = "Новообразувано", Value = NomenclatureConstants.CaseCreateFroms.New.ToString() });
            selectListItems.Add(new SelectListItem() { Text = "Получени по подсъдност", Value = NomenclatureConstants.CaseCreateFroms.Jurisdiction.ToString() });
            selectListItems.Add(new SelectListItem() { Text = "Върнати за ново разглеждане", Value = NomenclatureConstants.CaseCreateFroms.NewNumber.ToString() });
            selectListItems.Add(new SelectListItem() { Text = "Продължени под същия номер", Value = NomenclatureConstants.CaseCreateFroms.OldNumber.ToString() });

            if (instanceId == NomenclatureConstants.CaseInstanceType.SecondInstance)
            {
                selectListItems.Add(new SelectListItem() { Text = "Връщане след доразследване", Value = NomenclatureConstants.CaseCreateFroms.OldNumber.ToString() });
                selectListItems.Add(new SelectListItem() { Text = "Върнато след администриране", Value = NomenclatureConstants.CaseCreateFroms.ReturnedAfterAdministration.ToString() });
                selectListItems.Add(new SelectListItem() { Text = "Постъпили дела по чл. 80, ал.10 ПАС", Value = NomenclatureConstants.CaseCreateFroms.AcceptedCh80.ToString() });
            }
            else
            {
                selectListItems.Add(new SelectListItem() { Text = "Връщане след доразследване", Value = NomenclatureConstants.CaseCreateFroms.ReturnAfterFurtherInvestigation.ToString() });
            }

            return selectListItems;
        }

        /// <summary>
        /// Резултат от заседание по подадени параметри
        /// </summary>
        /// <param name="caseGroupId"></param>
        /// <param name="courtTypeId"></param>
        /// <param name="addDefaultElement"></param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_SessionResultFromRulesByFilter(int caseGroupId, int courtTypeId, bool addDefaultElement = true)
        {
            Expression<Func<SessionResultFilterRule, bool>> caseGroupWhere = x => true;
            if (caseGroupId > 0)
                caseGroupWhere = x => x.CaseGroupId == caseGroupId;

            Expression<Func<SessionResultFilterRule, bool>> courtTypeWhere = x => true;
            if (courtTypeId > 0)
                courtTypeWhere = x => x.CourtTypeId == courtTypeId;

            var selectListItems = await repo.AllReadonly<SessionResultFilterRule>()
                                            .Where(x => x.IsActive)
                                            .Where(x => x.SessionResult.IsActive)
                                            .Where(caseGroupWhere)
                                            .Where(courtTypeWhere)
                                            .Select(x => new
                                            {
                                                Text = x.SessionResult.Label,
                                                ValueId = x.SessionResultId
                                            })
                                            .GroupBy(x => new { x.ValueId })
                                            .Select(g => new SelectListItem()
                                            {
                                                Text = g.Select(c => c.Text).FirstOrDefault(),
                                                Value = g.Key.ValueId.ToString()
                                            })
                                            .OrderBy(x => x.Text)
                                            .ToListAsync() ?? new List<SelectListItem>();

            if (selectListItems.Count < 1)
            {
                selectListItems.AddRange(await repo.AllReadonly<SessionResult>()
                                                   .Where(x => x.IsActive)
                                                   .Select(x => new SelectListItem()
                                                   {
                                                       Text = x.Label,
                                                       Value = x.Id.ToString()
                                                   })
                                                   .OrderBy(x => x.Text)
                                                   .ToListAsync() ?? new List<SelectListItem>());
            }

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetDDL_IsFinalAct(bool addDefaultElement = true)
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.AnswerQuestionTextBG.Yes, Value = "Y" });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.AnswerQuestionTextBG.No, Value = "N" });

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "A" }).ToList();
            }

            return selectListItems;
        }
        public CaseRegNumberVM DecodeCaseRegNumber(string regNumber)
        {
            CaseRegNumberVM result = new CaseRegNumberVM()
            {
                IsValid = false
            };
            if (string.IsNullOrEmpty(regNumber) || regNumber.Length != 14)
            {
                result.ErrorMessage = "Невалиден номер дело.";
                return result;
            }

            if (repo.AllReadonly<Case>().Where(x => x.RegNumber == regNumber).Any())
            {
                result.ErrorMessage = $"Въведеното дело от друга система съществува в ЕИСС.";
                return result;
            }

            result.IsValid = true;
            try
            {
                var yearStr = regNumber.Substring(0, 4);
                var courtStr = regNumber.Substring(4, 3);
                var characterStr = regNumber.Substring(7, 2);
                var numberStr = regNumber.Substring(9, 5);

                result.Year = int.Parse(yearStr);

                result.CourtId = repo.AllReadonly<Court>()
                                                           .Where(x => x.Code == courtStr)
                                                           .Select(x => x.Id)
                                                           .FirstOrDefault();
                if (result.CourtId == 0)
                {
                    result.ErrorMessage = $"Невалиден код на съд: {courtStr}";
                    result.IsValid = false;
                    return result;
                }

                result.CaseCharacterId = repo.AllReadonly<CaseCharacter>()
                                           .Where(x => x.Code == characterStr)
                                           .Select(x => x.Id)
                                           .FirstOrDefault();

                if (result.CaseCharacterId == 0)
                {
                    result.ErrorMessage = $"Невалиден характер на дело: {characterStr}";
                    result.IsValid = false;
                    return result;
                }

                result.ShortNumberInt = int.Parse(numberStr);
                result.ShortNumber = result.ShortNumberInt.ToString();

                result.IsValid = true;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Грешка при декодиране на номер дело.";
                result.IsValid = false;
            }
            return result;
        }

        public IEnumerable<LabelValueVM> Get_ActLawBase(string query, int CaseId, int id)
        {

            Expression<Func<LawBase, bool>> filterId = x => true;
            if (id > 0)
                filterId = x => x.Id == id;
            else
            {
                if (CaseId > 0)
                {
                    var caseCase = GetCaseWithIncluded(CaseId);
                    filterId = x => x.CourtTypeId == caseCase.Court.CourtTypeId &&
                                    x.CaseInstanceId == caseCase.CaseType.CaseInstanceId &&
                                    x.CaseGroupId == caseCase.CaseGroupId;
                }
                else
                {
                    filterId = x => x.CourtTypeId == userContext.CourtTypeId;
                }
            }

            Expression<Func<LawBase, bool>> filterQuery = x => true;
            if (!string.IsNullOrEmpty(query))
            {
                var strfind = query.ToUpper();
                filterQuery = x => x.Label.ToUpper().Contains(strfind);
            }

            //var c = repo.AllReadonly<LawBase>()
            //            .Include(x => x.CaseGroup)
            //            .Where(filterId)
            //            .Where(filterQuery)
            //            .OrderBy(x => x.CaseGroupId)
            //            .Select(x => new LabelValueVM()
            //            {
            //                Value = x.Id.ToString(),
            //                Label = x.Label + " (" + x.CaseGroup.Label + ")"
            //            }).ToList();

            return repo.AllReadonly<LawBase>()
                            .Where(filterId)
                            .Where(filterQuery)
                            .OrderBy(x => x.CaseGroupId)
                            .Select(x => new LabelValueVM()
                            {
                                Value = x.Id.ToString(),
                                Label = x.Label + " (" + x.CaseGroup.Code + ")"
                            });
        }

        public List<SelectListItem> GetDDL_ObligationJuryReportPersonType()
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Всички", Value = "-1" });
            selectListItems.Add(new SelectListItem() { Text = "Съдебни заседатели", Value = SourceTypeSelectVM.LawUnitPrefix + NomenclatureConstants.LawUnitTypes.Jury });
            selectListItems.Add(new SelectListItem() { Text = "Вещи лица", Value = SourceTypeSelectVM.CasePersonPrefix + NomenclatureConstants.PersonRole.Expert });
            selectListItems.Add(new SelectListItem() { Text = "Експерти", Value = SourceTypeSelectVM.CasePersonPrefix + NomenclatureConstants.PersonRole.Consultant });
            selectListItems.Add(new SelectListItem() { Text = "Преводачи", Value = SourceTypeSelectVM.CasePersonPrefix + NomenclatureConstants.PersonRole.Translator });

            return selectListItems;
        }

        public int[] GetHtmlTemplateForCasePerson()
        {
            var dateTimeNow = DateTime.Now;
            var dateTimeAddOneYear = DateTime.Now.AddYears(1);
            return repo.AllReadonly<HtmlTemplate>()
                       .Where(x => ((x.HaveCasePerson ?? false) == true) &&
                                   (x.DateFrom <= dateTimeNow && dateTimeNow <= (x.DateTo ?? dateTimeAddOneYear)))
                       .Select(x => x.Id)
                       .ToArray();
        }

        public int[] GetHtmlTemplateForFromToDate()
        {
            var dateTimeNow = DateTime.Now;
            var dateTimeAddOneYear = DateTime.Now.AddYears(1);
            return repo.AllReadonly<HtmlTemplate>()
                       .Where(x => ((x.HaveFromToDate ?? false) == true) &&
                                   (x.DateFrom <= dateTimeNow && dateTimeNow <= (x.DateTo ?? dateTimeAddOneYear)))
                       .Select(x => x.Id)
                       .ToArray();
        }

        public List<SelectListItem> GetDDL_MoneyCountryReceiver()
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Избери", Value = "-1" });
            selectListItems.Add(new SelectListItem() { Text = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.Court), Value = SourceTypeSelectVM.Court.ToString() });
            selectListItems.Add(new SelectListItem() { Text = SourceTypeSelectVM.GetSourceTypeName(SourceTypeSelectVM.Instutution), Value = SourceTypeSelectVM.Instutution.ToString() });

            return selectListItems;
        }

        public IQueryable<LawBaseVM> LawBase_Select(int CaseId)
        {
            var caseCase = GetCaseWithIncluded(CaseId);
            return repo.AllReadonly<LawBase>()
                       .Where(x => x.CourtTypeId == caseCase.Court.CourtTypeId &&
                                   x.CaseInstanceId == caseCase.CaseType.CaseInstanceId &&
                                   x.CaseGroupId == caseCase.CaseGroupId)
                       .Select(x => new LawBaseVM()
                       {
                           Id = x.Id,
                           Code = x.Code,
                           Label = x.Label,
                           CourtTypeLabel = x.CourtType.Label,
                           CaseInstanceLabel = x.CaseInstance.Label,
                           CaseGroupLabel = x.CaseGroup.Label,
                           IsActive = x.IsActive
                       })
                       .AsQueryable();
        }

        public bool LawBase_SaveData(LawBaseEditVM model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<LawBase>(model.Id);
                    saved.Code = model.Label;
                    saved.Label = model.Label;
                    saved.IsActive = model.IsActive;
                    saved.DateStart = model.DateStart;
                    saved.DateEnd = model.DateEnd;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    var saved = new LawBase()
                    {
                        Id = model.Id,
                        CaseGroupId = model.CaseGroupId,
                        CaseInstanceId = model.CaseInstanceId,
                        CourtTypeId = model.CourtTypeId,
                        Code = model.Label,
                        Label = model.Label,
                        IsActive = model.IsActive,
                        DateStart = model.DateStart,
                        DateEnd = model.DateEnd,
                        OrderNumber = 0
                    };
                    repo.Add<LawBase>(saved);
                    repo.SaveChanges();
                    model.Id = saved.Id;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на нормативен текст={model.Id}");
                return false;
            }
        }

        public Case GetCaseWithIncluded(int CaseId)
        {
            return repo.AllReadonly<Case>()
                       .Include(x => x.Court)
                       .Include(x => x.CaseType)
                       .Where(x => x.Id == CaseId)
                       .FirstOrDefault();
        }

        public LawBaseEditVM LawBase_GetById(int id)
        {
            return repo.AllReadonly<LawBase>()
                       .Where(x => x.Id == id)
                       .Select(x => new LawBaseEditVM()
                       {
                           Id = x.Id,
                           CourtTypeId = x.CourtTypeId,
                           CaseInstanceId = x.CaseInstanceId,
                           CaseGroupId = x.CaseGroupId,
                           Code = x.Code,
                           Label = x.Label,
                           DateStart = x.DateStart,
                           DateEnd = x.DateEnd,
                           IsActive = x.IsActive
                       })
                       .FirstOrDefault();
        }

        public IEnumerable<SelectListItem> GetDDL_LawBase(int CaseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = LawBase_Select(CaseId).Where(x => x.IsActive).ToList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Label }) ?? new List<SelectListItem>();

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

        public bool IsExistsNameLawBase(string Label, int CaseId)
        {
            var _label = Label.ToUpper();
            var caseCase = GetCaseWithIncluded(CaseId);
            return repo.AllReadonly<LawBase>()
                       .Any(x => EF.Functions.ILike(x.Label, _label.ToPaternSearch()) &&
                                 x.CourtTypeId == caseCase.Court.CourtTypeId &&
                                 x.CaseInstanceId == caseCase.CaseType.CaseInstanceId &&
                                 x.CaseGroupId == caseCase.CaseGroupId);
        }

        public List<SelectListItem> GetDropDownListFromCode<T>(bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true) where T : class, ICommonNomenclature
        {
            var result = repo.AllReadonly<T>()
                         .Where(x => x.IsActive)
                         .ToSelectListFromCode(addDefaultElement, addAllElement, orderByNumber);

            return result;
        }
        public List<SelectListItem> GetDropDownListCodeCode<T>(bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true) where T : class, ICommonNomenclature
        {
            var result = repo.AllReadonly<T>()
                         .Where(x => x.IsActive)
                         .ToSelectListFromCode(addDefaultElement, addAllElement, orderByNumber, false);

            return result;
        }

        public IEnumerable<LabelValueVM> Get_PersonRoles(string query, int? id)
        {
            Expression<Func<PersonRole, bool>> filterId = x => true;
            if (id > 0)
                filterId = x => x.Id == id;

            Expression<Func<PersonRole, bool>> filterQuery = x => true;
            if (string.IsNullOrEmpty(query) == false)
                filterQuery = x => EF.Functions.ILike(x.Label, query.ToPaternSearch());

            return repo.AllReadonly<PersonRole>()
                       .Where(filterId)
                       .Where(filterQuery)
                       .Select(x => new LabelValueVM()
                       {
                           Value = x.Id.ToString(),
                           Label = x.Label,
                           ObjectKind = x.RoleKindId.ToString()
                       })
                       .OrderBy(x => x.Label)
                       .ToList();
        }
        public IEnumerable<LabelValueVM> Get_EISPPTblElement(string EisppTblCode, string term, string id)
        {
            Expression<Func<EisppTblElement, bool>> filter = x => true;
            if (!string.IsNullOrEmpty(term))
            {
                term = term.ToPaternSearch();
                filter = x => EF.Functions.ILike(x.Label, term);
            }
            if (!string.IsNullOrEmpty(id))
            {
                filter = x => x.Code == id;
            }
            return repo.AllReadonly<EisppTblElement>()
                            .Where(filter)
                            .Where(x => x.EisppTblCode == EisppTblCode && x.IsActive)
                            .OrderBy(x => x.Label)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Code,
                                Label = x.Label
                            }).ToList();
        }
        public EisppTblElement GetByCode_EISPPTblElement(string Code)
        {
            return repo.AllReadonly<EisppTblElement>()
                                .Where(x => x.Code == Code)
                                .FirstOrDefault();
        }

        public async Task<EisppTblElement> GetByCode_EISPPTblElementAsync(string Code)
        {
            return await repo.AllReadonly<EisppTblElement>()
                             .Where(x => x.Code == Code)
                             .FirstOrDefaultAsync();
        }

        public List<SelectListItem> GetDDL_VksSessionLawunitChange()
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "С промяна на състава", Value = NomenclatureConstants.VksSessionLawunitChange.WithChange.ToString() });
            selectListItems.Add(new SelectListItem() { Text = "Без промяна на състава", Value = NomenclatureConstants.VksSessionLawunitChange.NoChange.ToString() });
            return selectListItems;
        }

        public List<SelectListItem> GetDDL_DocumentGroupWithKind()
        {
            var result = repo.AllReadonly<DocumentGroup>()
                             .Select(x => new SelectListItem
                             {
                                 Value = x.Id.ToString(),
                                 Text = $"{x.Label} ({x.DocumentKind.Label})",
                             })
                             .ToList();

            result = result.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                           .ToList();

            return result;
        }

        public EkEkatte GetEkatteByEkatte(string ekatte)
        {
            return repo.AllReadonly<EkEkatte>()
                    .Where(x => x.Ekatte == ekatte)
                    .FirstOrDefault();
        }

        public List<SelectListItem> GetDDL_CaseSelectionChangeType(int typeId, bool addDefaultElement = true)
        {
            var selectListItems = repo.AllReadonly<CaseSelectionChangeType>()
                                      .Where(x => x.Id == typeId || typeId == 0)

                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label,
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Метод извличащ тип състав по точен вид дело
        /// </summary>
        /// <param name="caseTypeId">Вид дело</param>
        /// <param name="addDefaultElement">Флаг дали да се добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_CaseTypeUnit(int caseTypeId, bool addDefaultElement = true)
        {
            Expression<Func<CaseTypeUnit, bool>> caseTypeIdWhere = x => x.CaseTypeId == caseTypeId;

            List<SelectListItem> selectListItems = await repo.AllReadonly<CaseTypeUnit>()
                                                             .Where(caseTypeIdWhere)
                                                             .Where(x => x.IsActive)
                                                             .OrderBy(x => x.OrderNumber)
                                                             .Select(x => new SelectListItem()
                                                             {
                                                                 Value = x.Id.ToString(),
                                                                 Text = x.Label,
                                                             })
                                                             .ToListAsync()
                                                             .ConfigureAwait(false);

            if (addDefaultElement)
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();

            return selectListItems;
        }
        public int[] GetPunishmentFineNoValidate()
        {
            return repo.AllReadonly<SentenceType>()
                        .Where(x => x.Id == NomenclatureConstants.SentenceTypes.DeprivationOfMps ||
                                    x.Id == NomenclatureConstants.SentenceTypes.DeprivationOfMpsValue)
                        .Select(x => int.Parse(x.Code))
                        .ToArray();
        }
        public List<SelectListItem> GetDDL_IsGenerated()
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.AnswerQuestionTextBG.Yes, Value = NomenclatureConstants.YesNo.Yes });
            selectListItems.Add(new SelectListItem() { Text = NomenclatureConstants.AnswerQuestionTextBG.No, Value = NomenclatureConstants.YesNo.No });
            selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "A" }).ToList();
            return selectListItems;
        }

        /// <summary>
        /// Извличане на данни за падащ списък за видове мерки за престъпление
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_MeasureKind()
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Процесуална", Value = "1" });
            if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.Request9_2024))
            {
                selectListItems.Add(new SelectListItem() { Text = "Пробационна", Value = "2" });
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();
            }
            return selectListItems;
        }

        /// <summary>
        /// Метод извличащ настройки за наказанията
        /// </summary>
        /// <param name="sentenceTypeId">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<SentenceTypeHasVM> GetSentenceTypeHas(int sentenceTypeId)
        {
            return await repo.AllReadonly<SentenceType>()
                             .Where(x => x.Id == sentenceTypeId)
                             .Select(x => new SentenceTypeHasVM()
                             {
                                 HasMoney = x.HasMoney ?? false,
                                 HasPeriod = x.HasPeriod ?? false,
                                 HasPreliminaryDetention = x.HasPreliminaryDetention ?? false,
                                 HasProbation = x.HasProbation ?? false,
                                 IsEffective = x.IsEffective ?? false
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Извличане на данни за падащ списък за форма на вината за престъпление
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_FormGuilt()
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Умишлено", Value = "1" });
            selectListItems.Add(new SelectListItem() { Text = "Непредпазливо", Value = "2" });
            selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();
            return selectListItems;
        }

        /// <summary>
        /// Извличане на данни за падащ списък занаселени места
        /// </summary>
        /// <param name="countryId">Идентификатор на държава</param>
        /// <param name="addDefaultElement">Флаг дали да се добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_EkatteSobr(int? countryId, bool addDefaultElement = true)
        {
            var selectListItems = new List<SelectListItem>();

            if (countryId == NomenclatureConstants.CountryBGID)
            {
                selectListItems = await repo.AllReadonly<EkEkatte>()
                                            .OrderBy(x => x.Name)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = $"{x.TVM} {x.Name} ({x.District.Name})",
                                                Value = x.Id.ToString()
                                            })
                                            .ToListAsync();
            }

            if (addDefaultElement)
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();

            return selectListItems;
        }

        /// <summary>
        /// Извличане на данни за падащ списък за населени места от ЕИСПП
        /// </summary>
        /// <param name="countryId">Идентификатор на държава</param>
        /// <param name="addDefaultElement">Флаг дали да се добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_EisppEkatte(int? countryId, bool addDefaultElement = true)
        {
            var selectListItems = new List<SelectListItem>();

            if (countryId == NomenclatureConstants.CountryBGID)
            {
                selectListItems = await repo.AllReadonly<EisppEktteCode>()
                                            .Where(x => !string.IsNullOrEmpty(x.Name))
                                            .OrderBy(x => x.Name)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = (!string.IsNullOrEmpty(x.TypeNM) ? x.TypeNM + " " : string.Empty) + x.Name + (!string.IsNullOrEmpty(x.Rajon) ? " (" + x.Rajon + ")" : string.Empty),
                                                Value = x.Id.ToString()
                                            })
                                            .ToListAsync();
            }

            if (addDefaultElement)
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();

            return selectListItems;
        }

        /// <summary>
        /// Извличане на данни за падащ списък за WorkNotificationType
        /// </summary>
        /// <param name="expressionWhere">Where клауза</param>
        /// <param name="addDefaultElement">Флаг дали да се добави елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_WorkNotificationType(Expression<Func<WorkNotificationType, bool>> expressionWhere = null, bool addDefaultElement = true)
        {
            List<SelectListItem> selectListItems = await repo.AllReadonly<WorkNotificationType>()
                                                             .Where(x => x.IsActive)
                                                             .Where(expressionWhere ?? (x => true))
                                                             .OrderBy(x => x.OrderNumber)
                                                             .Select(x => new SelectListItem()
                                                             {
                                                                 Text = x.Label,
                                                                 Value = x.Id.ToString()
                                                             })
                                                             .ToListAsync();

            if (addDefaultElement)
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDDL_MongoFileTypes(string fileGroup)
        {
            if (string.IsNullOrEmpty(fileGroup))
            {
                return new List<SelectListItem>();
            }
            return await repo.AllReadonly<MongoFileTypeGrouping>()
                                .Where(x => x.TypeGroup == fileGroup)
                                .Where(x => x.MongoFileType.IsActive == true)
                                .OrderBy(x => x.MongoFileType.OrderNumber)
                                .Select(x => new SelectListItem
                                {
                                    Text = x.MongoFileType.Label,
                                    Value = x.MongoFileTypeId.ToString()
                                }).ToListAsync();
        }

        /// <summary>
        /// Метод връщащ списък с основания към резултати за среща за медиация
        /// </summary>
        /// <param name="mediationResultId">Идентификатор на избраният резултат</param>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_MediationResultBase(int mediationResultId, bool addDefaultElement = true)
        {
            DateTime dateNow = DateTime.Now;

            IQueryable<MediationResult> queryMR = repo.AllReadonly<MediationResult>().Where(r => r.Id == mediationResultId);
            Expression<Func<MediationResultBase, bool>> mediationResultIdWhere = x => x.MediationResultGroupId == queryMR.Select(r => r.MediationResultGroupId).FirstOrDefault();
            Expression<Func<MediationResultBase, bool>> dateWhere = x => x.DateStart <= dateNow && (x.DateEnd ?? dateNow) >= dateNow && x.IsActive;


            List<SelectListItem> selectListItems = await repo.AllReadonly<MediationResultBase>()
                                                             .Where(mediationResultIdWhere)
                                                             .Where(dateWhere)
                                                             .Select(x => new SelectListItem()
                                                             {
                                                                 Text = x.Label,
                                                                 Value = x.Id.ToString()
                                                             })
                                                             .OrderBy(x => x.Text)
                                                             .ToListAsync();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Метод извличащ точки за оценка на медиатор в среща за бедиация
        /// </summary>
        /// <param name="typePoint">Тип оценка от NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants</param>
        /// <returns></returns>
        public async Task<List<RatingVM>> GetMediationPointMediatorAppraisals(int typePoint)
        {
            return await repo.AllReadonly<MediationPointMediatorAppraisal>()
                             .Where(x => x.IsActive)
                             .Where(x => x.TypePoint == typePoint)
                             .OrderBy(x => x.OrderNumber)
                             .Select(x => new RatingVM
                             {
                                 Id = x.Id,
                                 IsViewValue = !x.WithoutAppraisal,
                                 Label = x.Label,
                                 Value = 0,
                                 IsSmall = x.TypePoint == NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Summary ? true : false,
                             })
                             .ToListAsync();
        }

        #region Подкодове на шифри

        /// <summary>
        /// Извличане на данни за подкодове на шифри
        /// </summary>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        public IQueryable<CaseCodeSubListDataVM> GetCaseCodeSubs(CaseCodeSubFilterVM filter)
        {
            Expression<Func<CaseCodeSub, bool>> caseCodeIdWhere = x => true;
            if ((filter.CaseCodeId ?? 0) > 0)
                caseCodeIdWhere = x => x.CaseCodeId == filter.CaseCodeId;

            return repo.AllReadonly<CaseCodeSub>()
                       .Where(caseCodeIdWhere)
                       .Select(x => new CaseCodeSubListDataVM()
                       {
                           Id = x.Id,
                           CaseCodeLabel = x.CaseCode.Code + " " + x.CaseCode.Label,
                           Code = x.Code,
                           Label = x.Label,
                           IsActiveText = x.IsActive ? MessageConstant.Yes : MessageConstant.No,
                           DateFrom = x.DateStart,
                           DateTo = x.DateEnd
                       });
        }

        /// <summary>
        /// Извличане на данни за редакция на подкодове на шифри
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        public async Task<CaseCodeSubVM> GetCaseCodeSubEditById(int id)
        {
            return await repo.AllReadonly<CaseCodeSub>()
                             .Where(x => x.Id == id)
                             .Select(x => new CaseCodeSubVM()
                             {
                                 Id = x.Id,
                                 CaseCodeId = x.CaseCodeId,
                                 OrderNumber = x.OrderNumber,
                                 Code = x.Code,
                                 Label = x.Label,
                                 IsActive = x.IsActive,
                                 Description = x.Description,
                                 DateFrom = x.DateStart,
                                 DateTo = x.DateEnd,
                             })
                             .FirstAsync();
        }

        /// <summary>
        /// Попълване на обект за добавяне на подкодове на шифри
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        private CaseCodeSub FillCaseCodeSub(CaseCodeSubVM model)
        {
            return new()
            {
                CaseCodeId = model.CaseCodeId.NumberEmptyToNull(),
                OrderNumber = model.OrderNumber,
                Code = model.Code,
                Label = model.Label,
                IsActive = model.IsActive,
                Description = model.Description,
                DateStart = model.DateFrom,
                DateEnd = model.DateTo,
            };
        }

        /// <summary>
        /// Попълване на данни за редакция на подкодове на шифри
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <param name="modelSave">Модел за редакция</param>
        private static void SetEditFieldsCaseCodeSub(CaseCodeSubVM model, CaseCodeSub modelSave)
        {
            modelSave.CaseCodeId = model.CaseCodeId.NumberEmptyToNull();
            modelSave.OrderNumber = model.OrderNumber;
            modelSave.Code = model.Code;
            modelSave.Label = model.Label;
            modelSave.IsActive = model.IsActive;
            modelSave.Description = model.Description;
            modelSave.DateStart = model.DateFrom;
            modelSave.DateEnd = model.DateTo;
        }

        /// <summary>
        /// Добавяне/редкация на данни за подкодове на шифри
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<int?> SaveCaseCodeSub(CaseCodeSubVM model)
        {
            try
            {
                CaseCodeSub modelSave = (model.Id > 0) ? await repo.All<CaseCodeSub>()
                                                                   .Where(x => x.Id == model.Id)
                                                                   .FirstAsync() : FillCaseCodeSub(model);

                if (model.Id > 0)
                    SetEditFieldsCaseCodeSub(model, modelSave);
                else
                    repo.Add(modelSave);

                await repo.SaveChangesAsync();
                return modelSave.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на подкодове на шифри id = {model.Id}");
                return null;
            }
        }

        /// <summary>
        /// Метод извличащ шифри по дело за падащ списък за избор
        /// </summary>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_CaseCode(bool addDefaultElement = true)
        {
            DateTime dateNow = DateTime.Now;

            List<SelectListItem> selectListItems = await repo.AllReadonly<CaseCode>()
                                                             .Where(x => x.IsActive)
                                                             .Where(x => x.DateStart <= dateNow)
                                                             .Where(x => (x.DateEnd ?? dateNow) >= dateNow)
                                                             .OrderBy(x => x.Label)
                                                             .Select(x => new SelectListItem
                                                             {
                                                                 Text = $"{x.Code} {x.Label}",
                                                                 Value = x.Id.ToString()
                                                             })
                                                             .ToListAsync();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Метод извличащ подшифри по дело за падащ списък за избор
        /// </summary>
        /// <param name="addDefaultElement">Флаг за добавяне на елемент "Избери"</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDDL_CaseCodeSub(bool addDefaultElement = true)
        {
            DateTime dateNow = DateTime.Now;

            List<SelectListItem> selectListItems = await repo.AllReadonly<CaseCodeSub>()
                                                             .Where(x => x.IsActive)
                                                             .Where(x => x.DateStart <= dateNow)
                                                             .Where(x => (x.DateEnd ?? dateNow) >= dateNow)
                                                             .Select(x => new SelectListItem
                                                             {
                                                                 Text = $"{x.Code} {x.Label}",
                                                                 Value = x.Id.ToString()
                                                             })
                                                             .ToListAsync();

            selectListItems = selectListItems.OrderBy(x => x.Text).ToList();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        #endregion

        public List<SelectListItem> GetDDL_ExcelReportTemplateReportType(bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = "Статистика", Value = NomenclatureConstants.ExcelReportTemplateReportTypes.Normal.ToString() });
            selectListItems.Add(new SelectListItem() { Text = "Медиация статистика", Value = NomenclatureConstants.ExcelReportTemplateReportTypes.Mediation.ToString() });

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public async Task<List<SelectListItem>> GetDLL_ActIspnReasonByGroup(int group, bool addDefaultElement = true, bool addAllElement = false, bool orderByNumber = true)
        {

            return await repo.AllReadonly<ActISPNReasonGrouping>()
                                .Where(x => x.ActISPNReasonGroup == group)
                                .Where(x => x.ActISPNReason.IsActive)
                                .Select(x => x.ActISPNReason)
                                .ToSelectListAsync(addDefaultElement, addAllElement, orderByNumber);
        }
    }
}
