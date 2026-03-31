// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions.HTML;
using IOWebApplication.Infrastructure.Models.Integrations.Cais;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;



namespace IOWebApplication.Core.Services
{
    public class CaisBuletinService : BaseService, ICaisBuletinService
    {
        private readonly ICounterService counterService;

        public CaisBuletinService(
            ILogger<CaisBuletinService> logger,
            IRepository repo,
            IUserContext userContext,
            ICounterService counterService)
        {
            this.logger = logger;
            this.repo = repo;
            this.userContext = userContext;
            this.counterService = counterService;
        }

        public IQueryable<CasePersonBulletinFileVM> SelectFiles(int bulletinId)
        {
            return repo.AllReadonly<CasePersonSentenceBulletinFile>()
                        .Where(x => x.CasePersonSentenceBulletinId == bulletinId)
                        .OrderBy(x => x.Id)
                        .Select(x => new CasePersonBulletinFileVM
                        {
                            Id = x.Id,
                            DateWrt = x.DateWrt,
                            RegDate = x.RegDate,
                            RegNumber = x.RegNumber,
                            DateSigned = x.DateSigned,
                            DateRegisteredInCais = x.DateRegisteredInCais
                        });
        }

        public async Task<int> InitOrGetBulletinFile(int bulletinId, bool newNumber)
        {
            var lastFile = await repo.AllReadonly<CasePersonSentenceBulletinFile>()
                      .Where(x => x.CasePersonSentenceBulletinId == bulletinId)
                      .OrderByDescending(x => x.Id)
                      .FirstOrDefaultAsync();

            if (lastFile != null)
            {
                //Докато не е регистриран, винаги връща последния подготвен файл
                if (lastFile.RegDate == null)
                {
                    return lastFile.Id;
                }
                else
                {
                    if (!newNumber)
                    {
                        return lastFile.Id;
                    }
                }
            }

            var newFile = new CasePersonSentenceBulletinFile()
            {
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
                CasePersonSentenceBulletinId = bulletinId
            };
            if (lastFile != null && !newNumber)
            {
                newFile.RegNumber = lastFile.RegNumber;
                newFile.RegDate = lastFile.RegDate;
            }

            if (!string.IsNullOrEmpty(newFile.RegNumber))
            {

            }

            repo.Add(newFile);
            await repo.SaveChangesAsync();
            return newFile.Id;
        }

        public async Task<SaveResultVM> RegisterBulletinFile(int bulletinFileId)
        {
            var file = await ReadByIdAsync<CasePersonSentenceBulletinFile>(bulletinFileId);
            if (!string.IsNullOrEmpty(file.RegNumber))
            {
                return new SaveResultVM(true);
            }

            var courtId = await GetPropByIdAsync<CasePersonSentenceBulletin, int>(file.CasePersonSentenceBulletinId, x => x.CourtId ?? 0);
            var counterOk = counterService.Counter_GetCaseBulletinFileCounter(file, courtId);
            if (counterOk)
            {
                await repo.SaveChangesAsync();
            }
            return new SaveResultVM();
        }



        #region Изчитане на ЕИСС модел с данни за бюлетин
        public async Task<CaisBuletinModel> InitBuletinModel(int casePersonBulletinId)
        {
            CaisBuletinModel model = new CaisBuletinModel()
            {
                BuletinInfo = new CaisBuletinInfoModel()
            };

            var buletinInfo = await repo.AllReadonly<CasePersonSentenceBulletin>()
                                        .Where(x => x.Id == casePersonBulletinId)
                                        .Select(x => new
                                        {
                                            x.Id,
                                            x.CasePersonId,
                                            LastFile = x.ExportFiles.OrderByDescending(f => f.Id).FirstOrDefault()
                                        }).FirstOrDefaultAsync();

            var info = await repo.AllReadonly<CasePersonSentence>()
                                 .Where(x => x.CasePersonId == buletinInfo.CasePersonId)
                                 .Where(x => x.IsActive == true)
                                 .OrderByDescending(x => x.Id)
                                 .Select(x => new
                                 {
                                     x.CasePersonId,
                                     CasePersonSentenceId = x.Id,
                                     x.CaseId,
                                     SentenceDescription = x.Description
                                 })
                                 .FirstOrDefaultAsync();

            model.BuletinInfo.SentenceDescription = info.SentenceDescription.Decode();

            await initContext(model, casePersonBulletinId);

            await initMainInfo(model, info.CasePersonId, casePersonBulletinId);

            await initIdentityDocuments(model, info.CasePersonId);

            await initAct(model, info.CasePersonSentenceId);

            await initCase(model, info.CaseId);

            await initCrimes(model, info.CasePersonId, info.CasePersonSentenceId);

            //await initProbations(model, info.CasePersonId);

            return model;
        }
        async Task initContext(CaisBuletinModel buletinModel, int casePersonBulletinId)
        {
            var buletinInfo = await repo.AllReadonly<CasePersonSentenceBulletin>()
                                        .Where(x => x.Id == casePersonBulletinId)
                                        .Select(x => new
                                        {
                                            CourtName = x.Court.Label,
                                            CourtUic = x.Court.Uic,
                                            CourtEISPP = x.Court.EISPPCode,
                                            JudgeFirstName = x.LawUnitSign.FirstName,
                                            JudgeMiddleName = x.LawUnitSign.MiddleName,
                                            JudgeFamilyName = x.LawUnitSign.FamilyName,
                                            JudgeFullName = x.LawUnitSign.FullName
                                        })
                                        .FirstOrDefaultAsync();

            if (buletinInfo == null)
            {
                return;
            }

            var lastFileInfo = await repo.AllReadonly<CasePersonSentenceBulletinFile>()
                                        .Where(x => x.CasePersonSentenceBulletinId == casePersonBulletinId)
                                        .OrderByDescending(x => x.Id)
                                        .Select(x => new
                                        {
                                            x.DateWrt,
                                            x.RegDate,
                                            UserFirstName = x.User.LawUnit.FirstName,
                                            UserMiddleName = x.User.LawUnit.MiddleName,
                                            UserFamilyName = x.User.LawUnit.FamilyName,
                                            UserFullName = x.User.LawUnit.FullName,
                                            LawUnitId = x.User.LawUnitId,
                                            CourtId = x.CasePersonSentenceBulletin.CourtId ?? 0
                                        })
                                        .FirstOrDefaultAsync();

            var context = new CaisBuletinContextModel()
            {
                AuthorityEIK = buletinInfo.CourtUic,
                AuthorityEISPP = buletinInfo.CourtEISPP,
                AuthorityName = buletinInfo.CourtName,

                ApproverPerson = new CaisBuletinPersonModel()
                {
                    FirstName = buletinInfo.JudgeFirstName,
                    MiddleName = buletinInfo.JudgeMiddleName,
                    FamilyName = buletinInfo.JudgeFamilyName,
                    FullName = buletinInfo.JudgeFullName,
                    PositionName = "Съдия"
                },
            };

            if (lastFileInfo != null)
            {
                context.CreateDate = lastFileInfo.RegDate ?? lastFileInfo.DateWrt;
                context.CreatorPerson = new CaisBuletinPersonModel()
                {
                    FirstName = lastFileInfo.UserFirstName,
                    MiddleName = lastFileInfo.UserMiddleName,
                    FamilyName = lastFileInfo.UserFamilyName,
                    FullName = lastFileInfo.UserFullName,
                    PositionName = await GetLawUnitPositionByLawUnitId(lastFileInfo.LawUnitId, lastFileInfo.CourtId)
                };
            }
            else
            {
                context.CreateDate = DateTime.Now;
                var currentUser = new CaisBuletinPersonModel();
                if (userContext.UserId != null)
                {
                    //Ако текущия потребител е логнат в ЕИСС
                    currentUser.ParseNames(userContext.FullName);
                    currentUser.PositionName = await GetLawUnitPositionByLawUnitId(userContext.LawUnitId, userContext.CourtId);
                }
                else
                {
                    //Ако модела се създава от EissProxy няма правилен userContext
                    currentUser.ParseNames("Валидиране бюлетин");
                    currentUser.PositionName = "система";
                }
                context.CreatorPerson = currentUser;
            }

            buletinModel.Context = context;
        }

        /// <summary>
        /// Метод връщащ позичията при назначаване
        /// </summary>
        /// <param name="lawUnitId">Идентификатор на записа в LawUnit</param>
        /// <returns></returns>
        private async Task<string> GetLawUnitPositionByLawUnitId(int lawUnitId, int courtId)
        {
            DateTime dateNow = DateTime.Now;

            return await repo.AllReadonly<CourtLawUnit>()
                             .Where(x => x.LawUnitId == lawUnitId)
                             .Where(x => x.CourtId == courtId)
                             .Where(x => x.DateFrom.Date <= dateNow.Date)
                             .Where(x => (x.DateTo ?? dateNow).Date >= dateNow.Date)
                             .OrderByDescending(x => x.DateFrom)
                             .Select(x => x.LawUnitPosition.Label)
                             .FirstOrDefaultAsync();
        }

        async Task initMainInfo(CaisBuletinModel buletinModel, int casePersonId, int casePersonBulletinId)
        {
            var info = await repo.AllReadonly<CasePerson>()
                                 .Where(x => x.Id == casePersonId)
                                 .Select(x => new
                                 {
                                     x.FirstName,
                                     x.MiddleName,
                                     x.FamilyName,
                                     x.Family2Name,
                                     x.FullName,
                                     GenderId = x.GenderId ?? 0,
                                     x.LatinName,
                                     x.UicTypeId,
                                     x.Uic
                                 })
                                 .FirstOrDefaultAsync();

            if (info == null)
            {
                setModelError(buletinModel, "Невалиден идентификатор на присъда");
                return;
            }


            var buletinInfo = await repo.AllReadonly<CasePersonSentenceBulletin>()
                                        .Where(x => x.Id == casePersonBulletinId)
                                        .Select(x => new
                                        {
                                            x.LawUnitSignId,
                                            x.BirthDay,
                                            x.BirthDayPlaceCountryId,
                                            x.BirthDayPlaceCityId,
                                            x.BirthDayPlaceCityText,
                                            x.BirthDayPlaceDescriptionLat,
                                            x.MotherName,
                                            x.MotherNameLatin,
                                            x.FatherName,
                                            x.FatherNameLatin,
                                            x.NationalityCountryOneId,
                                            x.NationalityCountryTwoId,
                                            x.Case.EISSPNumber,
                                            x.NumberAFIS,
                                            x.OtherUic,
                                            x.OtherUicIssuingCountryId,
                                            x.OtherUicPlaceCityId,
                                            IsAdministrativePunishment = x.IsAdministrativePunishment ?? false,
                                            RegNumber = x.ExportFiles.OrderByDescending(f => f.Id).Select(f => f.RegNumber).FirstOrDefault(),
                                            x.IsConvicted
                                        })
                                        .FirstOrDefaultAsync();

            if (buletinInfo == null)
            {
                setModelError(buletinModel, "Непопълнени данни за бюлетин съдимост");
                return;
            }

            buletinModel.BuletinInfo.BuletinNumber = buletinInfo.RegNumber;
            buletinModel.BuletinInfo.NumberEISPP = buletinInfo.EISSPNumber;
            buletinModel.BuletinInfo.NumberAFIS = buletinInfo.NumberAFIS;
            buletinModel.BuletinInfo.IsAdministrative78a = buletinInfo.IsAdministrativePunishment;
            buletinModel.BuletinInfo.BuletinName = buletinInfo.IsAdministrativePunishment ? "Бюлетин за наложени административни наказания по чл. 78а от НК" : "Бюлетин за съдимост";
            buletinModel.BuletinInfo.IsConvicted = buletinInfo.IsConvicted ?? false;
            //TODO: Останалите полета от шапката на бюлетина

            //Данни за лице
            var mainPerson = new CaisBuletinPersonModel();
            mainPerson.UIC = info.Uic;
            mainPerson.UicTypeId = info.UicTypeId;
            mainPerson.OtherUic = buletinInfo.OtherUic;
            mainPerson.FirstName = info.FirstName;
            mainPerson.MiddleName = info.MiddleName;
            mainPerson.FamilyName = info.FamilyName;
            if (!string.IsNullOrEmpty(info.Family2Name))
            {
                mainPerson.FamilyName += "-" + info.Family2Name;
            }
            mainPerson.FullName = info.FullName;

            if (string.IsNullOrEmpty(info.Uic))
            {
                setModelError(buletinModel, "Непопълнено поле 'Идентификатор' на основно лице в таб 'Лица' по делото");
                return;
            }

            if (string.IsNullOrEmpty(info.FirstName))
            {
                setModelError(buletinModel, "Непопълнено поле 'Име' на основно лице в таб 'Лица' по делото");
                return;
            }

            if (string.IsNullOrEmpty(info.FamilyName))
            {
                setModelError(buletinModel, "Непопълнено поле 'Фамилия' на основно лице в таб 'Лица' по делото");
                return;
            }

            if (string.IsNullOrEmpty(info.LatinName))
            {
                setModelError(buletinModel, "Непопълнено поле 'Име на латиница' на основно лице в таб 'Лица' по делото");
                //return;
            }

            mainPerson.ParseNames(info.LatinName, true);

            mainPerson.GenderId = info.GenderId;
            switch (info.GenderId)
            {
                case 1:
                    mainPerson.Sex = "Жена";
                    break;
                case 2:
                    mainPerson.Sex = "Мъж";
                    break;
            }

            //CasePersonSentenceBulletin
            mainPerson.DateOfBirth = buletinInfo.BirthDay;

            buletinModel.MainPerson = mainPerson;

            var motherPerson = new CaisBuletinPersonModel();
            motherPerson.ParseNames(buletinInfo.MotherName);
            if (!string.IsNullOrEmpty(buletinInfo.MotherNameLatin))
            {
                motherPerson.ParseNames(buletinInfo.MotherNameLatin, true);
            }
            buletinModel.MotherNames = motherPerson;



            var fatherPerson = new CaisBuletinPersonModel();
            fatherPerson.ParseNames(buletinInfo.FatherName);

            if (!string.IsNullOrEmpty(buletinInfo.FatherNameLatin))
            {
                fatherPerson.ParseNames(buletinInfo.FatherNameLatin, true);
            }

            buletinModel.FatherNames = fatherPerson;

            if (buletinInfo.NationalityCountryOneId < 1 && buletinInfo.NationalityCountryTwoId < 1)
            {
                setModelError(buletinModel, "Непопълнено поле 'Гражданство'");
                return;
            }

            var citizenshipCountryIds = new List<int>();
            if (buletinInfo.NationalityCountryOneId > 0)
            {
                citizenshipCountryIds.Add(buletinInfo.NationalityCountryOneId.Value);
            }
            if (buletinInfo.NationalityCountryTwoId > 0)
            {
                citizenshipCountryIds.Add(buletinInfo.NationalityCountryTwoId.Value);
            }
            buletinModel.Citizenship = await repo.AllReadonly<EkCountry>()
                                                 .Where(x => citizenshipCountryIds.ToArray().Contains(x.CountryId))
                                                 .Select(x => new CaisBuletinAddressModel
                                                 {
                                                     CountryName = x.Name,
                                                     CountryCodeD = x.CodeNumber,
                                                     CountryCodeL = x.Code3
                                                 })
                                                 .ToArrayAsync();

            if (buletinInfo.BirthDayPlaceCountryId == null || buletinInfo.BirthDayPlaceCountryId < 1)
            {
                setModelError(buletinModel, "Непопълнено поле 'Местораждане - държава'");
                return;
            }

            //if (buletinInfo.BirthDayPlaceCountryId == NomenclatureConstants.CountryBGID)
            //{
            //    if (buletinInfo.BirthDayPlaceCityId == null || buletinInfo.BirthDayPlaceCityId < 1)
            //    {
            //        setModelError(buletinModel, "Непопълнено поле 'Местораждане - населено място'");
            //        return;
            //    }
            //}

            if (buletinInfo.BirthDayPlaceCountryId > 0)
            {
                var birthDayPlace = await repo.AllReadonly<EkCountry>()
                                              .Where(x => x.CountryId == buletinInfo.BirthDayPlaceCountryId.Value)
                                              .Select(x => new CaisBuletinAddressModel
                                              {
                                                  CountryName = x.Name,
                                                  CountryCodeD = x.CodeNumber,
                                                  CountryCodeL = x.Code3
                                              })
                                              .FirstOrDefaultAsync();

                if (buletinInfo.BirthDayPlaceCityId > 0)
                {
                    var cityInfo = await repo.AllReadonly<EkEkatte>()
                                             .Where(x => x.Id == buletinInfo.BirthDayPlaceCityId.Value)
                                             .Select(x => new
                                             {
                                                 x.Ekatte,
                                                 x.Name,
                                             })
                                             .FirstOrDefaultAsync();
                    if (cityInfo != null)
                    {
                        birthDayPlace.CityEkatte = cityInfo.Ekatte;
                        birthDayPlace.CityName = cityInfo.Name;
                    }
                }
                else
                {
                    birthDayPlace.CityDescription = buletinInfo.BirthDayPlaceCityText.Decode();
                    birthDayPlace.CityDescriptionLat = buletinInfo.BirthDayPlaceDescriptionLat.Decode();
                }

                if (buletinInfo.OtherUicIssuingCountryId > 0)
                {
                    var otherUicIssuingCountry = await repo.AllReadonly<EkCountry>()
                                                           .Where(x => x.CountryId == buletinInfo.OtherUicIssuingCountryId.Value)
                                                           .Select(x => new CaisBuletinAddressModel
                                                           {
                                                               CountryName = x.Name,
                                                               CountryCodeD = x.CodeNumber,
                                                               CountryCodeL = x.Code3
                                                           })
                                                           .FirstOrDefaultAsync();

                    buletinModel.OtherUicAddress = new()
                    {
                        CountryName = otherUicIssuingCountry.CountryName,
                        CountryCodeD = otherUicIssuingCountry.CountryCodeD,
                        CountryCodeL = otherUicIssuingCountry.CountryCodeL
                    };

                    if (buletinInfo.OtherUicPlaceCityId > 0)
                    {
                        var cityInfo = await repo.AllReadonly<EkEkatte>()
                                             .Where(x => x.Id == buletinInfo.OtherUicPlaceCityId.Value)
                                             .Select(x => new
                                             {
                                                 x.Ekatte,
                                                 x.Name,
                                             })
                                             .FirstOrDefaultAsync();

                        if (cityInfo != null)
                        {
                            buletinModel.OtherUicAddress.CityEkatte = cityInfo.Ekatte;
                            buletinModel.OtherUicAddress.CityName = cityInfo.Name;
                        }
                    }
                }

                buletinModel.PrevNames = await repo.AllReadonly<CasePersonPrevName>()
                                .Where(x => x.CasePersonId == casePersonId)
                                .Where(x => x.DateExpired == null)
                                .Select(x => new CaisBuletinPersonModel
                                {
                                    FirstName = x.FirstName,
                                    MiddleName = x.MiddleName,
                                    FamilyName = x.FamilyName,
                                    FullName = x.FullName,
                                    PrevNameType = x.PersonPrevNamesType.Label
                                }).ToArrayAsync();

                buletinModel.MainPersonBirthPlace = birthDayPlace;
            }
        }

        async Task initIdentityDocuments(CaisBuletinModel buletinModel, int casePersonId)
        {
            var documents = await repo.AllReadonly<CasePersonDocument>()
                                      .Where(x => x.CasePersonId == casePersonId)
                                      .Where(x => x.DateExpired == null)
                                      .Select(x => new CaisBuletinIdentityDocumentModel
                                      {
                                          PersonalDocumentId = x.PersonalDocumentTypeId,
                                          TypeName = x.PersonalDocumentTypeLabel,
                                          Issuer = x.IssuerName,
                                          Number = x.DocumentNumber,
                                          DateIssue = x.DocumentDate,
                                          ValidTo = x.DocumentDateTo,
                                          Country = new CaisBuletinAddressModel()
                                          {
                                              CityEkatte = x.IssuerCountryCode
                                          }
                                      })
                                      .ToArrayAsync();

            foreach (var doc in documents)
            {
                if (!string.IsNullOrEmpty(doc.Country.CityEkatte))
                {
                    var countryInfo = await repo.AllReadonly<EkCountry>()
                                                .Where(x => x.Code == doc.Country.CityEkatte)
                                                .Select(x => new
                                                {
                                                    x.Code3,
                                                    x.CodeNumber,
                                                    x.Name
                                                })
                                                .FirstOrDefaultAsync();

                    if (countryInfo != null)
                    {
                        doc.Country.CountryCodeL = countryInfo.Code3;
                        doc.Country.CountryCodeD = countryInfo.CodeNumber;
                        doc.Country.CountryName = countryInfo.Name;
                    }
                }

                doc.Issuer = doc.Issuer.Decode();

            }
            buletinModel.IdentityDocuments = documents;
        }

        async Task initAct(CaisBuletinModel buletinModel, int casePersonSentenceId)
        {
            var actModel = await repo.AllReadonly<CasePersonSentence>()
                                     .Where(x => x.Id == casePersonSentenceId)
                                     .Select(x => x.CaseSessionAct)
                                     .Select(x => new CaisBuletinActModel
                                     {
                                         Court = new CaisBuletinCourtModel()
                                         {
                                             CourtCode = x.Court.Code,
                                             CourtName = x.Court.Label,
                                             CourtEispp = x.Court.EISPPCode,
                                         },
                                         ActDate = x.RegDate.Value,
                                         ActInforceDate = x.ActInforcedDate,
                                         ECLInumber = x.EcliCode,
                                         ActNumber = x.RegNumber,
                                         ActType = x.ActType.Label,
                                         ActTypeId = x.ActTypeId,
                                         ActTypeCode = x.ActTypeId.ToString(),
                                         CaseId = x.CaseId.Value

                                     })
                                     .FirstOrDefaultAsync();

            if (actModel == null)
            {
                setModelError(buletinModel, "Невалиден акт по присъда");
                return;
            }

            buletinModel.Act = actModel;
        }

        async Task initCase(CaisBuletinModel buletinModel, int caseId)
        {
            var caseModel = await repo.AllReadonly<Case>()
                                      .Where(x => x.Id == caseId)
                                      .Select(x => new CaisBuletinCaseModel
                                      {
                                          Court = new CaisBuletinCourtModel()
                                          {
                                              CourtCode = x.Court.Code,
                                              CourtName = x.Court.Label,
                                              CourtEispp = x.Court.EISPPCode,
                                          },
                                          CaseTypeId = x.CaseTypeId,
                                          CaseType = x.CaseType.Code,
                                          CaseNumber = x.ShortNumber,
                                          CaseYear = x.RegDate.Date.Year
                                      })
                                      .FirstOrDefaultAsync();

            if (caseModel == null)
            {
                setModelError(buletinModel, "Невалидно дело по присъда");
                return;
            }

            buletinModel.Case = caseModel;
        }

        async Task initCrimes(CaisBuletinModel buletinModel, int casePersonId, int casePersonSentenseId)
        {
            var queryEispp = repo.AllReadonly<EisppTblElement>()
                                 .Where(x => x.EisppTblCode == EISPPConstants.EisppTableCode.EISS_PNE);

            var crimesModel = await repo.AllReadonly<CasePersonCrime>()
                                        .Where(x => x.CasePersonId == casePersonId)
                                        .Where(x => x.DateExpired == null)
                                        .Where(x => x.CaseCrime.DateExpired == null)
                                        .Select(x => new CaisBuletinCrimeModel
                                        {
                                            CaseCrimeId = x.CaseCrimeId,
                                            CategoryDeedCode = queryEispp.Where(e => e.Code == x.CaseCrime.CrimeCode)
                                                                         .Select(e => e.Code)
                                                                         .FirstOrDefault(),
                                            CategoryDeedName = queryEispp.Where(e => e.Code == x.CaseCrime.CrimeCode)
                                                                         .Select(e => e.Label)
                                                                         .FirstOrDefault(),
                                            CategoryCommonDeedCode = x.CaseCrime.CategoryCommonDeed.Code,
                                            CategoryCommonDeedName = x.CaseCrime.CategoryCommonDeed.Label,
                                            CrimeDescription = x.CaseCrime.DescriptionOffence,
                                            LegalQualificationText = x.CaseCrime.LegalQualificationText,
                                            DeclaredPunishmentPriorSentence = x.CaseCrime.HasPriorProbation ?? false,
                                            ActPriorProbation = x.CaseCrime.ActPriorProbation,
                                            Address = new CaisBuletinAddressModel()
                                            {
                                                CountryCodeD = x.CaseCrime.CrimeSceneCountry.CodeNumber,
                                                CountryCodeL = x.CaseCrime.CrimeSceneCountry.Code3,
                                                CountryName = x.CaseCrime.CrimeSceneCountry.Name,
                                                CityEkatte = (x.CaseCrime.CrimeSceneCityEisppId > 0) ? x.CaseCrime.CrimeSceneCityEispp.EktteCode : "",
                                                CityName = (x.CaseCrime.CrimeSceneCityEisppId > 0) ? x.CaseCrime.CrimeSceneCityEispp.Name : "",
                                                CityDescription = x.CaseCrime.CrimeSceneText
                                            },
                                            DateFrom = x.CaseCrime.DateFrom,
                                            DateTo = x.CaseCrime.DateTo,
                                            FormOfGuiltId = (x.CaseCrime.FormGuiltId ?? 0),
                                            NotPunished = x.NotPunished ?? false,
                                            EISSPNumber = x.CaseCrime.EISSPNumber,
                                            RecidiveTypeId = x.RecidiveTypeId,
                                            RecidiveTypeLabel = x.RecidiveType.Label
                                        })
                                        .ToArrayAsync();

            foreach (var crime in crimesModel)
            {
                crime.Punishmets = await initPunishments(crime.CaseCrimeId, casePersonSentenseId);
                crime.CrimeDescription = crime.CrimeDescription.Decode();
                crime.LegalQualificationText = crime.LegalQualificationText.Decode();
                if (crime.Address != null)
                    crime.Address.CityDescription = crime.Address.CityDescription.Decode();

                if (!crime.NotPunished)
                {
                    if (crime.Punishmets == null || !crime.Punishmets.Any())
                    {
                        setModelError(buletinModel, "Няма въведено наказание към престъпление");
                        return;
                    }
                }
            }

            if (crimesModel == null || !crimesModel.Any())
            {
                setModelError(buletinModel, "Въведете престъпление през бутон Престъпления/бутон Лица към престъпление/бутон Наказания");
                return;
            }

            buletinModel.Crimes = crimesModel;
        }

        async Task<CaisBuletinPunishmentModel[]> initPunishments(int caseCrimeId, int casePersonSentenseId)
        {
            var punishmentModel = await repo.AllReadonly<CasePersonSentencePunishment>()
                                            .Where(x => x.CasePersonSentenceId == casePersonSentenseId)
                                            .Where(x => x.CasePersonSentencePunishmentCrimes.Any(c => c.CaseCrimeId == caseCrimeId &&
                                                                                                      c.DateExpired == null))
                                            .Where(x => x.DateExpired == null)
                                            .Where(x => x.CasePersonSentence.DateExpired == null)
                                            .Select(x => new CaisBuletinPunishmentModel
                                            {
                                                Id = x.Id,
                                                CasePersonSentencePunishmentId = x.Id,
                                                SentenceTypeCode = x.SentenceType.Code,
                                                SentenceTypeCode2 = x.SentenceType.Description,
                                                SentenceTypeName = x.SentenceType.Label,
                                                SentenceText = x.SentenceText,
                                                PunishmentGeneralCategoryCode = x.PunishmentGeneralCategory.Code,
                                                PunishmentGeneralCategoryName = x.PunishmentGeneralCategory.Label,
                                                ChargeAmount = x.SentenseMoney,
                                                Description = x.Description,
                                                SentenceYears = x.SentenseYears,
                                                SentenceMonths = x.SentenseMonths,
                                                SentenceWeeks = x.SentenseWeeks,
                                                SentenceDays = x.SentenseDays,
                                                PreliminaryDetentionYears = x.PreliminaryDetentionYears,
                                                PreliminaryDetentionMonths = x.PreliminaryDetentionMonths,
                                                PreliminaryDetentionWeeks = x.PreliminaryDetentionWeeks,
                                                PreliminaryDetentionDays = x.PreliminaryDetentionDays,
                                                ProbationYears = x.ProbationYears,
                                                ProbationMonths = x.ProbationMonths,
                                                ProbationDays = x.ProbationDays,
                                                YesLOS = NomenclatureConstants.SentenceTypes.LishavaneOtSvoboda.Contains(x.SentenceTypeId),
                                                DateWrt = x.DateWrt
                                            })
                                            .ToArrayAsync();

            var queryEispp = repo.AllReadonly<EisppTblElement>()
                            .Where(x => x.EisppTblCode == EISPPConstants.EisppTableCode.MeasureUnit);

            foreach (var punishment in punishmentModel)
            {
                punishment.Probations = await repo.AllReadonly<CasePersonSentencePunishmentMeasure>()
                                               .Where(x => x.CasePersonSentencePunishmentId == punishment.CasePersonSentencePunishmentId)
                                               .Where(x => x.CasePersonMeasure.MeasureKindId == NomenclatureConstants.PersonMeasureKinds.Probation)
                                               .Where(x => x.CasePersonMeasure.DateExpired == null)
                                               .Select(x => x.CasePersonMeasure)
                                               .Select(x => new CaisBuletinProbationModel
                                               {
                                                   Id = x.Id,
                                                   MeasureType = x.MeasureType,
                                                   MeasureTypeCode = queryEispp.Where(e => e.Code == x.MeasureType &&
                                                                                       e.EisppTblCode == EISPPConstants.EisppTableCode.ProbationMeasureType)
                                                                             .Select(e => e.SystemName)
                                                                             .FirstOrDefault() ?? x.MeasureType,
                                                   MeasureTypeName = x.MeasureTypeLabel,
                                                   MeasureQuantity = x.MeasureQuantity,
                                                   MeasureDate = x.MeasureStatusDate,
                                                   MeasureStatus = x.MeasureStatusLabel,
                                                   MeasureUnitCode = x.MeasureUnit,
                                                   MeasureUnitName = queryEispp.Where(e => e.Code == x.MeasureUnit &&
                                                                                   e.EisppTblCode == EISPPConstants.EisppTableCode.MeasureUnit)
                                                                           .Select(e => e.Label)
                                                                           .FirstOrDefault() ?? x.MeasureUnit,

                                                   ProbationYears = x.MeasureYears,
                                                   ProbationMonths = x.MeasureMonths,
                                                   ProbationWeeks = x.MeasureWeeks,
                                                   ProbationDays = x.MeasureDays
                                               })
                                               .ToArrayAsync();
            }

            return punishmentModel;
        }

        //async Task initProbations(CaisBuletinModel buletinModel, int casePersonId)
        //{
        //    var queryEispp = repo.AllReadonly<EisppTblElement>();

        //    var probationModel = await repo.AllReadonly<CasePersonMeasure>()
        //                                   .Where(x => x.CasePersonId == casePersonId)
        //                                   .Where(x => x.DateExpired == null)
        //                                   .Where(x => x.MeasureKindId == NomenclatureConstants.PersonMeasureKinds.Probation)
        //                                   .Select(x => new CaisBuletinProbationModel
        //                                   {
        //                                       Id = x.Id,
        //                                       MeasureType = x.MeasureType,
        //                                       MeasureTypeCode = queryEispp.Where(e => e.Code == x.MeasureType &&
        //                                                                                 e.EisppTblCode == EISPPConstants.EisppTableCode.ProbationMeasureType)
        //                                                                     .Select(e => e.SystemName)
        //                                                                     .FirstOrDefault() ?? x.MeasureType,
        //                                       MeasureTypeName = x.MeasureTypeLabel,
        //                                       MeasureQuantity = x.MeasureQuantity,
        //                                       MeasureUnitCode = x.MeasureUnit,
        //                                       MeasureUnitName = queryEispp.Where(e => e.Code == x.MeasureUnit &&
        //                                                                           e.EisppTblCode == EISPPConstants.EisppTableCode.MeasureUnit)
        //                                                               .Select(e => e.Label)
        //                                                               .FirstOrDefault() ?? x.MeasureUnit
        //                                   })
        //                                   .ToArrayAsync();

        //    if (probationModel == null)
        //    {
        //        setModelError(buletinModel, "Невалидни наказания по присъда");
        //        return;
        //    }

        //    //buletinModel.Probations = probationModel;
        //}

        void setModelError(CaisBuletinModel buletinModel, string errorMessage)
        {
            buletinModel.ErrorMessage = errorMessage;

            //TODO: throw new Exception?
        }

        #endregion


        public async Task<SaveResultVM> RegisterBulletinCallback(RegisterBulletinRequestModel request)
        {
            var fileInfo = await repo.AllReadonly<CasePersonSentenceBulletinFile>()
                                .Where(x => x.RegNumber == request.BulletinNumber)
                                .Where(x => x.CasePersonSentenceBulletin.Case.ShortNumberValue == request.CaseNumber)
                                .Where(x => x.CasePersonSentenceBulletin.Case.RegDate.Year == request.CaseYear)
                                .Where(x => x.CasePersonSentenceBulletin.Case.Court.EISPPCode == request.CourtCode)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.RegDate,
                                    x.DateRegisteredInCais
                                }).ToListAsync();

            if (fileInfo.Count == 0)
            {
                return new SaveResultVM(false)
                {
                    Content = "Невалидни данни за бюлетин/дело"
                };
            }

            bool hasInChecked = false;
            foreach (var item in fileInfo)
            {
                var file = await repo.GetByIdAsync<CasePersonSentenceBulletinFile>(item.Id);
                file.DateRegisteredInCais = request.DateRegistered;
                await repo.SaveChangesAsync();

                if (item.DateRegisteredInCais == null)
                {
                    hasInChecked = true;
                }
            }
            return new SaveResultVM(true)
            {
                Content = (hasInChecked) ? "Регистрирането премина успешно." : "Актуализирането премина успешно."
            };
        }
    }
}