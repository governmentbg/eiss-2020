// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseEvidenceService: IBaseService
    {
        IQueryable<CaseEvidenceVM> CaseEvidence_Select(int CaseId, DateTime? DateFrom, DateTime? DateTo, string RegNumber);
        bool CaseEvidence_SaveData(CaseEvidence model);
        IQueryable<CaseEvidenceMovementVM> CaseEvidenceMovement_Select(int CaseEvidenceId);
        bool CaseEvidenceMovement_SaveData(CaseEvidenceMovement model);
        byte[] CaseEvidenceSpr_ToExcel(int courtId, CaseEvidenceSprFilterVM model);
        bool IsExistMovmentType(int CaseId, int MovmentTypeId);
        bool IsExistMovment(int CaseEvidenceId);
    }
}
