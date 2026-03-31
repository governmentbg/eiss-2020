using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IOWebApplication.Components
{
    public class MainGroupComponent : ViewComponent
    {
        private readonly ITransactionService transactionService;
        public MainGroupComponent(ITransactionService _transactionService)
        {
            transactionService = _transactionService;
        }
        public async Task<IViewComponentResult> InvokeAsync(int sourceType, string title, string view = "Default")
        {
            var model = new MainTransactionVM()
            {
                Title = title,
                SourceType = sourceType,
                WaitingCount = await transactionService.SelectWaiting(sourceType)
            };
            var continueResult = await transactionService.ContinueOperation(sourceType);
            if (continueResult.Result == true)
            {
                model.ContinueSourceId = (long)continueResult.ObjectId;
                model.ContinueUrl = transactionService.GetOperationUrl(sourceType, model.ContinueSourceId);
            }
            return await Task.FromResult<IViewComponentResult>(View(view, model));
        }
    }
}
