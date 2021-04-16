using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IHtmlTemplate: IBaseService
    {
        IQueryable<HtmlTemplateVM> HtmlTemplate_Select(HtmlTemplateFilterVM filterData);
        bool HtmlTemplate_SaveData(ICollection<IFormFile> files, HtmlTemplate model);
        bool HtmlTemplate_ImportData(ICollection<IFormFile> files, HtmlTemplate model);
        string HtmlTemplate_FillData(string alias, IList<KeyValuePairVM> keyValuePairs);
        IQueryable<HtmlTemplateLinkVM> HtmlTemplateLink_Select(int HtmlTemplateId);
        IQueryable<HtmlTemplateParamLinkVM> HtmlTemplateParam_Select(int HtmlTemplateId);
        IQueryable<HtmlTemplateParamVM> HtmlTemplateParamAll_Select();
        bool HtmlTemplateLink_SaveData(HtmlTemplateLink model);
        bool HtmlTemplate_ImportParam();
        string HtmlTemplate_GetNotSetParam(string alias);
        HtmlTemplateCreateVM GetById_HtmlTemplateCreate(int id);
        bool HtmlTemplateCreate_SaveData(HtmlTemplateCreateVM model);
    }
}
