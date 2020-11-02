// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using AutoMapper;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;

namespace IOWebApplicationApi.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //Common
            CreateMap<LawUnit, LawUnitH>();

            //Case
            CreateMap<Case, CaseH>();
            CreateMap<CasePerson, CasePersonH>();
            CreateMap<CasePersonAddress, CasePersonAddressH>();
            CreateMap<CaseSession, CaseSessionH>();
            CreateMap<CaseSessionAct, CaseSessionActH>();
            CreateMap<CaseNotification, CaseNotificationH>();
        }
    }
}
