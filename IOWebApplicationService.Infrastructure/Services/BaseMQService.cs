using IO.SignTools.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml.Linq;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class BaseMQService
    {
        protected int? mqID = null;
        protected int fetchCount = 0;
        protected IRepository repo;
        protected int IntegrationTypeId;
        protected ICdnService cdnService;
        protected ILogger<BaseMQService> logger;
        protected DateTime? startTime;


        /// <summary>
        /// The метода за изпращане на чакащите заявки към ЕПЕП
        /// </summary>
        public async Task<bool> PushMQWithFetch(int fetchCount)
        {
            this.fetchCount = fetchCount;
            ResetMQ_Waiting();

            var model = FetchNotSent(fetchCount, mqID);

            if (!model.Any() && IntegrationTypeId != NomenclatureConstants.IntegrationTypes.EISPP)
            {
                return false;
            }
            if (!(await InitChanel()))
            {
                return false;
            }

            foreach (var mq in model)
            {
                try
                {
                    //logger.LogCritical($"mqId = {mq.Id}");
                    await SendMQ(mq);
                }
                catch (FaultException fex)
                {
                    var _error = fex.GetMessageFault();
                    SetErrorToMQ(mq, IntegrationStates.DataContentError, _error);
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    SetErrorToMQ(mq, IntegrationStates.TransferError, $"Exception: {ex.Message}");
                }
            };

            await CloseChanel();

            return true;
        }

        protected virtual async Task<bool> InitChanel() { return false; }
        protected virtual async Task CloseChanel() { }
        protected virtual async Task SendMQ(MQEpep mq) { }

        /// <summary>
        /// Извлича задачите с висок приоритет и ги изпълнява преди останалите
        /// </summary>
        /// <param name="fetchCount"></param>
        /// <returns></returns>
        protected virtual IEnumerable<MQEpep> FetchHighPriorityItems(int fetchCount)
        {
            return null;
        }
        #region Общи методи за свързване и управление на заявките 

        /// <summary>
        /// Връща най-старата неизпратена заявка
        /// </summary>
        /// <param name="mqId"></param>
        /// <returns></returns>
        protected MQEpep PopOneNotSent(long? mqId = null)
        {


            return repo.All<MQEpep>()
                            .Where(x => x.IntegrationTypeId == IntegrationTypeId)
                            .Where(x => x.DateTransfered == null && x.IntegrationStateId == IntegrationStates.New)
                            .Where(x => x.Id == (mqId ?? x.Id))
                            .OrderBy(x => x.Id)
                            .FirstOrDefault();
        }

        protected IEnumerable<MQEpep> FetchNotSent(int fetchCount, long? mqId = null)
        {
            if (mqId > 0)
            {
                return repo.All<MQEpep>()
                           .Where(x => x.IntegrationTypeId == IntegrationTypeId)
                           .Where(x => x.DateTransfered == null)
                           .Where(x => x.Id == mqId)
                           .ToList();
            }
            else
            {
                var result = FetchHighPriorityItems(fetchCount);
                if (result != null)
                    if (result.Any())
                    {
                        return result;
                    }

                return repo.All<MQEpep>()
                           .Where(x => x.IntegrationTypeId == IntegrationTypeId)
                           .Where(x => x.DateTransfered == null && x.IntegrationStateId == IntegrationStates.New)
                           .OrderBy(x => x.Id)
                           .Take(fetchCount)
                           .ToList();
            }
        }


        /// <summary>
        /// Връща чакащите заявки в опашката
        /// </summary>
        /// <returns></returns>
        protected bool ResetMQ_Waiting()
        {
            var mqs = repo.All<MQEpep>()
                            .Where(x => x.IntegrationTypeId == IntegrationTypeId)
                           .Where(x => x.DateTransfered == null && IntegrationStates.ReturnToMQStates.Contains(x.IntegrationStateId ?? 0))
                           .ToList();

            foreach (var item in mqs)
            {
                item.IntegrationStateId = IntegrationStates.New;
            }
            if (mqs.Count > 0)
            {
                repo.SaveChanges();
                return true;
            }
            return false;
        }

        protected bool AddIntegrationKey(MQEpep mq, Guid? Id)
        {
            if (Id.HasValue)
            {
                if (AddIntegrationKey(mq.SourceType, mq.SourceId, Id.ToString()))
                {

                    mq.IntegrationStateId = IntegrationStates.TransferOK;
                    mq.DateTransfered = DateTime.Now;
                    mq.ReturnGuidId = Id.ToString();
                    AppendTimeElapsed(mq);
                    repo.Update(mq);

                    switch (mq.SourceType)
                    {
                        case SourceTypeSelectVM.EpepUser:
                            var epepUser = repo.GetById<EpepUser>((int)mq.SourceId);
                            epepUser.EpepId = Id;
                            repo.Update(epepUser);
                            break;
                        case SourceTypeSelectVM.EpepUserAssignment:
                            var epepUserAssignment = repo.GetById<EpepUserAssignment>((int)mq.SourceId);
                            epepUserAssignment.EpepId = Id;
                            repo.Update(epepUserAssignment);
                            break;
                        default:
                            break;
                    }
                    repo.SaveChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                SetErrorToMQ(mq, IntegrationStates.TransferError);
                return false;
            }
        }

        private void AppendTimeElapsed(MQEpep model)
        {
            if (!startTime.HasValue)
            {
                return;
            }

            TimeSpan ts = DateTime.Now - startTime.Value;
            model.ErrorDescription = $"Elapsed:{ts.TotalSeconds:N3}s; {model.ErrorDescription}";
        }
        protected bool AddIntegrationKey(int sourceType, long sourceId, string value)
        {

            var model = new IntegrationKey()
            {
                SourceType = sourceType,
                SourceId = sourceId,
                IntegrationTypeId = IntegrationTypeId,
                OuterCode = value,
                DateWrt = DateTime.Now
            };
            repo.Add(model);
            return repo.SaveChanges() > 0;
        }

        protected bool RemoveIntegrationKeys(MQEpep mq)
        {
            var keys = repo.All<IntegrationKey>()
                                .Where(x => x.IntegrationTypeId == mq.IntegrationTypeId && x.SourceId == mq.SourceId && x.SourceType == mq.SourceType)
                                .ToList();

            repo.DeleteRange(keys);
            return repo.SaveChanges() > 0;
        }

        /// <summary>
        /// Отменя всички неприключили заявки за добавяне/редакция на същия обект по-стари от заявката за изтриване
        /// </summary>
        /// <param name="mq"></param>
        protected void RemoveUnfinishedTasksBeforeDelete(MQEpep mq)
        {
            var editItem = repo.All<MQEpep>()
                                .Where(x => x.IntegrationTypeId == mq.IntegrationTypeId && x.SourceId == mq.SourceId && x.SourceType == mq.SourceType)
                                .Where(x => EpepConstants.IntegrationStates.UnfinishedMQStates.Contains(x.IntegrationStateId ?? 0))
                                .Where(x => EpepConstants.Methods.EditMethods.Contains(x.MethodName))
                                .Where(x => x.Id < mq.Id)
                                .ToList();

            if (editItem.Any())
            {
                foreach (var item in editItem)
                {
                    item.IntegrationStateId = EpepConstants.IntegrationStates.DisabledByDelete;
                    item.ErrorDescription = $"Отменено поради заявка за изтриване с id={mq.Id}";
                    repo.Update(item);
                }
                repo.SaveChanges();
            }
        }

        public void UpdateMQ(MQEpep mq, bool result)
        {
            if (result)
            {
                UpdateMQ(mq, Guid.NewGuid());
            }
            else
            {
                UpdateMQ(mq, null);
            }
        }

        public void UpdateMQ(MQEpep mq, Guid? id)
        {
            if (id.HasValue)
            {
                mq.DateTransfered = DateTime.Now;
                mq.IntegrationStateId = IntegrationStates.TransferOK;
                AppendTimeElapsed(mq);
                repo.Update(mq);
                repo.SaveChanges();
            }
            else
            {
                SetErrorToMQ(mq, IntegrationStates.TransferError);
            }
        }

        public void SetErrorToMQ(MQEpep mq, int integrationState, string errorDescription = null)
        {
            if (IntegrationStates.ReturnToMQStates.Contains(integrationState))
            {
                if (mq.ErrorCount < IntegrationMaxErrorCount)
                {
                    mq.ErrorCount = (mq.ErrorCount ?? 0) + 1;
                }
                else
                {
                    integrationState = IntegrationStates.TransferErrorLimitExceeded;
                }
            }
            mq.IntegrationStateId = integrationState;
            if (!string.IsNullOrEmpty(errorDescription))
            {
                mq.ErrorDescription = errorDescription;
            }
            repo.Update(mq);
            repo.SaveChanges();
        }
        protected string AppendUpdateIntegrationKey(int sourceType, long sourceId)
        {
            string key = getKey(sourceType, sourceId);
            if (string.IsNullOrEmpty(key))
            {
                key = Guid.NewGuid().ToString();
                if (AddIntegrationKey(sourceType, sourceId, key))
                {
                    return key;
                }
                return null;
            }
            return key;
        }

        protected string getKey(int sourceType, long? sourceId)
        {
            return repo.AllReadonly<IntegrationKey>()
                                    .Where(x => x.IntegrationTypeId == IntegrationTypeId
                                      && x.SourceType == sourceType && x.SourceId == sourceId)
                                    .OrderBy(x => x.Id)
                                    .Select(x => x.OuterCode)
                                    .FirstOrDefault();
        }

        protected Guid getKeyGuid(int sourceType, long? sourceId)
        {
            string key = getKey(sourceType, sourceId);
            if (!string.IsNullOrEmpty(key))
            {
                return Guid.Parse(key);
            }
            else
            {
                return Guid.Empty;
            }
        }
        protected Guid? getKeyGuidNullable(int sourceType, long? sourceId)
        {
            string key = getKey(sourceType, sourceId);
            if (!string.IsNullOrEmpty(key))
            {
                return Guid.Parse(key);
            }
            else
            {
                return null;
            }
        }

        protected bool getParentKey(ref Guid? parentId, int sourceType, long? sourceId)
        {
            try
            {
                string res = getKey(sourceType, sourceId);
                if (!string.IsNullOrEmpty(res))
                {
                    parentId = Guid.Parse(res);
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// Връща външен код на номенклатура
        /// </summary>
        /// <param name="nomenclatureAlias">alias на номенклатура от nom_code_mapping</param>
        /// <param name="value">вътрешно ID на номенклатура</param>
        /// <returns>Връща външен код на номенклатура ако го намери или дава Exception ако не го</returns>
        protected virtual string GetNomValue(string nomenclatureAlias, object value)
        {
            string innerCode = value?.ToString();
            var result = repo.AllReadonly<CodeMapping>()
                            .Where(x => x.Alias == nomenclatureAlias && x.InnerCode == innerCode)
                            .Select(x => x.OuterCode)
                            .FirstOrDefault();

            if (result == null)
            {
                throw new Exception($"Ненамерена номенклатура: alias={nomenclatureAlias}; id={innerCode}");
            }
            return result;
        }
        protected int GetNomValueInt(string nomenclatureAlias, object value)
        {
            string outerId = GetNomValue(nomenclatureAlias, value);
            if (!string.IsNullOrEmpty(outerId))
            {
                return int.Parse(outerId);
            }
            return 0;
        }


        #endregion

    }
}



