using AutoMapper;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using iText.Kernel.XMP.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;
using CaseSessionResult = IOWebApplication.Infrastructure.Data.Models.Cases.CaseSessionResult;

namespace IOWebApplication.Core.Services
{
    public class BaseService : IBaseService
    {
        protected ILogger logger { get; set; }
        protected IRepository repo { get; set; }
        protected IUserContext userContext { get; set; }
        protected IMapper mapper { get; set; }

        public bool ChangeOrder<T>(object id, bool moveUp, Func<T, int?> orderProp, Expression<Func<T, int?>> setterProp, Expression<Func<T, bool>> predicate = null) where T : class
        {
            var DbSet = repo.All<T>();
            var current = repo.GetById<T>(id);

            try
            {
                T next;
                IQueryable<T> items = DbSet;
                if (predicate != null)
                {
                    items = DbSet.Where(predicate);
                }
                if (moveUp)
                {
                    next = items.AsEnumerable().Where(x => orderProp(x) < orderProp(current)).OrderByDescending(x => orderProp(x)).FirstOrDefault();
                }
                else
                {
                    next = items.AsEnumerable().Where(x => orderProp(x) > orderProp(current)).OrderBy(x => orderProp(x)).FirstOrDefault();
                }
                if (next != null)
                {
                    int? middleValue = orderProp(current);
                    current.SetPropertyValue<T, int?>(setterProp, orderProp(next));
                    next.SetPropertyValue<T, int?>(setterProp, middleValue.Value);
                    repo.Update(current);
                    repo.Update(next);
                    repo.SaveChanges();

                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        public T GetById<T>(object id) where T : class
        {
            return repo.GetById<T>(id);
        }

        protected void PersonNamesBase_SaveData(PersonNamesBase model)
        {
            switch (model.UicTypeId)
            {
                case NomenclatureConstants.UicTypes.EGN:
                case NomenclatureConstants.UicTypes.LNCh:
                case NomenclatureConstants.UicTypes.BirthDate:
                    model.FullName = model.MakeFullName();
                    break;
            }
            if (string.IsNullOrEmpty(model.Uic) || (model.UicTypeId == NomenclatureConstants.UicTypes.BirthDate))
            {
                //if (model.PersonId > 0)
                //{
                //    //Ако е избрано лице се премахва foreignkey-а за да се редактира първоначалния запис

                //    //Ако има намерен резултат за лице
                //    //var savedPerson = repo.GetById<Person>(model.PersonId);
                //    //savedPerson.CopyFrom(model);
                //    //Person_SaveData(savedPerson);

                //}

                model.Person = null;
                model.PersonId = null;
            }
            else
            {
                //търсене по UIC
                var savedPerson = repo.All<Person>(x => x.Uic == model.Uic && x.UicTypeId == model.UicTypeId).FirstOrDefault();
                if (savedPerson != null)
                {
                    model.PersonId = savedPerson.Id;
                    savedPerson.CopyFrom(model);
                    repo.Update<Person>(savedPerson);
                    //Person_SaveData(savedPerson);
                }
                else
                {
                    model.PersonId = null;
                    model.Person = new Person();
                    model.Person.ActualtoDate = DateTime.Now;
                    model.Person.CopyFrom(model);
                }
            }
        }

        protected void PersonNamesBase_UpdatePerson(PersonNamesBase model, bool saveToDatabase = true)
        {
            if (model.Person != null && model.Person.Id > 0)
            {
                model.PersonId = model.Person.Id;
            }
            if (saveToDatabase)
            {
                repo.SaveChanges();
            }
        }


        protected THistory CreateHistory<TActive, THistory>(TActive source)
            where THistory : class, IHistory
            where TActive : IHaveHistory<THistory>
        {

            THistory result = mapper.Map<TActive, THistory>(source);

            ExpireHistory<THistory>(result);
            source.History = source.History ?? new HashSet<THistory>();
            source.History.Add(result);

            return result;
        }

        private void ExpireHistory<T>(T lastVersion) where T : class, IHistory
        {
            var priorHistories = repo.All<T>().Where(x => x.Id == lastVersion.Id && x.HistoryDateExpire == null);
            if (priorHistories != null)
            {
                foreach (var item in priorHistories)
                {
                    item.HistoryDateExpire = lastVersion.DateWrt.AddSeconds(-1);
                }
            }
        }

        public string GetUserIdByLawUnitId(int lawUnitId)
        {
            return repo.AllReadonly<ApplicationUser>().Where(x => x.LawUnitId == lawUnitId && x.IsActive).Select(x => x.Id).FirstOrDefault();
        }

        #region Проверка за право на достъп до обекти

        public bool SaveExpireInfo<T>(ExpiredInfoVM model) where T : class, IExpiredInfo
        {
            T saved;
            if (model.LongId > 0)
            {
                saved = repo.GetById<T>(model.LongId);
            }
            else
            {
                saved = repo.GetById<T>(model.Id);
            }

            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = userContext.UserId;
                saved.DescriptionExpired = model.DescriptionExpired;
                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public Expression<Func<T, bool>> FilterExpireInfo<T>(bool showExpired) where T : class, IExpiredInfo
        {
            Expression<Func<T, bool>> result = x => x.DateExpired == null;
            if (showExpired)
            {
                result = x => true;
            }
            return result;
        }

        #endregion

        protected IEnumerable<CourtLawUnit> GetActual_CourtLawUnitsByDate(int courtId, int lawUnitTypeId, DateTime? date)
        {
            DateTime dateActualTo = (date ?? DateTime.Now);

            var dateActualToEndDate = dateActualTo.AddDays(1);
            var result = repo.AllReadonly<CourtLawUnit>()
                                 .Include(x => x.LawUnit)
                                 .Where(x => x.CourtId == courtId)
                                 .Where(x => x.LawUnit.LawUnitTypeId == lawUnitTypeId)
                                 .Where(x => x.DateFrom <= dateActualTo && (x.DateTo ?? dateActualToEndDate) >= dateActualTo)
                                 .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(x.PeriodTypeId))
                                 .Distinct().ToList();
            return result;

        }

        protected void SetUserDateWRT(IUserDateWRT model)
        {
            model.UserId = userContext.UserId;
            model.DateWrt = DateTime.Now;
        }

        public CurrentContextModel GetCurrentContext(int sourceType, long? sourceId, string operation = "", object parentId = null)
        {
            CurrentContextModel model = new CurrentContextModel(sourceType, sourceId, operation);

            model.Info.CourtId = userContext.CourtId;
            model.Info.UserId = userContext.UserId;
            //return model;
            //TODO

            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    {
                        setAccessRightsForDocument(model, sourceId);
                    }
                    break;
                case SourceTypeSelectVM.DocumentResolution:
                    {
                        if (sourceId > 0)
                        {
                            var info = repo.AllReadonly<DocumentResolution>().Where(x => x.Id == sourceId.Value).FirstOrDefault();
                            setAccessRightsForDocument(model, info.DocumentId);
                        }
                        else
                        {
                            setAccessRightsForDocument(model, null);
                        }
                    }
                    break;
                case SourceTypeSelectVM.DocumentDecision:
                    {
                        setAccessRightsForDocument(model, null);
                    }
                    break;
                case SourceTypeSelectVM.DocumentObligation:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetObligationDocument((int)sourceId);
                            setAccessRightsForDocument(model, info.DocumentId);
                        }
                        else
                        {
                            //parentId  е Id на документ
                            setAccessRightsForDocument(model, parentId);
                        }
                    }
                    break;
                case SourceTypeSelectVM.Case:
                    {
                        setAccessRightsForCase(model, sourceId);
                    }
                    break;
                case SourceTypeSelectVM.CasePersonBulletin:
                    {
                        setAccessRightsForCase(model, null);
                    }
                    break;
                case SourceTypeSelectVM.CaseSelectionProtokol:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSelectionProtokol((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            setAccessRightsForCase(model, parentId, "Регистрация на протокол");
                        }
                        if (operation == AuditConstants.Operations.Append)
                        {
                            model.CanAccess = userContext.IsUserInRole(AccountConstants.Roles.CaseInit);
                        }
                        model.CanChange &= userContext.IsUserInRole(AccountConstants.Roles.CaseInit);
                        model.CanChangeFull = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
                    }
                    break;
                case SourceTypeSelectVM.CasePerson:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCasePerson((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, parentId, "Регистрация на страна към дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionPerson:
                    {
                        //parentId е Id на заседание
                        var info = caseInfo_GetCasePersonForSession((int)parentId);
                        setAccessRightsForCase(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CasePersonAddress:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCasePersonAddress((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на страна по дело
                            var info = caseInfo_GetCasePerson((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonLink:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCasePersonLink((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на връзки по дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSession:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSession((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, parentId, "Регистрация на заседание");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionAct:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionAct((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = caseInfo_GetCaseSession((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActCoordination:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionAct((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, $"Съгласуване на акт {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseNotification((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, (int)parentId);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotification:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseNotification((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = caseInfo_GetCaseSession((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActNotification:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseNotification((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = caseInfo_GetCaseSessionAct((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseEvidence:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseEvidence((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на доказателство");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseEvidenceMovement:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseEvidenceMovement((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на доказателство
                            var info = caseInfo_GetCaseEvidence((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseMovement:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseMovement((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на местоположение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLoadIndex:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseLoadIndex((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на натовареност към дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLoadCorrection:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseLoadCorrection((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на коригиращи коефициенти по дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseCrime:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseCrime((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на престъпление");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonCrime:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCasePersonCrime((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на престъпление
                            var info = caseInfo_GetCaseCrime((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonMeasure:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCasePersonMeasure((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            var info = caseInfo_GetCasePerson((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonDocument:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCasePersonDocument((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            var info = caseInfo_GetCasePerson((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonSentence:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetPersonSentence((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            var info = caseInfo_GetCasePerson((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonSentencePunishment:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetPersonSentencePunishment((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на присъда
                            var info = caseInfo_GetPersonSentence((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonSentencePunishmentCrime:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetPersonSentencePunishmentCrime((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на наказание
                            var info = caseInfo_GetPersonSentencePunishment((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.DocumentTemplate:
                    {
                        // Да го коментираме
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetDocumentTemplate((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на изходящ документ");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLifecycle:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseLifecycle((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на интервал по дело");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CasePersonInheritance:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetPersonInheritance((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на лице в дело
                            var info = caseInfo_GetCasePerson((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLawUnit:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseLawUnit((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на дело
                            setAccessRightsForCase(model, (int)parentId);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLawUnitDismisal:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseLawUnitDismisal((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на член от състава
                            var info = caseInfo_GetCaseLawUnit((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseLawUnitDismisalList:
                    {
                        //parentId  е Id на дело
                        setAccessRightsForCase(model, (int)parentId);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionResult:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionResult((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = caseInfo_GetCaseSession((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionMeeting:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionMeeting((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = caseInfo_GetCaseSession((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionMeetingUser:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionMeetingUser((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на сесия
                            var info = caseInfo_GetCaseSessionMeeting((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Сесия: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActLawBase:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionActLawBase((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = caseInfo_GetCaseSessionAct((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActDivorce:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionActDivorce((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = caseInfo_GetCaseSessionAct((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActCompany:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionActCompany((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = caseInfo_GetCaseSessionAct((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActComplain:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionActComplain((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = caseInfo_GetCaseSessionAct((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.SessionActObligation:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetObligation((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на акт
                            var info = caseInfo_GetCaseSessionAct((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Акт: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.SessionObligation:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetObligation((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = caseInfo_GetCaseSession((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionFastDocument:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionFastDocument((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId  е Id на заседание
                            var info = caseInfo_GetCaseSession((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Заседание: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActComplainResult:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionActComplainResult((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на обжалване
                            var info = caseInfo_GetCaseSessionActComplain((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Обжалване: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionActComplainPerson:
                    {
                        //parentId е Id на обжалване - само това се подава
                        var info = caseInfo_GetCaseSessionActComplainPerson((int)parentId);
                        setAccessRightsForCase(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseMigration:
                    {
                        if (parentId != null && sourceId != null)
                        {
                            var info = caseInfo_GetCaseMigration((int)sourceId);
                            setAccessRightsForCase(model, (int)parentId, info.Info);
                        }
                        else
                        {
                            if (parentId == null)
                            {
                                parentId = repo.AllReadonly<CaseMigration>().Where(x => x.Id == (int)sourceId).Select(x => x.CaseId).FirstOrDefault();
                            }

                            //parentId е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на движение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseDeadLine:
                    {
                        //parentId е Id на дело
                        setAccessRightsForCase(model, (int)parentId, "Срокове");
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionLawUnit:
                    {
                        //parentId е Id на заседание
                        var info = caseInfo_GetCaseSessionLawUnit((int)parentId);
                        setAccessRightsForCase(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotificationList:
                    {
                        // тук е само редакция
                        var info = caseInfo_GetCaseSessionNotificationList((int)sourceId);
                        setAccessRightsForCase(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotificationListLawUnit:
                    {
                        //parentId е Id на заседание
                        var info = caseInfo_GetCaseSessionNotificationList((int)parentId, NomenclatureConstants.NotificationPersonType.CaseLawUnit);
                        setAccessRightsForCase(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotificationListPerson:
                    {
                        //parentId е Id на заседание
                        var info = caseInfo_GetCaseSessionNotificationList((int)parentId, NomenclatureConstants.NotificationPersonType.CasePerson);
                        setAccessRightsForCase(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionNotificationListPersonLawUnit:
                    {
                        //parentId е Id на заседание
                        var info = caseInfo_GetCaseSessionNotificationList((int)parentId, 0);
                        setAccessRightsForCase(model, info.CaseId, info.Info);
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionDoc:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseSessionDocById((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на заседание
                            var info = caseInfo_GetCaseSessionDoc((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseFastProcess:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseFastProcess((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Заповедно производство");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseBankAccount:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseBankAccount((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на движение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseMoneyClaim:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseMoneyClaim((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на движение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseMoneyExpense:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseMoneyExpense((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на дело
                            setAccessRightsForCase(model, (int)parentId, "Регистрация на движение");
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseMoneyCollection:
                    {
                        if (sourceId != null)
                        {
                            var info = caseInfo_GetCaseMoneyCollection((int)sourceId);
                            setAccessRightsForCase(model, info.CaseId, info.Info);
                        }
                        else
                        {
                            //parentId е Id на Обстоятелства по заповедни производства
                            var info = caseInfo_GetCaseMoneyClaim((int)parentId);
                            setAccessRightsForCase(model, info.CaseId, $"Обстоятелствo: {info.Info}");
                        }
                    }
                    break;
                case SourceTypeSelectVM.ExecList:
                    {
                        setAccessRightsForMoney(model, sourceId);
                    }
                    break;
                case SourceTypeSelectVM.ExchangeDoc:
                    {
                        setAccessRightsForMoney(model, sourceId);
                    }
                    break;
                default:
                    model.IsRead = false;
                    break;
            }

            return model;
        }

        private void setAccessRightsForDocument(CurrentContextModel model, object sourceId)
        {
            model.CanAccess = userContext.IsUserInFeature(AccountConstants.Features.Modules.Documents);
            if (sourceId != null)
            {
                long documentId = (long)sourceId;
                var info = repo.AllReadonly<Document>()
                                    .Include(x => x.DocumentType)
                                    .Include(x => x.DocumentGroup)
                                    .Where(x => x.Id == documentId)
                                    .Select(x => new
                                    {
                                        Id = x.Id,
                                        CourtId = x.CourtId,
                                        DocumentKindId = x.DocumentGroup.DocumentKindId,
                                        DocumentDirectionId = x.DocumentDirectionId,
                                        DateExpired = x.DateExpired,
                                        BaseObject = $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}",
                                        IsRestrictedAccess = x.IsRestictedAccess || (x.IsSecret == true)
                                    }).FirstOrDefault();
                if (info != null)
                {
                    model.Info.BaseObject = info.BaseObject;

                    //Ако няма достъп и метода е за преглед
                    if (!model.CanAccess && model.Info.Operation == AuditConstants.Operations.View)
                    {
                        //Ако няма достъп до модул Регистратура, но има задача насочена към лицето
                        model.CanAccess = repo.AllReadonly<WorkTask>()
                                              .Where(x => x.SourceType == SourceTypeSelectVM.Document && x.SourceId == info.Id)
                                              .Where(x => x.UserId == userContext.UserId)
                                              .Select(x => x.Id)
                                              .Any();
                    }
                    if (info.IsRestrictedAccess)
                    {
                        model.CanAccess &= userContext.IsUserInRole(AccountConstants.Roles.RestrictedAccess);
                    }

                    model.CanAccess &= userContext.CourtId == info.CourtId;
                    model.CanChangeFull = info.DateExpired == null && userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
                    if (model.CanChangeFull)
                    {
                        switch (info.DocumentKindId)
                        {
                            case DocumentConstants.DocumentKind.InitialDocument:
                                //Иницииращите документи могат да се премахват до образуването на делото
                                var caseRegnumber = repo.AllReadonly<Case>(x => x.DocumentId == documentId)
                                                .Select(x => x.RegNumber)
                                                .FirstOrDefault();
                                if (!string.IsNullOrEmpty(caseRegnumber))
                                {
                                    model.CanChangeFull = false;
                                }
                                break;
                            case DocumentConstants.DocumentKind.CompliantDocument:
                                //Съпровождащите документи се премахват преди да са разгледани/окончателно разгледани в заседание
                                if (repo.AllReadonly<CaseSessionDoc>()
                                .Where(x => x.DocumentId == documentId && NomenclatureConstants.SessionDocState.UsedInSession.Contains(x.SessionDocStateId)).Any())
                                {
                                    model.CanChangeFull = false;
                                }
                                break;
                        }
                    }
                    //Изходящите документи се премахват ако не са по бланка към процес DocumentTemplate
                    //if (model.CanChangeFull && info.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing)
                    //{
                    //    if (repo.AllReadonly<DocumentTemplate>()
                    //           .Where(x => x.DocumentId == documentId).Any())
                    //    {
                    //        model.CanChangeFull = false;
                    //    }
                    //}
                    //Коригирано е деактивирането на документи да освобождава бланката
                }
            }

            model.CanChange = model.CanAccess;
            switch (model.Info.Operation)
            {
                case AuditConstants.Operations.View:
                    model.CanChange = false;
                    break;
            }
        }

        private CaseInfoVM caseInfo_GetCaseBankAccount(int id)
        {
            return repo.AllReadonly<CaseBankAccount>()
                       .Include(x => x.CaseBankAccountType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = x.CaseBankAccountType.Label + " IBAN " + x.IBAN + " BIC: " + x.BIC + " Име на банката: " + x.BankName
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseFastProcess(int id)
        {
            return repo.AllReadonly<CaseFastProcess>()
                       .Include(x => x.Case)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = "Заповедно производство по дело: " + (x.Case.RegNumber ?? string.Empty)
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseMoneyClaim(int id)
        {
            return repo.AllReadonly<CaseMoneyClaim>()
                       .Include(x => x.CaseMoneyClaimGroup)
                       .Include(x => x.CaseMoneyClaimType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = x.CaseMoneyClaimGroup.Label + " " + x.CaseMoneyClaimType.Label + " номер " + x.ClaimNumber
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseMoneyCollection(int id)
        {
            return repo.AllReadonly<CaseMoneyCollection>()
                       .Include(x => x.CaseMoneyCollectionGroup)
                       .Include(x => x.CaseMoneyCollectionType)
                       .Include(x => x.CaseMoneyCollectionKind)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = x.CaseMoneyCollectionGroup.Label +
                                  (x.CaseMoneyCollectionType != null ? " " + x.CaseMoneyCollectionType.Label : string.Empty) +
                                  (x.CaseMoneyCollectionKind != null ? " " + x.CaseMoneyCollectionKind.Label : string.Empty)
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseMoneyExpense(int id)
        {
            return repo.AllReadonly<CaseMoneyExpense>()
                       .Include(x => x.CaseMoneyExpenseType)
                       .Include(x => x.Currency)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = x.CaseMoneyExpenseType.Label + " " + x.Amount.ToString("0.00") + " " + x.Currency.Label
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionFastDocument(int id)
        {
            return repo.AllReadonly<CaseSessionFastDocument>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.SessionDocType)
                       .Include(x => x.SessionDocState)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = x.CasePerson.FullName + " " + x.SessionDocType.Label + " " + x.SessionDocState.Label
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetObligation(int id)
        {
            return repo.AllReadonly<Obligation>()
                       .Include(x => x.CaseSession)
                       .Include(x => x.CaseSessionAct)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseSession != null ? x.CaseSession.CaseId : x.CaseSessionAct.CaseId ?? 0,
                           Info = x.ObligationNumber + "/" + x.ObligationDate.ToString("dd.MM.yyyy") + " " + x.Amount.ToString("0.00")
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private DocumentInfoLogVM caseInfo_GetObligationDocument(int id)
        {
            return repo.AllReadonly<Obligation>()
                       .Where(x => x.Id == id)
                       .Select(x => new DocumentInfoLogVM()
                       {
                           DocumentId = x.DocumentId ?? 0,
                           Info = x.ObligationNumber + "/" + x.ObligationDate.ToString("dd.MM.yyyy") + " " + x.Amount.ToString("0.00")
                       }).FirstOrDefault() ?? new DocumentInfoLogVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionDoc(int CaseSessionId)
        {
            return repo.AllReadonly<CaseSession>()
                       .Include(x => x.CaseSessionDocs)
                       .ThenInclude(x => x.Document)
                       .Include(x => x.CaseSessionDocs)
                       .ThenInclude(x => x.SessionDocState)
                       .Where(x => x.Id == CaseSessionId)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = "Съпровождащи документи: " + string.Join(",", x.CaseSessionDocs.Select(a => a.Document.DocumentNumber + "/" + a.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + a.SessionDocState.Label))
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionDocById(int id)
        {
            return repo.AllReadonly<CaseSessionDoc>()
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = "Съпровождащ документ: " + x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy") + " " + x.SessionDocState.Label
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseMigration(int id)
        {
            return repo.AllReadonly<CaseMigration>()
                       .Include(x => x.CaseMigrationType)
                       .Include(x => x.Case)
                       .ThenInclude(x => x.Court)
                       .Include(x => x.PriorCase)
                       .ThenInclude(x => x.Court)
                       .Include(x => x.SendToCourt)
                       .Include(x => x.SendToInstitution)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = x.CaseMigrationType.Label + " изпратено от " + ((x.CaseMigrationType.MigrationDirection == CaseMigrationDirections.Outgoing) ? x.Case.Court.Label : x.PriorCase.Court.Label) +
                                                              " изпратено към " + ((x.SendToCourt != null) ? x.SendToCourt.Label : (x.SendToInstitution != null ? x.SendToInstitution.FullName : ""))
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionNotificationList(int id)
        {
            return repo.AllReadonly<CaseSessionNotificationList>()
                       .Include(x => x.CaseLawUnit)
                       .ThenInclude(x => x.LawUnit)
                       .Include(x => x.CaseLawUnit)
                       .ThenInclude(x => x.JudgeRole)
                       .Include(x => x.CasePerson)
                       .ThenInclude(x => x.PersonRole)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = (x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CaseLawUnit) ? x.CaseLawUnit.LawUnit.FullName + " " + x.CaseLawUnit.JudgeRole.Label :
                                                                                                                           x.CasePerson.FullName + " " + x.CasePerson.PersonRole.Label
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionNotificationList(int CaseSessionId, int NotificationPersonType)
        {
            var caseSessionNotificationLists = repo.AllReadonly<CaseSessionNotificationList>()
                                                   .Include(x => x.CaseLawUnit)
                                                   .ThenInclude(x => x.LawUnit)
                                                   .Include(x => x.CaseLawUnit)
                                                   .ThenInclude(x => x.JudgeRole)
                                                   .Include(x => x.CasePerson)
                                                   .ThenInclude(x => x.PersonRole)
                                                   .Where(x => (x.CaseSessionId == CaseSessionId) &&
                                                               (x.DateExpired == null) &&
                                                               (NotificationPersonType > 0 ? x.NotificationPersonType == NotificationPersonType : true))
                                                   .ToList();

            var result = new CaseInfoVM()
            {
                CaseId = ((caseSessionNotificationLists.Count > 0) ? (caseSessionNotificationLists.FirstOrDefault()).CaseId ?? 0 : repo.GetById<CaseSession>(CaseSessionId).CaseId),
                Info = string.Join(",", caseSessionNotificationLists.Select(a => a.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CaseLawUnit ? a.CaseLawUnit.LawUnit.FullName + " " + a.CaseLawUnit.JudgeRole.Label : a.CasePerson.FullName + " " + a.CasePerson.PersonRole.Label))
            };

            return result;
        }

        private CaseInfoVM caseInfo_GetCaseSessionLawUnit(int CaseSessionId)
        {
            return repo.AllReadonly<CaseSession>()
                       .Include(x => x.CaseLawUnits)
                       .ThenInclude(x => x.LawUnit)
                       .Include(x => x.CaseLawUnits)
                       .ThenInclude(x => x.JudgeRole)
                       .Where(x => x.Id == CaseSessionId)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = "Състав: " + string.Join(",", x.CaseLawUnits.Select(a => a.LawUnit.FullName + " " + a.JudgeRole.Label))
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionActComplainPerson(int CaseSessionActComplainId)
        {
            return repo.AllReadonly<CaseSessionActComplain>()
                       .Include(x => x.CasePersons)
                       .ThenInclude(x => x.CasePerson)
                       .Where(x => x.Id == CaseSessionActComplainId)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = "Жалбоподатели: " + string.Join(",", x.CasePersons.Select(a => a.CasePerson.FullName))
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionActComplainResult(int id)
        {
            return repo.AllReadonly<CaseSessionActComplainResult>()
                       .Include(x => x.CaseSessionAct)
                       .ThenInclude(x => x.ActType)
                       .Include(x => x.ActResult)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = "Резултат от обжалване: " + x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.RegNumber + "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") + " - " + x.ActResult.Label
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionActComplain(int id)
        {
            return repo.AllReadonly<CaseSessionActComplain>()
                       .Include(x => x.ComplainDocument)
                       .ThenInclude(x => x.DocumentType)
                       .Include(x => x.ComplainState)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = "Обжалване по съпровождащ документ: " + x.ComplainDocument.DocumentType.Label + " " + x.ComplainDocument.DocumentNumber + "/" + x.ComplainDocument.DocumentDate.ToString("dd.MM.yyyy")
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionActLawBase(int id)
        {
            return repo.AllReadonly<CaseSessionActLawBase>()
                       .Include(x => x.LawBase)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = "Нормативен текст: " + x.LawBase.Label
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionMeetingUser(int id)
        {
            return repo.AllReadonly<CaseSessionMeetingUser>()
                       .Include(x => x.SecretaryUser)
                       .ThenInclude(x => x.LawUnit)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = "Секретар към сесия: " + x.SecretaryUser.LawUnit.FullName
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionMeeting(int id)
        {
            return repo.AllReadonly<CaseSessionMeeting>()
                       .Include(x => x.SessionMeetingType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = x.SessionMeetingType.Label + " от: " + x.DateFrom.ToString("dd.MM.yyyy") + " до: " + x.DateTo.ToString("dd.MM.yyyy")
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionResult(int id)
        {
            return repo.AllReadonly<CaseSessionResult>()
                       .Include(x => x.SessionResult)
                       .Include(x => x.SessionResultBase)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = x.SessionResult.Label + (x.SessionResultBase != null ? " - " + x.SessionResultBase.Label : string.Empty)
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseLawUnit(int id)
        {
            return repo.AllReadonly<CaseLawUnit>()
                       .Include(x => x.LawUnit)
                       .Include(x => x.JudgeRole)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.LawUnit.FullName} {x.JudgeRole.Label}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseLawUnitDismisal(int id)
        {
            return repo.AllReadonly<CaseLawUnitDismisal>()
                       .Include(x => x.DismisalType)
                       .Include(x => x.CaseLawUnit)
                       .ThenInclude(x => x.LawUnit)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseLawUnit.CaseId,
                           Info = $"{x.CaseLawUnit.LawUnit.FullName} {x.DismisalType.Label} {x.DismisalDate:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionActDivorce(int id)
        {
            return repo.AllReadonly<CaseSessionActDivorce>()
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = $"Съобщение за прекратяване на граждански брак {x.RegNumber} от {x.RegDate:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseLifecycle(int id)
        {
            return repo.AllReadonly<CaseLifecycle>()
                       .Include(x => x.LifecycleType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"Интервал по дело: {x.LifecycleType.Label} повторение {x.Iteration} от {x.DateFrom:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetDocumentTemplate(int id)
        {
            return repo.AllReadonly<DocumentTemplate>()
                       .Include(x => x.DocumentKind)
                       .Include(x => x.DocumentGroup)
                       .Include(x => x.DocumentType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = $"Изх. документ към дело: {x.DocumentKind.Label} {x.DocumentGroup.Label} {x.DocumentType.Label}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetPersonInheritance(int id)
        {
            return repo.AllReadonly<CasePersonInheritance>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.Court)
                       .Include(x => x.CaseSessionAct)
                       .Include(x => x.CasePersonInheritanceResult)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"Наследство на {x.CasePerson.FullName} Постановена от {x.Court.Label} Акт: {x.CaseSessionAct.RegNumber} {x.CaseSessionAct.RegDate:dd.MM.yyyy HH mm} - {x.CasePersonInheritanceResult.Label}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCasePersonMeasure(int id)
        {
            return repo.AllReadonly<CasePersonMeasure>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.MeasureInstitution)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = "Мярка към: " + x.CasePerson.FullName + " институция, определила мярката: " + x.MeasureInstitution.FullName + " вид мярка: " + x.MeasureTypeLabel
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCasePersonDocument(int id)
        {
            return repo.AllReadonly<CasePersonDocument>()
                       .Include(x => x.CasePerson)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = "Личен документ на: " + x.CasePerson.FullName + " държава: " + x.IssuerCountryName + " документ: " + x.PersonalDocumentTypeLabel + " номер: " + x.DocumentNumber
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetPersonSentence(int id)
        {
            return repo.AllReadonly<CasePersonSentence>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.Court)
                       .Include(x => x.CaseSessionAct)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.CasePerson.FullName} Постановена от {x.Court.Label} Акт: {x.CaseSessionAct.RegNumber} {x.CaseSessionAct.RegDate:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetPersonSentencePunishment(int id)
        {
            return repo.AllReadonly<CasePersonSentencePunishment>()
                       .Include(x => x.SentenceType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = $"Наложено наказание по НК: {x.SentenceType.Label} Сумарен ред за присъди: {(x.IsSummaryPunishment ? "Да" : "Не")}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetPersonSentencePunishmentCrime(int id)
        {
            return repo.AllReadonly<CasePersonSentencePunishmentCrime>()
                       .Include(x => x.CaseCrime)
                       .Include(x => x.PersonRoleInCrime)
                       .Include(x => x.RecidiveType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = $"Участие в наложени наказания към присъда. Престъпление: {x.CaseCrime.CrimeName} роля: {x.PersonRoleInCrime.Label} рецидив: {x.RecidiveType.Label}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseCrime(int id)
        {
            return repo.AllReadonly<CaseCrime>()
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.EISSPNumber} {x.CrimeName}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCasePersonCrime(int id)
        {
            return repo.AllReadonly<CasePersonCrime>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.PersonRoleInCrime)
                       .Include(x => x.RecidiveType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.CasePerson.FullName} {x.PersonRoleInCrime.Label} {x.RecidiveType.Label}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseLoadCorrection(int id)
        {
            return repo.AllReadonly<CaseLoadCorrection>()
                       .Include(x => x.CaseLoadCorrectionActivity)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.CaseLoadCorrectionActivity.Label} {x.CorrectionDate:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseLoadIndex(int id)
        {
            return repo.AllReadonly<CaseLoadIndex>()
                       .Include(x => x.LawUnit)
                       .Include(x => x.CaseLoadElementGroup)
                       .Include(x => x.CaseLoadElementType)
                       .Include(x => x.CaseLoadAddActivity)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = x.IsMainActivity ? $"Основна дейност {x.LawUnit.FullName} {x.CaseLoadElementGroup.Label} {x.CaseLoadElementType.Label} {x.DateActivity:dd.MM.yyyy HH mm}" : $"Допълнителна дейност {x.LawUnit.FullName} {x.CaseLoadAddActivity.Label} {x.DateActivity:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseMovement(int id)
        {
            return repo.AllReadonly<CaseMovement>()
                       .Include(x => x.MovementType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.MovementType.Label} {x.DateSend:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseEvidence(int id)
        {
            return repo.AllReadonly<CaseEvidence>()
                       .Include(x => x.EvidenceType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.EvidenceType.Label} {x.RegNumber}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseEvidenceMovement(int id)
        {
            return repo.AllReadonly<CaseEvidenceMovement>()
                       .Include(x => x.EvidenceMovementType)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = $"{x.EvidenceMovementType.Label} {x.MovementDate:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCasePerson(int id)
        {
            return repo.AllReadonly<CasePerson>()
                       .Include(x => x.PersonRole)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.FullName} {x.PersonRole.Label}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCasePersonLink(int id)
        {
            return repo.AllReadonly<CasePersonLink>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.CasePersonRel)
                       .Include(x => x.CasePersonSecondRel)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = x.CasePerson.FullName + (x.CasePersonRel != null ? " Упълномощено лице: " + x.CasePersonRel.FullName : string.Empty) +
                                                          (x.CasePersonSecondRel != null ? " Втори представляващ: " + x.CasePersonSecondRel.FullName : string.Empty)
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCasePersonForSession(int CaseSessionId)
        {
            return repo.AllReadonly<CaseSession>()
                       .Include(x => x.CasePersons)
                       .ThenInclude(x => x.Person)
                       .Include(x => x.CasePersons)
                       .ThenInclude(x => x.PersonRole)
                       .Where(x => x.Id == CaseSessionId)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = string.Join(",", x.CasePersons.Select(a => a.Person.FullName + " " + a.PersonRole.Label + " от " + a.DateFrom.ToString("dd.MM.yyyy") + " до " + (a.DateTo != null ? (a.DateTo ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty)))
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCasePersonAddress(int id)
        {
            return repo.AllReadonly<CasePersonAddress>()
                       .Include(x => x.CasePerson)
                       .Include(x => x.Address)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CasePerson.CaseId,
                           Info = $"{x.CasePerson.FullName} {x.Address.FullAddress}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSession(int id)
        {
            return repo.AllReadonly<CaseSession>()
                       .Include(x => x.SessionType)
                       .Include(x => x.SessionState)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.SessionState.Label} {x.SessionType.Label} {x.DateFrom:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionAct(int id)
        {
            return repo.AllReadonly<CaseSessionAct>()
                       .Include(x => x.ActType)
                       .Include(x => x.ActState)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId ?? 0,
                           Info = $"{x.ActState.Label} {x.ActType.Label}" + (x.RegDate != null ? $" {x.RegNumber}/{x.RegDate:dd.MM.yyyy HH mm}" : ""),
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseNotification(int id)
        {
            return repo.AllReadonly<CaseNotification>()
                       .Include(x => x.NotificationType)
                       .Include(x => x.NotificationState)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.NotificationType.Label} {x.NotificationState.Label}" + (x.RegDate != null ? $" {x.RegNumber}/{x.RegDate:dd.MM.yyyy}" : ""),
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSelectionProtokol(int id)
        {
            return repo.AllReadonly<CaseSelectionProtokol>()
                       .Include(x => x.SelectedLawUnit)
                       .Include(x => x.JudgeRole)
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"{x.SelectedLawUnit.FullName} ({x.JudgeRole.Label})"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private CaseInfoVM caseInfo_GetCaseSessionActCompany(int id)
        {
            return repo.AllReadonly<CaseSessionActCompany>()
                       .Where(x => x.Id == id)
                       .Select(x => new CaseInfoVM()
                       {
                           CaseId = x.CaseId,
                           Info = $"Заявление за регистрация с Акт: {x.CaseSessionAct.RegNumber} {x.CaseSessionAct.RegDate:dd.MM.yyyy HH mm}"
                       }).FirstOrDefault() ?? new CaseInfoVM();
        }

        private void setAccessRightsForCase(CurrentContextModel model, object id, string objectInfo = "")
        {
            //Да има Роля Деловодство 
            model.CanAccess = userContext.IsUserInRole(AccountConstants.Roles.DocumentEdit)
                            //или разпределител
                            || userContext.IsUserInRole(AccountConstants.Roles.CaseInit);

            if (id != null)
            {
                if (Convert.ToInt32(id) > 0)
                {
                    int caseId = Convert.ToInt32(id);

                    var info = auditInfo_Case(caseId);

                    model.Info.BaseObject = info.Info;
                    model.Info.ObjectInfo = objectInfo;
                    bool judgListCheck = info.JudgeLawUnits.Contains(userContext.LawUnitId);
                    if (info.CourtId == userContext.CourtId)
                    {
                        //Ако делото е на текущия съд, достъпа за редакция е същия като за преглед
                        //или Управление на дела но да е част от делото
                        model.CanAccess |= (userContext.IsUserInRole(AccountConstants.Roles.CaseEdit) && judgListCheck);

                        if (info.IsRestrictedAccess)
                        {
                            model.CanAccess &= userContext.IsUserInRole(AccountConstants.Roles.RestrictedAccess);
                        }

                        ////Проверява се кое е делото, в което е последното примащо движение.
                        ////Ако няма движения - това е текущото дело
                        //05.10.2020 - Премахва се заключването на редакцията при изпращане в друг съд
                        //Заключват се дела при изпращане на повече от един акт за обжалване и връщането на едното
                        //model.CanChange = info.LastInMigrationCaseId == caseId;

                        model.CanChangeFull = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);

                        //Забранява се редакцията на делата, които са в определени статуси 
                        if (NomenclatureConstants.CaseState.DisableEditStates.Contains(info.CaseStateId))
                        {
                            //TODO: GlobalAdmin - може би трябва да може да пипа
                            model.CanChange = false;
                            model.CanChangeFull = false;
                        }
                    }
                    else
                    {
                        //Ако е делото от друг съд, право на достъп имат само лицата в свързаните дела
                        model.CanAccess = info.OtherCourtsJudgeLawUnits.Contains(userContext.LawUnitId)
                            || userContext.IsUserInRole(AccountConstants.Roles.GlobalAdministrator)
                            //Ако делото има движение към или от текущия съд
                            || ((info.OtherCourts.Contains(userContext.CourtId) || info.ToCourts.Contains(userContext.CourtId)) && userContext.IsUserInFeature(AccountConstants.Features.Modules.CaseAccessData));
                        model.CanChange = false;
                        model.CanChangeFull = false;

                        if (model.Info.SourceType == SourceTypeSelectVM.CaseSessionAct && AuditConstants.Operations.ChangingOperations.Contains(model.Info.Operation))
                        {
                            //От друг съд няма право на достъп до актовете
                            model.CanAccess = false;
                        }
                    }
                }
            }
            else
            {
                model.CanAccess |= userContext.IsUserInRole(AccountConstants.Roles.CaseEdit);
            }
        }



        private CaseAuditInfoVM auditInfo_Case(int id)
        {
            var result = repo.AllReadonly<Case>()
                                    .Include(x => x.CaseType)
                                    .Include(x => x.CaseLawUnits)
                                    .Include(x => x.CaseMigrations)
                                    .Include(x => x.CaseClassifications)
                                    .Include(x => x.Document)
                                    .ThenInclude(x => x.DocumentType)
                                    .Where(x => x.Id == id)
                                    .Select(x => new CaseAuditInfoVM
                                    {
                                        LastInMigrationCaseId = id,
                                        CourtId = x.CourtId,
                                        CaseId = x.Id,
                                        CaseStateId = x.CaseStateId,
                                        Info = (string.IsNullOrEmpty(x.RegNumber)) ? $"{x.CaseType.Code} по {x.Document.DocumentType.Label} {x.Document.DocumentNumber}" : $"{x.CaseType.Code} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                                        JudgeLawUnits = x.CaseLawUnits
                                                         //Гледат се всички, независимо дали са в делото или в заседанието - заради заместванията
                                                         //.Where(c => c.CaseSessionId == null)
                                                         .Where(c => c.DateFrom <= DateTime.Now && (c.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                                         .Where(c => NomenclatureConstants.JudgeRole.JudgeAndManualRoles.Contains(c.JudgeRoleId))
                                                         .Select(c => c.LawUnitId).ToArray(),
                                        InitMigrations = x.CaseMigrations.Select(m => m.InitialCaseId).Distinct().ToArray(),
                                        OtherCourts = x.CaseMigrations.Select(m => m.CourtId ?? 0).Distinct().ToArray(),
                                        ToCourts = x.CaseMigrations.Select(m => m.SendToCourtId ?? 0).Distinct().ToArray(),
                                        IsRestrictedAccess = (x.CaseClassifications != null) ? x.CaseClassifications.Any(c => NomenclatureConstants.CaseClassifications.RestictedAccess.Contains(c.ClassificationId)) : false
                                    }).FirstOrDefault();



            if (result.InitMigrations.Length > 0)
            {
                if (result.CourtId != userContext.CourtId)
                {

                    result.OtherCourtsJudgeLawUnits = repo.AllReadonly<CaseMigration>()
                                                          .Include(x => x.Case)
                                                          .ThenInclude(x => x.CaseLawUnits)
                                                          .Where(x => result.InitMigrations.Contains(x.InitialCaseId))
                                                          .SelectMany(x => x.Case.CaseLawUnits)
                                                          .Where(c => c.CaseSessionId == null)
                                                          .Where(c => c.DateFrom <= DateTime.Now && (c.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                                          .Where(c => NomenclatureConstants.JudgeRole.JudgeAndManualRoles.Contains(c.JudgeRoleId))
                                                          .Select(c => c.LawUnitId).ToArray();
                }
                else
                {
                    result.LastInMigrationCaseId = repo.AllReadonly<CaseMigration>()
                                                 .Include(x => x.CaseMigrationType)
                                                 .Where(x => result.InitMigrations.Contains(x.InitialCaseId))
                                                 .Where(
                                                    m => m.CaseMigrationType.MigrationDirection == NomenclatureConstants.CaseMigrationDirections.Incoming
                                                    && m.OutCaseMigration.SendToCourtId > 0
                                                  )
                                                 .OrderByDescending(m => m.Id)
                                                 .Select(m => m.CaseId).DefaultIfEmpty(id).FirstOrDefault();
                }
            }
            return result ?? new CaseAuditInfoVM();
        }

        private void setAccessRightsForMoney(CurrentContextModel model, object id, string objectInfo = "")
        {
            //Да има Роля Счетоводство
            model.CanAccess = userContext.IsUserInRole(AccountConstants.Roles.MoneyAccount) ||
                                userContext.IsUserInRole(AccountConstants.Roles.DocumentEdit);
            model.CanChangeFull = userContext.IsUserInRole(AccountConstants.Roles.Supervisor);
        }

        protected IQueryable<LawUnit> SelectLawUnit_ByType(int courtId, int lawUnitType, DateTime? dtNow = null, bool currentlyAppointed = true)
        {
            int[] lawUnitTypes = new List<int>
            {
                lawUnitType
            }.ToArray();

            return SelectLawUnit_ByTypes(courtId, lawUnitTypes, dtNow, currentlyAppointed);
        }
        protected IQueryable<LawUnit> SelectLawUnit_ByTypes(int courtId, int[] lawUnitTypes, DateTime? dtNow = null, bool currentlyAppointed = true)
        {
            dtNow = dtNow ?? DateTime.Now;
            if (courtId == 0)
            {
                courtId = userContext.CourtId;
            }
            Expression<Func<LawUnit, bool>> courtSearch = x => true;
            if (courtId > 0)
            {
                courtSearch = x => x.Courts.Any() ? x.Courts.Any(c =>
                          c.CourtId == courtId
                       && NomenclatureConstants.PeriodTypes.CurrentlyAvailableExtended.Contains(c.PeriodTypeId)
                       && lawUnitTypes.Contains(c.LawUnitTypeId ?? x.LawUnitTypeId)
                       && c.DateFrom <= dtNow && (((c.DateTo ?? DateTime.MaxValue) >= dtNow) || (!currentlyAppointed))) : lawUnitTypes.Contains(x.LawUnitTypeId);
            }
            else
            {
                courtSearch = x => lawUnitTypes.Contains(x.LawUnitTypeId);
            }


            return repo.AllReadonly<LawUnit>()
                    .Include(x => x.Courts)
                    .Where(courtSearch)
                    .Where(x => x.DateFrom <= dtNow && ((x.DateTo ?? DateTime.MaxValue) >= dtNow || !currentlyAppointed))
                    .AsQueryable();
        }

        public SystemParam SystemParam_Select(string paramName)
        {
            return repo.GetById<SystemParam>(paramName);
        }

    }
}
