using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
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
    public class CaseSessionMeetingTest
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
        public void CaseSessionMeeting_SaveDataTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionMeetingService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionMeetingEditVM()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    SessionMeetingTypeId = 1,
                    DateFrom = DateTime.Now,
                    DateTo = DateTime.Now,
                    Description = "Test",
                    IsActive = true,
                    IsAutoCreate = true,
                    CourtHallId = 1,
                };
                saved.CaseSessionMeetingUser = new List<Infrastructure.Models.ViewModels.CheckListVM>();
                saved.CaseSessionMeetingUser.Add(new Infrastructure.Models.ViewModels.CheckListVM()
                {
                    Checked = true,
                    Label = "Test",
                    Value = "1"
                });

                service.CaseSessionMeeting_SaveData(saved);
                var caseSessionMeeting = repo.AllReadonly<CaseSessionMeeting>().FirstOrDefault();
                var caseSessionMeetingUser = repo.AllReadonly<CaseSessionMeetingUser>().FirstOrDefault();

                Assert.That(saved.CaseId, Is.EqualTo(caseSessionMeeting.CaseId));
                Assert.That(saved.CourtId, Is.EqualTo(caseSessionMeeting.CourtId));
                Assert.That(saved.SessionMeetingTypeId, Is.EqualTo(caseSessionMeeting.SessionMeetingTypeId));
                Assert.That(saved.DateFrom, Is.EqualTo(caseSessionMeeting.DateFrom));
                Assert.That(saved.DateTo, Is.EqualTo(caseSessionMeeting.DateTo));
                Assert.That(saved.IsActive, Is.EqualTo(caseSessionMeeting.IsActive));
                Assert.That(saved.IsAutoCreate, Is.EqualTo(caseSessionMeeting.IsAutoCreate));
                Assert.That(saved.CourtHallId, Is.EqualTo(caseSessionMeeting.CourtHallId));
                Assert.That(saved.Description, Is.EqualTo(caseSessionMeeting.Description));

                Assert.That(saved.CaseSessionMeetingUser.Select(x => x.Value).FirstOrDefault(), Is.EqualTo(caseSessionMeetingUser.SecretaryUserId));
            }
        }

        [Test]
        public void CaseSessionMeetingEdit_ByIdTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionMeetingService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionMeeting()
                {
                    Id = 1,
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    SessionMeetingTypeId = 1,
                    DateFrom = DateTime.Now,
                    DateTo = DateTime.Now,
                    Description = "Test",
                    IsActive = true,
                    IsAutoCreate = true,
                    CourtHallId = 1,
                };
                repo.Add<CaseSessionMeeting>(saved);

                var caseSessionMeetingEditVM = service.CaseSessionMeetingEdit_ById(1);

                Assert.That(saved.CaseId, Is.EqualTo(caseSessionMeetingEditVM.CaseId));
                Assert.That(saved.CourtId, Is.EqualTo(caseSessionMeetingEditVM.CourtId));
                Assert.That(saved.SessionMeetingTypeId, Is.EqualTo(caseSessionMeetingEditVM.SessionMeetingTypeId));
                Assert.That(saved.DateFrom, Is.EqualTo(caseSessionMeetingEditVM.DateFrom));
                Assert.That(saved.DateTo, Is.EqualTo(caseSessionMeetingEditVM.DateTo));
                Assert.That(saved.IsActive, Is.EqualTo(caseSessionMeetingEditVM.IsActive));
                Assert.That(saved.IsAutoCreate, Is.EqualTo(caseSessionMeetingEditVM.IsAutoCreate));
                Assert.That(saved.CourtHallId, Is.EqualTo(caseSessionMeetingEditVM.CourtHallId));
                Assert.That(saved.Description, Is.EqualTo(caseSessionMeetingEditVM.Description));
            }
        }

        [Test]
        public void IsExistMeetengInSessionTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionMeetingService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionMeeting()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    SessionMeetingTypeId = 1,
                    DateFrom = DateTime.Now.AddHours(-1),
                    DateTo = DateTime.Now.AddHours(1),
                    Description = "Test",
                    IsActive = true,
                    IsAutoCreate = true,
                    CourtHallId = 1,
                };
                repo.Add<CaseSessionMeeting>(saved);

                Assert.IsTrue(service.IsExistMeetengInSession(DateTime.Now, DateTime.Now, 1));
                Assert.IsFalse(service.IsExistMeetengInSession(DateTime.Now.AddHours(-3), DateTime.Now.AddHours(-3), 1));
            }
        }

        [Test]
        public void CaseSessionMeetingAutoCreateGetBySessionIdTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionMeetingService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var savedAuto = new CaseSessionMeeting()
                {
                    Id = 1,
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    SessionMeetingTypeId = 1,
                    DateFrom = DateTime.Now,
                    DateTo = DateTime.Now,
                    Description = "Auto",
                    IsActive = true,
                    IsAutoCreate = true,
                    CourtHallId = 1,
                };
                repo.Add<CaseSessionMeeting>(savedAuto);

                var saved = new CaseSessionMeeting()
                {
                    Id = 2,
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    SessionMeetingTypeId = 1,
                    DateFrom = DateTime.Now,
                    DateTo = DateTime.Now,
                    Description = "Normal",
                    IsActive = true,
                    IsAutoCreate = false,
                    CourtHallId = 1,
                };
                repo.Add<CaseSessionMeeting>(saved);

                var caseSessionMeeting = service.CaseSessionMeetingAutoCreateGetBySessionId(1);

                Assert.That(savedAuto.Description, Is.EqualTo(caseSessionMeeting.Description));
            }
        }

        [Test]
        public void IsExistMeetengInSessionAfterDateTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionMeetingService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionMeeting()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    SessionMeetingTypeId = 1,
                    DateFrom = DateTime.Now.AddHours(-1),
                    DateTo = DateTime.Now.AddHours(1),
                    Description = "Test",
                    IsActive = true,
                    IsAutoCreate = true,
                    CourtHallId = 1,
                };
                repo.Add<CaseSessionMeeting>(saved);

                Assert.IsTrue(service.IsExistMeetengInSessionAfterDate(DateTime.Now, 1, null));
                Assert.IsFalse(service.IsExistMeetengInSessionAfterDate(DateTime.Now.AddHours(2), 1, null));
            }
        }
    }
}
