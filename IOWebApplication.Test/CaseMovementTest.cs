using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
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
    public class CaseMovementTest
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
        public void CreateMovementTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseMovementService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseMovementVM()
                {
                    CaseId = 1,
                    CourtId = 1,
                    MovementTypeId = 1,
                    ToUserId = "1",
                    CourtOrganizationId = 1,
                    OtherInstitution = "Test",
                    DateSend = DateTime.Now,
                    AcceptUserId = null,
                    DateAccept = null,
                    Description = "Description",
                    AcceptDescription = "AcceptDescription",
                    UserId = userContext.UserId,

                };

                service.CreateMovement(saved);
                var caseMovement = repo.AllReadonly<CaseMovement>().FirstOrDefault();

                Assert.That(saved.CaseId, Is.EqualTo(caseMovement.CaseId));
                Assert.That(saved.CourtId, Is.EqualTo(caseMovement.CourtId));
                Assert.That(saved.MovementTypeId, Is.EqualTo(caseMovement.MovementTypeId));
                Assert.That(saved.ToUserId, Is.EqualTo(caseMovement.ToUserId));
                Assert.That(saved.CourtOrganizationId, Is.EqualTo(caseMovement.CourtOrganizationId));
                Assert.That(saved.OtherInstitution, Is.EqualTo(caseMovement.OtherInstitution));
                Assert.That(saved.Description, Is.EqualTo(caseMovement.Description));
                Assert.That(saved.UserId, Is.EqualTo(caseMovement.UserId));
            }
        }

        [Test]
        public void StornoMovementTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseMovementService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var model = new CaseMovement()
                {
                    Id = 1,
                    CaseId = 1,
                    CourtId = 1,
                    MovementTypeId = 1,
                    ToUserId = "1",
                    CourtOrganizationId = 1,
                    OtherInstitution = "Test",
                    DateSend = DateTime.Now,
                    AcceptUserId = null,
                    DateAccept = null,
                    Description = "Description",
                    AcceptDescription = "AcceptDescription",
                    UserId = userContext.UserId,
                    DisableDescription = "",
                    IsActive = true
                };

                var saved = new CaseMovementVM()
                {
                    Id = 1,
                    CaseId = 1,
                    CourtId = 1,
                    MovementTypeId = 1,
                    ToUserId = "1",
                    CourtOrganizationId = 1,
                    OtherInstitution = "Test",
                    DateSend = DateTime.Now,
                    AcceptUserId = null,
                    DateAccept = null,
                    Description = "Description",
                    AcceptDescription = "AcceptDescription",
                    UserId = userContext.UserId,
                    DisableDescription = "Test",
                    IsAccept = true
                };
                repo.Add<CaseMovement>(model);

                service.StornoMovement(saved);
                var caseMovement = repo.AllReadonly<CaseMovement>().FirstOrDefault();

                Assert.That(false, Is.EqualTo(caseMovement.IsActive));
                Assert.That(saved.DisableDescription, Is.EqualTo(caseMovement.DisableDescription));
            }
        }

        [Test]
        public void AcceptMovementTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseMovementService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var model = new CaseMovement()
                {
                    Id = 1,
                    CaseId = 1,
                    CourtId = 1,
                    MovementTypeId = 1,
                    ToUserId = "1",
                    CourtOrganizationId = 1,
                    OtherInstitution = "Test",
                    DateSend = DateTime.Now,
                    AcceptUserId = null,
                    DateAccept = null,
                    Description = "Description",
                    AcceptDescription = "AcceptDescription",
                    UserId = userContext.UserId,
                    DisableDescription = "",
                    IsActive = true
                };
                repo.Add<CaseMovement>(model);

                service.AcceptMovement(1);
                var caseMovement = repo.AllReadonly<CaseMovement>().FirstOrDefault();

                Assert.IsNotNull(caseMovement.DateAccept);
                Assert.IsNotNull(caseMovement.AcceptUserId);
            }
        }
    }
}
