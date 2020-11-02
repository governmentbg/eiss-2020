// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using AutoMapper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class LawUnitVM
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string[] CourtList { get; set; }

        public static MapperConfiguration GetMapping()
        {
            var dtNow = DateTime.Now;
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LawUnit, LawUnitVM>()
                    .ForMember(dest => dest.CourtList, s => s.MapFrom(m => m.Courts.AsQueryable()
                    .Where(x => NomenclatureConstants.PeriodTypes.CurrentlyAvailableExtended.Contains(x.PeriodTypeId))
                    .Where(x => x.DateFrom <= dtNow && (x.DateTo ?? DateTime.MaxValue) >= dtNow)
                    .Select(x => $"{x.Court.Label} ({x.PeriodType.Code}{((x.PeriodTypeId == NomenclatureConstants.PeriodTypes.ActAs) ? " " + x.LawUnitType.Label : "")})")));
                ;
            });
        }
    }

    public class LawUnitFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Специалност")]
        public int SpecialityId { get; set; }

        [Display(Name = "Имена")]
        public string FullName { get; set; }
    }

    public class JuryYearDays
    {
        public int Id { get; set; }
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
        [Display(Name = "Специалност")]
        public string SpecialityName { get; set; }

        public int SpecialityId { get; set; }

        [Display(Name = "Имена")]
        public string FullName { get; set; }
        [Display(Name = "Брой участия(дни)")]
        public int? DaysCount { get; set; }
        [Display(Name = "Брой насрочени (дни)")]
        public int? DaysCountAppointed { get; set; }
        [Display(Name = "Общо (дни)")]
        public int? DaysCountTotal
        {
            get { return (this.DaysCount ?? 0) + (this.DaysCountAppointed ?? 0); }

        }
        public int? SessionStateID { get; set; }
    }
}

