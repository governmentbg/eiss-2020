using DataTables.AspNet.Core;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Rotativa.Extensions;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System.Linq;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Rotativa.AspNetCore.Options;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;

namespace IOWebApplication.Controllers
{
    public class DocumentTemplateController : BaseController
    {
        private readonly IDocumentTemplateService service;
        private readonly IDocumentService docService;
        private readonly INomenclatureService nomService;
        private readonly ICdnService cdnService;
        private readonly IWorkTaskService taskService;
        private readonly ICommonService commonService;
        private readonly IPrintDocumentService printService;
        private readonly ICasePersonService casePersonService;

        public DocumentTemplateController(
            IDocumentTemplateService _service,
            IDocumentService _docService,
            INomenclatureService _nomService,
            ICdnService _cdnService,
            IWorkTaskService _taskService,
            ICommonService _commonService,
            IPrintDocumentService _printService,
            ICasePersonService _casePersonService
        )
        {
            service = _service;
            docService = _docService;
            nomService = _nomService;
            cdnService = _cdnService;
            taskService = _taskService;
            commonService = _commonService;
            printService = _printService;
            casePersonService = _casePersonService;
        }

        /// <summary>
        /// Страница с документ към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult Index(int sourceType, long sourceId)
        {
            if (!CheckAccess(service, sourceType, sourceId, AuditConstants.Operations.View))
            {
                return Redirect_Denied();
            }

            ViewBag.sourceType = sourceType;
            ViewBag.sourceId = sourceId;
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentTemplate(sourceType, sourceId);
            SetHelpFileBySourceType(sourceType);

            return View();
        }

        /// <summary>
        /// Извличане на данните за документ към обект
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ListData(IDataTablesRequest request, int sourceType, long sourceId)
        {
            var data = service.DocumentTemplate_Select(sourceType, sourceId);

            return request.GetResponse(data);
        }

        /// <summary>
        /// Добавяне/Редакция на документ към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult AppendUpdateSingle(int sourceType, long sourceId)
        {
            var singleDoc = service.DocumentTemplate_Select(sourceType, sourceId).FirstOrDefault();
            if (singleDoc != null)
            {
                return RedirectToAction(nameof(Edit), new { id = singleDoc.Id });
            }
            else
            {
                return RedirectToAction(nameof(Add), new { sourceType = sourceType, sourceId = sourceId });
            }
        }

        /// <summary>
        /// Добавяне на документ към обект
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IActionResult Add(int sourceType, long sourceId)
        {
            var model = service.DocumentTemplate_Init(sourceType, sourceId);
            if (!CheckAccess(service, sourceType, sourceId, AuditConstants.Operations.Append))
            {
                return Redirect_Denied();
            }

            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на документ към обект
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var model = service.GetById<DocumentTemplate>(id);
            if (!CheckAccess(service, model.SourceType, model.SourceId, AuditConstants.Operations.Update))
            {
                return Redirect_Denied();
            }

            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на документ към обект
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(DocumentTemplate model)
        {
            if (!ModelState.IsValid)
            {
                SetViewBag(model);
                return View(nameof(Edit), model);
            }
            int currentId = model.Id;
            if (service.DocumentTemplate_SaveData(model))
            {
                this.SaveLogOperation(currentId == 0, model.Id, null, "edit");
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }
            SetViewBag(model);
            return View(nameof(Edit), model);
        }

        private void SetViewBag(DocumentTemplate model)
        {
            ViewBag.DocumentKindId_ddl = nomService.GetDDL_DocumentKind(DocumentConstants.DocumentDirection.OutGoing);
            ViewBag.DocumentTemplateStateId_ddl = nomService.GetDropDownList<DocumentTemplateState>();
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentTemplate(model.SourceType, model.SourceId);
            if (model.DocumentId > 0)
            {
                ViewBag.docNumber = docService.Document_GetById(model.DocumentId.Value).Result.RegNumber;
            }

            if ((model.CaseId ?? 0) > 0)
            {
                ViewBag.haveCasePersonIds = nomService.GetHtmlTemplateForCasePerson();
                ViewBag.CasePersonId_ddl = casePersonService.CasePerson_SelectForDropDownList(model.CaseId ?? 0, null, "", "Изберете");
            }

            SetHelpFileBySourceType(model.SourceType);
        }

        private void SetHelpFileBySourceType(int sourceType)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    SetHelpFile(HelpFileValues.SessionAct);
                    break;
                case SourceTypeSelectVM.Case:
                    SetHelpFile(HelpFileValues.CaseDocumentTemplate);
                    break;
                case SourceTypeSelectVM.CaseNotification:
                    SetHelpFile(HelpFileValues.SessionNotification);
                    break;
                case SourceTypeSelectVM.CaseMigration:
                    SetHelpFile(HelpFileValues.CaseMigration);
                    break;
                case SourceTypeSelectVM.DocumentDecision:
                    SetHelpFile(HelpFileValues.DecisionTask);
                    break;
                case SourceTypeSelectVM.ExecList:
                    SetHelpFile(HelpFileValues.Finance);
                    break;
                case SourceTypeSelectVM.ExchangeDoc:
                    SetHelpFile(HelpFileValues.Finance);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Зареждане на данни за запис на файл по документ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Blank(int id, bool del = false)
        {
            var docTemplate = service.GetById<DocumentTemplate>(id);
            BlankEditVM blankModel = null;
            if (docTemplate.HtmlTemplateId > 0)
            {
                blankModel = await InitBlankFromTemplate(id, del);
            }
            else
            {
                //var actModel = service.CaseSessionAct_GetForBlankPrepare(id);
                int sourceType = SourceTypeSelectVM.DocumentTemplate;
                string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
                if (string.IsNullOrEmpty(html))
                {
                    var tmplt = service.GetById<DocumentTemplate>(id);
                    html = tmplt.Description;
                }
                var headerModel = service.DocumentTemplate_InitHeader(id);
                var headerTemplate = "Header";
                var headerHTML = string.Empty;
                if (!string.IsNullOrEmpty(headerModel.HeaderTemplateName))
                {
                    headerTemplate = headerModel.HeaderTemplateName;
                }
                headerHTML = await this.RenderViewAsync(headerTemplate, headerModel);
                var footerHTML = await this.RenderViewAsync("Footer", headerModel);

                blankModel = new BlankEditVM()
                {
                    Title = "Изготвяне на документ към " + SourceTypeSelectVM.GetSourceTypeName(sourceType).ToLower(),
                    SourceType = sourceType,
                    SourceId = id.ToString(),
                    HtmlHeader = headerHTML,
                    HtmlContent = html,
                    HtmlFooter = footerHTML,
                    FooterIsEditable = false,
                    ReturnUrl = Url.Action(nameof(Edit), new { id }),
                    HasPreviewButton = true,
                };
            }
            ViewBag.breadcrumbs = commonService.Breadcrumbs_GetForDocumentTemplate(docTemplate.SourceType, docTemplate.SourceId);
            return View("BlankEdit", blankModel);
        }



        private async Task<BlankEditVM> InitBlankFromTemplate(int id, bool del)
        {
            var docTemplate = service.GetById<DocumentTemplate>(id);
            int sourceType = SourceTypeSelectVM.DocumentTemplate;
            if (del)
            {
                var deleted = await cdnService.MongoCdn_DeleteFiles(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            }
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = id.ToString() });
            if (string.IsNullOrEmpty(html))
            {
                TinyMCEVM htmlModel = printService.FillHtmlTemplateDocumentTemplate(id);
                html = htmlModel.Text;
            }

            var model = new BlankEditVM()
            {
                Title = "Изготвяне на документ към " + SourceTypeSelectVM.GetSourceTypeName(sourceType).ToLower(),
                SourceType = sourceType,
                SourceId = id.ToString(),
                HtmlHeader = string.Empty,
                HtmlContent = html,
                HtmlFooter = string.Empty,
                FooterIsEditable = false,
                ReturnUrl = Url.Action(nameof(Edit), new { id }),
                HasPreviewButton = true,
                HasResetButton = true,
            };
            if (docTemplate.HtmlTemplateId > 0)
            {
                var htmlTemplate = service.GetById<HtmlTemplate>(docTemplate.HtmlTemplateId);
                var htmlModel = printService.ConvertToTinyMCVM(htmlTemplate, false);
                model.TemplateStyle = htmlModel.Style;
            }
            return model;
        }

        /// <summary>
        /// Преглед в pdf
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<IActionResult> blankPreview(BlankEditVM model)
        {
            int id = 0;
            int.TryParse(model.SourceId, out id);
            var docTemplate = service.GetById<DocumentTemplate>(id);

            byte[] pdfBytes = null;
            if (docTemplate.HtmlTemplateId > 0)
            {
                string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = model.SourceType, SourceId = model.SourceId });
                pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true, GetFooterInfoUrl(userContext.CourtId)).GetByte(this.ControllerContext);
            }
            else
            {
                var headerModel = service.DocumentTemplate_InitHeader(id);

                pdfBytes = await GetFileBytes(headerModel, model.SourceType, model.SourceId);
            }
            var contentDispositionHeader = new System.Net.Mime.ContentDisposition
            {
                Inline = true,
                FileName = "documentTemplatePreview.pdf"
            };
            return File(pdfBytes, NomenclatureConstants.ContentTypes.Pdf);
        }

        /// <summary>
        /// Запис на файл
        /// </summary>
        /// <param name="model"></param>
        /// <param name="btnPreview"></param>
        /// <param name="reset_mode"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Blank(BlankEditVM model, string btnPreview = null, string reset_mode = null)
        {
            int id = int.Parse(model.SourceId);
            if (!string.IsNullOrEmpty(reset_mode))
            {
                await cdnService.MongoCdn_DeleteFiles(new CdnFileSelect() { SourceType = model.SourceType, SourceId = model.SourceId });
                SetSuccessMessage("Информацията е обновена с данните от бланката.");
                this.SaveLogOperation(this.ControllerName, nameof(Edit), "Обновяване на данните от бланката", IO.LogOperation.Models.OperationTypes.Patch, id);
                return RedirectToAction(nameof(Blank), new { id = model.SourceId });
            }

            //return new ViewAsPdf("CreatePdf", model);
            var htmlRequest = new CdnUploadRequest()
            {
                SourceType = model.SourceType,
                SourceId = model.SourceId,
                FileName = "draft.html",
                ContentType = "text/html",
                FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.HtmlContent ?? ""))
            };
            if (await cdnService.MongoCdn_AppendUpdate(htmlRequest))
            {
                SetSuccessMessage(MessageConstant.Values.SaveOK);
                this.SaveLogOperation(this.ControllerName, nameof(Edit), "Изготвяне на документ", IO.LogOperation.Models.OperationTypes.Update, id);
                if (!string.IsNullOrEmpty(btnPreview))
                {
                    return await blankPreview(model);
                }
                return RedirectToAction(nameof(Edit), new { id = model.SourceId });
            }
            else
            {
                SetErrorMessage(MessageConstant.Values.SaveFailed);
            }



            return RedirectToAction(nameof(Blank), new { id = model.SourceId });
        }

        private async Task<byte[]> GetFileBytes(DocumentTemplateHeaderVM headerModel, int sourceType, string sourceId)
        {
            string html = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = sourceType, SourceId = sourceId.ToString() });

            var headerTemplate = "Header";
            var headerHTML = string.Empty;
            if (!string.IsNullOrEmpty(headerModel.HeaderTemplateName))
            {
                headerTemplate = headerModel.HeaderTemplateName;
            }
            headerHTML = await this.RenderViewAsync(headerTemplate, headerModel);
            var footerHTML = await this.RenderViewAsync("Footer", headerModel);
            return await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlHeader = headerHTML, HtmlContent = html, HtmlFooter = footerHTML }, true, GetFooterInfoUrl(userContext.CourtId)).GetByte(this.ControllerContext);
        }

        /// <summary>
        /// Запис на файл за документ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GenerateDocumentFile(int id, long documentId)
        {
            if (!service.DocumentTemplate_UpdateDocumentId(id, documentId))
            {
                SetErrorMessage("Проблем при изчитане на създаден документ");
                return RedirectToAction("Edit", "Document", new { id = documentId });
            }
            var docTemplate = service.GetById<DocumentTemplate>(id);
            var headerModel = service.DocumentTemplate_InitHeader(id);

            byte[] fileBytes = null;

            if (docTemplate.HtmlTemplateId > 0)
            {
                string preparedBlank = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { SourceType = SourceTypeSelectVM.DocumentTemplate, SourceId = id.ToString() });
                fileBytes = await PdfBytesFromTemplate(docTemplate.Id, preparedBlank);
                //string html = FillBlankByTemplate(docTemplate.Id, preparedBlank);
                ////string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
                //fileBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }).GetByte(this.ControllerContext);
            }
            else
            {
                int sourceType = SourceTypeSelectVM.DocumentTemplate;

                fileBytes = await GetFileBytes(headerModel, sourceType, id.ToString());
            }

            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.DocumentPdf,
                SourceId = documentId.ToString(),
                FileName = "documentTemplate.pdf",
                ContentType = "application/pdf",
                Title = $"{headerModel.DocumentTypeLabel} {headerModel.DocumentNumber}/{headerModel.DocumentDate:dd.MM.yyyy}",
                FileContentBase64 = Convert.ToBase64String(fileBytes)
            };

            await cdnService.MongoCdn_AppendUpdate(pdfRequest);

            var newTask = new WorkTaskEditVM()
            {
                SourceType = SourceTypeSelectVM.Document,
                SourceId = documentId,
                TaskTypeId = WorkTaskConstants.Types.Document_Sign,
                UserId = headerModel.AuthorId,
                TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser
            };
            taskService.CreateTask(newTask);

            if (docTemplate.SourceType == SourceTypeSelectVM.ExchangeDoc)
            {
                await SaveFileExchangeDoc((int)docTemplate.SourceId);
            }
            SetSuccessMessage("Регистрирането на създадения документ премина успешно.");
            return RedirectToAction("Edit", "Document", new { id = documentId });
        }

        /// <summary>
        /// Зареждане на бланка по документ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="preparedBlank"></param>
        /// <returns></returns>
        private async Task<string> FillBlankByTemplate(int id, string preparedBlank = null)
        {
            string result = "";

            TinyMCEVM htmlModel = printService.FillHtmlTemplateDocumentTemplate(id, preparedBlank);
            if (htmlModel == null)
            {
                return result;
            }
            return await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
        }

        private async Task<byte[]> PdfBytesFromTemplate(int id, string preparedBlank = null)
        {
            TinyMCEVM htmlModel = printService.FillHtmlTemplateDocumentTemplate(id, preparedBlank);
            if (htmlModel == null)
            {
                return null;
            }

            return await new ViewAsPdfByteWriter("~/Views/Shared/PreviewRaw.cshtml", htmlModel, true, GetFooterInfoUrl(userContext.CourtId))
            {
                PageOrientation = (Orientation)htmlModel.PageOrientation,
                PageMargins = new Margins(20, 20, 10, 30),
                //PageMargins = new Margins(10, 5, 10, 5),
                PageSize = Size.A4,
                CustomSwitches = htmlModel.SmartShrinkingPDF ? "" : "--disable-smart-shrinking"
            }.GetByte(this.ControllerContext);
        }

        public IActionResult DepersonalizeDocument()
        {
            var model = new DepersonalizationModel()
            {
                CaseId = 1,
                DocumentName = "РЕШЕНИЕ № 12489",
                DocumentContent = @"<p><strong>РЕШЕНИЕ</strong><br><br><strong>№ 12489</strong><br><strong>София, 09/30/2013</strong><br><br><strong>В ИМЕТО НА НАРОДА</strong></p><p><br><strong>Върховният административен съд на Република България - Четвърто отделение, </strong>в съдебно заседание на шестнадесети септември две хиляди и тринадесета година в състав:</p><table class=""table""><tbody><tr><td width=""394""><p><strong>ПРЕДСЕДАТЕЛ:</strong></p></td><td width=""272""><p>ГАЛИНА МАТЕЙСКА</p></td></tr><tr><td width=""394""><p><strong>ЧЛЕНОВЕ:</strong></p></td><td width=""272""><p>ТОДОР ПЕТКОВ<br>СВЕТОСЛАВ СЛАВОВ</p></td></tr></tbody></table><p><br></p><table class=""table""><tbody><tr><td width=""161""><p>при секретар</p></td><td width=""245""><p>Татяна Щерева</p></td><td width=""184""><p>и с участието</p></td></tr><tr><td width=""161""><p>на прокурора</p></td><td width=""245""><p>Чавдар Симеонов</p></td><td width=""184""><p>изслуша докладваното</p></td></tr><tr><td width=""161""><p>от съдията</p></td><td width=""245""><p>СВЕТОСЛАВ СЛАВОВ</p></td><td width=""184""><br></td></tr><tr><td colspan=""3"" width=""590""><p>по адм. дело № 11025/2013<strong>. </strong></p></td></tr></tbody></table><p><br><br>Производство по чл. 267, ал. 8 от Изборния кодекс, във връзка с чл. 208 - чл. 228 от АПК.<br>Образувано е по касационна жалба от Христо Стоянов Бозов, против решение № 2143 от 30.07.2013 г. по адм. д. № 2632/2013 г. на Административен съд - Варна, с което е потвърдено решение № 577 от 08.07.2013 г. на Общинска избирателна комисия - Варна, с което е обявен за избран за кмет на Община - Варна, след проведения втори тур на частични избори за кмет на Община Варна – Иван Николаев Портних. Излагат се доводи, че решението е неправилно, поради нарушение на материалния закон, необосновано и постановено при съществено нарушение на съдопроизводствените правила. Твърди се, че съдът не е изяснил фактическата обстановка, както и не са били приложени всички необходими за изясняване на спора доказателства. Твърди, че не е допуснато откриване на производство по оспорване на протоколи на СИК и ОИК по реда на чл. 193 от ГПК. Също така твърди, че не са допуснати до разпит свидетели, както и допускане на експертиза за извършване на проверки на вписаните обстоятелства в протоколите на СИК и ОИК. Излага, че допуснатите процесуални нарушения обуславят отмяна на решението. Моли, да бъде отменено обжалваното решение и се върне делото за ново разглеждане от друг състав. <br>Ответникът - Общинска избирателна комисия – Варна, чрез председателя Велин Жеков, оспорва жалбата и излага доводи за правилност на обжалваното решение, с искане да бъде оставено в сила.<br>Заинтересованата страна – Иван Николаев Портних, редовно призован не се явява.Представя писмено становище с което оспорва жалбата. В него се съдържа становище за неоснователността й, като са изложени твърдения, опровергаващи тези, направени в касационната жалба. Моли се, съдът да отхвърли касационната жалба. <br>Представителят на Върховна административна прокуратура дава мотивирано становище, че жалбата е неоснователна. Смята, че всички доказателства и доводите развити в първоинстанционното производство по делото са обсъдени поотделно и в тяхната съвкупност при постановяване на решението, както и направеният извод, че при произвеждане на избор за кмет на Община Варна не са допуснати съществени нарушения, които да водят до изборен резултат, различен от обявения с обжалваното решение. Излага, че в хода на съдебното производство не са установени нарушения на изборния кодекс, които като краен резултат да доведат до опорочаване волята на избирателя и до промяна на резултата от избора. Счита, че оспорваното съдебно решение не страда от пороците на чл. 209, т. 3 АПК и следва да се остави в сила на основание чл. 221, ал. 2, пр. 1 от АПК.<br>Касационната жалба е подадена от надлежна страна в срока по чл. 267, ал. 8 от ИК и е процесуално допустима.<br>Разгледана по същество касационната жалба е НЕОСНОВАТЕЛНА.<br>Административният съд - Варна е бил сезиран с жалба подадена от Христо Стоянов Бозов, в качеството му на кандидат за кмет на община Варна от Инициативен комитет, като независим кандидат за участие в частични избори за кмет на община гр. Варна, против решение № 577 от 08.07.2013 год. на ОИК – Варна. С решение № 2134 от 30.07.2013г. по адм. д. № 2632/2013 г. на Административен съд - Варна, е потвърдено решение № 577 от 08.07.2013 г. на Общинска избирателна комисия - Варна, с което е обявен за избран за кмет на Община - Варна, на втори тур – Иван Николаев Портних с получени 38642 гласа. В протокола на ОИК – Варна, е отразен общия брой на избирателите 288464 души , като от тях са гласували 76539 избирателя, от които 1370 гласа са определени като недействителни, а 75169 за действителни. За касационния жалбоподател Христо Стоянов Бозов е отразено, че е получил 36527 действителни гласа. <br>След извършената проверка относно валидността и законосъобразността на оспореното решение на ОИК съдът е изложил мотиви по всички възражения, направени с жалбата. На първо място е приел за неоснователно твърдението относно неправилното отразяване на изборните резултати в съставените протоколи от избирателни секции, тъй като всички са били подписани от всички техни членове на секционната избирателна комисия и не е съществувал спор между членовете на СИК за резултатите от гласуването. При извършената проверка от първоинстанционния съд е било установено, че след като протоколите на СИК са подписани от всички членове без особени мнения, няма възражения и жалби, не са допуснати нарушения, които да опорочават изборния резултат. Съдът е изложил съображенията си относно извършените поправки в протоколите на секционните комисии, които се отнасят до отразяване на описването на неизползвани бюлетини, унищожаване на бюлетина за образец за таблото, бюлетини не по установения образец, като е констатирал, че нито една от извършените поправки на протоколи не е извършена след обявяване на резултатите от гласуването. Като краен резултат съдът е приел, че при провеждането на избора не са допуснати нарушения, които да водят до изборен резултат, различен от обявения с обжалваното решение.<br>Касационният жалбоподател е изложил доводи за наличие на отменителни основания по чл. 209, т. 3 от АПК.<br>Настоящият съдебен състав на Върховния административен съд, четвърто отделение приема доводите на касационният жалбоподател за неоснователни, като в съответствие с чл. 168, ал. 1 от АПК, първоинстанционният съд е преценил всичките доказателства по делото и е основал решението си върху приетите от него за установени обстоятелства и върху приложимия материален закон, при спазване на съдопроизводствените правила. В жалбата, предмет на настоящото касационно производство са развити идентични оплаквания, каквито са изложени и пред първоинстанционния съд. <br>По направените доказателствени искания съдът в съотвествие със спецификата на административноправния спор е обосновал отказа да бъде открито производство по чл. 193 от ГПК по отношение на протоколите на СИК и ОИК, за които няма отразено оспорване на резултатите в съотвествие с чл.226, ал.4 от ИК. Административния съд е спазил изискванията на чл.171 от АПК и е допуснал и събрал относимите към административноправния спор доказателства.<br>Съдът, след обсъждане на приетите писмени доказателства е приел за установено, че изборните резултати от избора за кмет на община Варна на 07.07.2013 г. са отразени в 385 протокола на СИК, същите са обобщени в протокола на ОИК, въз основа на което е обявен за избран за кмет на Община Варна – Иван Николаев Портних. Протоколът и решението на ОИК са подписани от председател, зам. Председател, секретар и всички членове на Общинската избирателна комисия – Варна, като са положени 32 подписа, без отразени възражения и особени мнения. Протоколите на СИК също са подписани от всички членове, като решенията са взети при необходимия кворум и мнозинство.<br>Оспорването на протоколите на СИК от четиринадесетте секции с № 03-06-02-122, 03-06-02-147 , 03-06-02 - 169, 03-06-02-090, 03-06-03 -198, 03-06-01-062, 03-06-04-296, 03-06-03 – 248, 03-06-03-193, 03-06-02-153, 03-06-02-164, 03-06-03-275, 03-06-05-345, 03-06-02-110, с твърдения за извършени поправки, съдът е приел за недоказано. Приел, че протоколите са надлежно попълнени в одобрения от ЦИК образец, в същите не се съдържат данни за възникнали спорове относно действителността на отчетените като недействителни бюлетини, съответно липсват решения по чл. 226, ал. 4 от ИК, отразено е, че изборите са протекли при нормална и спокойна обстановка. Посоченото е относимо към всички протоколи на СИК, а не само по отношение оспорените, поради което съдът е приел, че не са установени твърдените в жалбата нарушения, както и каквито и да е други, които биха довели до опорочаване на волята на избирателите, съответно до промяна на крайния резултат. Съдът подробно е и описал техническите грешки, като правилно е приел, че са били отстранени при спазване на разпоредбата на чл. 212, ал.2 от ИК. Съдът правилно е достигнал до извода, че техническите грешки не се дължат на такива в пресмятането, поради което оспореното решение на ОИК, което е издадено от компетентен орган, в изискуемата форма и при спазване на административнопроцесуалните правила и материалния закон е прието за законосъобразно и го е потвърдил.<br>Така постановеното решение е правилно и следва да бъде оставено в сила.<br>Неоснователно е твърдението в касационната жалба, че оставяйки без уважение искането за назначаването на експертиза за която процесуалния представител на жалбоподателя не е направил уточнение, както по нейния вид и от какви специални знания следва да се ползват съда и страните за изясняване на спорните обстоятелства. В настоящия случай, първоинстанционния съд правилно е приел, че след като липсват каквито и да спорове относно отчитането на бюлетините като недействителни от страна на членовете на СИК, които са представители на различни политически партии и са извършили броенете в присъствието на застъпници, представители на партии, коалиции и наблюдатели няма основание за назначаване на вещи лица за установяване на съответствие на фактическото положение с удостовереното в оспорените протоколи фактическо положение.<br>В случая оспорването не е свързано с факти относно вещите - протоколи като материален носител (хартия), свързани с тяхното състояние, нарушаване или други, а е свързано с протоколите като документи (писмени доказателства), в които е материализирано изявление. Настоящият състав на Върховния административен съд, в конкретния случай, намира, че не е налице и твърдяното процесуално нарушение, изразяващо се в неизпълнение на задължението на съда по чл. 171, ал. 2 от АПК. Съдът е указал на страните доказателствената тежест, като е съобразил както нормите на чл. 170 и 171 от АПК, така и тази на чл. 193, ал. 3 от ГПК. Първоинстанционния съд правилно е преценил, че в случая липсва и необходимост от назначаване на вещо лице, тъй като преценката за действителността на оспорваните документи е правна и съдът не се нуждае от специалните знания на експерт, за да я извърши. С посоченото се изчерпват възможностите на съда за служебно събиране на доказателства. Следва да се има предвид и обстоятелството, че дадената на съда, с нормата на чл. 171, ал. 2 от АПК възможност за събиране на доказателства е приложима по негова преценка. Нормата е диспозитивна, като е посочено, че съдът ""може"" да назначава вещи лица, оглед и освидетелстване и служебно. <br>Правилно съдът е приел, че липсват сочените от настоящия касатор нарушения при провеждане на изборите. С нормите на ИК са въведени изисквания, спазването на които дава гаранция, че изборите са проведени без нарушения. На първо място това е начина на сформиране на СИК, именно чието правомощие, съгласно чл. 36, ал. 1, т. 4 и чл. 39, ал. 1, т. 4 от ИК е да извършат преброяване на гласовете. Членовете на СИК се назначават след консултации на парламентарно представените партии и коалиции от партии, като има забрана представителите на една партия или коалиция от партии да имат мнозинство в една и съща избирателна комисия, както и председателят и секретарят не могат да бъдат от една и съща партия или коалиция от партии, т. е. осигурено е участие на представителя на различни партии, които осъществяват взаимен контрол. Освен това контрол е осигурен и чрез възможността при отваряне на избирателните урни и при установяване на резултатите от гласуването в изборното помещение да присъстват кандидати, един от застъпниците, по един представител на партия, коалиция от партии и инициативен комитет, наблюдатели и журналисти, като им се осигурява пряка видимост при преброяване на гласовете. Само един член по решение на комисията има достъп до бюлетините, под наблюдението и контрола на останалите членове. В протоколите на СИК задължително се отбелязват, ако се констатират обстоятелствата по чл. 180, 181, чл. 185 и 201 от ИК. Съгласно чл. 226, ал. 4 от ИК при оспорване действителността на някой глас, случаят се описва в протокол, който се прилага към протокола на СИК, а на гърба на бюлетината се отбелязва номерът на решението на СИК относно действителността. В настоящия случай липсват каквито и да подобни отбелязвания, липсват протоколи и решения по чл. 226, ал. 4 от ИК, липсват особени мнения, забележки или възражения, поради което правилно съдът е приел липса на нарушения.<br>В касационната жалба отново се повдига възражението за нарушение при съставяне на избирателните списъци. В настоящия случай, правилно съдът е приел, че по отношение избирателните списъци, които несъмнено са изключително важни за да бъде осъществено правото на гласуване, законодателят е предвидил възможност за поправки и промени, в това число и поради промени в статуса на избирателите, като са разписани процедури за заличаване, вписване и дописване на имена, както и за отстраняване на непълноти и грешки. Разписан е и контрол, върху актовете на органите по чл. 46, ал. 1, съответно чл. 40, ал. 1 от ИК, като в нормата на чл. 48 от ИК е посочен редът и сроковете затова, когато се касае до заличаване, вписване или дописване, а по отношение отстраняването на непълноти и грешки, редът е посочен в чл. 50 от ИК. В случая липсват данни, за каквито и да са спорове по отношение неправомерно съдържание на списъците за гласуване. Не е имало спорове по съдържанието на списъка по чл. 187, ал. 1, т. 7 от ИК. Правилно е отклонено и искането за назначаване на съдебна експертиза за установяване на вписването в избирателните списъци, тъй като подобна преценка е изцяло правна и не може да бъде предмет на заключение на вещо лице. Освен това, както вече се посочи, същата преценка и контрол може да бъде направена в производството по чл. 48 и 50 от ИК, но не и в настоящото производство. В тази връзка правилно е отхвърлено и искането за допускане на гласни доказателства, тъй като данните в избирателните списъци е недопустимо да бъдат оборвани с такива.<br>На последно място е отхвърлено възражението за допуснато съществено нарушение, изразяващо се в проведена незаконосъобразна агитация, като са били използвани материали уронващи честта и името на касационния жалбоподател, излизаща извън рамките на добрите нрави и правила, както и водената предизборна кампания с навеждане на внушения целящи да въведат заблуждение у избирателите.За да достигне до този извод, първоинстанционния съд е приел, че от депозираната към преписката жалба не може да се направи извод за предизборна агитация по чл. 133, ал. 6 от ИК в полза на избрания кандидат и, че именно тя е повлияла върху изборния резултат. Изложил е още съображения, че една част от жалбите в съответствие с правомощията на ОИК регламентирани с разпоредбата на чл. 33, ал. 1, т. 17 от ИК са били разгледани от ОИК и са приети за неоснователни, а друга част от сигналите са изпратени по компетентност на Районна прокуратура - Варна. <br>Постановеното решение не страда от сочените в касационната жалба пороци. Противно на твърдяното, действията на съда не са ограничили правото на ефективна защита на жалбоподателя. Видно от протоколите на проведените съдебни заседания, неоспорени по надлежния ред, съдът е спазил принципа за равнопоставеност на страните и им е съдействал в еднаква степен за изясняване на спорните обстоятелства, като съобразил, че доказателствата следва да бъдат допустими и относими към предмета на спора.<br>Ето защо и на основание чл. 221, ал. 2, предл. първо от АПК, Върховният административен съд, четвърто отделение,</p><p><strong>РЕШИ:</strong></p><p><br><strong>ОСТАВЯ В СИЛА</strong> решение № 2134 от 30.07.2013 г., постановено по адм. дело № 2632/2013 г. по описа на Административен съд - Варна.<br><br>Решението е окончателно. <br><br></p><table class=""table""><tbody><tr><td width=""208""><p><strong>Вярно с оригинала, </strong></p></td><td width=""207""><p><strong>ПРЕДСЕДАТЕЛ:</strong></p></td><td width=""236""><p>/п/ Галина Матейска</p></td></tr><tr><td width=""208""><p><strong>секретар: </strong></p></td><td width=""207""><p><strong>ЧЛЕНОВЕ:</strong></p></td><td width=""236""><p>/п/ Тодор Петков<br>/п/ Светослав Славов</p></td></tr></tbody></table><p>С.С.</p>",
                CancelUrl = "/"
            };

            return View(model);
        }

        /// <summary>
        /// Запис на файл за бланка
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> PrintBlankByTemplate(int id)
        {
            TinyMCEVM htmlModel = printService.FillHtmlTemplateDocumentTemplate(id);
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            return File(pdfBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "Template" + id.ToString() + ".pdf");
        }

        /// <summary>
        /// Запис на файл за Протокол на изпращане на ИЛ в НАП
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SaveFileExchangeDoc(int id)
        {
            TinyMCEVM htmlModel = printService.FillHtmlTemplateExchangeDoc(id);
            string html = await this.RenderPartialViewAsync("~/Views/Shared/", "PreviewRaw.cshtml", htmlModel, true);
            var pdfBytes = await new ViewAsPdfByteWriter("CreatePdf", new BlankEditVM() { HtmlContent = html }, true).GetByte(this.ControllerContext);
            var pdfRequest = new CdnUploadRequest()
            {
                SourceType = SourceTypeSelectVM.ExchangeDoc,
                SourceId = id.ToString(),
                FileName = "exchangeDoc.pdf",
                ContentType = "application/pdf",
                Title = "Протокол на изпращане на ИЛ в НАП",
                FileContentBase64 = Convert.ToBase64String(pdfBytes)
            };
            bool result = await cdnService.MongoCdn_AppendUpdate(pdfRequest);
            Response.Headers.Clear();

            return result;
        }
        public string GetFooterInfoText(int courtId)
        {
            var court = service.GetById<Infrastructure.Data.Models.Common.Court>(courtId);

            if (court != null)
            {
                return $"{court.Address}, {court.CityName}";
            }
            return "";
        }
    }
}