using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Audit;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.AuditLog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class AuditLogService : BaseService, IAuditLogService
    {
        private readonly IConfiguration config;
        private readonly IHttpContextAccessor httpContextAccessor;
        public AuditLogService(IRepository _repo,
                               IUserContext _userContext,
                               IConfiguration _config,
                               IHttpContextAccessor _httpContextAccessor)
        {
            repo = _repo;
            userContext = _userContext;
            config = _config;
            httpContextAccessor = _httpContextAccessor;
        }
        /*
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
        */

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

        public IQueryable<AuditLogSprVM> AuditLog_SelectNew(DateTime DateFrom, DateTime DateTo, string RegNumber, string Operation, string UserId, int courtId)
        {
            if (Operation == "-1")
            {
                Operation = null;
            }
            Expression<Func<AuditLog, bool>> whereRegNumber = x => true;
            if (!string.IsNullOrEmpty(RegNumber))
            {
                whereRegNumber = x => EF.Functions.ILike(x.BaseObject, RegNumber.ToPaternSearch());
            }
            Expression<Func<AuditLog, bool>> whereUser = x => x.UserId != null;
            if (!string.IsNullOrEmpty(UserId))
            {
                whereUser = x => x.UserId == UserId;
            }
            Expression<Func<AuditLog, bool>> whereOper = x => true;
            if (!string.IsNullOrEmpty(Operation))
            {
                whereOper = x => x.Operation == Operation;
            }
            return repo.AllReadonly<AuditLog>()
                       .Where(x => x.InsertedDate >= DateFrom && x.InsertedDate <= DateTo)
                       .Where(x => x.CourtId == courtId)
                       .Where(whereUser)
                       .Where(whereOper)
                       .Where(whereRegNumber)
                       .Select(x => new AuditLogSprVM
                       {
                           Id = x.Id,
                           Date = x.InsertedDate,
                           Operation = x.Operation,
                           ObjectType = x.ObjectType,
                           ObjectInfo = x.ObjectInfo,
                           BaseObject = x.BaseObject,
                           UserName = x.ApplicationUser.LawUnit.FullName,
                           Email = x.ApplicationUser.Email,
                           RequestUrl = x.RequestUrl,
                           ShowLink = x.Method == "GET",
                           UserId = x.UserId,
                           CourtId = x.CourtId,
                           ClientIP = x.ClientIP
                       })
                       .AsQueryable();
        }

        public bool SaveLog(AuditLog model)
        {
            try
            {
                //Гърми ако се използва repo от DI тук, понеже контролера се disposе-ва 
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

                using (var dbContext = new ApplicationDbContext(optionsBuilder.Options))
                {

                    model.Id = Guid.NewGuid();
                    model.InsertedDate = DateTime.Now;
                    model.CourtId = model.CourtId.EmptyToNull(0).EmptyToNull(-1);
                    model.UserId = model.UserId.EmptyToNull();
                    dbContext.AuditLog.Add(model);
                    dbContext.SaveChanges();
                    return true;

                }

            }
            catch { return false; }
        }

    }
}
