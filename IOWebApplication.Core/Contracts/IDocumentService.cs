using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IDocumentService : IBaseService
    {
        Task<DocumentVM> Document_Init(int documentDirection, int templateId = 0, long electronicDocumentId = 0);

        Task<DocumentVM> Document_GetById(long id);
        Task<bool> Document_SaveData(DocumentVM model);
        bool Document_CorrectData(DocumentVM model);

        /// <summary>
        /// Метод извличащ данни за регистрирани документи
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<DocumentListVM> Document_Select(DocumentFilterVM filter);

        Task<List<SelectListItem>> GetDocumentRegistratures(bool appendallItem = false);

        /// <summary>
        /// Метод зареждащ данни за начини на получаване/изпращане по направление на документ
        /// </summary>
        /// <param name="documentDirection">Направление</param>
        /// <param name="addDefaultElement">Флаг дали да добави елемент "Избери"</param>
        /// <returns></returns>
        List<SelectListItem> GetDeliveryGroups(int documentDirection, bool addDefaultElement = false);

        IEnumerable<LabelValueVM> GetDocument(int courtId, string documentNumber, int docDirection);
        bool CheckDocumentOldNumber(int courtId, int docDirectionId, string documentNumber, DateTime documentDate);
        LabelValueVM GetDocumentById(int id);
        List<SelectListItem> DocumentPerson_SelectForDropDownList(long documentId);
        string GetDataInstitutionCaseInfoForDocument(long documentId);
        DocumentSelectPersonsVM Case_SelectPersons(int caseId);
        Task<List<DocumentPersonVM>> SelectDocumentPersonsFromCase(DocumentSelectPersonsVM model, int index);
        Task<IQueryable<DocumentSelectAddressVM>> SelectAddressListByPerson(string uic, int uicTypeId, int? personSourceType,
                        long? personSourceId);
        Task<(bool result, string errorMessage)> DocumentDecision_SaveData(DocumentDecision model);
        DocumentDecision DocumentDecision_SelectForDocument(long documentId);
        IQueryable<DocumentDecisionListVM> DocumentDecision_Select(int courtId, DocumentDecisionFilterVM model);
        IQueryable<DocumentDecisionCaseListVM> DocumentDecisionCase_Select(long documentDecisionId);
        IQueryable<DocumentDecisionCaseListVM> DocumentDecisionCaseByCase_Select(int CaseId);
        (bool result, string errorMessage) DocumentDecisionCase_SaveData(DocumentDecisionCase model);
        DocumentSelectPersonsVM Document_SelectPersons(long documentId);
        List<DocumentPersonVM> SelectDocumentPersonsFromDocument(DocumentSelectPersonsVM model, int index);

        /// <summary>
        /// Метод извличащ данни за справка съпровождащи документи
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<DocumentCaseInfoSprVM> DocumentCaseInfoSpr_Select(DocumentCaseInfoSprFilterVM filter);

        Document GetByIdWithData(long id);
        bool IsCanExpireCompliantDocument(long id);
        SaveResultVM CheckCanExpireDocument(long id);
        Task<bool> DocumentExpire(ExpiredInfoVM model);

        Task<bool> Reactivate(DocumentReactivateVM model);

        IQueryable<DocumentInstitutionCaseInfoListVM> DocumentInstitutionCaseInfo_Select(long documentId);
        bool DocumentInstitutionCaseInfo_SaveData(DocumentInstitutionCaseInfoEditVM model);
        DocumentInstitutionCaseInfoEditVM GetById_InstitutionCaseInfoEditVM(long Id);
        IQueryable<DocumentInfoVM> DocumentsOtherFromSameCourtByCaseId_Select(int CaseId);
        IQueryable<DocumentInfoVM> DocumentsOtherFromDifferentCourtByCaseId_Select(int CaseId);

        /// <summary>
        /// всички съпровождащи документи по свързано дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        List<SelectListItem> GetCompliantDocumentsByCaseId(int caseId, bool addInitDoc = false);
        Task<List<SelectListItem>> GetCompliantDocumentsByCaseIdAsync(int caseId, bool addInitDoc = false);
        List<SelectListItem> GetDocumentPersonsByDocumentId(long documentId);
        List<SelectListItem> GetDocumentPersonsByDocumentIdWithIdName(long documentId);
        Task<bool> Document_SaveCommonToCompliant(DocumentVM model);
        Task<ElectronicDocumentInfoVM> GetElectronicDocumentInfo(long id);
        IQueryable<ElectronicDocumentNewVM> GetElectronicDocumentNew();
        Task<long> CheckForRegisteredDocumentByElectronicId(long electronicDocumentId);
        Task<bool> InitializeDocumentVMFromRequest(DocumentVM documentModel, string requestCode, int id = 0);
        Task<DocumentVM> Document_InitFromRequestCodeForAssignment(int requestTypeId);
        Task<bool> FinishElectronicDocumentSaveMoney(long? electronicDocumentId, Document model, DocumentPerson firstPerson);
        Task<SaveResultVM> ValidatePersonOrgs(DocumentVM documentModel);
        Task<SaveResultVM> ValidateDocumentAfterCR(DocumentVM documentModel);

        Task<long?> GetDocumentRequestTypeId(long documentId);
        Task<List<long>> GetAssignedDocumentList(long documentId);
        Task<AssignedDocumentInfoVM> GetAssignedDocumentsInfo(long documentId);
        Task<List<SelectListItem>> GetDDL_DocumentRequestTypes(bool appendallItem = false);
    }
}
