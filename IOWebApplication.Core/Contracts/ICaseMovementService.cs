using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseMovementService: IBaseService
    {
        /// <summary>
        /// Извличане на данни за местоположение
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        Task<IEnumerable<CaseMovementVM>> GetCaseMovementData(int caseId);

        /// <summary>
        /// Извличане на данни за редкация на местоположение
        /// </summary>
        /// <param name="id">Идентификатор на местоположението</param>
        /// <returns></returns>
        Task<CaseMovementVM> GetCaseMovementByEdit(int id);

        /// <summary>
        /// Метод за запис на местоположение
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<bool> CreateMovement(CaseMovementVM model);

        /// <summary>
        /// Сторно на местоположение
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<bool> StornoMovement(CaseMovementVM model);

        /// <summary>
        /// Редакция на приемане на местоположение
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        Task<bool> EditAcceptMovement(CaseMovementVM model);

        /// <summary>
        /// Приемане на местоположение
        /// </summary>
        /// <param name="id">Идентификатор на записа</param>
        /// <returns></returns>
        Task<bool> AcceptMovement(int id);

        /// <summary>
        /// Проверка дали може да се добави местоположение
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<bool> IsAddNewMovement(int caseId);

        /// <summary>
        /// Създаване на обратно действие за местоположение
        /// </summary>
        /// <param name="id">Идентификатор на движението</param>
        /// <returns></returns>
        Task<int> CreateReturnMovement(int id);

        /// <summary>
        /// Извличане на данни за местоположение за начален екран
        /// </summary>
        /// <returns></returns>
        IQueryable<CaseMovementVM> Select_ToDo();

        /// <summary>
        /// Извличанена бройки за начален екран
        /// </summary>
        /// <returns></returns>
        Task<int> Select_ToDoCount();

        /// <summary>
        /// Справка за местоположение
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="CaseRegNum"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        IQueryable<CaseMovementVM> Select_Spr(int courtId, string CaseRegNum, string UserId);

        /// <summary>
        /// Извличане на последно местоположение за дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        Task<string> GetLastMovmentForCaseId(int caseId);

        /// <summary>
        /// Метод извличащ данни за компонента
        /// </summary>
        /// <returns></returns>
        IQueryable<CaseMovementVM> Select_ToDoForComponent();
    }
}
