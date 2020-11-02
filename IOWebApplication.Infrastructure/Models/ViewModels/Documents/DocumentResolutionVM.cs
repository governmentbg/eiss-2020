// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using AutoMapper;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentResolutionVM
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentNumber { get; set; }
        public int ResolutionTypeId { get; set; }
        public string ResolutionTypeName { get; set; }
        public string RegNumber { get; set; }
        public DateTime? RegDate { get; set; }
        public string JudgeName { get; set; }
        public string JudgeUserId { get; set; }
        public string StateName { get; set; }
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public string CourtCity { get; set; }
        public string Content { get; set; }

        public string GetFileTitle
        {
            get
            {
                if (RegDate != null)
                {
                    return $"{ResolutionTypeName} {RegNumber}/{RegDate:dd.MM.yyyy}";
                }
                else
                {
                    return $"{ResolutionTypeName}";
                }
            }
        }

        public static MapperConfiguration GetMapping()
        {
            var dtNow = DateTime.Now;
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DocumentResolution, DocumentResolutionVM>()
                    .ForMember(d => d.DocumentNumber, s => s.MapFrom(m => m.Document.DocumentNumber))
                    .ForMember(d => d.DocumentTypeName, s => s.MapFrom(m => m.Document.DocumentType.Label))
                    .ForMember(d => d.ResolutionTypeName, s => s.MapFrom(m => m.ResolutionType.Label))
                    .ForMember(d => d.JudgeUserId, s => s.MapFrom(m => m.JudgeDecisionUserId))
                    .ForMember(d => d.JudgeName, s => s.MapFrom(m => m.JudgeDecisionLawunit.FullName))
                    .ForMember(d => d.StateName, s => s.MapFrom(m => m.ResolutionState.Label))
                    .ForMember(d => d.CourtName, s => s.MapFrom(m => m.Court.Label))
                    .ForMember(d => d.CourtCity, s => s.MapFrom(m => m.Court.CityName))
                ;
            });


            /*
            Id = x.Id,
                                DocumentId = x.DocumentId,
                                ResolutionTypeId = x.ResolutionTypeId,
                                ResolutionTypeName = x.ResolutionType.Label,
                                JudgeName = x.JudgeDecisionLawunit.FullName,
                                JudgeUserId = x.JudgeDecisionUserId,
                                RegNumber = x.RegNumber,
                                RegDate = x.RegDate,
                                StateName = x.ResolutionState.Label,
                                CourtId = x.CourtId,
                                CourtName = x.Court.Label,
                                CourtCity = x.Court.CityName
            */
        }
    }
}
