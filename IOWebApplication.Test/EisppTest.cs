using IOWebApplication.Core.Contracts;
using IOWebApplication.Test.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Test
{
    [TestFixture]
    public class EisppTest
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
        public void CheckSumTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<IEisppService>();
                var configuration = serviceProvider.GetService<IConfiguration>();
                string[] testCodes = configuration.GetSection("Eispp:TestCodes").Get<string[]>();
                foreach (var code in testCodes)
                {
                    var checkCode = service.CheckSum(code);
                    Assert.That(checkCode, Is.EqualTo(code.Substring(12,2)));
                }    

            }
        }
    }
}
