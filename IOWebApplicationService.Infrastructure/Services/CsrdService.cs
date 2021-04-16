using IO.SignTools.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.CSRD;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;
using System.Net.Http;
using IOWebApplication.Infrastructure.Extensions;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class CsrdService : BaseMQService, ICsrdService
    {


        private readonly IConfiguration config;
        private Uri serviceUri;
        //private string CertificatePath;
        //private string CertificatePassword;
       // private readonly ICsrdHttpRequester csrdRequester;
        private HttpRequester requester;
        private readonly IHttpClientFactory clientFactory;

        public CsrdService(IRepository _repo,
            ICdnService _cdnService,
            IConfiguration _config,
            ILogger<CsrdService> _logger,
            IHttpClientFactory _clientFactory)
        {
            repo = _repo;
            cdnService = _cdnService;
            config = _config;
            logger = _logger;
            clientFactory = _clientFactory;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.CSRD;
        }
        protected override async Task<bool> InitChanel()
        {
            var endPoint = config.GetValue<string>("CSRD:Endpoint");
            var method = config.GetValue<string>("CSRD:Method");

            serviceUri = new Uri(new Uri(endPoint), method);

            requester = new HttpRequester(clientFactory.CreateClient("csrdHttpClient"));

            return true;
        }

        protected override Task CloseChanel()
        {
            //httpClient.Dispose();
            return Task.CompletedTask;
        }

        protected override async Task SendMQ(MQEpep mq)
        {
            switch (mq.SourceType)
            {
                case SourceTypeSelectVM.CaseSelectionProtokol:
                    await SendSelectionProtokol(mq);
                    break;
                default:
                    break;
            }
        }

        private async Task SendSelectionProtokol(MQEpep mq)
        {
            try
            {
                var model = await initModel((int)mq.SourceId);
                //HttpRequester http = new HttpRequester();
                //http.ValidateServerCertificate = false;
                //http.CertificatePath = this.CertificatePath;
                //http.CertificatePassword = this.CertificatePassword;

                //var response = http.PostAsync(serviceUri.AbsoluteUri, model).Result;
                var response = await requester.PostAsync(serviceUri.AbsoluteUri, model);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(content))
                    {
                        long assignmentId = long.Parse(content);
                        if (assignmentId > 0)
                        {
                            mq.IntegrationStateId = IntegrationStates.TransferOK;
                            mq.DateTransfered = DateTime.Now;
                            mq.ReturnGuidId = assignmentId.ToString();
                            repo.Update(mq);
                            repo.SaveChanges();
                            return;
                        }
                    }
                }
                else
                {
                    logger.LogError("CSRDService: Responce code:{code}, Message: {message}, MqID: {id}", response.StatusCode, response.ReasonPhrase, mq.Id);
                    mq.ErrorDescription = "Грешка при извикване на услуга към ЦСРД";
                }
                SetErrorToMQ(mq, IntegrationStates.TransferError);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                var innerException = ex.InnerException;

                while (innerException != null)
                {
                    logger.LogError(innerException.Message);

                    innerException = innerException.InnerException;
                }

                mq.ErrorDescription = ex.Message;
                SetErrorToMQ(mq, IntegrationStates.TransferError);
            }

        }


        private async Task<AssignmentRequestModel> initModel(int protocolId)
        {
            AssignmentRequestModel request = new AssignmentRequestModel();

            var protcol = repo.AllReadonly<CaseSelectionProtokol>()
              .Include(x => x.SelectedLawUnit)

              .Where(x => x.Id == protocolId)
              .FirstOrDefault();

            Case current_case = repo.AllReadonly<Case>()
              .Include(x => x.Court)
              .Include(x => x.CaseCharacter)
              .Include(x => x.CaseClassifications)
              .Where(x => x.Id == protcol.CaseId)
              .FirstOrDefault();
            request.Judge_ID = (protcol.SelectedLawUnitId ?? 0);
            request.TypeOfAssignment = protcol.SelectionModeId;
            if (protcol.SelectionModeId == 2)
            { request.TypeOfAssignment = 3; }
            if (protcol.SelectionModeId == 3)
            { request.TypeOfAssignment = 2; }
            request.Case_ID = current_case.Id;

            request.CourtNumber = Int32.Parse(repo.AllReadonly<CodeMapping>()
                                 .Where(x => x.Alias == "csrd_court")
                                 .Where(x => x.InnerCode == current_case.Court.Code)
                                .FirstOrDefault().OuterCode.ToString());


            request.CaseYear = current_case.RegDate.Year;
            string mappingCaseCode = current_case.CaseCharacter.Code;
            if (current_case.CaseClassifications.Where(x => x.ClassificationId == NomenclatureConstants.CaseClassifications.Secret)
                                                .Where(x => (x.CaseSessionId ?? 0) < 1).Count() > 0)
            { mappingCaseCode = mappingCaseCode + "secret"; }



            request.CaseCode = Int32.Parse(
              repo.AllReadonly<CodeMapping>()
                                 .Where(x => x.Alias == "csrd_character")
                                 .Where(x => x.InnerCode == mappingCaseCode)
                                 .FirstOrDefault().OuterCode.ToString());


            request.CaseFormationDate = current_case.RegDate;
            request.AssignmentDate = protcol.SelectionDate;
            request.Name = protcol.SelectedLawUnit.FirstName;
            if (string.IsNullOrEmpty(protcol.SelectedLawUnit.FamilyName))
            {
                request.Family = protcol.SelectedLawUnit.MiddleName ?? "";
            }
            else
            {
                request.Family = protcol.SelectedLawUnit.FamilyName;
                if (!string.IsNullOrEmpty(protcol.SelectedLawUnit.Family2Name))
                {
                    request.Family = request.Family + "-" + protcol.SelectedLawUnit.Family2Name;
                }
            }

            request.CaseNumber = current_case.ShortNumber;
            var fileBytes = (await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSelectionProtokol, SourceId = protocolId.ToString() }, CdnFileSelect.PostProcess.Flatten)).GetBytes();
            request.Protocol = fileBytes;


            return request;
        }

    }
}



