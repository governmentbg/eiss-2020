using Integration.Epep;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Test.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace IOWebApplication.Test
{
    [TestFixture]
    public class DocumentTest
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
        public void DocumentInitTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<IDocumentService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                int documentDirection = 5;

                var document = service.Document_Init(documentDirection);

                Assert.That(document.DocumentDirectionId, Is.EqualTo(documentDirection));
                Assert.That(document.DocumentCaseInfo.CourtId, Is.EqualTo(userContext.CourtId));
                Assert.That(document.ProcessPriorityId, Is.EqualTo(DocumentConstants.ProcessPriority.Common));
            }
        }

        [Test]
        public void DocumentInitWithTemplateTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<IDocumentService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();
                int documentDirection = 5;
                DocumentTemplate template = new DocumentTemplate()
                {
                    Id = 100,
                    CourtId = userContext.CourtId,
                    Description = "Test Template",
                    DocumentKindId = 101,
                    DocumentGroupId = 102,
                    DocumentTypeId = 103
                };

                repo.Add(template);

                var document = service.Document_Init(documentDirection, template.Id);

                Assert.That(document.TemplateId, Is.EqualTo(template.Id));
                Assert.That(document.DocumentKindId, Is.EqualTo(template.DocumentKindId));
                Assert.That(document.DocumentGroupId, Is.EqualTo(template.DocumentGroupId));
                Assert.That(document.DocumentTypeId, Is.EqualTo(template.DocumentTypeId));
                Assert.That(document.Description, Is.EqualTo(template.Description));
            }
        }

        [Test]
        public void DocumentInitWithCaseTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<IDocumentService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();
                int documentDirection = 5;
                DocumentTemplate template = new DocumentTemplate()
                {
                    Id = 100,
                    CaseId = 200,
                };

                repo.Add(template);
                Infrastructure.Data.Models.Cases.Case templateCase = new Infrastructure.Data.Models.Cases.Case 
                {
                    Id = template.CaseId ?? 0,
                    CourtId = userContext.CourtId
                };

                repo.Add(templateCase);

                var document = service.Document_Init(documentDirection, template.Id);

                Assert.That(document.DocumentCaseInfo.CourtId, Is.EqualTo(templateCase.CourtId));
                Assert.That(document.DocumentCaseInfo.CaseId, Is.EqualTo(templateCase.Id));
                Assert.That(document.HasCaseInfo, Is.EqualTo(true));
            }
        }
    }
}