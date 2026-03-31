using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Extensions.HTML;
using IOWebApplication.Infrastructure.Models.Cdn;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Extensions
{
    public static class NomenclatureExtensions
    {
        /// <summary>
        /// Creates SelectList from IQueryable&lt;ICommonNomenclature&gt;
        /// </summary>
        /// <param name="model"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <param name="orderByNumber"></param>
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
                })
                .ToList() ?? new List<SelectListItem>();

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

            return result.Decode();
        }

        /// <summary>
        /// Creates SelectList from IQueryable&lt;ICommonNomenclature&gt;
        /// </summary>
        /// <param name="model"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <param name="orderByNumber"></param>
        /// <returns></returns>
        public static async Task<List<SelectListItem>> ToSelectListAsync(this IQueryable<ICommonNomenclature> model, bool addDefaultElement = false, bool addAllElement = false, bool orderByNumber = true)
        {
            DateTime today = DateTime.Today;

            Expression<Func<ICommonNomenclature, object>> order = x => x.OrderNumber;
            if (!orderByNumber)
            {
                order = x => x.Label;
            }

            var result = await model
                .Where(x => x.IsActive)
                .Where(x => x.DateStart <= today)
                .Where(x => (x.DateEnd ?? today) >= today)
                .OrderBy(order)
                .Select(x => new SelectListItem()
                {
                    Text = x.Label,
                    Value = x.Id.ToString()
                }).ToListAsync() ?? new List<SelectListItem>();

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

            return result.Decode();
        }

        public static List<SelectListItem> Decode(this List<SelectListItem> model)
        {
            foreach (var item in model)
            {
                item.Text = item.Text.Decode();
            }
            return model;
        }

        public static List<SelectListItem> ToSelectListFromCode(this IQueryable<ICommonNomenclature> model, bool addDefaultElement = false, bool addAllElement = false, bool orderByNumber = true, bool labelText = true)
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
                    Text = (labelText) ? x.Label : x.Code,
                    Value = x.Code
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

            return result.Decode();
        }

        public static List<SelectListItem> SingleOrChoose(this List<SelectListItem> model)
        {
            if (model == null)
            {
                return null;
            }
            if (model.Count(x => x.Value != "-1" && x.Value != "-2" && !string.IsNullOrEmpty(x.Value)) == 1)
            {
                if (model.ElementAt(0).Value == "-1" || model.ElementAt(0).Value == "-2" || string.IsNullOrEmpty(model.ElementAt(0).Value))
                {
                    model.RemoveAt(0);
                }
                if (model.ElementAt(0).Value == "-1" || model.ElementAt(0).Value == "-2" || string.IsNullOrEmpty(model.ElementAt(0).Value))
                {
                    model.RemoveAt(0);
                }
            }

            return model;
        }
        /// <summary>
        /// Creates SelectList from IQueryable&lt;ICommonNomenclature&gt;
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

            return result.Decode();
        }

        /// <summary>
        /// Creates SelectList from IQueryable&lt;ICommonNomenclature&gt;
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

            return result.Decode();
        }

        /// <summary>
        /// Creates SelectList from IQueryable&lt;ICommonNomenclature&gt;
        /// </summary>
        /// <param name="model"></param>
        /// <param name="addDefaultElement"></param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectListCodeDescription(this IQueryable<ICommonNomenclature> model, bool addDefaultElement = false, bool addAllElement = false)
        {
            DateTime today = DateTime.Today;

            var result = model
                .Where(x => x.IsActive)
                .Where(x => x.DateStart <= today)
                .Where(x => (x.DateEnd ?? today) >= today)
                .Select(x => new SelectListItem()
                {
                    Text = (x.Description != null) ? x.Description : x.Label,
                    Value = x.Code
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

            return result.Decode();
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
            string valueName = valueField.GetName();
            string labelName = labelField.GetName();
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

        public static DateTime MakeEndDate(this DateTime model)
        {
            if (model.Hour == 0 && model.Minute == 0)
            {
                return model.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            return model;
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

        public static DateTime MakeZeroSeconds(this DateTime model)
        {
            return new DateTime(model.Year, model.Month, model.Day, model.Hour, model.Minute, 0);
        }

        public static string DateToString(this DateTime? model)
        {
            if (model != null)
                return (model ?? DateTime.Now).ToString("dd.MM.yyyy");

            return String.Empty;
        }

        /// <summary>
        /// Конвертиране на дата в стринг
        /// </summary>
        /// <param name="model">Дата</param>
        /// <returns></returns>
        public static string ConvertDateTimeToString(this DateTime? model)
        {
            if (model.HasValue)
            {
                DateTime dateTime = model.Value;
                return dateTime.ToString("dd.MM.yyyy");
            }
            else
                return string.Empty;
        }

        /// <summary>
		/// Конвертиране на дата в стринг
		/// </summary>
		/// <param name="model">Дата</param>
		/// <returns></returns>
		public static string ConvertDateTimeToStringCriminalReport(this DateTime? model)
        {
            if (model.HasValue)
            {
                DateTime dateTime = model.Value;
                return $"{dateTime.ToString("yyyy")}, {dateTime.ToString("MM")}, {dateTime.ToString("dd")}";
            }
            else
                return string.Empty;
        }

        public static string ConvertDateTimeToStringYear(this DateTime? model)
        {
            if (model.HasValue)
            {
                DateTime dateTime = model.Value;
                return dateTime.ToString("yyyy");
            }
            else
                return string.Empty;
        }

        public static string ConvertDateTimeToStringMonth(this DateTime? model)
        {
            if (model.HasValue)
            {
                DateTime dateTime = model.Value;
                return dateTime.ToString("MM");
            }
            else
                return string.Empty;
        }

        public static string ConvertDateTimeToStringDay(this DateTime? model)
        {
            if (model.HasValue)
            {
                DateTime dateTime = model.Value;
                return dateTime.ToString("dd");
            }
            else
                return string.Empty;
        }

        public static bool CompareDatesToMinutes(DateTime date1, DateTime date2)
        {
            return date1.ToString("yyyyMMddHHmm") == date2.ToString("yyyyMMddHHmm");
        }
        //public static DateTime ForceEndDate(this DateTime model)
        //{
        //    return model.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        //}

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

        /// <summary>
        /// Сетва краен час на дата и ако е null добавя към днешна датат години
        /// </summary>
        /// <param name="model">Дата</param>
        /// <param name="year">Години за добавяне</param>
        /// <returns></returns>
        public static DateTime? ForceEndDateWithAddYear(this DateTime? model, int year)
        {
            return (model ?? DateTime.Now.AddYears(year)).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        public static DateTime? ForceStartDate(this DateTime? model)
        {
            if (model.HasValue)
            {
                return model.Value.ForceStartDate();
            }
            return model;
        }

        /// <summary>
        /// Сетва начален час на датат и ако е null добавя към днешна дата години
        /// </summary>
        /// <param name="model">Дата</param>
        /// <param name="year">Години за добавяне</param>
        /// <returns></returns>
        public static DateTime? ForceStartDateWithAddYear(this DateTime? model, int year)
        {
            return (model ?? DateTime.Now.AddYears(year)).ForceStartDate();
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

        public static DateTime GetPastDate()
        {
            return new DateTime(1900, 1, 1);
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

        public static string DayName(this DateTime? model)
        {
            if (model.HasValue)
            {
                string datestring = model.Value.Day.ToString();
                switch (model.Value.DayOfWeek)
                {
                    case DayOfWeek.Friday:
                        datestring = datestring + " (петък)";
                        break;
                    case DayOfWeek.Monday:
                        datestring = datestring + " (понед.)";
                        break;
                    case DayOfWeek.Saturday:
                        datestring = datestring + " (събота)";
                        break;
                    case DayOfWeek.Sunday:
                        datestring = datestring + " (неделя)";
                        break;
                    case DayOfWeek.Thursday:
                        datestring = datestring + " (четвъртък)";
                        break;
                    case DayOfWeek.Tuesday:
                        datestring = datestring + " (вторник)";
                        break;
                    case DayOfWeek.Wednesday:
                        datestring = datestring + " (сряда)";
                        break;
                    default:
                        break;
                }
                return datestring;
            }
            return "";
        }
        public static string DayNameOnly(this DateTime? model)
        {
            if (model.HasValue)
            {
                string datestring = "";
                switch (model.Value.DayOfWeek)
                {
                    case DayOfWeek.Friday:
                        datestring = datestring + "петък";
                        break;
                    case DayOfWeek.Monday:
                        datestring = datestring + "понедeлник";
                        break;
                    case DayOfWeek.Saturday:
                        datestring = datestring + "събота";
                        break;
                    case DayOfWeek.Sunday:
                        datestring = datestring + "неделя";
                        break;
                    case DayOfWeek.Thursday:
                        datestring = datestring + "четвъртък";
                        break;
                    case DayOfWeek.Tuesday:
                        datestring = datestring + "вторник";
                        break;
                    case DayOfWeek.Wednesday:
                        datestring = datestring + "сряда";
                        break;
                    default:
                        break;
                }
                return datestring;
            }
            return "";
        }

        public static int[] ToIntArray(this string model)
        {
            try
            {
                return model.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
            }
            catch
            {
                return (new List<int>()).ToArray();
            }
        }

        public static string IntToMonth(int month)
        {
            string result = "";
            switch (month)
            {
                case 1:
                    result = "Януари";
                    break;
                case 2:
                    result = "Февруари";
                    break;
                case 3:
                    result = "Март";
                    break;
                case 4:
                    result = "Април";
                    break;
                case 5:
                    result = "Май";
                    break;
                case 6:
                    result = "Юни";
                    break;
                case 7:
                    result = "Юли";
                    break;
                case 8:
                    result = "Август";
                    break;
                case 9:
                    result = "Септември";
                    break;
                case 10:
                    result = "Октомври";
                    break;
                case 11:
                    result = "Ноември";
                    break;
                case 12:
                    result = "Декември";
                    break;
                default:
                    result = "";
                    break;
            }
            return result;


        }

        public static decimal ParseDecimal(string decValue)
        {
            try
            {
                decValue = decValue.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                decValue = decValue.Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                return Convert.ToDecimal(decValue, System.Globalization.CultureInfo.CurrentCulture);
            }
            catch (Exception e)
            {
                return 0M;
            }
        }
    }
}
