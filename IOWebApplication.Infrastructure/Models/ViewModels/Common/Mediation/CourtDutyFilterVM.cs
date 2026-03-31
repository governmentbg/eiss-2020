// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Модел за филтър на CourtDuty
    /// </summary>
    public class CourtDutyFilterVM
    {
        /// <summary>
        /// Вид: null - дежурство, 1 - заместване
        /// </summary>
        public int? Kind { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int? CourtId { get; set; }
    }
}
