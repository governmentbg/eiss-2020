using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Models.Cdn;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;

namespace IOWebApplication.Infrastructure.Extensions
{
    public static class NomenclatureExtensions
    {
        /// <summary>
        /// Creates SelectList from IQueryable<ICommonNomenclature>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="addDefaultElement"></param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectList(this IQueryable<ICommonNomenclature> model, bool addDefaultElement = false, bool addAllElement = false, bool orderByNumber = true)
        {
            DateTime today = DateTime.Today;

            Expression<Func<ICommonNomenclature, object>> order = x => x.OrderNumber;
            if (!orderByNumber)
            {
                order = x => x.Label;
            }

            var result = model
                .Where(x => x.IsActive)
                .Where(x => x.DateStart <= today)
                .Where(x => (x.DateEnd ?? today) >= today)
                .OrderBy(order)
                .Select(x => new SelectListItem()
                {
                    Text = x.Label,
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        public static List<SelectListItem> SingleOrChoose(this List<SelectListItem> model)
        {
            if (model == null)
            {
                return null;
            }
            if (model.Count(x => x.Value != "-1" && x.Value != "-2") == 1)
            {
                if (model.ElementAt(0).Value == "-1" || model.ElementAt(0).Value == "-2")
                {
                    model.RemoveAt(0);
                }
                if (model.ElementAt(0).Value == "-1" || model.ElementAt(0).Value == "-2")
                {
                    model.RemoveAt(0);
                }
            }

            return model;
        }
        /// <summary>
        /// Creates SelectList from IQueryable<ICommonNomenclature>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="addDefaultElement"></param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectListCodeLabel(this IQueryable<ICommonNomenclature> model, bool addDefaultElement = false, bool addAllElement = false)
        {
            DateTime today = DateTime.Today;

            var result = model
                .Where(x => x.IsActive)
                .Where(x => x.DateStart <= today)
                .Where(x => (x.DateEnd ?? today) >= today)
                .Select(x => new SelectListItem()
                {
                    Text = (x.Code != null) ? x.Code + " " + x.Label : x.Label,
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Creates SelectList from IQueryable<ICommonNomenclature>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="addDefaultElement"></param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectListDescription(this IQueryable<ICommonNomenclature> model, bool addDefaultElement = false, bool addAllElement = false)
        {
            DateTime today = DateTime.Today;

            var result = model
                .Where(x => x.IsActive)
                .Where(x => x.DateStart <= today)
                .Where(x => (x.DateEnd ?? today) >= today)
                .Select(x => new SelectListItem()
                {
                    Text = (x.Description != null) ? x.Description : x.Label,
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        public static List<SelectListItem> ToSelectList<TSource, TValue, TText>(
         this IEnumerable<TSource> source,
         Expression<Func<TSource, TValue>> valueField,
         Expression<Func<TSource, TText>> labelField,
            object selected = null)
        {
            if (source == null)
            {
                return new List<SelectListItem>();
            }
            string valueName = ExpressionHelper.GetExpressionText(valueField);
            string labelName = ExpressionHelper.GetExpressionText(labelField);
            return (new SelectList(source, valueName, labelName, selected)).ToList();
        }

        public static int? NumberEmptyToNull(this int? model)
        {
            if (model < 1)
            {
                return null;
            }
            return model;
        }

        public static int? EmptyToNull(this int? model, int emptyValue = -1)
        {
            if (model == emptyValue)
            {
                return null;
            }
            return model;
        }

        public static string EmptyToNull(this string model, string emptyValue = "")
        {
            if (string.IsNullOrWhiteSpace(model) || model == emptyValue)
            {
                return null;
            }
            return model;
        }

        public static string ToPaternSearch(this string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                return "%";
            }
            return $"%{model.Replace(" ", "%")}%";
        }

        public static string ToEndingPaternSearch(this string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                return "%";
            }
            return $"%{model}";
        }


        public static string SafeLower(this string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                return null;
            }
            return model.ToLower();
        }

        public static DateTime? MakeEndDate(this DateTime? model)
        {
            if (model.HasValue && model.Value.Hour == 0 && model.Value.Minute == 0)
            {
                return model.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            return model;
        }

        //Добавя 59 секунди на DateTime ако няма
        public static DateTime MakeEndSeconds(this DateTime model)
        {
            if (model.Second == 0)
            {
                return model.AddSeconds(59);
            }

            return model;
        }

        public static bool CompareDatesToMinutes(DateTime date1, DateTime date2)
        {
            return date1.ToString("yyyyMMddHHmm") == date2.ToString("yyyyMMddHHmm");
        }

        /// <summary>
        /// Връща датата ако не е null с час 23:59, независимо от часа в нея
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DateTime? ForceEndDate(this DateTime? model)
        {
            if (model.HasValue)
            {
                return model.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            return model;
        }


        public static DateTime? ForceStartDate(this DateTime? model)
        {
            if (model.HasValue)
            {
                return model.Value.ForceStartDate();
            }
            return model;
        }

        public static DateTime ForceEndDate(this DateTime model)
        {
            model = model.AddHours(-model.Hour);
            model = model.AddMinutes(-model.Minute);
            model = model.AddSeconds(-model.Second);
            return model.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        public static DateTime ForceStartDate(this DateTime model)
        {
            return model.Date.AddHours(0).AddMinutes(0).AddSeconds(0);
        }

        public static DateTime StrToDateFormat(this string value, string formatDate)
        {
            if (value.Trim().Length == 0)
                return DateTime.MinValue;

            DateTime _dt = DateTime.Now;
            try
            {
                DateTime.TryParseExact(value, formatDate, new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out _dt);
                return _dt;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        public static void SetPropertyValue<T, Tobj>(this T target, Expression<Func<T, Tobj>> memberLamda, Tobj value)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            // TODO: Дали да остане така     
            if (memberSelectorExpression == null)
            {
                var expressionBody = memberLamda.Body;
                if (expressionBody is UnaryExpression expression && expression.NodeType == ExpressionType.Convert)
                {
                    expressionBody = expression.Operand;
                }
                memberSelectorExpression = (MemberExpression)expressionBody;
            }

            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    property.SetValue(target, value, null);
                }
            }
        }

        public static IEnumerable<CdnItemVM> SetCanDelete(this IEnumerable<CdnItemVM> model, bool canDelete)
        {
            if (model == null)
            {
                return null;
            }
            foreach (var item in model)
            {
                item.CanDelete = canDelete;
            }
            return model;
        }

        public static string GetIOReqClass(this Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata model)
        {
            string result = "";

            try
            {
                if (model.ContainerType != null)
                {
                    var Member = model.ContainerType.GetMember(model.PropertyName);
                    var reqTypes = new[] { typeof(Attributes.IORequiredAttribute), typeof(RequiredAttribute), typeof(RangeAttribute) };
                    var hasIOreq = Member[0].CustomAttributes.Any(a => reqTypes.Contains(a.AttributeType));
                    if (hasIOreq)
                    {
                        result = "io-req";
                    }
                }
            }
            catch { }

            return result;
        }

        public static string GetEisppRuleFullName(this Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata model, string fieldPrefix, bool isVM)
        {
            try
            {
                if (model.ContainerType != null)
                {
                    if (!fieldPrefix.StartsWith("Data.Events[0]."))
                        fieldPrefix = "Data.Events[0]." + fieldPrefix.Replace("NewEventObj.", "");
                    fieldPrefix = model.GetEisppRulePath(fieldPrefix);

                    string propertyName = model.PropertyName;
                    if (isVM && propertyName.EndsWith("VM"))
                        propertyName = propertyName.Substring(0, propertyName.Length - 2);
                    var Member = model.ContainerType.GetMember(propertyName);
                    object[] attribs = Member[0].GetCustomAttributes(typeof(System.Xml.Serialization.XmlAttributeAttribute), false);
                    bool doesPropertyHaveAttrib = attribs.Length > 0;
                    if (doesPropertyHaveAttrib)
                    {
                        string name = ((System.Xml.Serialization.XmlAttributeAttribute)attribs[0]).AttributeName;
                        if (fieldPrefix.Contains("."))
                        {
                            fieldPrefix = fieldPrefix.Replace("." + model.PropertyName, "." + name, StringComparison.InvariantCulture);
                        }
                        else
                        {
                            fieldPrefix = fieldPrefix.Replace(model.PropertyName, name, StringComparison.InvariantCulture);
                        }
                        return fieldPrefix;
                    }

                }
            }
            catch (Exception ex)
            {
            }

            return "";
        }
        public static string GetEisppRulePath(this Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata model, string fieldPrefix)
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    fieldPrefix = fieldPrefix.Replace(i.ToString(), "", StringComparison.InvariantCultureIgnoreCase);
                }
                fieldPrefix = fieldPrefix.Replace("[]", "", StringComparison.InvariantCultureIgnoreCase);
                fieldPrefix = fieldPrefix.Replace("Data.Events.", "", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace("CriminalProceeding.", "NPR.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".Case.", ".DLO.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".Status.", ".DLOSTA.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".ConnectedCases.", ".DLOOSN.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".Persons.", ".FZL.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".Punishments.", ".NKZ.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".ProbationMeasure.", ".PBC.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".Crimes.", ".PNE.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".PersonCPStatus.", ".NPRFZLSTA.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".CrimeStatus.", ".PNESTA.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".CPPersonCrimes.", ".NPRFZLPNE.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".CrimeSanction.", ".SCQ.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".Measures.", ".MPP.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".BirthPlace.", ".MRD.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace("EisppSrok.", "SRK.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace("EventFeature.", "SBH.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".CrimePunishments.", ".NKZPNE.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".CrimeSubjectStatisticData.", ".SBC.", StringComparison.InvariantCulture);
                fieldPrefix = fieldPrefix.Replace(".CiminalProceedingCrime.", ".NPRPNESTA.", StringComparison.InvariantCulture);
                if (fieldPrefix.Contains("NPR.DLO.PNE.NPRFZLPNE.SCQ.", StringComparison.InvariantCulture) ||
                    fieldPrefix.Contains("NPR.DLO.PNE.NPRFZLPNE.SBC.", StringComparison.InvariantCulture))
                {
                    fieldPrefix = fieldPrefix.Replace(".PNE.", ".", StringComparison.InvariantCulture);
                }
                return fieldPrefix;
            }
            catch (Exception ex)
            {
            }

            return "";
        }

        public static string GetEisppRuleElement(this Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata model, string fieldPrefix)
        {
            if (!fieldPrefix.StartsWith("Data.Events[0]."))
                fieldPrefix = "Data.Events[0]." + fieldPrefix.Replace("NewEventObj.", "");

            fieldPrefix = model.GetEisppRulePath(fieldPrefix + ".");
            if (fieldPrefix.EndsWith("."))
                fieldPrefix = fieldPrefix.Substring(0, fieldPrefix.Length - 1);
            return fieldPrefix;
        }

        public static DateTime GetStartYear()
        {
            return GetStartYear(DateTime.Now);
        }

        public static DateTime GetStartYear(DateTime model)
        {
            return new DateTime(model.Year, 1, 1);
        }

        public static DateTime GetEndYear()
        {
            return GetEndYear(DateTime.Now);
        }

        public static DateTime GetEndYear(DateTime model)
        {
            return new DateTime(model.Year, 12, 31);
        }

        public static string ConcatenateWithSeparator(this ICollection<int> model, string separator = ",")
        {
            string result = string.Empty;
            if (model != null)
            {
                foreach (var item in model)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result += separator;
                    }
                    result += item.ToString();
                }

            }
            return result;
        }

        public static string EscapeSingleQuotes(this string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                return null;
            }
            return model.Replace("''", "\\\\\"").Replace("'", "\\\\\"");
        }

        public static DateTime OrMinDate(this DateTime? value)
        {
            return value ?? DateTime.MinValue;
        }
        public static DateTime OrMaxDate(this DateTime? value)
        {
            if (value.HasValue)
            {
                return value.MakeEndDate().Value;
            }
            return value ?? DateTime.MaxValue;
        }

        public static string DateToStr(this DateTime? value, string format)
        {
            if (value.HasValue)
            {
                return value?.ToString(format);
            }
            return "";
        }

        public static string ToJSstring(this int[] model, bool addValueSeparator = true)
        {
            string result = "";
            foreach (var item in model)
            {
                if (addValueSeparator)
                {
                    result += "|";
                }
                result += item.ToString();
                if (addValueSeparator)
                {
                    result += "|";
                }
                result += ",";
            }
            if (result.Length > 0)
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }
    }
}
