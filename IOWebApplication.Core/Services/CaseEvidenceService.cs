using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseEvidenceService : BaseService, ICaseEvidenceService
    {
        private readonly ICounterService counterService;

        public CaseEvidenceService(
        ILogger<CaseEvidenceService> _logger,
        IRepository _repo,
        ICounterService _counterService,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            counterService = _counterService;
        }

        /// <summary>
        /// Извличане на данни за доказателства
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="RegNumber"></param>
        /// <returns></returns>
        public IQueryable<CaseEvidenceVM> CaseEvidence_Select(int CaseId, DateTime? DateFrom, DateTime? DateTo, string RegNumber, string CaseRegNumber, int EvidenceTypeId)
        {
            Expression<Func<CaseEvidence, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(CaseRegNumber))
                caseRegnumberSearch = x => x.Case.RegNumber.ToLower().EndsWith(CaseRegNumber.ToShortCaseNumber().ToLower());

            return repo.AllReadonly<CaseEvidence>()
                       .Include(x => x.Case)
                       .Include(x => x.EvidenceType)
                       .Include(x => x.EvidenceState)
                       .Where(x => ((CaseId > 0) ? (x.CaseId == CaseId) : true) &&
                                   (x.DateExpired == null) &&
                                   ((DateFrom != null) ? ((x.DateAccept.Date >= (DateFrom ?? DateTime.Now).Date) && (x.DateAccept.Date <= (DateTo ?? DateTime.Now).Date)) : true) &&
                                   (!string.IsNullOrEmpty(RegNumber) ? x.RegNumber.ToUpper().Contains(RegNumber.ToUpper()) : true) &&
                                   x.Case.CourtId == userContext.CourtId &&
                                   (EvidenceTypeId > 0 ? x.EvidenceTypeId == EvidenceTypeId : true))
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Where(caseRegnumberSearch)
                       .Select(x => new CaseEvidenceVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           CaseName = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                           EvidenceTypeLabel = x.EvidenceType.Label,
                           RegNumber = x.RegNumber,
                           FileNumber = x.FileNumber,
                           DateAccept = x.DateAccept,
                           Description = x.Description,
                           AddInfo = x.AddInfo,
                           EvidenceStateLabel = (x.EvidenceState != null) ? x.EvidenceState.Label : string.Empty,
                           DateWrt = x.DateWrt
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Метод за запис на доказателство към дело
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseEvidence_SaveData(CaseEvidence model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseEvidence>(model.Id);
                    saved.EvidenceTypeId = model.EvidenceTypeId;
                    saved.FileNumber = model.FileNumber;
                    saved.DateAccept = model.DateAccept;
                    saved.Description = model.Description;
                    saved.AddInfo = model.AddInfo;
                    saved.Location = model.Location;
                    saved.EvidenceStateId = model.EvidenceStateId;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;

                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    if (counterService.Counter_GetEvidenceCounter(model, userContext.CourtId))
                    {
                        model.DateWrt = DateTime.Now;
                        model.UserId = userContext.UserId;
                        repo.Add<CaseEvidence>(model);
                        repo.SaveChanges();
                    }
                    else
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на доказателство по дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за движение на доказателство
        /// </summary>
        /// <param name="CaseEvidenceId"></param>
        /// <returns></returns>
        public IQueryable<CaseEvidenceMovementVM> CaseEvidenceMovement_Select(int CaseEvidenceId)
        {
            var caseEvidence = repo.GetById<CaseEvidence>(CaseEvidenceId);
            var caseSessionActs = repo.AllReadonly<CaseSessionAct>()
                                      .Include(x => x.CaseSession)
                                      .Include(x => x.ActType)
                                      .Where(x => x.CaseSession.CaseId == caseEvidence.CaseId)
                                      .ToList();

            return repo.AllReadonly<CaseEvidenceMovement>()
                       .Include(x => x.CaseEvidence)
                       .Include(x => x.EvidenceMovementType)
                       .Where(x => ((CaseEvidenceId > 0) ? (x.CaseEvidenceId == CaseEvidenceId) : true))
                       .Select(x => new CaseEvidenceMovementVM()
                       {
                           Id = x.Id,
                           CaseEvidenceId = x.CaseEvidenceId,
                           CaseEvidenceLabel = x.CaseEvidence.RegNumber,
                           EvidenceMovementTypeLabel = x.EvidenceMovementType.Label,
                           EvidenceMovementTypeId = x.EvidenceMovementTypeId,
                           MovementDate = x.MovementDate,
                           ActDescription = x.ActDescription,
                           CaseSessionActName = ((x.CaseSessionActId ?? 0) > 0) ? (caseSessionActs.Where(a => a.Id == x.CaseSessionActId).FirstOrDefault().ActType.Label + " " + caseSessionActs.Where(a => a.Id == x.CaseSessionActId).FirstOrDefault().RegNumber + "/" + (caseSessionActs.Where(a => a.Id == x.CaseSessionActId).FirstOrDefault().RegDate ?? DateTime.Now).ToString("dd.MM.yyyy")) : string.Empty,
                           Description = x.Description,
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на движение на доказателство
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseEvidenceMovement_SaveData(CaseEvidenceMovement model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseEvidenceMovement>(model.Id);
                    saved.MovementDate = model.MovementDate;
                    saved.EvidenceMovementTypeId = model.EvidenceMovementTypeId;
                    saved.Description = model.Description;
                    saved.ActDescription = model.ActDescription;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;

                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseEvidenceMovement>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на движение на доказателство по дело Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Взема само десните страни от лица по дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private string GetCasePersonsRightSide(int caseId)
        {
            var casePeoples = repo.AllReadonly<CasePerson>()
                .Include(x => x.PersonRole)
                .Where(x => x.CaseId == caseId && ((x.CaseSessionId ?? 0) < 1))
                .ToList();

            string _rightSide = string.Empty;
            foreach (var casePerson in casePeoples.Where(x => x.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide))
            {
                _rightSide += (!string.IsNullOrEmpty(_rightSide)) ? ", " : string.Empty;
                _rightSide += casePerson.FullName;
            }

            return _rightSide;
        }

        /// <summary>
        /// Извлича стринг за движенията към дело
        /// </summary>
        /// <param name="caseEvidenceMovementVMs"></param>
        /// <returns></returns>
        private string GetEvidenceMovements(List<CaseEvidenceMovementVM> caseEvidenceMovementVMs)
        {
            //var caseEvidenceMovementVMs = CaseEvidenceMovement_Select(evidenceId);
            string _movements = string.Empty;
            foreach (var caseEvidenceMovement in caseEvidenceMovementVMs)
            {
                _movements += (!string.IsNullOrEmpty(_movements)) ? ", " : string.Empty;
                _movements += caseEvidenceMovement.EvidenceMovementTypeLabel + " " + caseEvidenceMovement.MovementDate.ToString("dd.MM.yyyy HH:mm");
                if (string.IsNullOrEmpty(caseEvidenceMovement.Description) == false)
                    _movements += Environment.NewLine + caseEvidenceMovement.Description;
            }

            return _movements;
        }

        /// <summary>
        /// Извлича стринг за движение към друга институция
        /// </summary>
        /// <param name="caseEvidenceMovementVMs"></param>
        /// <param name="isSend"></param>
        /// <returns></returns>
        private string GetEvidenceMovementsDateSendOrReciveOtherInstitution(List<CaseEvidenceMovementVM> caseEvidenceMovementVMs, bool isSend)
        {
            //var caseEvidenceMovementVMs = CaseEvidenceMovement_Select(evidenceId);
            string _movements = string.Empty;

            foreach (var caseEvidenceMovement in caseEvidenceMovementVMs.Where(x => (x.EvidenceMovementTypeId == ((isSend) ? NomenclatureConstants.EvidenceMovementType.IzprashtaneDrugSyd : NomenclatureConstants.EvidenceMovementType.PoluchavaneDrugSyd))))
            {
                _movements += (!string.IsNullOrEmpty(_movements)) ? ", " : string.Empty;
                _movements += caseEvidenceMovement.MovementDate.ToString("dd.MM.yyyy");
            }

            return _movements;
        }

        /// <summary>
        /// Извлича данни за справка за движения
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private IQueryable<CaseEvidenceSprVM> CaseEvidenceSpr(int courtId, CaseEvidenceSprFilterVM model)
        {
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;

            Expression<Func<CaseEvidence, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.DateAccept.Date >= dateFromSearch.Date && x.DateAccept.Date <= dateToSearch.Date;

            Expression<Func<CaseEvidence, bool>> regNumberSearch = x => true;
            if (model.FromNumber != null || model.ToNumber != null)
            {
                regNumberSearch = x => (x.RegNumberValue ?? 0) >= (model.FromNumber ?? 1) && (x.RegNumberValue ?? 0) <= (model.ToNumber ?? int.MaxValue);
            }

            var caseEvidenceSprVMs = repo.AllReadonly<CaseEvidence>()
                .Where(x => x.Case.CourtId == courtId && x.DateExpired == null)
                .Where(x => x.EvidenceTypeId != NomenclatureConstants.EvidenceType.Electronically)
                .Where(regNumberSearch)
                .Where(dateSearch)
                .Select(x => new CaseEvidenceSprVM()
                {
                    Id = x.Id,
                    CaseId = x.CaseId,
                    CaseNumber = (x.Case != null) ? x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy") : string.Empty,
                    CaseGroupLabel = x.Case.CaseType.Label,
                    EvidenceTypeLabel = (x.EvidenceType != null) ? x.EvidenceType.Label : string.Empty,
                    RegNumber = x.RegNumber,
                    FileNumber = x.FileNumber ?? string.Empty,
                    DateAccept = x.DateAccept,
                    Description = (x.Description != null) ? x.Description : string.Empty,
                    EvidenceStateLabel = (x.EvidenceState != null) ? x.EvidenceState.Label : string.Empty
                })
                .ToList();

            foreach (var caseEvidence in caseEvidenceSprVMs)
            {
                caseEvidence.NamePodsydim = GetCasePersonsRightSide(caseEvidence.CaseId);
                var caseEvidenceMovementVMs = CaseEvidenceMovement_Select(caseEvidence.Id).ToList();
                caseEvidence.Movements = GetEvidenceMovements(caseEvidenceMovementVMs);
                caseEvidence.MovementsDateSend = GetEvidenceMovementsDateSendOrReciveOtherInstitution(caseEvidenceMovementVMs, true);
                caseEvidence.MovementsDateReceive = GetEvidenceMovementsDateSendOrReciveOtherInstitution(caseEvidenceMovementVMs, false);
            }

            return caseEvidenceSprVMs.AsQueryable();
        }

        /// <summary>
        /// Справка за доказателства в ексел
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public byte[] CaseEvidenceSpr_ToExcel(int courtId, CaseEvidenceSprFilterVM model)
        {
            var caseEvidenceSprs = CaseEvidenceSpr(courtId, model).OrderBy(x => x.DateAccept).ToList();

            var htmlTemplate = repo.AllReadonly<HtmlTemplate>()
                        .Where(x => x.Alias.ToUpper() == "Evidence".ToUpper())
                        .FirstOrDefault();
            NPoiExcelService excelService = new NPoiExcelService(htmlTemplate.Content, 0);
            excelService.rowIndex = (htmlTemplate.XlsDataRow ?? 0) - 1;

            excelService.InsertList(
                        caseEvidenceSprs,
                        new List<Expression<Func<CaseEvidenceSprVM, object>>>()
                        {
                            x => x.RegNumber,
                            x => x.DateAccept,
                            x => x.FileNumber,
                            x => x.CaseGroupLabel,
                            x => x.CaseNumber,
                            x => x.NamePodsydim,
                            x => x.Description,
                            x => x.MovementsDateSend,
                            x => x.MovementsDateReceive,
                            x => x.Movements
                        }
                    );
            
            return excelService.ToArray();
        }

        /// <summary>
        /// Проверка по дело дали е налично разпоредително действие от даден тип
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="MovmentTypeId"></param>
        /// <returns></returns>
        public bool IsExistMovmentType(int CaseId, int MovmentTypeId)
        {
            return repo.AllReadonly<CaseEvidenceMovement>()
                       .Any(x => x.CaseId == CaseId && x.EvidenceMovementTypeId == MovmentTypeId);
        }

        public bool IsExistMovment(int CaseEvidenceId)
        {
            return repo.AllReadonly<CaseEvidenceMovement>()
                       .Any(x => x.CaseEvidenceId == CaseEvidenceId);
        }
    }
}
