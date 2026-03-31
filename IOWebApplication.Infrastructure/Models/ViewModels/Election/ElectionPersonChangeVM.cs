// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Election
{
    public class ElectionPersonChangeVM
    {
        public int Id { get; set; }

        public int ElectionGroupId { get; set; }


        //--статус в протокола
        [Display(Name = "Участие в разпределение")]
        public int ElectionPersonStateId { get; set; }

        [Display(Name = "Основание")]
        public string DescriptionState { get; set; }
        //--статус в протокола

        //---отвода
        [Display(Name = "Причина за неучастие")]
        public int? ElectionPersonDismissalTypeId { get; set; }

        [Display(Name = "Основание за неучастие")]
        public string DescriptionRemoved { get; set; }
    }
}
