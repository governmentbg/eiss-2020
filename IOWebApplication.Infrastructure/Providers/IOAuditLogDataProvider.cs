// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Audit.Core;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Providers
{
    public class IOAuditLogDataProvider : AuditDataProvider
    {
        private readonly IConfiguration config;

        public IOAuditLogDataProvider(IConfiguration _config)
        {
            config = _config;
        }

        public override object InsertEvent(AuditEvent auditEvent)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

            using (var dbContext = new ApplicationDbContext(optionsBuilder.Options))
            {
                using (var repo = new Repository(dbContext))
                {
                    AuditLog logItem = new AuditLog()
                    {
                        InsertedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        Data = auditEvent.ToJson()
                    };

                    repo.Add(logItem);
                    repo.SaveChanges();
                    return logItem.Id;
                }
            }
        }


        public override Task<object> InsertEventAsync(AuditEvent auditEvent)
        {
            return Task.FromResult(InsertEvent(auditEvent));
        }

        public override void ReplaceEvent(object eventId, AuditEvent auditEvent)
        {
        }

        public override Task ReplaceEventAsync(object eventId, AuditEvent auditEvent)
        {
            ReplaceEvent(eventId, auditEvent);

            return Task.CompletedTask;
        }
    }
}
