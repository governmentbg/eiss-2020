using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.VKS;
using IOWebApplication.Infrastructure.Models;
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
    public interface IVKSSelectionService : IBaseService
    {
    IQueryable<VksSelectionHeaderVM> VKSSelectionsHeader_GetList(bool Nakazatelni);
    IQueryable<VksSelectionProtocolVM> VKSSelectionProtocolList(int selectionId);

    IQueryable<VksSelectionLawunit> VKSSelections_Lawunit_GetList(int selectionId);
    List<SelectListItem> GetDDL_Kolegia(int courtId, bool? Nakazatelno = true);
    List<SelectListItem> GetDDL_SelectionYear();
    List<SelectListItem> GetDDL_SelectionPeriod();
    List<SelectListItem> GetDDL_Otdelenie(int KolegiaId);
    List<SelectListItem> GetDDL_OtdelenieByCourt(int KolegiaId, int? courtId = 0,int? caseGroupId = 0);
    List<SelectListItem> GetDDL_ReplacingLawunits( int SelectioId);
    VksSelectionHeaderVM VKSSelectionsHeader_Get(int id);
    VksSelection VKSSelection_Get(int id);
    int VKSSelectionHeader_Save(VksSelectionHeaderVM model);
    int VKSSelectionHeaderTO_Save(VksSelectionHeaderVM model);
    bool Already_Exist(VksSelectionHeaderVM model);
    bool GenerateSelectionSTAFF(int selectionID);
    bool SelectionLawUnitsUpdate(VksSelection selection);
    List<CourtDepartmentLawUnit> GetActualCourtDepartmentLawunits(int courtDepartrmentID, bool includeParentPredsedatel = false);
    bool AddUnknownLawUnits(VksSelection selection);
    int DeleteLastUnknownLawUnits(int id);
    int DeleteLawUnits(int id);
    VksSelectionCalendarVM GetSelectionCalendar(int selection_id);
    VksSelectionLawunit GetSelectionLawunitByID(int id);
    Boolean ReplacedLawunitSave(VksSelectionLawunit model);

    bool Save_MonthSessionsDates(VksSelectionCalendarVM model);
    bool Save_MonthSessionsDatesEDIT(VksSelectionCalendarVM model);
    VksSelectionProtocol Save_VksSelection_protocol(VksSelectionProtocol protocol);
    VksSelectionProtocol Get_VksSelection_protocol(int id);
    VksSelectionProtocol Get_VksSelection_protocol_byId(int id);
    bool DeleteUnsignedSelectionProtocols(int selectionId);
    string GetSelectionTitle(int selectionId);
    bool SelectionHasSignedProtocol(int selectionId);

    bool HeaderHasSignedProtocol(int selectionHeaderId);
    List<VksSessionDayCalendarVM> GetvksSessionDayCalendar(int[] LawunitsInCalendar, int? courtDepartmentId);
    int GetCourtDepartmentIdByCaseId(int caseId);
    int GetLawUnitIdByCaseId(int caseId);

    int GetUserOtdelenieID(int lawunitId);
    bool CanEditCurrentOtdelenie(int lawunitId, int OtdelenieID);

    int GetCourtDepartmentIdFromSelection(int selectionId);
    }
}
