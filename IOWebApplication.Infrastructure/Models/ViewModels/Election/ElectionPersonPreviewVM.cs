// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Election
{
    public class ElectionPersonPreviewVM
    {
        public int Id { get; set; }
        public Guid PersonGid { get; set; }
        public int ElectionGroupId { get; set; }
        public int? DismissalTypeId { get; set; }
        public string ElectionTitle { get; set; }

        [Display(Name = "Имена")]
        public string FullName { get; set; }

        [Display(Name = "ЕГН")]
        public string Uic { get; set; }

        [Display(Name = "Съд")]
        public string CourtName { get; set; }

        public int StateId { get; set; }

        [Display(Name = "Статус")]
        public string StateName { get; set; }

        [Display(Name = "Описание")]
        public string StateNameDescription { get; set; }

        [Display(Name = "Причина за неучастие")]
        public string DismissalType { get; set; }

        [Display(Name = "Основание")]
        public string DismissalTypeDescription { get; set; }

        [Display(Name = "Дата на декларацията")]
        public DateTime? DateDeclaration { get; set; }
        public bool HasDeclaration
        {
            get
            {
                return this.DateDeclaration != null;
            }
        }
        public bool CanChange { get; set; }
    }
}
