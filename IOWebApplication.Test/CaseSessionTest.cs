// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Test.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Test
{
    [TestFixture]
    public class CaseSessionTest
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
        public void IsExistMainResultTrueTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionResult()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    SessionResultId = 1,
                    SessionResultBaseId = 1,
                    Description = "Test",
                    IsActive = true,
                    IsMain = true
                };
                repo.Add<CaseSessionResult>(saved);

                var result = service.IsExistMainResult(1, 2);

                Assert.IsTrue(result);
            }
        }

        [Test]
        public void IsExistMainResultFalseTest()
        {
            using (serviceProvider.CreateScope())
            {
                var service = serviceProvider.GetService<ICaseSessionService>();
                var userContext = serviceProvider.GetService<IUserContext>();
                var repo = serviceProvider.GetService<IRepository>();

                var saved = new CaseSessionResult()
                {
                    CaseId = 1,
                    CourtId = 1,
                    CaseSessionId = 1,
                    SessionResultId = 1,
                    SessionResultBaseId = 1,
                    Description = "Test",
                    IsActive = true,
                    IsMain = false
                };
                repo.Add<CaseSessionResult>(saved);

                var result = service.IsExistMainResult(1, 2);

                Assert.IsFalse(result);
            }
        }
    }
}
