// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за лист с избор за лица
    /// </summary>
    public class CasePersonForCheckListVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на лице
        /// </summary>
        public string PersonFullName { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        public string PersonUic { get; set; }

        /// <summary>
        /// Роля на лице
        /// </summary>
        public string PersonRoleLabel { get; set; }

        /// <summary>
        /// Данни за лице
        /// </summary>
        public string PersonLabel 
        {
            get
            {
                return PersonFullName +
                       (!string.IsNullOrEmpty(PersonUic) ? " (" + PersonUic + ")" : string.Empty) +
                       (!string.IsNullOrEmpty(PersonRoleLabel) ? " - " + PersonRoleLabel : string.Empty);
            }
        }

        /// <summary>
        /// До дата
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Уникален идентификатор
        /// </summary>
        public string CasePersonIdentificator { get; set; }
    }
}
