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
    public class CaseSessionDocTest
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
        public void CaseSessionDoc_SaveDataTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionDocService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionDoc()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    DocumentId = 1,
                    SessionDocStateId = 1,
                    Description = "Test",
                    DateFrom = DateTime.Now,
                    DateTo = null
                };

                service.CaseSessionDoc_SaveData(saved);
                var caseSessionDoc = repo.AllReadonly<CaseSessionDoc>().FirstOrDefault();

                Assert.That(saved.CaseId, Is.EqualTo(caseSessionDoc.CaseId));
                Assert.That(saved.CourtId, Is.EqualTo(caseSessionDoc.CourtId));
                Assert.That(saved.CaseSessionId, Is.EqualTo(caseSessionDoc.CaseSessionId));
                Assert.That(saved.DateFrom, Is.EqualTo(caseSessionDoc.DateFrom));
                Assert.That(saved.DateTo, Is.EqualTo(caseSessionDoc.DateTo));
                Assert.That(saved.DocumentId, Is.EqualTo(caseSessionDoc.DocumentId));
                Assert.That(saved.Description, Is.EqualTo(caseSessionDoc.Description));
            }
        }

        [Test]
        public void IsExistDocumentIdDifferentStatusNerazgledanTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionDocService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionDoc()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    DocumentId = 1,
                    SessionDocStateId = 1,
                    Description = "Test",
                    DateFrom = DateTime.Now,
                    DateTo = null
                };
                repo.Add<CaseSessionDoc>(saved);

                var savedOther = new CaseSessionDoc()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    DocumentId = 2,
                    SessionDocStateId = 2,
                    Description = "Test",
                    DateFrom = DateTime.Now,
                    DateTo = null
                };
                repo.Add<CaseSessionDoc>(savedOther);

                Assert.IsTrue(service.IsExistDocumentIdDifferentStatusNerazgledan(2));
                Assert.IsFalse(service.IsExistDocumentIdDifferentStatusNerazgledan(1));
            }
        }
    }
}
