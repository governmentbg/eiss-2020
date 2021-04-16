using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class ExcelExportColumnVM<T>
    {
        public ExcelExportColumnVM(Expression<Func<T, object>> col, int width, string colName = null)
        {
            Column = col;
            Width = width;
            ColumnName = colName;
        }
        public Expression<Func<T, object>> Column { get; set; }
        public int Width { get; set; }
        public string ColumnName { get; set; }
    }

    public class ExcelExportColumnList<T> : List<ExcelExportColumnVM<T>> where T : class
    {
        public ExcelExportColumnList(DataTableFilterExportVM filter)
        {
            visibility = filter.ColumnVisibility.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        string[] visibility;
        public int[] GetColumnsWidth()
        {
            return this.Where(x => (x.ColumnName != null) ? visibility.Any(v => v.Contains("name:" + x.ColumnName.ToLower())) : visibility.Any(v => v.Contains("data:" + PropertyName(x.Column).ToLower())))
                .Select(x => x.Width).ToArray();
        }

        public List<Expression<Func<T, object>>> GetColumns()
        {
            return this.Where(x => (x.ColumnName != null) ? visibility.Any(v => v.Contains("name:" + x.ColumnName.ToLower())) :  visibility.Any(v => v.Contains("data:" + PropertyName(x.Column).ToLower())))
                .Select(x => x.Column).ToList();
        }

        private string PropertyName(Expression<Func<T, object>> expression)
        {
            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }

            return body.Member.Name;
        }
    }
}
