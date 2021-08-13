using Audit.Core;
using Audit.Mvc;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace IOWebApplication.Providers
{
    public class IOAuditLogDataProvider : AuditDataProvider
    {
        private readonly IConfiguration config;
        //private readonly IRepository repo;
        private readonly IUserContext userContext;

        //public IOAuditLogDataProvider(IConfiguration _config)
        //{
        //    config = _config;
        //}
        public IOAuditLogDataProvider(IConfiguration _config, IUserContext _userContext)
        {
            config = _config;
            userContext = _userContext;
        }

        public override object InsertEvent(AuditEvent auditEvent)
        {
            AuditLog logItem = new AuditLog()
            {
                InsertedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                CourtId = userContext.CourtId,
                UserId = userContext.UserId,
                Data = "{}"
                //,Data = auditEvent.ToJson()
            };

            logItem.ParseFromEvent(auditEvent);

            try
            {
                if (auditEvent is AuditEventMvcAction)
                {
                    var mvcEvent = (AuditEventMvcAction)auditEvent;
                    logItem.RequestUrl = mvcEvent.Action.RequestUrl;
                    logItem.Method = mvcEvent.Action.HttpMethod;
                }

            }
            catch { }

            if (logItem.CourtId > 0 && !string.IsNullOrEmpty(logItem.UserId))
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

                using (var dbContext = new ApplicationDbContext(optionsBuilder.Options))
                {
                    using (var repo = new Repository(dbContext))
                    {
                       
                        repo.Add(logItem);
                        repo.SaveChanges();
                        return logItem.Id;
                    }
                }
            }
            else
            {
                return 0;
            }


            
        }


        public override Task<object> InsertEventAsync(AuditEvent auditEvent)
        {
            return Task.FromResult(InsertEvent(auditEvent));
        }

        public override void ReplaceEvent(object eventId, AuditEvent auditEvent)
        {
            //var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            //optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

            //using (var dbContext = new ApplicationDbContext(optionsBuilder.Options))
            //{
            //    using (var repo = new Repository(dbContext))
            //    {
            //        var evt = repo.GetById<AuditLog>(eventId);

            //        if (evt != null)
            //        {
            //            evt.UpdatedDate = DateTime.Now;
            //            evt.Data = auditEvent.ToJson();

            //            repo.SaveChanges();
            //        }
            //    }
            //}
        }

        public override Task ReplaceEventAsync(object eventId, AuditEvent auditEvent)
        {
            ReplaceEvent(eventId, auditEvent);

            return Task.CompletedTask;
        }
    }
}
