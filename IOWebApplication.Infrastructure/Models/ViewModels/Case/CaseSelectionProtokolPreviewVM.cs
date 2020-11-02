// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
  public class CaseSelectionProtokolPreviewVM
  {
    public int Id { get; set; }

    public int CaseId { get; set; }
    public int CourtId { get; set; }

    [Display(Name = "")]
    public string CourtName { get; set; }

    [Display(Name = "Дата на разпределението:")]
    public string SelectionDate { get; set; }

    [Display(Name = "Разпределение по дело №:")]
    public string RegNumber { get; set; }

    [Display(Name = "")]
    public string JudgeRoleName { get; set; }
    public int? JudgeRoleId { get; set; }

    [Display(Name = "")]
    public int SelectionModeId { get; set; }

    [Display(Name = "Начин на разпределение:")]
    public string SelectionModeName { get; set; }

    [Display(Name = "Точен вид на делото:")]
    public string CaseTypeName { get; set; }

    [Display(Name = "Статистически шифър:")]
    public string CaseCodeName { get; set; }

    [Display(Name = "Година на делото:")]
    public int CaseYear { get; set; }

    [Display(Name = "Входящ номер на иницииращ документ:")]
    public string Document_Number { get; set; }

    [Display(Name = "Причина за ръчно разпределение:")]
    public string Description { get; set; }

    [Display(Name = "Разпределен съдия:")]
    public string SelectedLawUnitName { get; set; }

    public int? SelectedLawUnitId { get; set; }

    public string SelectedLawUnitTypeName { get; set; }


    [Display(Name = "Група по натовареност:")]
    public string LoadGroupLinkName { get; set; }

    [Display(Name = "Група на разпределение:")]
    public string CourtGroupName { get; set; }

    [Display(Name = "Дежурство разпределение:")]
    public string CourtDutyName { get; set; }

    [Display(Name = "Извършил разпределението:")]
    public string UserName { get; set; }
    public string UserUIC { get; set; }

    [Display(Name = "Добавяне на съдебния състав на Съдия-докладчика към делото ")]
    public bool IncludeComparementJudges { get; set; }
    public int? ComparementID { get; set; }
    [Display(Name = "Име на състав")]
    public string ComparentmentName { get; set; }
    public int SelectionProtokolStateId { get; set; }



    [Display(Name = "Досегашен съдия:")]
    public string PrevSelectedLawUnitName { get; set; }
    [Display(Name = "Разпределен от:")]
    public string PrevUserName { get; set; }
    [Display(Name = "Причина за преразпределение:")]
    public string DismisalReason { get; set; }

    public int ? DismisalId { get; set; }


    public IEnumerable<CaseSelectionProtokolLawUnitPreviewVM> LawUnits { get; set; }
    public CaseSelectionProtokolLawUnitPreviewVM[] CompartmentLawUnits { get; set; }


  }
}
