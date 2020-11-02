// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseDeactivationVM
    {
        public static string RegisterName
        {
            get
            {
                return "Регистър анулирани дела";
            }
        }

        public int Id { get; set; }
        public int CaseId { get; set; }
        public string CourtName { get; set; }
        [Display(Name = "Група на разпределение")]
        public string CourtGroupName { get; set; }
        [Display(Name = "Група по натовареност")]
        public string LoadGroupName { get; set; }
        [Display(Name = "Точен вид на делото")]
        public string CaseTypeName { get; set; }
        [Display(Name = "Статистически шифър")]
        public string CaseCodeName { get; set; }
        [Display(Name = "Входящ номер на иницииращ документ")]
        public string DocumentNumber { get; set; }
        [Display(Name = "Дело No")]
        public string CaseNumber { get; set; }
        [Display(Name = "Дата на образуване")]
        public DateTime CaseDate { get; set; }
        [Display(Name = "Година на делото")]
        public int CaseYear
        {
            get
            {
                return CaseDate.Year;
            }
        }
        [Display(Name = "Дата на анулиране")]
        public DateTime DateWrt { get; set; }
        public DateTime? DeclaredDate { get; set; }
        [Display(Name = "Причина за анулиране на делото")]
        public string Description { get; set; }
        public string DeactivateUserName { get; set; }
        public string DeactivateUserUIC { get; set; }
    }

    public class CaseDeactivationFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер дело")]
        public string RegNumber { get; set; }

        public int? Id { get; set; }
    }
}
