// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using System.Collections.Generic;
using System.Linq;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;

namespace IOWebApplication.Core.Contracts
{
    public interface IMQEpepService : IBaseService
    {
        int ReturnAllErrorsInMQ();

        #region ЕПЕП

        string RecoverData(object client);

        bool AppendDocument(Document model, ServiceMethod method);
        bool AppendFile(CdnUploadRequest model, ServiceMethod method);
        bool AppendCase(Case model, ServiceMethod method);
        bool AppendCasePerson(CasePerson model, ServiceMethod method);
        bool AppendCaseSelectionProtocol(CaseSelectionProtokol model, ServiceMethod method);
        bool AppendJudgeReporter(int caseLawunitId, ServiceMethod method);
        bool AppendCaseSession(CaseSession model, ServiceMethod method);
        bool AppendCaseSessionAct(CaseSessionAct model, ServiceMethod method);
        bool AppendCaseSessionAct_Private(int actId, ServiceMethod method);
        bool AppendCaseSessionAct_Public(int actId, ServiceMethod method);
        bool AppendCaseSessionAct_PrivateMotive(int actId, ServiceMethod method);
        bool AppendCaseSessionAct_PublicMotive(int actId, ServiceMethod method);
        bool AppendCaseSessionLawUnit(CaseLawUnit model, ServiceMethod method);
        bool AppendCaseNotification(CaseNotification model, ServiceMethod method);

        IQueryable<EpepUserVM> EpepUser_Select(int? userType, string search);
        bool EpepUser_SaveData(EpepUser model);
        string EpepUser_Validate(EpepUser model);

        IQueryable<EpepUserAssignmentVM> EpepUserAssignment_Select(int epepUserId);
        bool EpepUserAssignment_SaveData(EpepUserAssignment model);

        IntegrationKey IntegrationKey_GetByOuterKey(int integrationType, string key);

        List<IntegrationKey> IntegrationKey_SelectToCorrect(int sourceType);
        bool IntegrationKey_Correct(IntegrationKey model, bool withError);

        IEnumerable<MQEpepVM> MQEpep_Select(int integrationType, int sourceType, long sourceId);

        #endregion

        #region ЦУБИПСА
        bool LegalActs_SendAct(int actId, ServiceMethod method);
        #endregion

        void InitMQ(int integrationTypeId, int sourceType, long sourceId, EpepConstants.ServiceMethod method, long? parentSourceId = null, object model = null);
        long InitMQFromString(int integrationTypeId, int sourceType, long sourceId, EpepConstants.ServiceMethod method, long? parentSourceId, string message);
        bool ISPN_CaseSessionResult(int resultId, ServiceMethod method, long? caseId);
        bool ISPN_CaseSessionActComplain(int actComplainId, ServiceMethod method, long? caseId);
        bool ISPN_IsISPN(Case _case, int caseId);
    }
}
