using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Extensions
{
    public static class BreadcrumbsExtentions
    {
        public static List<BreadcrumbsVM> DeleteOrDisableLast (this List<BreadcrumbsVM> list)
        {
            if (list != null && list.Count() >= 1)
            {
                if (list.Count() == 1)
                    list.Last().Active = true;
                else
                    list.RemoveAt(list.Count() - 1);
            }
            return list;
        }
        public static string ReturnUrlFromLast(this List<BreadcrumbsVM> list)
        {
            string result = "#";
            if (list != null && list.Count > 0)
            {
                if (list.Count == 1 && list.Last().Active)
                    return @"/";
                result = list.Last().Href;
                if (result.Contains("()"))
                    result = "#";
            }
            return result;
        }
        public static string ReturnOnClickFromLast(this List<BreadcrumbsVM> list)
        {
            string result = "#";
            if (list != null && list.Count > 0)
            {
                if (list.Count == 1 && list.Last().Active)
                    return "";
                result = list.Last().Href;
                if (!result.Contains("()"))
                    result = "";
            }
            return result;
        }
    }

}
