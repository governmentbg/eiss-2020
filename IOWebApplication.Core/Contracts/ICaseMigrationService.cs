using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseMigrationService : IBaseService
    {
        IQueryable<CaseMigrationVM> Select(int caseId);
        IQueryable<CaseMigrationVM> SelectOutMove(int caseId);
        Task<CaseMigration> InitNewMigration(int caseId);
        SaveResultVM CheckData(CaseMigration model);
        bool SaveData(CaseMigration model);
        bool UnionCase(CaseMigrationUnionVM model);
        int GetLastMigrationAcceptToUse(CaseMigrationFindCaseVM model);
        SaveResultVM AcceptCaseMigration(int id, int caseId, string description = null, bool isNewInterval = false, int? migrationKind = null);
        List<SelectListItem> Get_MigrationTypes(int direction, int[] migrationTypes = null, int? caseId = null);

        /// <summary>
        /// Извличане на всчики съдилища от Вертикално движение на дело - между институциите за комбо
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="addDefaultElement">Добавяне на елемен "Избери"</param>
        /// <param name="addAllElement">Добавяне на елемент "Всички"</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDropDownList_Court(int caseId, bool addDefaultElement = true, bool addAllElement = false);

        List<SelectListItem> GetDropDownList_CourtCase(int caseId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDropDownList_ReturnCase(int caseId, bool addDefaultElement = true, bool addAllElement = false);
        bool IsExistMigrationWithComplainWithDocumentId(long DocumentId);
        bool IsExistMigrationWithAct(long ActId);
        Task<CaseMigrationVM> Case_GetPriorCase(long documentId);
        DateTime? GetDateTimeAcceptCaseAfterComplain(int CaseId, int PriorCaseId);
        Task<CaseMigrationPriorVM> GetPriorCaseInfo(int caseId);
        Task<SaveResultVM> SaveData_PriorCase(CaseMigrationPriorVM model);

        // <summary>
        /// Извличане на първото дело от Вертикално движение на дело - между институциите
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        int[] GetInitialCasesByCaseId(int caseId);

        int[] GetConnectedCasesByCaseId(int caseId, bool activeOnly = true);
    }
}
