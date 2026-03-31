// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за визуализация на данни за действителен зает щат в съд
    /// </summary>
    public class CourtJudgeCountVM
    {
        /// <summary>
        /// Име на съд
        /// </summary>
        public string CourtLabel { get; set; }

        /// <summary>
        /// Брой съдии в съд
        /// </summary>
        public int JudgeCount { get; set; }
    }
}
