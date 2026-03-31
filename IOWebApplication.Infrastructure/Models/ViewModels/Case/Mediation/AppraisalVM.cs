// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за оценки
    /// </summary>
    public class AppraisalVM
    {
        /// <summary>
        /// Сбор от оценките
        /// </summary>
        public int SumAppraisal { set; get; }

        public string TextAppraisal
        {
            get
            {
                return SumAppraisal == -1 ? "Няма оценка" :
                                            (SumAppraisal >= 0 && SumAppraisal <= 14 ? "Лоша" :
                                                                                       (SumAppraisal >= 15 && SumAppraisal <= 28 ? "Задоволителна" :
                                                                                                                                   (SumAppraisal >= 29 && SumAppraisal <= 42 ? "Добра" :
                                                                                                                                                                               (SumAppraisal >= 43 && SumAppraisal <= 56 ? "Много добра" :
                                                                                                                                                                                                                           "Отлична"))));
            }
        }
    }
}
