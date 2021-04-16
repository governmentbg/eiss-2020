using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
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
    public class MoneyTest
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
        public void Obligation_SaveDataTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<IMoneyService>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new ObligationEditVM()
                {
                    CourtId = 1,
                    CaseSessionActId = 1,
                    Description = "Test",
                    MoneyTypeId = 1,
                    MoneySign = 1,
                    IsActive = true,
                    Amount = 1.11M,
                };
                repo.Add<ObligationEditVM>(saved);

                var moneytype = new MoneyType()
                {
                    Id = 1,
                };
                repo.Add<MoneyType>(moneytype);

                service.Obligation_SaveData(saved);
                var money = repo.AllReadonly<ObligationEditVM>().FirstOrDefault();

                Assert.That(saved.CourtId, Is.EqualTo(money.CourtId));
                Assert.That(saved.CaseSessionActId, Is.EqualTo(money.CaseSessionActId));
                Assert.That(saved.Description, Is.EqualTo(money.Description));
                Assert.That(saved.MoneyTypeId, Is.EqualTo(money.MoneyTypeId));
                Assert.That(saved.MoneySign, Is.EqualTo(money.MoneySign));
                Assert.That(saved.IsActive, Is.EqualTo(money.IsActive));
            }
        }
    }
}
