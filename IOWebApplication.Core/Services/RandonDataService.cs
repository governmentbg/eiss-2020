// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Dynamic;

namespace IOWebApplication.Core.Services
{
    public class RandonDataService : BaseService
    {

        Random rand = new Random();
        public RandonDataService(
            IRepository _repo,
            ILogger<RandonDataService> _logger,
            IUserContext _userContext)
        {
            repo = _repo;
            logger = _logger;
            userContext = _userContext;
        }


        DocumentVM generateDocument(int documentTypeId, int courtId)
        {
            DocumentVM doc = new DocumentVM();
            return doc;
        }


        PersonNamesBase generatePerson<TPersonType>() where TPersonType : PersonNamesBase
        {
            string[] names = { "Иван", "Петър", "Евгени", "Георги", "Димитър", "Прокопи", "Махмуд" };
            var person = (TPersonType)Activator.CreateInstance(typeof(TPersonType));


            person.FirstName = names[rand.Next(names.Length - 1)];
            person.FamilyName = names[rand.Next(names.Length - 1)] + "ов";
            person.FullName = $"{person.FirstName} {person.FamilyName}";
            person.UicTypeId = NomenclatureConstants.UicTypes.EGN;

            return person;
        }
    }
}
