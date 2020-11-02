// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.Integrations.Experts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IExpertsSearchService
    {
        Task<bool> Update();

        /// <summary>
        /// Търсене на вещи лица в регистъра
        /// </summary>
        /// <param name="competenceCode">Код на специалност (по номенклатура на регистъра)</param>
        /// <param name="keyword">Име или входящ номер</param>
        /// <param name="start">Брой пропуснати вещи лица</param>
        /// <param name="length">Брой върнати вещи лица</param>
        /// <param name="region">Код на Съдебен район (по номенклатура на регистъра)</param>
        /// <returns></returns>
        Task<List<ExpertIntegrationModel>> SearchExperts(string competenceCode = "any", string keyword = null, int start = 0, int length = 20, string region = "any");
    }
}
