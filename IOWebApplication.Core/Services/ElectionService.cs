// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Election;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Election;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class ElectionService : BaseService, IElectionService
    {
        public ElectionService(IRepository _repo, ILogger<ElectionService> _logger, IUserContext _userContext)
        {
            repo = _repo;
            logger = _logger;
            userContext = _userContext;
        }

        public IQueryable<LabelValueVM> SearchLawunitsForElection(string query)
        {

            int[] courtTypes = new int[] { NomenclatureConstants.CourtType.Apeal, NomenclatureConstants.CourtType.VKS, NomenclatureConstants.CourtType.DistrictCourt };
            return repo.AllReadonly<CourtLawUnit>()
                        .Include(x => x.LawUnit)
                            .Where(x => x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Judge)
                            .Where(x => x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Appoint)
                            .Where(x => ((x.LawUnit.DateTo ?? DateTime.MaxValue) > DateTime.Now) && ((x.DateTo ?? DateTime.MaxValue) > DateTime.Now))
                            .Where(x => EF.Functions.ILike(x.LawUnit.FullName, query.ToPaternSearch()) || (x.LawUnit.Uic == query))
                           .Where(x=> courtTypes.Contains( x.Court.CourtTypeId))
                            .Select(x => new LabelValueVM
                            {
                                Label = $"{x.LawUnit.FullName} ({x.Court.Label} от {x.DateFrom:dd.MM.yyyy}), ЕГН:{x.LawUnit.Uic}",
                                Value = $"{x.Id}"
                            }).AsQueryable();
        }

        public SaveResultVM PersonAdd(ElectionPersonAddVM model)
        {
            var info = repo.AllReadonly<CourtLawUnit>()
                            .Where(x => x.Id == model.CourtLawunitId)
                            .Select(x => new
                            {
                                x.LawUnitId,
                                x.CourtId,
                                CourtName = x.Court.Label,
                                x.LawUnit.FullName
                            }).FirstOrDefault();

            var person = new ElectionPerson()
            {
                ElectionGroupId = model.ElectionGroupId,
                LawunitId = info.LawUnitId,
                LawunitFullName = info.FullName,
                CourtId = info.CourtId,
                PersonGid = Guid.NewGuid(),
                DateAdded = DateTime.Now,
                UserAddedId = userContext.UserId,
                ElectionPersonStateId = NomenclatureConstants.ElectionPersonStates.Include
            };
            repo.Add(person);
            repo.SaveChanges();

            var log = new ElectionLog()
            {
                Operation = NomenclatureConstants.ElectionLogOperation.AddPerson,
                Description = $"{info.FullName}, {info.CourtName}",
                ElectionGroupId = model.ElectionGroupId,
                PersonGid = person.PersonGid,
                ElectionPersonlId = person.Id,
                LawunitId = info.LawUnitId,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId
            };
            repo.Add(log);
            repo.SaveChanges();

            return new SaveResultVM()
            {
                Result = true,
                ObjectId = person.Id
            };
        }
        public bool CheckIfLawunitIsAddedPersonAdd(ElectionPersonAddVM model)
        {
            var result = false;
            var info = repo.AllReadonly<CourtLawUnit>()
                            .Where(x => x.Id == model.CourtLawunitId)
                            .Select(x => new
                            {
                                x.LawUnitId,
                                x.CourtId,
                                CourtName = x.Court.Label,
                                x.LawUnit.FullName
                            }).FirstOrDefault();

            var added = repo.AllReadonly<ElectionPerson>()

                      .Where(x => x.ElectionPersonStateId != NomenclatureConstants.ElectionPersonStates.TechError)
                      .Where(x => x.ElectionGroupId == model.ElectionGroupId)
                      .Where(x => x.LawunitId == info.LawUnitId).FirstOrDefault();
            if (added != null)
            {
                result = true;
            }


            return result;

        }

        public SaveResultVM ValidatePersonAdd(ElectionPersonAddVM model)
        {
            if (repo.AllReadonly<ElectionProtocol>()
                   .Where(x => x.ElectionGroupId == model.ElectionGroupId)
                   .Where(x => x.DateElection == null)
                    .Any())
            {
                return new SaveResultVM(false, "Списъкът е финализиран! Не можете да добавяте нови лица");
            }

            return new SaveResultVM(true);
        }

        public SaveResultVM PersonChange(ElectionPersonChangeVM model)
        {
            var saved = repo.GetById<ElectionPerson>(model.Id);
            if (saved == null)
            {
                return new SaveResultVM(false, "Несъществувало лице");
            }

            if (saved.ElectionPersonStateId == NomenclatureConstants.ElectionPersonStates.TechError)
            {
                return new SaveResultVM(false, "Лицето не може да бъде коригирано.");
            }

            if (model.ElectionPersonStateId == NomenclatureConstants.ElectionPersonStates.Include)
            {
                model.ElectionPersonDismissalTypeId = null;
                model.DescriptionRemoved = null;
            }
            model.ElectionPersonDismissalTypeId = model.ElectionPersonDismissalTypeId.EmptyToNull();

            if (model.ElectionPersonStateId == NomenclatureConstants.ElectionPersonStates.Exclude)
            {
                if (model.ElectionPersonDismissalTypeId == null)
                {
                    return new SaveResultVM(false, "Изберете причина за неучастие.");
                }
                if (string.IsNullOrEmpty(model.DescriptionRemoved))
                {
                    return new SaveResultVM(false, "Въведете основание за неучастие.");
                }
            }





            saved.ElectionPersonStateId = model.ElectionPersonStateId;
            saved.DescriptionState = model.DescriptionState;
            saved.ElectionPersonDismissalTypeId = model.ElectionPersonDismissalTypeId;
            saved.DescriptionRemoved = model.DescriptionRemoved;
            if (model.ElectionPersonDismissalTypeId > 0)
            {
                saved.UserRemovedId = userContext.UserId;
                saved.DateRemoved = DateTime.Now;
            }
            else
            {
                saved.UserRemovedId = null;
                saved.DateRemoved = null;
            }

            repo.SaveChanges();

            var info = repo.AllReadonly<ElectionPerson>()
                            .Where(x => x.Id == model.Id)
                            .Select(x => new
                            {
                                StateName = x.State.Label,
                                x.DescriptionState,
                                DismissalType = (x.ElectionPersonDismissalTypeId > 0) ? x.DismissalType.Label : "",
                                x.DescriptionRemoved,
                                x.DateRemoved
                            }).FirstOrDefault();
            var operationDescription = $"{info.StateName} ({info.DescriptionState})";
            if (!string.IsNullOrEmpty(info.DismissalType))
            {
                operationDescription += $"; Причина: {info.DismissalType} ,{info.DateRemoved}({info.DescriptionRemoved})";
            }

            var log = new ElectionLog()
            {
                ElectionGroupId = saved.ElectionGroupId,
                PersonGid = saved.PersonGid,
                ElectionPersonlId = saved.Id,
                LawunitId = saved.LawunitId,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId,
                Operation = NomenclatureConstants.ElectionLogOperation.EditPerson,
                Description = operationDescription
            };

            repo.Add(log);
            repo.SaveChanges();


            return new SaveResultVM(true);
        }

        public ElectionPersonPreviewVM PersonGetById(int id)
        {
            var result = repo.AllReadonly<ElectionPerson>()
                        .Where(x => x.Id == id)
                        .Where(x => x.ElectionProtocolId == null)
                        .Select(x => new ElectionPersonPreviewVM
                        {
                            Id = x.Id,
                            PersonGid = x.PersonGid,
                            ElectionGroupId = x.ElectionGroupId,
                            ElectionTitle = x.ElectionGroup.Label,
                            FullName = x.LawunitFullName,
                            CourtName = x.Court.Label,
                            Uic = x.LawUnit.Uic,
                            StateId = x.ElectionPersonStateId,
                            StateName = x.State.Label,
                            StateNameDescription = x.DescriptionState,
                            DismissalTypeId = x.ElectionPersonDismissalTypeId,
                            DismissalType = (x.ElectionPersonDismissalTypeId > 0) ? x.DismissalType.Label : "",
                            DismissalTypeDescription = x.DescriptionRemoved,
                            DateDeclaration = x.DateDeclaration
                        }).FirstOrDefault();

            result.CanChange = true;
            if (NomenclatureConstants.ElectionDismissalTypes.FinalStates.Contains(result.DismissalTypeId))
            {
                result.CanChange = false;
            }
            return result;
        }

        public IQueryable<ElectionPersonVM> Persons_Select(int groupId)
        {
            return repo.AllReadonly<ElectionPerson>()
                        .Where(x => x.ElectionGroupId == groupId)
                        .OrderBy(x => x.LawunitFullName)
                        .ThenBy(x => x.Court.Label)
                        .ThenBy(x => x.DateAdded)
                        .Where(x => x.ElectionProtocolId == null)
                        .Select(x => new ElectionPersonVM
                        {
                            Id = x.Id,
                            CourtName = x.Court.Label,
                            FullName = x.LawunitFullName,
                            DateAdded = x.DateAdded,
                            DateRemoved = x.DateRemoved,
                            StateName = x.State.Label,
                            Egn = x.LawUnit.Uic,
                            DescriptionRemoved = x.DescriptionRemoved,
                            DeclarationDate = x.DateDeclaration
                        });
        }
        public IQueryable<ElectionPersonVM> Persons_SelectByProtocol(int protcoolId)
        {
            return repo.AllReadonly<ElectionPerson>()
                        .OrderBy(x => x.LawunitFullName)
                        .ThenBy(x => x.Court.Label)
                        .ThenBy(x => x.DateAdded)
                         .Where(x => x.ElectionProtocolId == protcoolId)
                        .Select(x => new ElectionPersonVM
                        {
                            Id = x.Id,
                            CourtName = x.Court.Label,
                            FullName = x.LawunitFullName,
                            DateAdded = x.DateAdded,
                            DateRemoved = x.DateRemoved,
                            StateName = x.State.Label,
                            Egn = x.LawUnit.Uic,
                            DescriptionRemoved = x.DescriptionRemoved

                        });

        }
        public IQueryable<ElectionGroupVM> ElectionGroup_Select()
        {

            return repo.AllReadonly<ElectionGroup>()
                
                        .Select(x => new ElectionGroupVM()
                        {
                            Id = x.Id,
                            CourtId = x.CourtId,
                            DocumentId = x.DocumentId,
                            DocumentNumber = x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy г."),
                            DocumentDate = x.Document.DocumentDate,
                            ElectionTypeId = x.ElectionTypeId,
                            ElectionType = x.ElectionType.Label,
                            Description = x.Description,
                            DateWrt = x.DateWrt,
                            Label = x.Label,
                            UserId = x.UserId

                        }).AsQueryable();
        }
        public ElectionGroupVM ElectionGroup_SelectById(int id)
        {

            return repo.AllReadonly<ElectionGroup>()
                        .Include(x => x.Document)
                        .Include(x => x.ElectionType)
                        .Select(x => new ElectionGroupVM()
                        {
                            Id = x.Id,
                            CourtId = x.CourtId,
                            DocumentId = x.DocumentId,
                            DocumentNumber = x.Document.DocumentNumber + "/" + x.Document.DocumentDate.ToString("dd.MM.yyyy г."),
                            ElectionTypeId = x.ElectionTypeId,
                            ElectionType = x.ElectionType.Label,
                            Description = x.Description,
                            DateWrt = x.DateWrt,
                            Label = x.Label,
                            UserId = x.UserId
                        }).Where(x => x.Id == id)
                           .FirstOrDefault();
        }
        public ElectionGroup GetElectionGroupByID(int id)
        {
            return repo.AllReadonly<ElectionGroup>()
                .Include(x => x.ElectionType)
                .Include(x => x.Document)
                .Where(x => x.Id == id).FirstOrDefault();
        }
        public ElectionProtocol GetElectionProtocolByID(int id)
        {
            return repo.AllReadonly<ElectionProtocol>().Where(x => x.Id == id).FirstOrDefault();
        }
        public List<SelectListItem> GetDDL_ElectionType()
        {
            var selectList = repo.All<ElectionType>()

                                       .Select(x => new SelectListItem()
                                       {
                                           Text = x.Label,
                                           Value = x.Id.ToString()
                                       })
                                        .ToList();


            selectList = selectList
                            .Prepend(new SelectListItem() { Text = "Изберете", Value = "-1" })
                            .ToList();

            return selectList;
        }
        public int Election_Save(ElectionGroupVM model)

        {
            int current_id = model.Id ?? 0;

            try
            {
                if (current_id > 0)
                // Редакция 
                {
                    var saved = repo.All<ElectionGroup>()
                    .Where(x => x.Id == model.Id).FirstOrDefault();

                    saved.UserId = userContext.UserId;
                    // saved.DateWrt = DateTime.Now;
                    saved.DocumentId = model.DocumentId;
                    saved.Description = model.Description;
                    saved.Label = model.Label;
                    repo.Update(saved);
                    repo.SaveChanges();
                    var el = ElectionGroup_SelectById(saved.Id);
                    WriteElectionLog(el.Id.Value, null, null, NomenclatureConstants.ElectionLogOperation.ElectionEdit, string.Format("Сесия: {0}; Описание:{1}; Вид избор:{2}; Документ:{3} ", el.Label, el.Description, el.ElectionType, el.DocumentNumber), null);

                }
                else
                //Нов
                {
                    ElectionGroup electionGroup = new ElectionGroup();
                    electionGroup.UserId = userContext.UserId;
                    electionGroup.DateWrt = DateTime.Now;
                    electionGroup.DocumentId = model.DocumentId;
                    electionGroup.Description = model.Description;
                    electionGroup.Label = model.Label;
                    electionGroup.CourtId = NomenclatureConstants.VKScourtId;
                    electionGroup.ElectionTypeId = model.ElectionTypeId;


                    repo.Add<ElectionGroup>(electionGroup);
                    repo.SaveChanges();
                    var el = ElectionGroup_SelectById(electionGroup.Id);
                    WriteElectionLog(el.Id.Value, null, null, NomenclatureConstants.ElectionLogOperation.ElectionAdd, string.Format("Сесия: {0}; Описание:{1}; Вид избор:{2}; Документ:{3} ", el.Label, el.Description, el.ElectionType, el.DocumentNumber), null);

                    current_id = electionGroup.Id;
                }

            }
            catch (Exception ex)
            {

                throw;
            }


            return current_id;



        }
        public ElectionProtocolVM ElectionProtokol_Preview(int id)
        {
            var result = repo.All<ElectionProtocol>()
                 .Include(x => x.ElectionPersons)
                 .ThenInclude(x => x.LawUnit)
                 .ThenInclude(x => x.Courts)
                  .Include(x => x.ElectionGroup)
                  .Include(x => x.ElectionGroup.Document)
                  .Where(x => x.Id == id)

                 .Select(x => new ElectionProtocolVM()
                 {
                     Id = x.Id,
                     ElectionGroupId = x.ElectionGroupId,
                     CourtName = x.ElectionGroup.Court.Label,
                     ElectionGroupTypeID = x.ElectionGroup.ElectionTypeId,
                     DocumentNumber = x.ElectionGroup.Document.DocumentNumber + "/" + x.ElectionGroup.Document.DocumentDate.ToString("dd.MM.yyyy г."),
                     SelectionType = "Автоматично случайно",
                     SelectedElectionPersonId = x.SelectedElectionPersonId,
                     SelectedLawunitId = x.SelectedLawunitId,
                     SelectedLawunitCourtId = x.SelectedLawunitCourtId,
                     SelectedLawunitFullName = x.SelectedLawunitFullName + " _ " + x.SelectedLawunitCourt.Label,
                     SelectedLawunitCourtName = x.SelectedLawunitCourt.Label,
                     DateAdded = x.DateAdded,
                     DateAddedString = x.DateAdded.ToString("dd.MM.yyyy HH:mm"),
                     DateSigned = x.DateSigned,
                     PrevLawunitId = x.PrevLawunitId,
                     PrevLawunitFullName = x.PrevLawunitFullName,
                     PrevLawunitCourtId = x.PrevLawunitCourtId,
                     Description = x.Description,
                     UserAddedId = x.UserAddedId,
                     UserAddedName = x.UserAdded.LawUnit.FullName,
                     DateElection = x.DateElection,
                     ElectionGroupLabel=x.ElectionGroup.Label,

                     ElectionProtocolPersons = x.ElectionPersons.Select(p => new ElectionProtocolPersonVM
                     {
                         Id = p.Id,
                         LawUnitFullName = p.LawunitFullName,
                         CourtId = p.CourtId,
                         CourtName = p.Court.Label,
                         ElectionPersonStateId = p.ElectionPersonStateId,
                         ElectionPersonDismissalTypeId = p.ElectionPersonDismissalTypeId,
                         ElectionPersonDismissalTypeName = "",
                         DescriptionRemoved = p.DescriptionRemoved,
                         DescriptionState = p.DescriptionState



                     }

                     )
                 }).FirstOrDefault();


            return result;
        }
        public ElectionProtocol Get_ElectionProtokol(int id)
        {
            var result = repo.GetById<ElectionProtocol>(id);


            return result;
        }
        public bool Save_ElectionProtokol(ElectionProtocol protocol)
        {
            bool result = false;

            try
            {
                repo.Update<ElectionProtocol>(protocol);
                repo.SaveChanges();
                result = true;
            }
            catch (Exception)
            {

                throw;
            }
            return result;


        }
        public IQueryable<ElectionProtocolVM> ElectionProtocolList(int ElectionGroupId)
        {

            var files = repo.AllReadonly<MongoFile>();
            var tbl = repo.All<Document>().Include(x => x.DocumentType);
            return repo.AllReadonly<ElectionProtocol>()
                 .Include(x => x.UserAdded)
                 .ThenInclude(x => x.LawUnit)
                  .Include(x => x.UserElection)
                   .ThenInclude(x => x.LawUnit)
                   .Where(x => x.SelectedElectionPersonId > 0)
                   .Where(x=>x.ElectionGroupId== ElectionGroupId)
               .Select(x => new ElectionProtocolVM()
               {
                   Id = x.Id,
                   UserAddedName = x.UserAdded.LawUnit.FullName,
                   DateAdded = x.DateAdded,
                   DateElection = x.DateElection,
                   DateSigned = x.DateSigned,
                   SelectedLawunitFullName = x.SelectedLawunitFullName,
                   FileId = files.Where(a => (a.SourceIdNumber == x.Id) && (a.SourceType == SourceTypeSelectVM.ElectionProtocol)).FirstOrDefault().FileId,
                   FileName = files.Where(a => (a.SourceIdNumber == x.Id) && (a.SourceType == SourceTypeSelectVM.ElectionProtocol)).FirstOrDefault().FileName,
                   IsSigned = (x.DateSigned != null)

               }

            )
            .ToList().AsQueryable();


        }
        public ICollection<ElectionPerson> GetFinishedListByGroup(int electionGroupId)
        {
            return repo.AllReadonly<ElectionPerson>()
                           .Where(x => x.ElectionGroupId == electionGroupId)

                           .Where(x => x.ElectionProtocolId == null)
                           .Where(x => x.HasDeclaration == true)
                           .Where(x => x.ElectionPersonStateId != NomenclatureConstants.ElectionPersonStates.TechError)
                           .OrderBy(x => x.LawunitFullName)
                           .ThenBy(x => x.Court.Label)
                           .ThenBy(x => x.DateAdded)
                           .ToList();
        }

        public int PersonFinish_Create(int electionGroupId)
        {
            int result = 0;
            ICollection<ElectionPerson> finished = GetFinishedListByGroup(electionGroupId);


            ElectionProtocol electionProtocol = new ElectionProtocol();
            electionProtocol.ElectionGroupId = electionGroupId;
            electionProtocol.DateAdded = DateTime.Now;
            electionProtocol.UserAddedId = userContext.UserId;
            repo.Add<ElectionProtocol>(electionProtocol);
            repo.SaveChanges();
            foreach (var person in finished)
            {
                ElectionPerson newPerson = new ElectionPerson();
                newPerson.ElectionGroupId = electionProtocol.ElectionGroupId;
                newPerson.ElectionProtocolId = electionProtocol.Id;
                newPerson.PersonGid = person.PersonGid;
                newPerson.LawunitId = person.LawunitId;
                newPerson.LawunitFullName = person.LawunitFullName;
                newPerson.CourtId = person.CourtId;
                newPerson.DateAdded = person.DateAdded;
                newPerson.UserAddedId = person.UserAddedId;
                newPerson.ElectionPersonStateId = person.ElectionPersonStateId;
                newPerson.DescriptionState = person.DescriptionState;
                newPerson.ElectionPersonDismissalTypeId = person.ElectionPersonDismissalTypeId;
                newPerson.UserRemovedId = person.UserRemovedId;
                newPerson.DateRemoved = person.DateRemoved;
                newPerson.DescriptionRemoved = person.DescriptionRemoved;
                newPerson.DateDeclaration = person.DateDeclaration;
                newPerson.HasDeclaration = person.HasDeclaration;
                repo.Add<ElectionPerson>(newPerson);



            }
            repo.SaveChanges();
            result = electionProtocol.Id;
            WriteElectionLog(electionGroupId, null, null, NomenclatureConstants.ElectionLogOperation.FinalizeList, "", null);
            return result;

        }
        public int GetNotFilledProtocol_ID(int electionGroupId)
        {
            int result = 0;
            var protocol = repo.AllReadonly<ElectionProtocol>()
                .Where(x => x.ElectionGroupId == electionGroupId)
                .Where(x => x.DateSigned == null).FirstOrDefault();
            if (protocol != null)
            { result = protocol.Id; }
            return result;
        }

        public int GetNotSignedProtocol_ID(int electionGroupId)
        {
            int result = 0;
            var protocol = repo.AllReadonly<ElectionProtocol>()
                .Where(x => x.ElectionGroupId == electionGroupId)
                .Where(x => x.DateSigned == null)
                .Where(x=>x.DateElection!=null).FirstOrDefault();
            if (protocol != null)
            { result = protocol.Id; }
            return result;
        }
        public bool ElectionProtocolSetSelectedLawUnit(int electionProtocolID)
        {
            bool result = false;
            try
            {
                var electionProtocol = repo.GetById<ElectionProtocol>(electionProtocolID);


                var persons = GetFinishedListByGroup(electionProtocol.ElectionGroupId).Where(x => x.ElectionPersonStateId == NomenclatureConstants.ElectionPersonStates.Include);
                List<Guid> lawUnits = new List<Guid>();
                for (int i = 0; i < 10; i++)
                {
                    foreach (var person in persons)
                    {
                        lawUnits.Add(person.PersonGid);
                    }
                }

                //Избор след като 2 пъти рандом се избере един  съдия
                List<Guid> LawUnnitsSelectedForFirstTime = new List<Guid>();
                bool exitLoop = false;
                int r = 0;
                Guid? selectedPersonGuid = null;
                while (exitLoop == false)
                {
                    Random rnd = new Random();
                    r = rnd.Next(lawUnits.Count);

                    if (LawUnnitsSelectedForFirstTime.Contains(lawUnits[r]))
                    {
                        selectedPersonGuid = lawUnits[r];
                        exitLoop = true;
                    }
                    else
                    { LawUnnitsSelectedForFirstTime.Add(lawUnits[r]); }


                }
                ElectionPerson selectedPerson = repo.AllReadonly<ElectionPerson>()
                                             .Where(x => x.ElectionProtocolId == electionProtocolID)
                                             .Where(x => x.PersonGid == selectedPersonGuid).FirstOrDefault();


                electionProtocol.DateElection = DateTime.Now;
                electionProtocol.SelectedElectionPersonId = selectedPerson.Id;
                electionProtocol.SelectedLawunitId = selectedPerson.LawunitId;
                electionProtocol.SelectedLawunitFullName = selectedPerson.LawunitFullName;
                electionProtocol.SelectedLawunitCourtId = selectedPerson.CourtId;
                repo.Update<ElectionProtocol>(electionProtocol);
                repo.SaveChanges();
                result = true;

                var selectedElectionPerson = persons.Where(x => x.PersonGid == selectedPersonGuid).FirstOrDefault();
                WriteElectionLog(electionProtocol.ElectionGroupId, selectedPersonGuid, null, NomenclatureConstants.ElectionLogOperation.ElectionProtocolAdd, string.Format("Избран съдия: {0};", selectedElectionPerson.LawunitFullName), selectedElectionPerson.Id);


            }
            catch (Exception)
            {

                throw;
            }

            return result;
        }


        public bool IsPersonInUse(Guid personGid)
        {
            return repo.AllReadonly<ElectionPerson>()
                            .Where(x => x.PersonGid == personGid)
                            .Where(x => x.ElectionProtocolId != null)
                            .Any();
        }

        public List<SelectListItem> GetDDL_PersonStatesForPerson(int personId)
        {
            var person = repo.GetById<ElectionPerson>(personId);

            Expression<Func<ElectionPersonState, bool>> expressionStates = x => true;
            if (IsPersonInUse(person.PersonGid))
            {
                expressionStates = x => x.Id != NomenclatureConstants.ElectionPersonStates.TechError;
            }

            if (NomenclatureConstants.ElectionDismissalTypes.FinalStates.Contains(person.ElectionPersonDismissalTypeId)
                || person.ElectionPersonStateId == NomenclatureConstants.ElectionPersonStates.TechError)
            {
                expressionStates = x => x.Id == person.ElectionPersonStateId;
            }

            var selectList = repo.All<ElectionPersonState>()
                                    .Where(expressionStates)
                                    .Select(x => new SelectListItem()
                                    {
                                        Text = x.Label,
                                        Value = x.Id.ToString()
                                    })
                                    .ToList();

            return selectList;
        }

        public List<SelectListItem> GetDDL_DismissalTypeForPerson(int personId)
        {
            var person = repo.GetById<ElectionPerson>(personId);
            //Всички статуси
            int[] states = repo.AllReadonly<ElectionPersonDismissalType>()
                                .Where(x => x.IsActive)
                                .Select(x => x.Id)
                                .ToArray();

            Expression<Func<ElectionPersonDismissalType, bool>> expressionStates = x => true;

            bool isFinalState = NomenclatureConstants.ElectionDismissalTypes.FinalStates.Contains(person.ElectionPersonDismissalTypeId);
            if (isFinalState)
            {
                expressionStates = x => x.Id == person.ElectionPersonDismissalTypeId;
            }

            var selectList = repo.All<ElectionPersonDismissalType>()
                                    .Where(expressionStates)
                                    .Select(x => new SelectListItem()
                                    {
                                        Text = x.Label,
                                        Value = x.Id.ToString()
                                    })
                                    .ToList();

            if (!isFinalState)
            {
                selectList = selectList
                                .Prepend(new SelectListItem() { Text = "Изберете", Value = "-1" })
                                .ToList();
            }
            return selectList;
        }

        public IQueryable<ElectionLogVM> SelectLog(int? electionGroupId, Guid? personGid, int? electionProtocolId)
        {
            Expression<Func<ElectionLog, bool>> expression = x => false;
            if (electionGroupId > 0)
            {
                expression = x => x.ElectionGroupId == electionGroupId.Value;
            }
            if (personGid.HasValue)
            {
                expression = x => x.PersonGid == personGid.Value;
            }
            if (electionProtocolId > 0)
            {
                expression = x => x.ElectionProtocolId == electionProtocolId.Value;
            }

            return repo.AllReadonly<ElectionLog>()
                        .Where(expression)
                        .OrderByDescending(x => x.Id)
                        .Select(x => new ElectionLogVM
                        {
                            DateWrt = x.DateWrt,
                            Operation = x.Operation,
                            Description = x.Description,
                            UserName = $"{x.User.LawUnit.FullName} ({x.User.Email})",
                            AfectedElectionPerson = x.ElectionPerson.LawunitFullName

                        });
        }

        public SaveResultVM PersonSaveDeclaration(int id)
        {
            var person = repo.GetById<ElectionPerson>(id);
            if (person == null)
            {
                return new SaveResultVM(false, "Лицето не е намерено.");
            }

            if ((person.HasDeclaration == true) || (person.DateDeclaration != null))
            {
                return new SaveResultVM(false, "Лицето вече има декларация.");
            }
            if (person.ElectionProtocolId != null)
            {
                return new SaveResultVM(false, "Лицето не е от основен списък.");
            }

            person.HasDeclaration = true;
            person.DateDeclaration = DateTime.Now;

            var log = new ElectionLog()
            {
                ElectionGroupId = person.ElectionGroupId,
                PersonGid = person.PersonGid,
                LawunitId = person.LawunitId,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId,
                ElectionPersonlId = person.Id,
                Operation = NomenclatureConstants.ElectionLogOperation.DeclarationPerson,
                Description = "Добавена декларация. Лицето участва в избор."
            };

            repo.Add(log);
            repo.SaveChanges();

            return new SaveResultVM(true);
        }

        public void WriteElectionLog(int electionGroupId, Guid? personGid, int? lawunitId, string operation, string description, int? electionPersonId)
        {
            var log = new ElectionLog()
            {
                ElectionGroupId = electionGroupId,
                PersonGid = personGid,
                LawunitId = lawunitId,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId,
                ElectionPersonlId = electionPersonId,
                Operation = operation,
                Description = description
            };

            repo.Add(log);
            repo.SaveChanges();
        }

        public ElectionPerson GetElectionPersonBySelectedElectionPersonId(int id)
        {
            var selectedElectionPerson = repo.GetById<ElectionPerson>(id);

            return repo.AllReadonly<ElectionPerson>().Where(x => x.ElectionProtocolId == null)
                                                     .Where(x => x.PersonGid == selectedElectionPerson.PersonGid)
                                                     .Where(x => x.ElectionGroupId == selectedElectionPerson.ElectionGroupId).FirstOrDefault();
        }
    }

}
