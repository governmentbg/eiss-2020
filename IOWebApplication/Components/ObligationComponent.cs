using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    public class ObligationComponent : ViewComponent
    {
        private readonly IMoneyService moneyService;
        public ObligationComponent(IMoneyService moneyService)
        {
            this.moneyService = moneyService;
        }
        public async Task<IViewComponentResult> InvokeAsync(int caseId)
        {
            var documentInfo = await moneyService.GetPropByIdAsync<Case, DocumentIndetifiersVM>(x => x.Id == caseId, x => new DocumentIndetifiersVM
            {
                DocumentId = x.DocumentId,
                AssignmentDocumentId = x.Document.AssignmentDocumentId
            });

            var data = await moneyService.Obligation_Select(0, documentInfo.DocumentId, 0, NomenclatureConstants.Courts.RandomAssignment, documentInfo.AssignmentDocumentId ?? 0)
                                         .Where(x => x.IsActive)
                                        .ToListAsync();

            return await Task.FromResult<IViewComponentResult>(View(data));
        }


        protected class DocumentIndetifiersVM
        {
            public long DocumentId { get; set; }
            public long? AssignmentDocumentId { get; set; }
        }
    }


}
