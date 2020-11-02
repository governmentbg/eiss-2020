// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Constants;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class HtmlTemplateService : BaseService, IHtmlTemplate
    {
        public HtmlTemplateService(
            ILogger<HtmlTemplateService> _logger,
            IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
        }

        public class BlankParam
        {
            public BlankParam(int intValue, string strValue)
            {
                BlankId = intValue;
                ParamName = strValue;
                ParamId = 0;
            }

            public int BlankId { get; set; }
            public string ParamName { get; set; }
            public int ParamId { get; set; }
        }

        /// <summary>
        /// Извличане на списък
        /// </summary>
        /// <returns></returns>
        public IQueryable<HtmlTemplateVM> HtmlTemplate_Select(HtmlTemplateFilterVM filterData)
        {
            return repo.AllReadonly<HtmlTemplate>()
                       .Include(x => x.HtmlTemplateType)
                       .Where(x => x.HtmlTemplateTypeId == filterData.HtmlTemplateTypeId || filterData.HtmlTemplateTypeId <=  0)
                       .Select(x => new HtmlTemplateVM()
                       {
                           Id = x.Id,
                           HtmlTemplateTypeLabel = (x.HtmlTemplateType != null) ? x.HtmlTemplateType.Label : string.Empty,
                           Alias = x.Alias,
                           Label = x.Label,
                           Description = x.Description,
                           Content = x.Content,
                           FileName = x.FileName,
                           ContentType = x.ContentType,
                           IsCreate = x.IsCreate
                       })
                       .AsQueryable();
        }

        public IQueryable<HtmlTemplateLinkVM> HtmlTemplateLink_Select(int HtmlTemplateId)
        {
            return repo.AllReadonly<HtmlTemplateLink>()
                .Include(x => x.CourtType)
                .Include(x => x.CaseGroup)
                .Where(x => x.HtmlTemplateId == HtmlTemplateId)
                .Select(x => new HtmlTemplateLinkVM()
                {
                    Id = x.Id,
                    HtmlTemplateId = x.HtmlTemplateId,
                    CourtTypeLabel = (x.CourtType != null) ? x.CourtType.Label : "Всички",
                    CaseGroupLabel = (x.CaseGroup != null) ? x.CaseGroup.Label : "Всички",
                    IsActiveLabel = (x.IsActive ?? false) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                    SourceType = x.SourceType ?? 0
                })
                .AsQueryable();
        }

        public IQueryable<HtmlTemplateParamLinkVM> HtmlTemplateParam_Select(int HtmlTemplateId)
        {
            return repo.AllReadonly<HtmlTemplateParamLink>()
                       .Include(x => x.HtmlTemplate)
                       .Include(x => x.HtmlTemplateParam)
                       .Where(x => x.HtmlTemplateId == HtmlTemplateId)
                       .Select(x => new HtmlTemplateParamLinkVM()
                       {
                           Id = x.Id,
                           HtmlTemplateName = (x.HtmlTemplate != null) ? x.HtmlTemplate.Label : string.Empty,
                           HtmlTemplateFile = (x.HtmlTemplate != null) ? x.HtmlTemplate.FileName : string.Empty,
                           HtmlTemplateParamLabel = (x.HtmlTemplateParam != null) ? x.HtmlTemplateParam.Label : string.Empty,
                           HtmlTemplateParamDescr = (x.HtmlTemplateParam != null) ? x.HtmlTemplateParam.Description : string.Empty
                       })
                       .AsQueryable();
        }

        public IQueryable<HtmlTemplateParamVM> HtmlTemplateParamAll_Select()
        {
            return repo.AllReadonly<HtmlTemplateParam>()
                       .Where(x => x.IsActive)
                       .Select(x => new HtmlTemplateParamVM()
                       {
                           Id = x.Id,
                           Code = x.Code,
                           Label = x.Label,
                           Description = x.Description
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Попълване на данните свързани с файла който се качва
        /// </summary>
        /// <param name="html"></param>
        /// <param name="files"></param>
        private void FillDataFile(HtmlTemplate html, ICollection<IFormFile> files)
        {
            if (files != null && files.Count() > 0)
            {
                var file = files.First();
                using (var memory = new MemoryStream())
                {
                    file.CopyTo(memory);
                    html.Content = memory.ToArray();
                    html.ContentType = file.ContentType;
                    html.FileName = Path.GetFileName(file.FileName);
                    html.DateUploaded = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Добавяне/редакция на данните
        /// </summary>
        /// <param name="files"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool HtmlTemplate_SaveData(ICollection<IFormFile> files, HtmlTemplate model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    HtmlTemplate saved = repo.GetById<HtmlTemplate>(model.Id);
                    saved.HtmlTemplateTypeId = model.HtmlTemplateTypeId;
                    saved.Alias = model.Alias;
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.SmartShrinkingPDF = model.SmartShrinkingPDF;
                    saved.StyleTemplateId = model.StyleTemplateId;
                    saved.HaveSessionAct = model.HaveSessionAct;
                    saved.HaveSessionActComplain = model.HaveSessionActComplain;
                    saved.HaveExpertReport = model.HaveExpertReport;
                    saved.XlsTitleRow = model.XlsTitleRow;
                    saved.XlsDataRow = model.XlsDataRow;
                    saved.XlsRecapRow = model.XlsRecapRow;

                    FillDataFile(saved, files);
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    FillDataFile(model, files);
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }

        public bool HtmlTemplateLink_SaveData(HtmlTemplateLink model)
        {
            try
            {
                model.CourtTypeId = model.CourtTypeId.NumberEmptyToNull();
                model.CaseGroupId = model.CaseGroupId.NumberEmptyToNull();
                model.SourceType =  model.SourceType.NumberEmptyToNull();
                if (model.Id > 0)
                {
                    //Update
                    HtmlTemplateLink saved = repo.GetById<HtmlTemplateLink>(model.Id);
                    saved.CourtTypeId = model.CourtTypeId;
                    saved.CaseGroupId = model.CaseGroupId;
                    saved.IsActive = model.IsActive;
                    saved.SourceType = model.SourceType;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    repo.Add(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }

        /// <summary>
        /// Взема се името на файла без разширение
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetAliasFromFileName(string fileName)
        {
            int indexPoint = fileName.IndexOf(".");
            string alias = fileName.Substring(0, indexPoint);
            return alias;
        }

        private void FillHtmlLink(IRow row, HtmlTemplate html, List<CourtType> inTypes, List<CaseGroup> inGroups)
        {
            List<string> _types = row.GetCell(4).ToString().Replace(" ", string.Empty).Split(',').ToList();
            List<string> _groupes = row.GetCell(5).ToString().Replace(" ", string.Empty).Split(',').ToList();

            List<CourtType> typeExists = (row.GetCell(4).ToString() == "всички") ? inTypes : inTypes.Where(x => _types.Any(p => p == x.Code)).ToList();
            List<CaseGroup> groupExists = (row.GetCell(5).ToString() == "БЕЗ") ? new List<CaseGroup>() : inGroups.Where(x => _groupes.Any(p => p == x.Code)).ToList();

            foreach (var courtType in typeExists)
            {
                HtmlTemplateLink templateLink = new HtmlTemplateLink();
                templateLink.CourtTypeId = courtType.Id;
                templateLink.IsActive = true;

                foreach (var caseGroup in groupExists)
                {
                    templateLink.CaseGroupId = caseGroup.Id;
                }

                html.HtmlTemplateLinks.Add(templateLink);
            }
        }

        /// <summary>
        /// Попълване на данните за автоматичние импорт
        /// </summary>
        /// <param name="row"></param>
        /// <param name="file"></param>
        /// <param name="htmlTemplateTypes"></param>
        /// <returns></returns>
        private HtmlTemplate FillHtmlTemplate(IRow row, IFormFile file, IQueryable<HtmlTemplateType> htmlTemplateTypes)
        {
            var court_types = repo.AllReadonly<CourtType>().ToList();
            var case_groupes = repo.AllReadonly<CaseGroup>().ToList();
            var htt = htmlTemplateTypes.Where(x => x.Label.ToUpper() == row.GetCell(3).ToString().ToUpper()).DefaultIfEmpty(null).FirstOrDefault();
            HtmlTemplate html = new HtmlTemplate
            {
                HtmlTemplateTypeId = (htt != null) ? htt.Id : 0,
                Alias = GetAliasFromFileName(row.GetCell(0).ToString().ToUpper()),
                Label = row.GetCell(1).ToString(),
                Description = row.GetCell(6).ToString()
            };

            html.HtmlTemplateLinks = new HashSet<HtmlTemplateLink>();
            FillHtmlLink(row, html, court_types, case_groupes);

            using (var memory = new MemoryStream())
            {
                file.CopyTo(memory);
                html.Content = memory.ToArray();
                html.ContentType = file.ContentType;
                html.FileName = Path.GetFileName(file.FileName);
                html.DateUploaded = DateTime.Now;
            }

            return html;
        }

        /// <summary>
        /// Попълване на списъка за импорт и изчитане на данните от екселският файл
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private IList<HtmlTemplate> FillHtmlTemplatesImport(ICollection<IFormFile> files)
        {
            IList<HtmlTemplate> saveImports = new List<HtmlTemplate>();

            string noFile = string.Empty;

            var memory = new MemoryStream();
            var xlsFile = files.Where(x => x.FileName == "Test.xls").DefaultIfEmpty(null).FirstOrDefault();
            xlsFile.CopyTo(memory);

            HSSFWorkbook hssfwb = new HSSFWorkbook(memory);
            ISheet sheet = hssfwb.GetSheetAt(0);

            var _rowCount = sheet.LastRowNum;
            IQueryable<HtmlTemplateType> HtmlTemplateTypes = repo.AllReadonly<HtmlTemplateType>().Select(x => x);

            for (int i = 0; i <= _rowCount; i++)
            {
                var _row = sheet.GetRow(i);
                var formFile = files.Where(x => x.FileName.ToUpper() == _row.GetCell(0).ToString().ToUpper()).DefaultIfEmpty(null).FirstOrDefault();

                if (formFile != null)
                    saveImports.Add(FillHtmlTemplate(_row, formFile, HtmlTemplateTypes));
                else
                    noFile += _row.GetCell(0).ToString() + ", ";
            }

            foreach (var save in saveImports.Where(x => x.HtmlTemplateTypeId == 0))
                noFile += "Има с нули: " + save.FileName + ", ";

            return saveImports;
        }

        /// <summary>
        /// Запис на данните за автоматичният импорт
        /// </summary>
        /// <param name="htmlTemplates"></param>
        /// <returns></returns>
        private bool SaveHtmlTemplatesImport(IList<HtmlTemplate> htmlTemplates)
        {
            try
            {
                foreach (var html in htmlTemplates)
                {
                    repo.Add(html);
                    repo.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }

        /// <summary>
        /// Попълване на данните и запис за автоматичният импорт
        /// </summary>
        /// <param name="files"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool HtmlTemplate_ImportData(ICollection<IFormFile> files, HtmlTemplate model)
        {
            IList<HtmlTemplate> saveImports = FillHtmlTemplatesImport(files);
            return SaveHtmlTemplatesImport(saveImports);
        }

        /// <summary>
        /// Замества в един стринг стринг с друг
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Find"></param>
        /// <param name="Replace"></param>
        /// <returns></returns>
        private string ReplaceFirstOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        private string ReplaceString(string Source, string Find, string Replace)
        {
            return Source.Replace(Find, Replace);
        }

        private void KeyValuePairVMs(IList<KeyValuePairVM> keyValuePairVMs)
        {
            KeyValuePairVM keyValuePairVM = new KeyValuePairVM()
            {
                Key = "F_COURT",
                Value = "Враца",
                Label = ""
            };

            keyValuePairVMs.Add(keyValuePairVM);

            KeyValuePairVM keyValuePairVM1 = new KeyValuePairVM()
            {
                Key = "F_STAMP",
                Value = "Тест за попълване",
                Label = ""
            };

            keyValuePairVMs.Add(keyValuePairVM1);
        }

        /// <summary>
        /// Намира бланка и замества параметрите със стойност
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public string HtmlTemplate_FillData(string alias, IList<KeyValuePairVM> keyValuePairs)
        {
            string htmlString = string.Empty;

            var html = repo.AllReadonly<HtmlTemplate>().Where(x => x.Alias.ToUpper() == alias.ToUpper()).DefaultIfEmpty(null).FirstOrDefault();

            if (html == null)
                return htmlString;

            htmlString = GetStringFromByte(html.Content);

            foreach (var keyValue in keyValuePairs)
                htmlString = ReplaceString(htmlString, "{" + keyValue.Key + "}", keyValue.Value);

            return htmlString;
        }

        private string GetStringFromByte(byte[] vs)
        {
            string _result = string.Empty;
            Stream stream = new MemoryStream(vs);

            using (StreamReader sr = new StreamReader(stream))
                _result = sr.ReadToEnd();

            return _result;
        }

        private List<int> AllIndexesOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        private bool SaveParam(List<string> _params)
        {
            try
            {
                foreach (var param in _params)
                {
                    HtmlTemplateParam templateParam = new HtmlTemplateParam()
                    {
                        Label = param,
                        Code = param
                    };

                    repo.Add(templateParam);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }

        private bool SaveBlankParam(List<BlankParam> blankParams)
        {
            try
            {
                foreach (var param in blankParams)
                {
                    HtmlTemplateParamLink templateParam = new HtmlTemplateParamLink()
                    {
                        HtmlTemplateId = param.BlankId,
                        HtmlTemplateParamId = param.ParamId
                    };

                    repo.Add(templateParam);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }

        private void FillIdParam(List<BlankParam> blankParams)
        {
            var paramsList = repo.AllReadonly<HtmlTemplateParam>();

            foreach (var param in blankParams)
            {
                var templateParams = paramsList.Where(x => x.Code == param.ParamName).FirstOrDefault();
                param.ParamId = templateParams.Id;
            }
        }

        public bool HtmlTemplate_ImportParam()
        {
            var htmls = repo.AllReadonly<HtmlTemplate>();
            List<string> _params = new List<string>();
            List<BlankParam> blankParams = new List<BlankParam>();

            foreach (var htmlTemplate in htmls)
            {
                string _html = GetStringFromByte(htmlTemplate.Content);
                List<int> _starts = AllIndexesOf(_html, "{");
                List<int> _ends = AllIndexesOf(_html, "}");

                for (int i = 0; i < _starts.Count; i++)
                {
                    string param = _html.Substring(_starts[i] + 1, (_ends[i] - _starts[i]) - 1);
                    if ((!_params.Any(x => x == param)) && (param == param.ToUpper()))
                        _params.Add(param);

                    if (param == param.ToUpper())
                    {
                        if (!blankParams.Any(x => x.BlankId == htmlTemplate.Id && x.ParamName == param))
                            blankParams.Add(new BlankParam(htmlTemplate.Id, param));
                    }
                }
            }

            //if (SaveParam(_params))
            {
                FillIdParam(blankParams);
                SaveBlankParam(blankParams);
            }

            return true;
        }

        public string HtmlTemplate_GetNotSetParam(string alias)
        {
            string result = string.Empty;

            var html = repo.AllReadonly<HtmlTemplate>().Where(x => x.Alias.ToUpper() == alias.ToUpper()).DefaultIfEmpty(null).FirstOrDefault();

            var htmlTemplateParams = repo.AllReadonly<HtmlTemplateParamLink>()
                .Include(x => x.HtmlTemplate)
                .Include(x => x.HtmlTemplateParam)
                .Where(x => x.HtmlTemplateId == html.Id)
                .Select(x => new HtmlTemplateParamLinkVM()
                {
                    Id = x.Id,
                    HtmlTemplateName = (x.HtmlTemplate != null) ? x.HtmlTemplate.Label : string.Empty,
                    HtmlTemplateFile = (x.HtmlTemplate != null) ? x.HtmlTemplate.FileName : string.Empty,
                    HtmlTemplateParamLabel = (x.HtmlTemplateParam != null) ? x.HtmlTemplateParam.Label : string.Empty,
                    HtmlTemplateParamDescr = (x.HtmlTemplateParam != null) ? x.HtmlTemplateParam.Description : string.Empty
                })
                .ToList();

            foreach (var templateParamLinkVM in htmlTemplateParams.Where(x => ((x.HtmlTemplateParamDescr ?? string.Empty) == string.Empty)))
            {
                var part1 = @"keyValuePairs.Add(new KeyValuePairVM() { Key = """", Label = """", Value = """" }); ";
                result += ReplaceString(part1, @"Key = """"", @"Key = ""{" + templateParamLinkVM.HtmlTemplateParamLabel + @"}""");
            }

            return result;
        }

        public HtmlTemplateCreateVM GetById_HtmlTemplateCreate(int id)
        {
            var htmlTemplate = repo.GetById<HtmlTemplate>(id);
            return new HtmlTemplateCreateVM()
            {
                Id = htmlTemplate.Id,
                HtmlTemplateTypeId = htmlTemplate.HtmlTemplateTypeId,
                Alias = htmlTemplate.Alias,
                Label = htmlTemplate.Label,
                Description = htmlTemplate.Description,
                Text = GetStringFromByte(htmlTemplate.Content)
            };
        }

        private HtmlTemplate FillHtmlTemplate(HtmlTemplateCreateVM model)
        {
            return new HtmlTemplate()
            {
                Id = model.Id,
                Label = model.Label,
                Alias = model.Alias,
                HtmlTemplateTypeId = model.HtmlTemplateTypeId,
                FileName = "HtmlTemplateCreate",
                ContentType = "text/html",
                Content = Encoding.UTF8.GetBytes(model.Text),
                DateUploaded = DateTime.Now,
                IsCreate = true
            };
        }

        public bool HtmlTemplateCreate_SaveData(HtmlTemplateCreateVM model)
        {
            try
            {
                var modelSave = FillHtmlTemplate(model);
                if (model.Id > 0)
                {
                    //Update
                    HtmlTemplate saved = repo.GetById<HtmlTemplate>(modelSave.Id);
                    saved.HtmlTemplateTypeId = modelSave.HtmlTemplateTypeId;
                    saved.Alias = modelSave.Alias;
                    saved.Label = modelSave.Label;
                    saved.Description = modelSave.Description;
                    saved.Content = modelSave.Content;
                    saved.DateUploaded = modelSave.DateUploaded;
                    saved.FileName = modelSave.FileName;
                    saved.ContentType = modelSave.ContentType;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    repo.Add(modelSave);
                    repo.SaveChanges();
                    model.Id = modelSave.Id;
                }

                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }
    }
}
