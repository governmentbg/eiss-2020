// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Election;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Election;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IElectionService : IBaseService
    {
        IQueryable<LabelValueVM> SearchLawunitsForElection(string query);
        IQueryable<ElectionPersonVM> Persons_Select(int groupId);
        IQueryable<ElectionPersonVM> Persons_SelectByProtocol(int protcoolId);
        IQueryable<ElectionGroupVM> ElectionGroup_Select();
        ElectionGroup GetElectionGroupByID(int id);
        ElectionProtocol GetElectionProtocolByID(int id);
        List<SelectListItem> GetDDL_ElectionType();
        ElectionGroupVM ElectionGroup_SelectById(int id);
        int Election_Save(ElectionGroupVM model);
        ElectionProtocolVM ElectionProtokol_Preview(int id);
        SaveResultVM PersonAdd(ElectionPersonAddVM model);
        bool CheckIfLawunitIsAddedPersonAdd(ElectionPersonAddVM model);
        IQueryable<ElectionProtocolVM> ElectionProtocolList(int ElectionGroupId);
        ElectionProtocol Get_ElectionProtokol(int id);
        bool Save_ElectionProtokol(ElectionProtocol protocol);
        int PersonFinish_Create(int electionGrouproupId);
        int GetNotFilledProtocol_ID(int electionGroupId);
        int GetNotSignedProtocol_ID(int electionGroupId);
        bool ElectionProtocolSetSelectedLawUnit(int electionProtocolID);
        SaveResultVM ValidatePersonAdd(ElectionPersonAddVM model);
        ElectionPersonPreviewVM PersonGetById(int id);
        List<SelectListItem> GetDDL_DismissalTypeForPerson(int personId);
        List<SelectListItem> GetDDL_PersonStatesForPerson(int personId);
        SaveResultVM PersonChange(ElectionPersonChangeVM model);
        IQueryable<ElectionLogVM> SelectLog(int? electionGroupId, Guid? personGid, int? electionProtocolId);
        SaveResultVM PersonSaveDeclaration(int id);
        void WriteElectionLog(int electionGroupId, Guid? personGid, int? lawunitId, string operation, string description, int? electionPersonId);
        ElectionPerson GetElectionPersonBySelectedElectionPersonId(int id);
    }
}
