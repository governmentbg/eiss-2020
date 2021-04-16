using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Test.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Test
{
    [TestFixture]
    public class CaseEvidenceTest
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
        public void CaseEvidence_SaveDataTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseEvidenceService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseEvidence()
                {
                    CaseId = 1,
                    CourtId = 1,
                    EvidenceTypeId = 1,
                    FileNumber = "Test",
                    DateAccept = DateTime.Now,
                    Description = "Test",
                    AddInfo = "Test",
                    Location = "Test",
                    EvidenceStateId = 1,
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,
                    
                };

                service.CaseEvidence_SaveData(saved);
                var caseEvidence = repo.AllReadonly<CaseEvidence>().FirstOrDefault();

                Assert.That(saved.CaseId, Is.EqualTo(caseEvidence.CaseId));
                Assert.That(saved.CourtId, Is.EqualTo(caseEvidence.CourtId));
                Assert.That(saved.EvidenceTypeId, Is.EqualTo(caseEvidence.EvidenceTypeId));
                Assert.That(saved.FileNumber, Is.EqualTo(caseEvidence.FileNumber));
                Assert.That(saved.DateAccept, Is.EqualTo(caseEvidence.DateAccept));
                Assert.That(saved.Description, Is.EqualTo(caseEvidence.Description));
                Assert.That(saved.AddInfo, Is.EqualTo(caseEvidence.AddInfo));
                Assert.That(saved.Location, Is.EqualTo(caseEvidence.Location));
                Assert.That(saved.EvidenceStateId, Is.EqualTo(caseEvidence.EvidenceStateId));
                Assert.That(saved.DateWrt, Is.EqualTo(caseEvidence.DateWrt));
                Assert.That(saved.UserId, Is.EqualTo(caseEvidence.UserId));
            }
        }

        [Test]
        public void CaseEvidenceMovement_SaveDataTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseEvidenceService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseEvidenceMovement()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseEvidenceId = 1,
                    EvidenceMovementTypeId = 1,
                    MovementDate = DateTime.Now,
                    Description = "Test",
                    ActDescription = "Test",
                    CaseSessionActId = 1,
                    DateWrt = DateTime.Now,
                    UserId = userContext.UserId,

                };

                service.CaseEvidenceMovement_SaveData(saved);
                var caseEvidence = repo.AllReadonly<CaseEvidenceMovement>().FirstOrDefault();

                Assert.That(saved.CaseId, Is.EqualTo(caseEvidence.CaseId));
                Assert.That(saved.CourtId, Is.EqualTo(caseEvidence.CourtId));
                Assert.That(saved.CaseEvidenceId, Is.EqualTo(caseEvidence.CaseEvidenceId));
                Assert.That(saved.EvidenceMovementTypeId, Is.EqualTo(caseEvidence.EvidenceMovementTypeId));
                Assert.That(saved.MovementDate, Is.EqualTo(caseEvidence.MovementDate));
                Assert.That(saved.Description, Is.EqualTo(caseEvidence.Description));
                Assert.That(saved.ActDescription, Is.EqualTo(caseEvidence.ActDescription));
                Assert.That(saved.CaseSessionActId, Is.EqualTo(caseEvidence.CaseSessionActId));
                Assert.That(saved.DateWrt, Is.EqualTo(caseEvidence.DateWrt));
                Assert.That(saved.UserId, Is.EqualTo(caseEvidence.UserId));
            }
        }
    }
}
