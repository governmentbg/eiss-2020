// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IOWebApplication.Core.Services
{
    public class CounterService : BaseService, ICounterService
    {
        public CounterService(
            ILogger<CounterService> _logger,
            IUserContext _userContext,
            IRepository _repo)
        {
            logger = _logger;
            userContext = _userContext;
            repo = _repo;
        }
        public IQueryable<CounterVM> Counter_Select(int courtId, string label)
        {
            label = label?.ToLower();
            //return repo.AllReadonly<Counter>(x => x.CourtId == courtId && x.Label.ToLower().Contains(label ?? x.Label.ToLower()))
            return repo.AllReadonly<Counter>()
                .Include(x => x.CounterType)
                .Include(x => x.ResetType)
                .Where(x => x.CourtId == courtId)
                //.OrderBy(x => x.Label)
                .Select(x => new CounterVM()
                {
                    Id = x.Id,
                    Label = x.Label,
                    CounterTypeName = x.CounterType.Label,
                    ResetTypeName = (x.ResetType != null) ? x.ResetType.Label : "Име на съда!!!!"
                }).AsQueryable();
        }

        public CounterEditVM Counter_GetById(int id)
        {
            CounterEditVM result = repo.AllReadonly<Counter>(x => x.Id == id)
                .Select(x => new CounterEditVM()
                {
                    Id = x.Id,
                    CourtId = x.CourtId,
                    Label = x.Label,
                    CounterTypeId = x.CounterTypeId,
                    ResetTypeId = x.ResetTypeId,
                    Prefix = x.Prefix,
                    Suffix = x.Suffix,
                    DigitCount = x.DigitCount,
                    InitValue = x.InitValue,
                }).FirstOrDefault();

            switch (result.CounterTypeId)
            {
                case NomenclatureConstants.CounterTypes.Document:
                    var _counterDoc = repo.AllReadonly<CounterDocument>(x => x.CounterId == id).FirstOrDefault();
                    if (_counterDoc != null)
                    {
                        result.DocumentDirectionId = _counterDoc.DocumentDirectionId;
                    }
                    break;
                case NomenclatureConstants.CounterTypes.Case:
                    var _counterCase = repo.AllReadonly<CounterCase>(x => x.CounterId == id).FirstOrDefault();
                    if (_counterCase != null)
                    {
                        result.CaseGroupId = _counterCase.CaseGroupId;
                    }
                    break;
                case NomenclatureConstants.CounterTypes.SessionAct:
                    var _counterAct = repo.AllReadonly<CounterSessionAct>(x => x.CounterId == id).FirstOrDefault();
                    if (_counterAct != null)
                    {
                        result.SessionActGroupId = _counterAct.SessionActGroupId;
                    }
                    break;
            }
            return result;
        }
        public bool Counter_SaveData(CounterEditVM model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<Counter>(model.Id);
                    saved.Label = model.Label;
                    saved.CounterTypeId = model.CounterTypeId;
                    saved.Prefix = model.Prefix;
                    saved.Suffix = model.Suffix;
                    saved.InitValue = model.InitValue;
                    saved.DigitCount = model.DigitCount;
                    saved.ResetTypeId = model.ResetTypeId;

                    repo.DeleteRange<CounterDocument>(x => x.CounterId == model.Id);
                    repo.DeleteRange<CounterCase>(x => x.CounterId == model.Id);
                    repo.DeleteRange<CounterSessionAct>(x => x.CounterId == model.Id);

                    switch (model.CounterTypeId)
                    {
                        case NomenclatureConstants.CounterTypes.Document:
                            saved.CounterDocument = new List<CounterDocument>() { new CounterDocument() { DocumentDirectionId = model.DocumentDirectionId } };
                            break;
                        case NomenclatureConstants.CounterTypes.Case:
                            saved.CounterCase = new List<CounterCase>() { new CounterCase() { CaseGroupId = model.CaseGroupId } };
                            break;
                        case NomenclatureConstants.CounterTypes.SessionAct:
                            saved.CounterSessionAct = new List<CounterSessionAct>() { new CounterSessionAct() { SessionActGroupId = model.SessionActGroupId } };
                            break;
                    }

                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    switch (model.CounterTypeId)
                    {
                        case NomenclatureConstants.CounterTypes.Document:
                            model.CounterDocument = new List<CounterDocument>() { new CounterDocument() { DocumentDirectionId = model.DocumentDirectionId } };
                            break;
                        case NomenclatureConstants.CounterTypes.Case:
                            model.CounterCase = new List<CounterCase>() { new CounterCase() { CaseGroupId = model.CaseGroupId } };
                            break;
                        case NomenclatureConstants.CounterTypes.SessionAct:
                            model.CounterSessionAct = new List<CounterSessionAct>() { new CounterSessionAct() { SessionActGroupId = model.SessionActGroupId } };
                            break;
                    }

                    //Insert
                    repo.Add<Counter>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Counter Id={ model.Id }");
                return false;
            }
        }

        public bool Counter_GetDocumentCounter(Document model)
        {
            try
            {
                var counterId = repo.AllReadonly<CounterDocument>()
                                    .Where(x => x.DocumentDirectionId == model.DocumentDirectionId)
                                    .Where(x => x.Counter.CourtId == model.CourtId)
                                    .Select(x => x.CounterId)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    (int intValue, string stringValue) = Counter_GetValueMulti(counterId);
                    model.DocumentNumber = stringValue;
                    model.DocumentNumberValue = intValue;
                    model.DocumentDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за документи. DocumentDirection={ model.DocumentDirectionId },Court={ model.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Counter. DocumentDirection={ model.DocumentDirectionId },Court={ model.CourtId}");
            }
            return false;
        }

        public GetCounterValueVM Counter_GetDocumentCounterMulti(int counterCount, int docDirection, int courtId)
        {
            try
            {
                var counterId = repo.AllReadonly<CounterDocument>()
                                    .Where(x => x.DocumentDirectionId == docDirection)
                                    .Where(x => x.Counter.CourtId == courtId)
                                    .Select(x => x.CounterId)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    var counter = repo.ExecuteProc<GetCounterValueVM>("public.get_counter_value_multi({0},{1})", counterId, counterCount).FirstOrDefault();
                    return counter;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за документи. DocumentDirection={ docDirection },Court={ courtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Counter. DocumentDirection={ docDirection },Court={ courtId}");
            }
            return null;
        }

        private string Counter_GetValue(int counterId)
        {
            (int intValue, string stringValue) = Counter_GetValueMulti(counterId);
            return stringValue;
        }

        private (int intValue, string stringValue) Counter_GetValueMulti(int counterId)
        {
            var counter = repo.ExecuteProc<GetCounterValueVM>("public.get_counter_value({0})", counterId).FirstOrDefault();

            return (intValue: counter.Value, stringValue: string.Format("{0}{1:D" + counter.DigitCount.ToString() + "}{2}", counter.Prefix, counter.Value, counter.Suffix));
        }

        public bool Counter_GetCaseCounter(Case model, int? oldNumber = null, DateTime? oldDate = null)
        {
            try
            {

                var courtCode = repo.AllReadonly<Court>().FirstOrDefault(x => x.Id == model.CourtId)?.Code;
                var characterCode = repo.AllReadonly<CaseCharacter>()
                                            .Where(x => x.Id == model.CaseCharacterId)
                                            .Select(x => x.Code)
                                            .FirstOrDefault();

                var counterId = repo.AllReadonly<CounterCase>()
                                    .Include(x => x.Counter)
                                    .Where(x => x.CaseGroupId == model.CaseGroupId)
                                    .Where(x => x.Counter.CourtId == model.CourtId && x.Counter.CounterTypeId == NomenclatureConstants.CounterTypes.Case)
                                    .Select(x => x.CounterId)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    if (model.IsOldNumber == true && oldNumber.HasValue && oldDate.HasValue)
                    {
                        model.ShortNumber = oldNumber.Value.ToString().PadLeft(5, '0');
                        model.RegDate = oldDate.Value;
                    }
                    else
                    {
                        model.ShortNumber = Counter_GetValue(counterId);
                        model.RegDate = DateTime.Now;
                    }
                    model.RegNumber = $"{model.RegDate.Year}{courtCode}{characterCode}{model.ShortNumber}";
                    model.ShortNumber = model.ShortNumber.TrimStart('0');
                    try
                    {
                        model.ShortNumberValue = int.Parse(model.ShortNumber);
                    }
                    catch (Exception ee) { }
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за дела. CaseGroupId={ model.CaseGroupId },Court={ model.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Counter. CaseGroupId={ model.CaseGroupId },Court={ model.CourtId}");
            }
            return false;
        }

        public bool Counter_GetCaseArchiveCounter(CaseArchive model)
        {
            Case caseModel = null;
            try
            {
                caseModel = model.Case ?? repo.GetById<Case>(model.CaseId);
                var courtCode = repo.AllReadonly<Court>().FirstOrDefault(x => x.Id == caseModel.CourtId)?.Code;
                var characterCode = repo.AllReadonly<CaseCharacter>()
                                            .Where(x => x.Id == caseModel.CaseCharacterId)
                                            .Select(x => x.Code)
                                            .FirstOrDefault();

                var counterId = repo.AllReadonly<CounterCase>()
                                    .Include(x => x.Counter)
                                    .Where(x => x.CaseGroupId == caseModel.CaseGroupId)
                                    .Where(x => x.Counter.CourtId == caseModel.CourtId && x.Counter.CounterTypeId == NomenclatureConstants.CounterTypes.CaseArchive)
                                    .Select(x => x.CounterId)
                                    .FirstOrDefault();
                if (counterId > 0)
                {

                    var shortNumber = Counter_GetValue(counterId);
                    model.RegNumber = $"A{DateTime.Now.Year}{courtCode}{characterCode}{shortNumber}";
                    model.RegDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за архивиране. CaseGroupId={ caseModel.CaseGroupId },Court={ caseModel.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Counter. CaseGroupId={ caseModel.CaseGroupId },Court={ caseModel.CourtId}");
            }
            return false;
        }

        public bool Counter_GetActCounter(CaseSessionAct model, int caseGroupId, int courtId)
        {
            try
            {
                var sessionActGroupId = repo.AllReadonly<SessionActType>()
                                                .Include(x => x.SessionActGroup)
                                                .Where(x => x.ActTypeId == model.ActTypeId && x.SessionActGroup.CaseGroupId == caseGroupId)
                                                .Select(x => x.SessionActGroupId)
                                                .FirstOrDefault();

                if (sessionActGroupId < 1)
                {
                    throw new Exception($"Няма настроена група за номериране на актове. ActTypeId={model.ActTypeId },Court={courtId}");
                }


                var counterId = repo.AllReadonly<CounterSessionAct>()
                                    .Include(x => x.Counter)
                                    .Where(x => x.Counter.CourtId == courtId && x.SessionActGroupId == sessionActGroupId)
                                    .Select(x => x.CounterId)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    var counterResult = Counter_GetValueMulti(counterId);
                    if (counterResult.intValue > 0)
                    {
                        model.RegNumber = counterResult.stringValue;
                        model.RegDate = DateTime.Now;
                        model.ActDate = model.RegDate;
                        return true;
                    }
                    else
                    {
                        throw new Exception($"Грешка вземане на стойност от брояч CounterId={counterId}. CaseGroupId={caseGroupId },Court={courtId}");
                    }
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за актове. CaseGroupId={caseGroupId },Court={courtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Counter. CaseGroupId={caseGroupId },Court={courtId}");
            }
            return false;
        }

        public bool Counter_GetNotificationCounter(CaseNotification model, int courtId)
        {
            try
            {
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == courtId && x.CounterTypeId == NomenclatureConstants.CounterTypes.Notification)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    var courtCode = repo.AllReadonly<Court>().FirstOrDefault(x => x.Id == courtId)?.Code;


                    model.RegNumber = $"{DateTime.Now.Year}{courtCode}{Counter_GetValue(counterId)}";
                    model.RegDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за известия. Court={courtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на брояч за известия. Court={courtId}");
            }
            return false;
        }

        public bool Counter_GetEvidenceCounter(CaseEvidence model, int courtId)
        {
            try
            {
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == courtId && x.CounterTypeId == NomenclatureConstants.CounterTypes.Evidence)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    var courtCode = repo.AllReadonly<Court>().FirstOrDefault(x => x.Id == courtId)?.Code;

                    (int intValue, string stringValue) = Counter_GetValueMulti(counterId);
                    model.RegNumber = $"{DateTime.Now.Year}{courtCode}{stringValue}";
                    model.RegNumberValue = intValue;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за доказателства. Court={courtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на брояч за доказателства. Court={courtId}");
            }
            return false;
        }

        public bool Counter_GetObligationCounter(Obligation model)
        {
            try
            {
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == model.CourtId && x.CounterTypeId == NomenclatureConstants.CounterTypes.Obligation)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    var courtCode = repo.AllReadonly<Court>().FirstOrDefault(x => x.Id == model.CourtId)?.Code;

                    model.ObligationNumber = $"{DateTime.Now.Year}{courtCode}{Counter_GetValue(counterId)}";
                    model.ObligationDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за задължения. Court={model.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на брояч за задължения. Court={model.CourtId}");
            }
            return false;
        }

        public bool Counter_GetPaymentCounter(Payment model)
        {
            try
            {
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == model.CourtId && x.CounterTypeId == NomenclatureConstants.CounterTypes.Payment)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    var courtCode = repo.AllReadonly<Court>().FirstOrDefault(x => x.Id == model.CourtId)?.Code;

                    model.PaymentNumber = $"{DateTime.Now.Year}{courtCode}{Counter_GetValue(counterId)}";
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за плащания. Court={model.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на брояч за плащания. Court={model.CourtId}");
            }
            return false;
        }

        public bool Counter_GetExpenseOrderCounter(ExpenseOrder model)
        {
            try
            {
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == model.CourtId && x.CounterTypeId == NomenclatureConstants.CounterTypes.ExpenseOrder)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    var courtCode = repo.AllReadonly<Court>().FirstOrDefault(x => x.Id == model.CourtId)?.Code;

                    model.RegNumber = $"{DateTime.Now.Year}{courtCode}{Counter_GetValue(counterId)}";
                    model.RegDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за разходен ордер. Court={model.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на брояч за разходен ордер. Court={model.CourtId}");
            }
            return false;
        }

        public bool Counter_GetDocumentDecisionCounter(DocumentDecision model)
        {
            try
            {
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == model.CourtId && x.CounterTypeId == NomenclatureConstants.CounterTypes.DocumentDecision)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    var courtCode = repo.AllReadonly<Court>().FirstOrDefault(x => x.Id == model.CourtId)?.Code;

                    model.RegNumber = $"{DateTime.Now.Year}{courtCode}{Counter_GetValue(counterId)}";
                    model.RegDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за решения по доументи. Court={model.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на брояч за решения по доументи. Court={model.CourtId}");
            }
            return false;
        }
        public bool Counter_GetDivorceCounter(CaseSessionActDivorce model, int courtId)
        {
            try
            {
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == courtId && x.CounterTypeId == NomenclatureConstants.CounterTypes.Divorce)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    var courtCode = repo.AllReadonly<Court>().FirstOrDefault(x => x.Id == courtId)?.Code;

                    model.RegNumber = $"{DateTime.Now.Year}{courtCode}{Counter_GetValue(counterId)}";
                    model.RegDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за Съобщение за граждански брак. Court={courtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на брояч за Съобщение за граждански брак. Court={courtId}");
            }
            return false;
        }

        public bool Counter_GetExecListCounter(ExecList model)
        {
            try
            {
                int counterTypeId = 0;
                switch (model.ExecListTypeId)
                {
                    case NomenclatureConstants.ExecListTypes.Country:
                        counterTypeId = NomenclatureConstants.CounterTypes.ExecListCountry;
                        break;
                    case NomenclatureConstants.ExecListTypes.ThirdPerson:
                        counterTypeId = NomenclatureConstants.CounterTypes.ExecListThirdPerson;
                        break;
                }
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == model.CourtId && x.CounterTypeId == counterTypeId)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    (int intValue, string stringValue) = Counter_GetValueMulti(counterId);
                    model.RegNumber = intValue.ToString();
                    model.RegDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за изпълнителен лист. Court={model.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на брояч за изпълнителен лист. Court={model.CourtId}");
            }
            return false;
        }

        public bool Counter_GetExchangeCounter(ExchangeDoc model)
        {
            try
            {
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == model.CourtId && x.CounterTypeId == NomenclatureConstants.CounterTypes.ExchangeDoc)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    (int intValue, string stringValue) = Counter_GetValueMulti(counterId);
                    model.RegNumber = intValue.ToString();
                    model.RegDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за Приемо предавателен протокол за ИЛ. Court={model.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на брояч за Приемо предавателен протокол за ИЛ. Court={model.CourtId}");
            }
            return false;
        }

        public void InitAllCounters()
        {

            return;
            //if (repo.AllReadonly<Counter>()
            //        .Where(x => x.CounterTypeId == NomenclatureConstants.CounterTypes.DocumentResolution)
            //        .Any())
            //{
            //    return;
            //}

            var courts = repo.AllReadonly<Court>().ToList();
            var docDirections = repo.AllReadonly<DocumentDirection>().ToList();
            var caseGroups = repo.AllReadonly<CaseGroup>().ToList();
            var actGroups = repo.AllReadonly<SessionActGroup>().ToList();
            foreach (var court in courts)
            {
                ////Броячи за документи
                //foreach (var docDir in docDirections)
                //{
                //    var counter = new Counter()
                //    {
                //        CourtId = court.Id,
                //        CounterTypeId = NomenclatureConstants.CounterTypes.Document,
                //        Label = $"{court.Label} : {docDir.Label}",
                //        ResetTypeId = 1,
                //        InitValue = 0,
                //        Value = 0,
                //        DigitCount = 1
                //    };
                //    counter.CounterDocument = new List<CounterDocument>()
                //    {
                //        new CounterDocument(){DocumentDirectionId = docDir.Id}
                //    };
                //    repo.Add(counter);
                //}

                ////Броячи за дела
                //foreach (var caseGroup in caseGroups)
                //{
                //    var counter = new Counter()
                //    {
                //        CourtId = court.Id,
                //        CounterTypeId = NomenclatureConstants.CounterTypes.Case,
                //        Label = $"{court.Label} : {caseGroup.Label}",
                //        ResetTypeId = 1,
                //        InitValue = 0,
                //        Value = 0,
                //        DigitCount = 5
                //    };
                //    counter.CounterCase = new List<CounterCase>()
                //    {
                //        new CounterCase(){CaseGroupId = caseGroup.Id}
                //    };
                //    repo.Add(counter);
                //}

                //Броячи за архивиране на дела
                //foreach (var caseGroup in caseGroups)
                //{
                //    var counter = new Counter()
                //    {
                //        CourtId = court.Id,
                //        CounterTypeId = NomenclatureConstants.CounterTypes.CaseArchive,
                //        Label = $"{court.Label} : {caseGroup.Label}",
                //        ResetTypeId = 1,
                //        InitValue = 0,
                //        Value = 0,
                //        DigitCount = 5
                //    };
                //    counter.CounterCase = new List<CounterCase>()
                //    {
                //        new CounterCase(){CaseGroupId = caseGroup.Id}
                //    };
                //    repo.Add(counter);
                //}

                //Броячи за задължения
                //var obligation = new Counter()
                //{
                //  CourtId = court.Id,
                //  CounterTypeId = NomenclatureConstants.CounterTypes.Obligation,
                //  Label = $"{court.Label} : Задължения",
                //  ResetTypeId = 1,
                //  InitValue = 0,
                //  Value = 0,
                //  DigitCount = 7
                //};
                //repo.Add(obligation);

                //Броячи за плащания
                //var payment = new Counter()
                //{
                //  CourtId = court.Id,
                //  CounterTypeId = NomenclatureConstants.CounterTypes.Payment,
                //  Label = $"{court.Label} : Плащания",
                //  ResetTypeId = 1,
                //  InitValue = 0,
                //  Value = 0,
                //  DigitCount = 7
                //};
                //repo.Add(payment);

                //Брояч за разходен ордер
                //var expenseOrder = new Counter()
                //{
                //    CourtId = court.Id,
                //    CounterTypeId = NomenclatureConstants.CounterTypes.ExpenseOrder,
                //    Label = $"{court.Label} : Разходен ордер",
                //    ResetTypeId = 1,
                //    InitValue = 0,
                //    Value = 0,
                //    DigitCount = 7
                //};
                //repo.Add(expenseOrder);

                //Броячи за актове
                //foreach (var actGroup in actGroups.Where(x => x.Id == 27 || x.Id == 28 || x.Id == 29))
                //{
                //    var counter = new Counter()
                //    {
                //        CourtId = court.Id,
                //        CounterTypeId = NomenclatureConstants.CounterTypes.SessionAct,
                //        Label = $"{court.Label} : {actGroup.Label}",
                //        ResetTypeId = 1,
                //        InitValue = 0,
                //        Value = 0,
                //        DigitCount = 1
                //    };
                //    counter.CounterSessionAct = new List<CounterSessionAct>()
                //    {
                //        new CounterSessionAct(){SessionActGroupId = actGroup.Id}
                //    };
                //    repo.Add(counter);
                //}

                ////Брояч за известия
                //var counterNotification = new Counter()
                //{
                //    CourtId = court.Id,
                //    CounterTypeId = NomenclatureConstants.CounterTypes.Notification,
                //    Label = $"{court.Label} : Известия",
                //    ResetTypeId = 1,
                //    InitValue = 0,
                //    Value = 0,
                //    DigitCount = 5
                //};
                //repo.Add(counterNotification);

                ////Брояч за доказателства
                //var counterEvidence = new Counter()
                //{
                //    CourtId = court.Id,
                //    CounterTypeId = NomenclatureConstants.CounterTypes.Evidence,
                //    Label = $"{court.Label} : Доказателства",
                //    ResetTypeId = 1,
                //    InitValue = 0,
                //    Value = 0,
                //    DigitCount = 1
                //};
                //repo.Add(counterEvidence);

                //Брояч за Решения по документи
                //var documentDecision = new Counter()
                //{
                //    CourtId = court.Id,
                //    CounterTypeId = NomenclatureConstants.CounterTypes.DocumentDecision,
                //    Label = $"{court.Label} : Решения по документи",
                //    ResetTypeId = 1,
                //    InitValue = 0,
                //    Value = 0,
                //    DigitCount = 5
                //};
                //repo.Add(documentDecision);

                //Брояч за Съобщение за граждански брак
                //var divorce = new Counter()
                //{
                //    CourtId = court.Id,
                //    CounterTypeId = NomenclatureConstants.CounterTypes.Divorce,
                //    Label = $"{court.Label} : Съобщение за граждански брак",
                //    ResetTypeId = 1,
                //    InitValue = 0,
                //    Value = 0,
                //    DigitCount = 5
                //};
                //repo.Add(divorce);

                ////Брояч за ИЛ в полза на Държавата
                //var execListCountry = new Counter()
                //{
                //    CourtId = court.Id,
                //    CounterTypeId = NomenclatureConstants.CounterTypes.ExecListCountry,
                //    Label = $"{court.Label} : Изпълнителен лист в полза на Държавата",
                //    ResetTypeId = 1,
                //    InitValue = 0,
                //    Value = 0,
                //    DigitCount = 5
                //};
                //repo.Add(execListCountry);

                ////Брояч за ИЛ в полза на трети лица
                //var execListThirdPerson = new Counter()
                //{
                //    CourtId = court.Id,
                //    CounterTypeId = NomenclatureConstants.CounterTypes.ExecListThirdPerson,
                //    Label = $"{court.Label} : Изпълнителен лист в полза на трети лица",
                //    ResetTypeId = 1,
                //    InitValue = 0,
                //    Value = 0,
                //    DigitCount = 5
                //};
                //repo.Add(execListThirdPerson);

                //Брояч за приемо предавателен протокол за ИЛ
                //var execListThirdPerson = new Counter()
                //{
                //    CourtId = court.Id,
                //    CounterTypeId = NomenclatureConstants.CounterTypes.ExchangeDoc,
                //    Label = $"{court.Label} : Приемо предавателен протокол за ИЛ",
                //    ResetTypeId = 1,
                //    InitValue = 0,
                //    Value = 0,
                //    DigitCount = 5
                //};
                //repo.Add(execListThirdPerson);

                //Броячи за разпореждания по документи
                //var counterDocResolution = new Counter()
                //{
                //    CourtId = court.Id,
                //    CounterTypeId = NomenclatureConstants.CounterTypes.DocumentResolution,
                //    Label = $"{court.Label} : Разпореждания по документи",
                //    ResetTypeId = 1,
                //    InitValue = 0,
                //    Value = 0,
                //    DigitCount = 1
                //};
                //repo.Add(counterDocResolution);

                repo.SaveChanges();
            }
        }

        public CounterVM[] Counter_GetCurrentValues(int courtId)
        {
            var result = repo.AllReadonly<Counter>()
                            .Include(x => x.CounterType)
                            .Where(x => x.CourtId == courtId)
                            .Select(x => new CounterVM
                            {
                                Id = x.Id,
                                CounterTypeId = x.CounterTypeId,
                                CounterTypeName = x.CounterType.Label,
                                Label = x.Label,
                                CurrentValue = x.Value
                            }).OrderBy(x => x.CounterTypeName).ThenBy(x => x.Label).ToArray();

            foreach (var item in result)
            {
                item.Label = item.Label.Replace(userContext.CourtName + " : ", "");

            }
            return result;
        }

        public bool Counter_SetCurrentValues(CounterVM[] model)
        {
            int[] counterIds = model.Select(x => x.Id).ToArray();
            var counters = repo.All<Counter>().Where(x => counterIds.Contains(x.Id)).ToList();
            foreach (var counter in counters)
            {
                var newVal = model.Where(x => x.Id == counter.Id).Select(x => x.CurrentValue).FirstOrDefault();
                counter.Value = newVal;
            }
            if (counterIds.Any())
            {
                repo.SaveChanges();
            }
            return true;
        }

        public bool Counter_GetDocumentResolutionCounter(DocumentResolution model)
        {
            try
            {
                var counterId = repo.AllReadonly<Counter>()
                                    .Where(x => x.CourtId == model.CourtId && x.CounterTypeId == NomenclatureConstants.CounterTypes.DocumentResolution)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
                if (counterId > 0)
                {
                    model.RegNumber = $"{Counter_GetValue(counterId)}";
                    model.RegDate = DateTime.Now;
                    return true;
                }
                else
                {
                    throw new Exception($"Няма настроен брояч за разпореждания по документ. Court={model.CourtId}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Counter_GetDocumentResolutionCounter");
            }
            return false;
        }
    }
}
