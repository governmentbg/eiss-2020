// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Audit;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Models.ViewModels.AuditLog;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class AuditLogService: BaseService, IAuditLogService
    {
        public AuditLogService(IRepository _repo,
                               IUserContext _userContext)
        {
            repo = _repo;
            userContext = _userContext;
        }

        public IQueryable<AuditLogSprVM> AuditLog_Select(DateTime DateFrom, DateTime DateTo, string RegNumber, string Operation, string UserId, int courtId)
        {
            List<AuditLogSprVM> auditLogSprVMs = new List<AuditLogSprVM>();

            var applicationUsers = repo.AllReadonly<ApplicationUser>()
                                       .Include(x => x.LawUnit)
                                       .ToDictionary(k => k.Id, v => v);

            repo.AllReadonly<AuditLog>()
                .FromSql($"SELECT * FROM audit_log.audit_log WHERE inserted_date BETWEEN '{DateFrom.ToString("yyyy-MM-dd HH:mm:ss")}' AND  '{DateTo.ToString("yyyy-MM-dd HH:mm:ss")}' AND \"data\" -> 'currentContext' ->> 'CourtId' = '{courtId}'" +
                $" AND ({Operation == "-1"} OR \"data\" -> 'currentContext' -> 'Operation' ? '{Operation}')" +
                $" AND ({UserId == null} OR \"data\" -> 'currentContext' -> 'UserId' ? '{UserId}')" +
                $" AND ({RegNumber == null} OR \"data\" -> 'currentContext' ->> 'BaseObject' ILIKE '%{RegNumber}%')")
                .ToList()
                .ForEach(x =>
                {
                    var auditLogVM = JsonConvert.DeserializeObject<AuditLogVM>(x.Data);
                    auditLogSprVMs.Add(new AuditLogSprVM
                    {
                        Date = x.InsertedDate,
                        Operation = auditLogVM.currentContext.Operation,
                        ObjectType = auditLogVM.currentContext.ObjectType,
                        ObjectInfo = auditLogVM.currentContext.ObjectInfo,
                        BaseObject = auditLogVM.currentContext.BaseObject,
                        UserName = (auditLogVM.currentContext.UserId != null && applicationUsers.ContainsKey(auditLogVM.currentContext.UserId)) ?
                            applicationUsers[auditLogVM.currentContext.UserId].LawUnit.FullName : string.Empty,
                        UserId = auditLogVM.currentContext.UserId,
                        CourtId = auditLogVM.currentContext.CourtId
                    });
                });

            return auditLogSprVMs
                .GroupBy(k => new { k.UserId, k.Date.Date, k.Date.Hour, k.Date.Minute, k.Date.Second, k.Operation, k.BaseObject })
                .Select(grp => grp.FirstOrDefault())
                .AsQueryable();
        }

        private SelectListItem FillSelectListItem(string Value)
        {
            return new SelectListItem()
            {
                Text = Value,
                Value = Value
            };
        }

        public List<SelectListItem> GetDDL_Operation(bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = new List<SelectListItem>();
            result.Add(FillSelectListItem(AuditConstants.Operations.Append));
            result.Add(FillSelectListItem(AuditConstants.Operations.Update));
            result.Add(FillSelectListItem(AuditConstants.Operations.Delete));
            result.Add(FillSelectListItem(AuditConstants.Operations.Init));
            result.Add(FillSelectListItem(AuditConstants.Operations.View));
            result.Add(FillSelectListItem(AuditConstants.Operations.List));
            result.Add(FillSelectListItem(AuditConstants.Operations.ChoiceByList));
            result.Add(FillSelectListItem(AuditConstants.Operations.Print));
            result.Add(FillSelectListItem(AuditConstants.Operations.GeneratingFile));

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
    }
}
