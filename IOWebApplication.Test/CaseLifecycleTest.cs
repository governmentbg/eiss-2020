using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Test.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Test
{
    [TestFixture]
    public class CaseLifecycleTest
    {
        private ServiceProvider serviceProvider { get; set; }

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddTestServices();
            serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public void CaseLifecycle_SaveDataTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseLifecycleService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseLifecycle()
                {
                    CaseId = 1,
                    CourtId = 1,
                    LifecycleTypeId = 1,
                    Iteration = 1,
                    DateFrom = DateTime.Now,
                    DateTo = DateTime.Now,
                    DurationMonths = 1,
                    CaseSessionActId = 1,
                    CaseSessionResultId = 1,
                    Description = "Test",
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,

                };

                service.CaseLifecycle_SaveData(saved);
                var caseLifecycle = repo.AllReadonly<CaseLifecycle>().FirstOrDefault();

                Assert.That(saved.CaseId, Is.EqualTo(caseLifecycle.CaseId));
                Assert.That(saved.CourtId, Is.EqualTo(caseLifecycle.CourtId));
                Assert.That(saved.LifecycleTypeId, Is.EqualTo(caseLifecycle.LifecycleTypeId));
                Assert.That(saved.Iteration, Is.EqualTo(caseLifecycle.Iteration));
                Assert.That(saved.DateFrom, Is.EqualTo(caseLifecycle.DateFrom));
                Assert.That(saved.DateTo, Is.EqualTo(caseLifecycle.DateTo));
                Assert.That(saved.DurationMonths, Is.EqualTo(caseLifecycle.DurationMonths));
                Assert.That(saved.CaseSessionActId, Is.EqualTo(caseLifecycle.CaseSessionActId));
                Assert.That(saved.CaseSessionResultId, Is.EqualTo(caseLifecycle.CaseSessionResultId));
                Assert.That(saved.Description, Is.EqualTo(caseLifecycle.Description));
                Assert.That(saved.DateWrt, Is.EqualTo(caseLifecycle.DateWrt));
                Assert.That(saved.UserId, Is.EqualTo(caseLifecycle.UserId));
            }
        }

        [Test]
        public void CaseLifecycle_CloseIntervalTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseLifecycleService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseLifecycle()
                {
                    CaseId = 1,
                    CourtId = 1,
                    LifecycleTypeId = 1,
                    Iteration = 1,
                    DateFrom = DateTime.Now.AddMonths(-2),
                    DateTo = null,
                    DurationMonths = 0,
                    CaseSessionActId = 0,
                    CaseSessionResultId = 0,
                    Description = "Test",
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                };
                repo.Add<CaseLifecycle>(saved);

                var dateTo = DateTime.Now;
                service.CaseLifecycle_CloseInterval(1, 1, dateTo);
                var caseLifecycle = repo.AllReadonly<CaseLifecycle>().FirstOrDefault();

                Assert.That(dateTo, Is.EqualTo(caseLifecycle.DateTo));
                Assert.That(1, Is.EqualTo(caseLifecycle.CaseSessionActId));
                Assert.IsTrue(caseLifecycle.DurationMonths > 0);
            }
        }

        [Test]
        public void CaseLifecycle_UndoCloseIntervalTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseLifecycleService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseLifecycle()
                {
                    CaseId = 1,
                    CourtId = 1,
                    LifecycleTypeId = 1,
                    Iteration = 1,
                    DateFrom = DateTime.Now.AddMonths(-2),
                    DateTo = DateTime.Now,
                    DurationMonths = 2,
                    CaseSessionActId = 1,
                    CaseSessionResultId = 0,
                    Description = "Test",
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                };
                repo.Add<CaseLifecycle>(saved);

                service.CaseLifecycle_UndoCloseInterval(1, 1);
                var caseLifecycle = repo.AllReadonly<CaseLifecycle>().FirstOrDefault();

                Assert.That(null, Is.EqualTo(caseLifecycle.DateTo));
                Assert.That(null, Is.EqualTo(caseLifecycle.CaseSessionActId));
                Assert.That(0, Is.EqualTo(caseLifecycle.DurationMonths));
            }
        }

        [Test]
        public void CaseLifecycle_IsExistLifcycleAfterTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseLifecycleService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseLifecycle()
                {
                    Id = 1,
                    CaseId = 1,
                    CourtId = 1,
                    LifecycleTypeId = 1,
                    Iteration = 1,
                    DateFrom = DateTime.Now.AddMonths(-2),
                    DateTo = DateTime.Now,
                    DurationMonths = 2,
                    CaseSessionActId = 1,
                    CaseSessionResultId = 0,
                    Description = "Test",
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                };
                repo.Add<CaseLifecycle>(saved);

                var result = service.CaseLifecycle_IsExistLifcycleAfter(1, 1);

                Assert.That(false, Is.EqualTo(result));

                var savedLast = new CaseLifecycle()
                {
                    Id = 2,
                    CaseId = 1,
                    CourtId = 1,
                    LifecycleTypeId = 1,
                    Iteration = 2,
                    DateFrom = DateTime.Now.AddMonths(-1),
                    DateTo = null,
                    DurationMonths = 0,
                    CaseSessionActId = 0,
                    CaseSessionResultId = 0,
                    Description = "Test",
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                };
                repo.Add<CaseLifecycle>(savedLast);

                result = service.CaseLifecycle_IsExistLifcycleAfter(1, 1);
                Assert.That(true, Is.EqualTo(result));
            }
        }

        [Test]
        public void CaseLifecycle_NewIntervalSaveTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseLifecycleService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseLifecycle()
                {
                    Id = 1,
                    CaseId = 1,
                    CourtId = 1,
                    LifecycleTypeId = 1,
                    Iteration = 1,
                    DateFrom = DateTime.Now.AddMonths(-2),
                    DateTo = DateTime.Now.AddHours(-1),
                    DurationMonths = 2,
                    CaseSessionActId = 1,
                    CaseSessionResultId = 0,
                    Description = "Test",
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                };
                repo.Add<CaseLifecycle>(saved);

                var dateFrom = DateTime.Now;
                service.CaseLifecycle_NewIntervalSave(1, dateFrom);
                var caseLifecycle = repo.AllReadonly<CaseLifecycle>().OrderByDescending(x => x.DateFrom).FirstOrDefault();

                Assert.That(dateFrom, Is.EqualTo(caseLifecycle.DateFrom));
                Assert.That(2, Is.EqualTo(caseLifecycle.Iteration));
            }
        }

        [Test]
        public void CaseLifecycle_IsAllLifcycleCloseTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseLifecycleService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseLifecycle()
                {
                    Id = 1,
                    CaseId = 1,
                    CourtId = 1,
                    LifecycleTypeId = 1,
                    Iteration = 1,
                    DateFrom = DateTime.Now.AddMonths(-2),
                    DateTo = DateTime.Now.AddHours(-1),
                    DurationMonths = 2,
                    CaseSessionActId = 1,
                    CaseSessionResultId = 0,
                    Description = "Test",
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                };
                repo.Add<CaseLifecycle>(saved);

                var result = service.CaseLifecycle_IsAllLifcycleClose(1);

                Assert.That(true, Is.EqualTo(result));

                var savedLast = new CaseLifecycle()
                {
                    Id = 1,
                    CaseId = 1,
                    CourtId = 1,
                    LifecycleTypeId = 1,
                    Iteration = 2,
                    DateFrom = DateTime.Now,
                    DateTo = null,
                    DurationMonths = 0,
                    CaseSessionActId = 0,
                    CaseSessionResultId = 0,
                    Description = "Test",
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                };
                repo.Add<CaseLifecycle>(savedLast);

                result = service.CaseLifecycle_IsAllLifcycleClose(1);
                Assert.That(false, Is.EqualTo(result));
            }
        }
    }
}
