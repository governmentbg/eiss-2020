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
    public interface ICasePersonSentenceService : IBaseService
    {
        IQueryable<CasePersonSentenceVM> CasePersonSentence_Select(int CasePersonId);

        /// <summary>
        /// Запис на Присъда по лице в дело
        /// </summary>
        /// <param name="model">модел попълнен от потребител</param>
        /// <returns></returns>
        Task<bool> CasePersonSentence_SaveData(CasePersonSentenceEditVM model);

        CasePersonSentenceEditVM CasePersonSentence_GetById(int id);
        List<CheckListVM> FillLawBase();
        IQueryable<CaseCrimeVM> CaseCrime_Select(int CaseId);
        Task<bool> CaseCrime_SaveData(CaseCrime model);
        CaseCrimeVM CaseCrime_GetById(int id);
        IQueryable<CasePersonCrimeVM> CasePersonCrime_Select(int CaseCrimeId);
        bool CasePersonCrime_SaveData(CasePersonCrime model);
        bool IsExistPersonCasePersonCrime(int CaseCrimeId, int CasePersonId);
        Task<bool> CasePersonCrimeFillFromEispp_SaveData(int caseId, string pnenmr);
        List<SelectListItem> GetDropDownList_CasePersonCrime(int caseId, bool addDefaultElement = true, bool addAllElement = false);
        IQueryable<CasePersonSentencePunishmentVM> CasePersonSentencePunishment_Select(int CasePersonSentenceId);
        bool CasePersonSentencePunishment_SaveData(CasePersonSentencePunishment model);
        CasePersonSentencePunishmentVM CasePersonSentencePunishment_GetById(int id);
        bool IsExistMainPunishment(int CasePersonSentenceId, int? WithoutId);
        IQueryable<CasePersonSentencePunishmentCrimeVM> CasePersonSentencePunishmentCrime_Select(int CasePersonSentencePunishmentId);
        bool CasePersonSentencePunishmentCrime_SaveData(CasePersonSentencePunishmentCrime model);

        /// <summary>
        /// Изтриване на Наложени наказания към присъда
        /// </summary>
        /// <param name="Id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<bool> CasePersonSentencePunishmentCrime_DeleteData(int Id);
        List<SelectListItem> GetDropDownList_CasePersonSentence(int CasePersonId, int ModelId, bool addDefaultElement = true, bool addAllElement = false);
        CasePersonSentenceBulletin CasePersonSentenceBulletin_GetByIdPerson(int personId);
        Task<CasePersonSentenceBulletinEditVM> CasePersonSentenceBulletin_GetById(int id);
        Task<(bool result, string errorMessage)> CasePersonSentenceBulletin_SaveData(CasePersonSentenceBulletinEditVM model);
        CasePersonSentence CasePersonSentence_GetByPerson(int personId);
        bool IsEISPPNumberExists(int caseId, string eisppNumber);
        string GetTextNewBulletin(int sentenceId, int? sentenceChangeId);
        Task<SaveResultVM> SendBuletinForSign_Init(int caseBuletinId, int buletinFileId, long taskId);
    }
}
