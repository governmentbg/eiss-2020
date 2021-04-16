using IOWebApplicationService.Infrastructure.Data.DW;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Contracts
{
  public interface IDWDocumentService
  {
    bool DocumentInsertUpdate(DWDocument current, DWCourt court);
    IEnumerable<DWDocument> SelectDocumentTransfer(int selectedRowCount, DWCourt court);
    void DocumentTransfer(DWCourt court);

    bool DocumentCaseInfoInsertUpdate(DWDocumentCaseInfo current);
    IEnumerable<DWDocumentCaseInfo> SelectDocumentCaseInfoTransfer(long documentId, DWCourt court);
    void DocumentCaseInfoTransfer(DWCourt court, long documenId);


    bool DocumentInstitutionCaseInfoInsertUpdate(DWDocumentInstitutionCaseInfo current);
    IEnumerable<DWDocumentInstitutionCaseInfo> SelectDocumentInstitutionCaseInfoTransfer(long documentId, DWCourt court);

    void DocumentInstitutionCaseInfoTransfer(DWCourt court, long documenId);

    bool DocumentLinkInsertUpdate(DWDocumentLink current);
    IEnumerable<DWDocumentLink> SelectDocumentLinkTransfer(long documentId, DWCourt court);
     void DocumentLinkTransfer(DWCourt court, long documenId);
    bool DocumentDecisionInsertUpdate(DWDocumentDecision current, DWCourt court);
    IEnumerable<DWDocumentDecision> SelectDocumentDecisionTransfer(int selectedRowCount, DWCourt court);
    void DocumentDecisionTransfer(DWCourt court);
    bool DocumentDecisionCaseInsertUpdate(DWDocumentDecisionCase current);
    IEnumerable<DWDocumentDecisionCase> SelectDocumentDecisionCaseTransfer(long decisionId, DWCourt court);
    void DocumentDecisionCaseTransfer(DWCourt court, long decisionId);

  }
}
