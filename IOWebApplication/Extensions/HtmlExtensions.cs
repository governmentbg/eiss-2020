using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.Mvc.Rendering
{


    public static class HiddenIndexHtmlHelper
    {
        /// <summary>
        /// Hiddens the index for.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="index">The Index</param>
        /// <returns>Returns Hidden Index For</returns>
        public static IHtmlContent HiddenIndexFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, int index)
        {
            var modelExplorer = ExpressionMetadataProvider.FromLambdaExpression(expression, htmlHelper.ViewData, htmlHelper.MetadataProvider);

            var metadata = modelExplorer.Metadata;
            var propName = metadata.PropertyName;

            var tagBuilder = new TagBuilder("input") { TagRenderMode = TagRenderMode.Normal };
            tagBuilder.MergeAttribute("type", "hidden");
            tagBuilder.MergeAttribute("name", string.Format("{0}.Index", propName, index));
            tagBuilder.MergeAttribute("value", index.ToString());
            return new HtmlString(tagBuilder.ToString());
        }

        public static string GetDbName(this IConfiguration config)
        {

            string conStrDatabase = config.GetValue<string>("ConnectionStrings:DefaultConnection")
                                        .Split(';')
                                        .Where(x => x.Contains("database", StringComparison.InvariantCultureIgnoreCase))
                                        .FirstOrDefault();
            if (!string.IsNullOrEmpty(conStrDatabase))
            {
                return conStrDatabase.ToUpper().Replace("DATABASE=", "");
            }

            return string.Empty;
        }
    }

}
