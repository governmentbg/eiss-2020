// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Election
{
    public class ElectionProtocolVM
    {
        public int Id { get; set; }


        public int ElectionGroupId { get; set; }
        public int ElectionGroupTypeID { get; set; }
        [Display(Name = "Сесия: ")]
        public string ElectionGroupLabel{ get; set; }

        [Display(Name = "Разпределение по преписка №: ")]
        public string DocumentNumber { get; set; }
        [Display(Name = "Начин на разпределение: ")]
        public string SelectionType { get; set; }
        [Display(Name = "Причина за преразпределение: ")]
        public string SelectionReazon { get; set; }

        //Нарочно е нулево, за да може да се запише без избрано и след записа на колекцията  ElectionPersons да се редактира на правилното

        public int? SelectedElectionPersonId { get; set; }

        public string CourtName { get; set; }
        public int? SelectedLawunitId { get; set; }

        [Display(Name = "Избран Съдия:  ")]
        public string SelectedLawunitFullName { get; set; }


        public int? SelectedLawunitCourtId { get; set; }
        public string SelectedLawunitCourtName { get; set; }

        public int? PrevLawunitId { get; set; }

        public string PrevLawunitFullName { get; set; }

        public int? PrevLawunitCourtId { get; set; }

        public string Description { get; set; }

        public string UserAddedId { get; set; }
        [Display(Name = "Извършил разпределението: ")]
        public string UserAddedName { get; set; }

        [Display(Name = "Дата на разпределението: ")]
        public DateTime? DateElection { get; set; }
        public DateTime DateAdded { get; set; }
        public string DateAddedString { get; set; }

        public DateTime? DateSigned { get; set; }
        public string DateSignedString { get; set; }
        public bool IsSigned { get; set; }

        public string FileId { get; set; }
        public string FileName { get; set; }
        public IEnumerable<ElectionProtocolPersonVM> ElectionProtocolPersons { get; set; }

        public ElectionProtocolVM()
        {
            ElectionProtocolPersons = new List<ElectionProtocolPersonVM>();
        }
    }
}
