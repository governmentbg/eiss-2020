// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class BaseIntegrationService : BaseService
    {

        public int getNomValueInt(string nomenclatureAlias, object value)
        {
            string resultStr = getNomValue(nomenclatureAlias, value);
            if (!string.IsNullOrEmpty(resultStr))
            {
                return int.Parse(resultStr);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Връща външен код на номенклатура
        /// </summary>
        /// <param name="nomenclatureAlias">alias на номенклатура от nom_code_mapping</param>
        /// <param name="value">вътрешно ID на номенклатура</param>
        /// <returns>Връща външен код на номенклатура ако го намери или дава Exception ако не го</returns>
        public string getNomValue(string nomenclatureAlias, object value)
        {
            string innerCode = value?.ToString();
            var result = repo.AllReadonly<CodeMapping>()
                            .Where(x => x.Alias == nomenclatureAlias && x.InnerCode == innerCode)
                            .Select(x => x.OuterCode)
                            .FirstOrDefault();
            if (result == null)
            {
                throw new Exception($"ЕПЕП/Ненамерена номенклатура: alias={nomenclatureAlias}; id={innerCode}");
            }
            return result;
        }

        public string getKey(int sourceType, long? sourceId)
        {
            return repo.AllReadonly<IntegrationKey>()
                                    .Where(x => x.IntegrationTypeId == NomenclatureConstants.IntegrationTypes.EPEP
                                            && x.SourceType == sourceType && x.SourceId == sourceId)
                                    .OrderBy(x => x.Id)
                                    .Select(x => x.OuterCode)
                                    .FirstOrDefault();
        }

        public Guid? getKeyGUID(int sourceType, long? sourceId)
        {
            string code = getKey(sourceType, sourceId);
            if (!string.IsNullOrEmpty(code))
            {
                try
                {
                    return Guid.Parse(code);
                }
                catch
                {
                }
            }
            return null;
        }
    }
}
