// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionActLawBaseService : BaseService, ICaseSessionActLawBaseService
    {
        public CaseSessionActLawBaseService(
            ILogger<CaseSessionActLawBaseService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        /// <summary>
        /// Извличане на данни за Нормативната текстове към акт
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActLawBaseVM> CaseSessionActLawBase_Select(int caseSessionActId)
        {
            return repo.AllReadonly<CaseSessionActLawBase>()
           .Include(x => x.LawBase)
           .Where(x => x.CaseSessionActId == caseSessionActId)
           .Select(x => new CaseSessionActLawBaseVM()
           {
               Id = x.Id,
               LawBaseName = x.LawBase.Label,
               DateFrom = x.DateFrom,
               DateTo = x.DateTo,
           }).AsQueryable();
        }

        /// <summary>
        /// Запис на Нормативната текстове към акт
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionActLawBase_SaveData(CaseSessionActLawBase model)
        {
            //Datefrom/Dateto се маха
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionActLawBase>(model.Id);
                    saved.LawBaseId = model.LawBaseId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.DateFrom = new DateTime(2000, 1, 1);
                    repo.Add<CaseSessionActLawBase>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseSessionActLawBase Id={ model.Id }");
                return false;
            }
        }

    }
}
