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
    public class CaseSessionActComplainTest
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
        public void IsExistComplainTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionActComplainService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionActComplain()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionActId = 1,
                    ComplainDocumentId = 1,
                    ComplainStateId = 1,
                };
                repo.Add<CaseSessionActComplain>(saved);

                Assert.IsTrue(service.IsExistComplain(1, 1));
                Assert.IsFalse(service.IsExistComplain(1, 2));
            }
        }

        [Test]
        public void IsExistComplainByDocumentIdDifferentStatusRecivedTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionActComplainService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionActComplain()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionActId = 1,
                    ComplainDocumentId = 1,
                    ComplainStateId = 2,
                };
                repo.Add<CaseSessionActComplain>(saved);

                var savedTwo = new CaseSessionActComplain()
                {
                    CaseId = 2,
                    CourtId = 1,
                    CaseSessionActId = 1,
                    ComplainDocumentId = 1,
                    ComplainStateId = 1,
                };
                repo.Add<CaseSessionActComplain>(savedTwo);

                Assert.IsTrue(service.IsExistComplainByDocumentIdDifferentStatusRecived(1));
                Assert.IsFalse(service.IsExistComplainByDocumentIdDifferentStatusRecived(2));
            }
        }

        [Test]
        public void CaseSessionActComplain_SaveDataTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionActComplainService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionActComplain()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionActId = 1,
                    ComplainDocumentId = 1,
                    ComplainStateId = 2,
                    DateWrt = DateTime.Now,
                    UserExpiredId = userContext.UserId,
                    RejectDescription = "Test"
                };

                service.CaseSessionActComplain_SaveData(saved);
                var caseSessionActComplain = repo.AllReadonly<CaseSessionActComplain>().FirstOrDefault();

                Assert.That(saved.CaseId, Is.EqualTo(caseSessionActComplain.CaseId));
                Assert.That(saved.CourtId, Is.EqualTo(caseSessionActComplain.CourtId));
                Assert.That(saved.CaseSessionActId, Is.EqualTo(caseSessionActComplain.CaseSessionActId));
                Assert.That(saved.ComplainDocumentId, Is.EqualTo(caseSessionActComplain.ComplainDocumentId));
                Assert.That(saved.ComplainStateId, Is.EqualTo(caseSessionActComplain.ComplainStateId));
                Assert.That(saved.UserExpiredId, Is.EqualTo(caseSessionActComplain.UserExpiredId));
                Assert.That(saved.RejectDescription, Is.EqualTo(caseSessionActComplain.RejectDescription));
            }
        }

        [Test]
        public void CaseSessionActComplainResult_SaveDataTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionActComplainService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var caseCase = new Case()
                {
                    Id = 1,
                    CourtId = 1,
                };
                repo.Add<Case>(caseCase);

                var saved = new CaseSessionActComplainResultEditVM()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionActId = 1,
                    ComplainCourtId = 1,
                    ComplainCaseId = 1,
                    CaseSessionActComplainId = 1,
                    ActResultId = 1,
                    Description = "Test",
                    DateResult = DateTime.Now,
                    IsStartNewLifecycle = false
                };

                service.CaseSessionActComplainResult_SaveData(saved);
                var caseSessionActComplain = repo.AllReadonly<CaseSessionActComplainResult>().FirstOrDefault();

                Assert.That(saved.CaseId, Is.EqualTo(caseSessionActComplain.CaseId));
                Assert.That(saved.CourtId, Is.EqualTo(caseSessionActComplain.CourtId));
                Assert.That(saved.CaseSessionActId, Is.EqualTo(caseSessionActComplain.CaseSessionActId));
                Assert.That(saved.ComplainCourtId, Is.EqualTo(caseSessionActComplain.ComplainCourtId));
                Assert.That(saved.ComplainCaseId, Is.EqualTo(caseSessionActComplain.ComplainCaseId));
                Assert.That(saved.CaseSessionActComplainId, Is.EqualTo(caseSessionActComplain.CaseSessionActComplainId));
                Assert.That(saved.ActResultId, Is.EqualTo(caseSessionActComplain.ActResultId));
                Assert.That(saved.Description, Is.EqualTo(caseSessionActComplain.Description));
            }
        }
    }
}
