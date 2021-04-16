using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Integrations.Lawyers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class MigrationDataService : BaseService, IMigrationDataService
    {
        private DateTime DefaultDateFrom = new DateTime(2000, 1, 1);
        private DateTime DefaultDateTo = new DateTime(2019, 12, 31);
        private readonly UserManager<ApplicationUser> userManager;
        public MigrationDataService(
            ILogger<DocumentService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            UserManager<ApplicationUser> _userManager
            )
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            userManager = _userManager;
        }

        public string MigrateForCourt(int courtId)
        {
            var itemsToMigrate = repo.All<MigrationData>()
                                .Where(x => x.CourtId == courtId && x.MigrationDate == null)
                                //.Where(x => x.Id == 22)
                                .OrderBy(x => x.DataType)
                                .ThenBy(x => x.Data).ToList();

            var prevMigrationErrors = itemsToMigrate.Where(x => x.Message != null).Count();


            foreach (var item in itemsToMigrate)
            {
                bool migratedOk = false;
                if (string.IsNullOrEmpty(item.Data))
                {
                    item.Data = "test";
                }
                switch (item.DataType)
                {
                    case "lawunit-1":
                    case "lawunit-2":
                    case "lawunit-4":
                    case "lawunit-5":
                    case "lawunit-6":
                        migratedOk = migrate_CourtLawUnit(item).Result;
                        break;
                    case "lawunit-3":
                        if (item.CourtId == 9999)
                        {
                            migratedOk = migrate_CourtLawUnit(item).Result;
                        }
                        else
                        {
                            item.Message = "Прокурори не се мигрират.";
                        }
                        break;
                    case "hall":
                        migratedOk = migrate_CourtHall(item);
                        break;
                    case "bank":
                        migratedOk = migrate_CourtBankAccount(item);
                        break;
                    case "court-group1":
                        migratedOk = migrate_CourtGroup(item);
                        break;
                    case "court-group2":
                        migratedOk = migrate_CourtGroupCode(item);
                        break;
                    case "zarh-index1":
                        migratedOk = migrate_ArchiveCommittee(item);
                        break;
                    case "zarh-index2":
                        migratedOk = migrate_ArchiveCommitteeLawunit(item);
                        break;
                    case "zarh-index3":
                        migratedOk = migrate_ArchiveIndex(item);
                        break;
                    case "zarh-index4":
                        migratedOk = migrate_ArchiveIndexCode(item);
                        break;
                    case "institution-6":
                    case "institution-14":
                    case "institution-15":
                        migratedOk = migrate_Institution(item);
                        break;
                        //case "load-group":
                        //    migratedOk = migrate_LoadGroup(item);
                        //    break;
                }

                if (migratedOk)
                {
                    item.MigrationDate = DateTime.Now;

                }
                repo.Update(item);
                repo.SaveChanges();
            }

            if (itemsToMigrate.Count > 0)
            {
                var currentMigrated = itemsToMigrate.Where(x => x.MigrationDate != null).Count();
                var totalErrors = itemsToMigrate.Count - currentMigrated;
                var currentErrors = totalErrors - prevMigrationErrors;

                return $"Мигрирани: {currentMigrated} от {itemsToMigrate.Count} елемента. Текущи грешки: {currentErrors}";
            }
            else
            {
                return "Няма данни за мигриране.";
            }
        }

        private bool migrate_ArchiveCommittee(MigrationData item)
        {
            try
            {
                var model = new CourtArchiveCommittee()
                {
                    CourtId = item.CourtId,
                    Label = item.Data,
                    Description = item.Code,
                    DateStart = DefaultDateFrom
                };

                repo.Add(model);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return false;
            }
        }

        private bool migrate_ArchiveCommitteeLawunit(MigrationData item)
        {
            var data = item.Data.Split('|');
            try
            {
                var model = new CourtArchiveCommitteeLawUnit()
                {
                    DateFrom = DefaultDateFrom
                };

                var committee = repo.AllReadonly<CourtArchiveCommittee>().Where(x => x.CourtId == item.CourtId && x.Description == item.ParentCode).FirstOrDefault();
                var uic = data[0];
                var lawunit = repo.AllReadonly<CourtLawUnit>()
                                            .Include(x => x.LawUnit)
                                            .Where(x => x.CourtId == item.CourtId && x.LawUnit.Uic == uic
                                            && x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Appoint)
                                            .Select(x => x.LawUnit).FirstOrDefault();


                if ((committee == null) || (lawunit == null))
                {
                    item.Message = $"Ненамерена комисия {item.ParentCode} или лице {uic} {data[1]}";
                    return false;
                }
                if (repo.AllReadonly<CourtArchiveCommitteeLawUnit>().Where(x => x.CourtArchiveCommitteeId == committee.Id && x.LawUnitId == lawunit.Id).Any())
                {
                    item.Message = $"Лицето {uic} {data[1]} вече съществува";
                    return false;
                }
                model.CourtArchiveCommitteeId = committee.Id;
                model.LawUnitId = lawunit.Id;
                repo.Add(model);
                repo.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                item.Message = ex.Message;
                return false;
            }
        }

        private bool migrate_ArchiveIndex(MigrationData item)
        {
            var data = item.Data.Split('|');
            try
            {
                var model = new CourtArchiveIndex()
                {
                    CourtId = item.CourtId,
                    Code = data[0],
                    Label = data[1],
                    StorageYears = int.Parse(data[2]),
                    Description = item.Code,
                    IsActive = true,
                    DateStart = DefaultDateFrom
                };

                var committee = repo.AllReadonly<CourtArchiveCommittee>().Where(x => x.CourtId == item.CourtId && x.Description == item.ParentCode).FirstOrDefault();

                if (committee != null)
                {
                    model.CourtArchiveCommitteeId = committee.Id;
                    repo.Add(model);
                    repo.SaveChanges();
                    return true;
                }
                else
                {
                    item.Message = $"Ненамерена комисия с ID = {item.ParentCode}";
                    return false;
                }
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return false;
            }
        }

        private bool migrate_ArchiveIndexCode(MigrationData item)
        {
            var data = item.Data.Split('|');
            try
            {
                var model = new CourtArchiveIndexCode()
                {
                    DateFrom = DefaultDateFrom
                };

                var index = repo.AllReadonly<CourtArchiveIndex>().Where(x => x.CourtId == item.CourtId && x.Description == item.ParentCode).FirstOrDefault();

                int caseCodeId = getCaseCodeByCode(item.Code);

                if (index != null && caseCodeId > 0)
                {
                    model.CourtArchiveIndexId = index.Id;
                    model.CaseCodeId = caseCodeId;

                    if (repo.AllReadonly<CourtArchiveIndexCode>()
                                .Where(x => x.CourtArchiveIndexId == model.CourtArchiveIndexId && x.CaseCodeId == model.CaseCodeId).Any())
                    {
                        item.Message = "Кода се повтаря.";
                        return true;
                    }

                    repo.Add(model);
                    repo.SaveChanges();
                    return true;
                }
                else
                {
                    item.Message = $"Ненамерен индекс с ID = {item.ParentCode} или шифър {item.Code}";
                    return false;
                }
            }
            catch (Exception ex)
            {
                item.Message = ex.Message;
                return false;
            }
        }

        private bool migrate_CourtHall(MigrationData item)
        {
            var data = item.Data.Split('|');

            try
            {
                var model = new CourtHall()
                {
                    CourtId = item.CourtId,
                    Name = data[0],
                    Location = data[1],
                    DateFrom = DefaultDateFrom
                };

                repo.Add(model);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return false;
            }
        }

        private bool migrate_CourtBankAccount(MigrationData item)
        {
            var data = item.Data.Split('|');

            try
            {
                var model = new CourtBankAccount()
                {
                    CourtId = item.CourtId,
                    Iban = data[0],
                    Label = data[1],
                    MoneyGroupId = int.Parse(data[2]),
                    IsActive = true,
                    DateStart = DefaultDateFrom
                };

                repo.Add(model);
                repo.SaveChanges();
                model.OrderNumber = model.Id;
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return false;
            }
        }

        private async Task<bool> migrate_CourtLawUnit(MigrationData item)
        {
            var data = item.Data.Split('|');

            try
            {
                int lawUnitTypeId = int.Parse(item.DataType.Substring(8));
                string email = string.Empty;

                var lawUnit = new LawUnit()
                {
                    LawUnitTypeId = lawUnitTypeId,
                    UicTypeId = NomenclatureConstants.UicTypes.EGN,
                    Uic = data[1],
                    FirstName = data[2]?.Trim(),
                    MiddleName = data[3]?.Trim(),
                    FamilyName = data[4]?.Trim(),
                    Family2Name = data[5]?.Trim(),
                    DateFrom = DefaultDateFrom,
                    DateWrt = DateTime.Now
                };

                lawUnit.FullName = lawUnit.MakeFullName();

                if (!string.IsNullOrEmpty(lawUnit.Uic))
                {
                    lawUnit.Person.CopyFrom(lawUnit);
                }
                else
                {
                    lawUnit.Person = null;
                }
                var lawUnitPeriods = new List<CourtLawUnit>();
                var appointPeriod = new CourtLawUnit();
                if (!NomenclatureConstants.LawUnitTypes.NoApointmentPersons.Contains(lawUnitTypeId))
                {
                    appointPeriod.CourtId = item.CourtId;
                    appointPeriod.DateFrom = DefaultDateFrom;
                    appointPeriod.PeriodTypeId = NomenclatureConstants.PeriodTypes.Appoint;

                    lawUnitPeriods.Add(appointPeriod);

                }

                switch (lawUnitTypeId)
                {
                    case NomenclatureConstants.LawUnitTypes.Judge:
                        email = data[6].Trim();
                        try
                        {
                            if (!string.IsNullOrEmpty(data[7]))
                            {
                                var moveToCourt = int.Parse(data[7]);
                                var moveFromDate = data[8];


                                var movePeriod = new CourtLawUnit()
                                {
                                    CourtId = moveToCourt,
                                    DateFrom = moveFromDate.StrToDateFormat("dd.MM.yyyy"),
                                    PeriodTypeId = NomenclatureConstants.PeriodTypes.Move
                                };

                                if (moveToCourt < 0)
                                {
                                    //Ако е командирован в текущия съд, се разменят двата вида период
                                    appointPeriod.PeriodTypeId = NomenclatureConstants.PeriodTypes.Move;
                                    appointPeriod.DateFrom = movePeriod.DateFrom;
                                    movePeriod.CourtId = Math.Abs(movePeriod.CourtId);
                                    movePeriod.PeriodTypeId = NomenclatureConstants.PeriodTypes.Appoint;
                                    movePeriod.DateFrom = DefaultDateFrom;
                                }


                                lawUnitPeriods.Add(movePeriod);
                            }
                        }
                        catch { }
                        break;
                    case NomenclatureConstants.LawUnitTypes.MessageDeliverer:
                        email = data[6].Trim();
                        break;
                    case NomenclatureConstants.LawUnitTypes.OtherEmployee:
                        email = data[6].Trim();
                        lawUnit.Department = data[7];
                        break;
                    case NomenclatureConstants.LawUnitTypes.Jury:
                        //Кой съд и специалност ако има
                        lawUnit.Department = data[6] + ":" + (data[7] ?? "");
                        break;
                    case NomenclatureConstants.LawUnitTypes.Prosecutor:
                        //Кой съд и специалност ако има
                        lawUnit.Code = data[6] ?? "";
                        lawUnit.Department = data[7] ?? "";

                        if (string.IsNullOrEmpty(lawUnit.Code))
                        {
                            item.Message = $"{lawUnit.FullName} няма код";
                            return false;
                        }
                        if (repo.AllReadonly<LawUnit>().Where(x => x.Code == lawUnit.Code && x.LawUnitTypeId == lawUnitTypeId).Any())
                        {
                            item.Message = $"{lawUnit.Code} {lawUnit.FullName} вече съществува";
                            return false;
                        }
                        break;
                    default:
                        break;
                }
                lawUnit.Courts = lawUnitPeriods;

                if (!string.IsNullOrEmpty(lawUnit.Uic))
                {
                    var savedLU = repo.AllReadonly<LawUnit>()
                                .Where(x => x.Uic == lawUnit.Uic && x.UicTypeId == lawUnit.UicTypeId)
                                .FirstOrDefault();

                    if (savedLU != null)
                    {
                        item.Message = $"{savedLU.Uic} {savedLU.FullName} вече съществува";
                        return false;
                    }
                }

                //Лицата трябва да имат уникално ЕГН и уникален email
                if (NomenclatureConstants.LawUnitTypes.EissUserTypes.Contains(lawUnitTypeId))
                {
                    if (string.IsNullOrEmpty(lawUnit.Uic))
                    {
                        item.Message = "Лицето няма ЕГН";
                        return false;
                    }


                    long egn;
                    if (!long.TryParse(lawUnit.Uic, out egn))
                    {
                        item.Message = "Невалидно ЕГН";
                        return false;
                    }


                    if (string.IsNullOrEmpty(email))
                    {
                        item.Message = "Лицето няма електронна поща";
                        return false;
                    }

                    if (repo.AllReadonly<ApplicationUser>().Where(x => x.NormalizedEmail == email.ToUpper()).Any())
                    {
                        item.Message = "Съществуваща електронна поща";
                        return false;
                    }
                }




                repo.Add(lawUnit);
                repo.SaveChanges();

                //Ако има електронна поща се създава и потребител
                if (!string.IsNullOrEmpty(email))
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        LawUnitId = lawUnit.Id,
                        CourtId = item.CourtId,
                        WorkNotificationToMail = true,
                        IsActive = true
                    };

                    IdentityResult res = null;

                    res = await userManager.CreateAsync(user).ConfigureAwait(false);

                    if (res.Succeeded)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                item.Message = ex.Message;
                return false;
            }
        }

        private bool migrate_Institution(MigrationData item)
        {
            var data = item.Data.Split('|');

            try
            {
                int institutionTypeId = int.Parse(item.DataType.Substring(12));

                var model = new Institution()
                {
                    Person = null,
                    InstitutionTypeId = institutionTypeId,
                    UicTypeId = NomenclatureConstants.UicTypes.EGN,
                    FirstName = data[0],
                    MiddleName = data[1],
                    FamilyName = data[2],
                    Family2Name = data[3],
                    Code = data[4]
                };
                if (!string.IsNullOrEmpty(item.Code))
                {
                    model.Uic = item.Code;
                }

                if (!string.IsNullOrEmpty(model.Code))
                {
                    var savedInst = repo.AllReadonly<Institution>()
                                .Where(x => x.Code == model.Code && x.InstitutionTypeId == model.InstitutionTypeId)
                                .FirstOrDefault();

                    if (savedInst != null)
                    {
                        item.Message = $"{savedInst.Code} {savedInst.FullName} вече съществува";
                        return false;
                    }
                }
                if (!string.IsNullOrEmpty(model.Uic))
                {
                    var savedInst = repo.AllReadonly<Institution>()
                                .Where(x => x.Uic == model.Uic && x.UicTypeId == model.UicTypeId && x.InstitutionTypeId == model.InstitutionTypeId)
                                .FirstOrDefault();

                    if (savedInst != null)
                    {
                        item.Message = $"{savedInst.Uic} {savedInst.FullName} вече съществува";
                        return false;
                    }
                }

                switch (institutionTypeId)
                {
                    case NomenclatureConstants.InstitutionTypes.Notary:
                        model.DepartmentName = data[5].Trim();
                        break;
                    default:
                        break;
                }

                model.FullName = model.MakeFullName();



                repo.Add(model);
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                item.Message = ex.Message;
                return false;
            }
        }


        private bool migrate_CourtGroup(MigrationData item)
        {
            var data = item.Data.Split('|');

            var caseGroupTXT = data[1].ToLower();
            int caseGroupId = 0;
            if (caseGroupTXT.Contains("аказа", StringComparison.InvariantCultureIgnoreCase))
            {
                caseGroupId = NomenclatureConstants.CaseGroups.NakazatelnoDelo;
            }
            if (caseGroupTXT.Contains("ажданск", StringComparison.InvariantCultureIgnoreCase))
            {
                caseGroupId = NomenclatureConstants.CaseGroups.GrajdanskoDelo;
            }
            if (caseGroupTXT.Contains("ирмен", StringComparison.InvariantCultureIgnoreCase))
            {
                caseGroupId = NomenclatureConstants.CaseGroups.Company;
            }
            if (caseGroupTXT.Contains("ърговск", StringComparison.InvariantCultureIgnoreCase))
            {
                caseGroupId = NomenclatureConstants.CaseGroups.Trade;
            }
            if (caseGroupTXT.Contains("министра", StringComparison.InvariantCultureIgnoreCase))
            {
                caseGroupId = NomenclatureConstants.CaseGroups.Administrative;
            }
            try
            {

                if (caseGroupId == 0)
                {
                    throw new Exception($"Невалидна група '{caseGroupTXT}'");
                }
                var model = new CourtGroup()
                {
                    CourtId = item.CourtId,
                    Label = data[0],
                    CaseGroupId = caseGroupId,
                    Description = item.Code.Trim(),
                    DateFrom = DefaultDateFrom
                };

                repo.Add(model);
                repo.SaveChanges();
                model.OrderNumber = model.Id;
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return false;
            }
        }

        private int getCaseCodeByCode(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                code = code.Trim();
            }
            int caseCodeId = repo.AllReadonly<CaseCode>().Where(x => x.Code == code)
                                                            .Select(x => x.Id).FirstOrDefault();

            if (caseCodeId == 0)
            {
                caseCodeId = repo.AllReadonly<CaseCode>().Where(x => x.Code == ("0" + code))
                                                         .Select(x => x.Id).FirstOrDefault();
            }
            if (caseCodeId == 0 && code?.Length == 4 && code.StartsWith("0", StringComparison.InvariantCultureIgnoreCase))
            {
                caseCodeId = repo.AllReadonly<CaseCode>().Where(x => x.Code == code.TrimStart('0'))
                                                         .Select(x => x.Id).FirstOrDefault();
            }
            return caseCodeId;
        }

        private bool migrate_CourtGroupCode(MigrationData item)
        {
            try
            {
                int groupId = repo.AllReadonly<CourtGroup>().Where(x => x.CourtId == item.CourtId
                                                                    && x.Description == item.ParentCode.Trim())
                                                            .Select(x => x.Id).FirstOrDefault();

                int caseCodeId = getCaseCodeByCode(item.Code);

                if (groupId == 0)
                {
                    throw new Exception($"Ненамерена група с id={item.ParentCode}");
                }
                if (caseCodeId == 0)
                {
                    throw new Exception($"Ненамерен шифър с код={item.Code}");
                }
                var model = new CourtGroupCode()
                {
                    CourtGroupId = groupId,
                    CaseCodeId = caseCodeId,
                    DateFrom = DefaultDateFrom
                };

                item.Message = null;
                if (repo.AllReadonly<CourtGroupCode>().Where(x => x.CourtGroupId == groupId && x.CaseCodeId == caseCodeId).Any())
                {
                    return true;
                }
                else
                {
                    repo.Add(model);
                    repo.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                item.Message = ex.Message;
                return false;
            }
        }

        public string MigrateLawyers()
        {
            string data = File.ReadAllText("wwwroot/data/get_all_lawyers.json");
            var model = JsonConvert.DeserializeObject<IEnumerable<LawyerModel>>(data);
            var disabledCourt = model.Where(x => x.PracticeAllowed == false).Count();
            var hasPast = model.Where(x => x.PastPersonalId.Any()).Count();

            //мигриране на колегии
            //var colegii = model.Select(x => x.BarAssociation).Distinct().OrderBy(x => x).ToList();

            //foreach (var item in colegii)
            //{
            //    var cm = new CodeMapping()
            //    {
            //        Alias = "lawyers_bar_association",
            //        OuterCode = item,
            //    };

            //    repo.Add(cm);
            //    repo.SaveChanges();
            //}

            var codeMapping = repo.AllReadonly<CodeMapping>().Where(x => x.Alias == "lawyers_bar_association").ToList();

            int i = 0;
            bool commited = true;
            const int commitCount = 100;
            foreach (var item in model)
            {
                commited = false;
                i++;
                var lawunit = mapLawyer(item, codeMapping);

                repo.Add(lawunit);

                if (i % commitCount == 0)
                {
                    repo.SaveChanges();
                    commited = true;
                }

                //if (i >= 1)
                //{
                //    break;
                //}
            }

            if (!commited)
            {
                repo.SaveChanges();
            }
            return i.ToString();
        }

        private LawUnit mapLawyer(LawyerModel lawyer, List<CodeMapping> bars)
        {
            var model = new LawUnit()
            {
                UicTypeId = NomenclatureConstants.UicTypes.EGN,
                LawUnitTypeId = NomenclatureConstants.LawUnitTypes.Lawyer,
                Code = lawyer.PersonalId,
                Department = bars.Where(x => x.OuterCode == lawyer.BarAssociation).Select(x => x.Description).FirstOrDefault(),
                DateFrom = DefaultDateFrom,
                DateWrt = DateTime.Now
            };

            if (!lawyer.PracticeAllowed)
            {
                model.DateTo = DefaultDateTo;
            }

            model.SplitPersonNames(lawyer.Name);
            model.FullName = model.MakeFullName();

            if (!string.IsNullOrEmpty(model.Uic))
            {
                model.Person.CopyFrom(model);
            }
            else
            {
                model.Person = null;
            }
            return model;
        }

        private bool migrate_LoadGroup(MigrationData item)
        {
            try
            {
                var data = item.Data.Split('$');
                var groupModel = new LoadGroup()
                {
                    Code = data[0],
                    Label = data[1],
                    DateStart = DefaultDateFrom,
                    IsActive = true,
                    Description = "migrated",
                    LoadGroupLinks = new HashSet<LoadGroupLink>()
                };

                var linkModel = new LoadGroupLink()
                {
                    CourtTypeId = int.Parse(data[2]),
                    CaseInstanceId = int.Parse(data[3]),
                    LoadIndex = decimal.Parse(data[4].Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)),
                    GroupCodes = new HashSet<LoadGroupLinkCode>()
                };

                var codeModel = new LoadGroupLinkCode()
                {
                    CaseCodeId = int.Parse(data[5])
                };

                linkModel.GroupCodes.Add(codeModel);

                groupModel.LoadGroupLinks.Add(linkModel);

                repo.Add(groupModel);
                repo.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                item.Message = ex.Message;
                return false;
            }
        }

        public string MigrateLoadGroupLinkFromCourtType(int fromCourtType, int fromInstance, int toCourtType, int toInstance, int[] caseGroups)
        {
            var enabledCodes = repo.AllReadonly<CaseTypeCode>()
                                        .Include(x => x.CaseType)
                                        .Where(x => caseGroups.Contains(x.CaseType.CaseGroupId))
                                        .Select(x => x.CaseCodeId)
                                        .ToArray();


            var savedLinks = repo.AllReadonly<LoadGroupLink>()
                                        .Include(x => x.GroupCodes)
                                        .Where(x => x.CourtTypeId == fromCourtType && x.CaseInstanceId == fromInstance)
                                        .Where(x => x.GroupCodes.Where(c => enabledCodes.Contains(c.CaseCodeId)).Any())
                                        .ToList();

            int i = 0;
            foreach (var link in savedLinks)
            {
                var newLink = new LoadGroupLink()
                {
                    LoadGroupId = link.LoadGroupId,
                    CourtTypeId = toCourtType,
                    CaseInstanceId = toInstance,
                    LoadIndex = link.LoadIndex,
                    GroupCodes = new HashSet<LoadGroupLinkCode>()
                };
                if (link.GroupCodes != null)
                {
                    foreach (var code in link.GroupCodes.Where(x => enabledCodes.Contains(x.CaseCodeId)))
                    {
                        var newLinkCode = new LoadGroupLinkCode()
                        {
                            CaseCodeId = code.CaseCodeId
                        };

                        newLink.GroupCodes.Add(newLinkCode);
                    }
                }

                var savedLinkItem = repo.AllReadonly<LoadGroupLink>().
                    Where(x => x.LoadGroupId == newLink.LoadGroupId
                    && x.CourtTypeId == newLink.CourtTypeId
                    && x.CaseInstanceId == newLink.CaseInstanceId).FirstOrDefault();

                if (savedLinkItem != null)
                {
                }
                else
                {
                    repo.Add(newLink);
                    repo.SaveChanges();
                    i++;
                }
            }

            return $"Копирани {i} от {savedLinks.Count} броя връзки към групи по натовареност";
        }
    }
}
