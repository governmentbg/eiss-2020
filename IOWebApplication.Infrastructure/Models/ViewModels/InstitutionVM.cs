// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using AutoMapper;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class InstitutionVM
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public string EISPPCode { get; set; }

        public static MapperConfiguration GetMapping()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Institution, InstitutionVM>()
                   //.ForMember(dest => dest.TypeExamStatusName, opt => opt.MapFrom(src => (src.TypeExamStatus != null) ? src.TypeExamStatus.TypeExamStatusName : ""))
                   //.ForMember(dest => dest.UicTypeName, opt => opt.MapFrom(src => src.UicType.Name))
                   //.ForMember(dest => dest.SessionExamDate, opt => opt.MapFrom(src => src.Session.SessionExampDate))
                   //.ForMember(dest => dest.IsReadyApplList, opt => opt.MapFrom(src => src.Session.IsReadyApplList))
                   //.ForMember(dest => dest.IsReadyExamResList, opt => opt.MapFrom(src => src.Session.IsReadyExamResList))
                   //.ForMember(dest => dest.IsReadyForPublish, opt => opt.MapFrom(src => src.Session.IsReadyForPublish));
                   ;
            });

        }
    }
}
