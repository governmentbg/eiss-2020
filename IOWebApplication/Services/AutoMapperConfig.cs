// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using AutoMapper;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Services
{
    public static class AutoMapperConfig
    {
        public static IMapperConfigurationExpression RegisterMappings(this IMapperConfigurationExpression cfg)
        {
            //Common
            cfg.CreateMap<LawUnit, LawUnitH>();

            //Case
            cfg.CreateMap<Case, CaseH>();
            cfg.CreateMap<CasePerson, CasePersonH>();
            cfg.CreateMap<CasePersonAddress, CasePersonAddressH>();
            cfg.CreateMap<CaseSession, CaseSessionH>();
            cfg.CreateMap<CaseSessionAct, CaseSessionActH>();
            cfg.CreateMap<CaseNotification, CaseNotificationH>();

            return cfg;
        }
    }
}
