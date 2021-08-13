using IOWebApplication.Infrastructure.Extensions.HTML;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class NPoiExcelService
    {
        public int rowIndex = 0;
        public int colIndex = 0;
        private IWorkbook workBook;
        private ISheet sheet;
        private ICreationHelper creationHelper;
        private IRow row;
        private XSSFCellStyle defaultCellStyle;
        public NPoiExcelService(string sheetName)
        {
            workBook = new XSSFWorkbook();
            sheet = workBook.CreateSheet(sheetName);
            creationHelper = workBook.GetCreationHelper();
            row = sheet.CreateRow(rowIndex);
            defaultCellStyle = CreateDefaultStyle();
            sheet.PrintSetup.PaperSize = (short)PaperSize.A4 + 1;
            sheet.PrintSetup.HeaderMargin = 0.2d;
            sheet.PrintSetup.FooterMargin = 0.2d;
            sheet.SetMargin(MarginType.LeftMargin, 0.2d);
            sheet.SetMargin(MarginType.RightMargin, 0.2d);
            sheet.SetMargin(MarginType.TopMargin, 0.3d);
            sheet.SetMargin(MarginType.BottomMargin, 0.3d);
        }
        public NPoiExcelService(byte[] content, int sheetNum)
        {
            using (Stream stream = new MemoryStream(content))
            {
                workBook = new XSSFWorkbook(stream);
                sheet = workBook.GetSheetAt(sheetNum);
            }
            creationHelper = workBook.GetCreationHelper();
            defaultCellStyle = CreateDefaultStyle();
        }
        public void SetFontTimesNewRoman(XSSFCellStyle cellStyle)
        {
            var font = workBook.CreateFont();
            font.FontHeightInPoints = 12;
            font.FontName = "Times New Roman";
            cellStyle.SetFont(font);
        }
        public XSSFCellStyle CreateDefaultStyle()
        {
            XSSFCellStyle cellStyle = CreateStyle();
            SetStyleTop(cellStyle);
            SetStyleWrap(cellStyle);
            SetFontTimesNewRoman(cellStyle);
            return cellStyle;
        }
        public XSSFCellStyle CreateTitleStyle()
        {
            XSSFCellStyle cellStyle = CreateDefaultStyle();
            SetStyleHorisontalCenter(cellStyle);
            return cellStyle;
        }
        public XSSFCellStyle CreateStyle()
        {
            return (XSSFCellStyle)workBook.CreateCellStyle();
        }
        public void SetStyleWrap(XSSFCellStyle cellStyle)
        {
            cellStyle.WrapText = true;
        }
        public void SetStyleTop(XSSFCellStyle cellStyle)
        {
            cellStyle.VerticalAlignment = VerticalAlignment.Top;
        }
        public void SetStyleHorisontalCenter(XSSFCellStyle cellStyle)
        {
            cellStyle.Alignment = HorizontalAlignment.Center;
        }

        public void SetStyleBorderThin(XSSFCellStyle cellStyle)
        {
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
        }
        public void SetStyleBorderMedium(XSSFCellStyle cellStyle)
        {
            cellStyle.BorderTop = BorderStyle.Medium;
            cellStyle.BorderBottom = BorderStyle.Medium;
            cellStyle.BorderLeft = BorderStyle.Medium;
            cellStyle.BorderRight = BorderStyle.Medium;
        }
        public void SetColor(XSSFCellStyle cellStyle, short color)
        {
            if (color > 0)
            {
                cellStyle.FillForegroundColor = color;
                cellStyle.FillPattern = FillPattern.SolidForeground;
            }
        }
        public void AddCell(string data, XSSFCellStyle cellStyle = null, bool isNumeric = false)
        {
            row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            ICell cell;
            if (isNumeric)
            {
                cell = row.CreateCell(colIndex, CellType.Numeric);
            }
            else
            {
                cell = row.CreateCell(colIndex);
            }
            cell.CellStyle = cellStyle ?? defaultCellStyle;

            if (isNumeric)
            {
                cell.SetCellFormula(data);
            }
            else
            {
                cell.SetCellValue(creationHelper.CreateRichTextString(data));
            }
            colIndex++;
        }
        public void AddCellFormula(string data, XSSFCellStyle cellStyle = null)
        {
            row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            ICell cell = row.CreateCell(colIndex);
            cell.CellStyle = cellStyle ?? defaultCellStyle;
            cell.SetCellFormula(data);
            colIndex++;
        }
        public void SetCellData(string data)
        {
            data = Truncate(data);
            row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            ICell cell = row.GetCell(colIndex) ?? row.CreateCell(colIndex);
            cell.SetCellValue(creationHelper.CreateRichTextString(data));
            colIndex++;
        }
        public void SetCellFormula(string data)
        {
            row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            ICell cell = row.GetCell(colIndex) ?? row.CreateCell(colIndex);
            cell.SetCellFormula(data);
            colIndex++;
        }

        public void AddRow()
        {
            rowIndex++;
            colIndex = 0;
            row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
        }
        public void InsertRow(bool copyStyle = false, int moveRows = 1)
        {
            for (int i = 0; i < moveRows; i++)
            {
                rowIndex++;
                colIndex = 0;
                if (rowIndex <= sheet.LastRowNum)
                    sheet.ShiftRows(rowIndex, sheet.LastRowNum, 1, true, false);
                row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
                if (copyStyle)
                {
                    var row1 = sheet.GetRow(rowIndex - 1);
                    if (row1 == null)
                        row1 = sheet.CreateRow(rowIndex - 1);
                    if (sheet.IsRowBroken(rowIndex - 1))
                    {
                        sheet.RemoveRowBreak(rowIndex - 1);
                        sheet.SetRowBreak(rowIndex);
                    }
                    for (int j = 0; j < row1.LastCellNum; j++)
                    {
                        if (row.GetCell(j) == null)
                            row.CreateCell(j);
                        row.GetCell(j).CellStyle = row1.GetCell(j).CellStyle;
                    }
                }
            }
        }
        public void CopyRowStyle(int fromRow)
        {
            row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            var row1 = sheet.GetRow(fromRow);
            if (row1 == null)
                row1 = sheet.CreateRow(rowIndex - 1);
            if (sheet.IsRowBroken(rowIndex - 1))
            {
                sheet.RemoveRowBreak(rowIndex - 1);
                sheet.SetRowBreak(rowIndex);
            }
            for (int j = 0; j < row1.LastCellNum; j++)
            {
                if (row.GetCell(j) == null)
                    row.CreateCell(j);
                row.GetCell(j).CellStyle = row1.GetCell(j).CellStyle;
            }
        }
        public void InsertRowTitle(bool copyStyle = false, int moveRows = 1)
        {
            InsertRow(copyStyle, moveRows);
            rowIndex -= moveRows;
        }
        public void AddRange(string data, int colLen, int rowLen, XSSFCellStyle cellStyle = null)
        {
            row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            ICell cell = row.CreateCell(colIndex);
            cell.CellStyle = cellStyle ?? defaultCellStyle;
            cell.SetCellValue(creationHelper.CreateRichTextString(data));

            //Да не прави Range на една клетка, че има проблем с Wrap трябва да измислим и как да оправим Autofit-a  на Merge cells/rows
            if (colLen != 1 || rowLen != 1)
            {
                CellRangeAddress cra = new CellRangeAddress(rowIndex, rowIndex + rowLen - 1, colIndex, colIndex + colLen - 1);
                sheet.AddMergedRegion(cra);
                if (cellStyle != null)
                {
                    RegionUtil.SetBorderTop((int)cellStyle.BorderTop, cra, sheet, workBook);
                    RegionUtil.SetBorderBottom((int)cellStyle.BorderBottom, cra, sheet, workBook);
                    RegionUtil.SetBorderLeft((int)cellStyle.BorderLeft, cra, sheet, workBook);
                    RegionUtil.SetBorderRight((int)cellStyle.BorderRight, cra, sheet, workBook);
                }
            }
            //if (cellStyle != null && cellStyle.Alignment == HorizontalAlignment.Center)
            //    CellUtil.SetAlignment(cell, workBook, (int)HorizontalAlignment.Center);
        }
        public void AddRange(string data, int colLen, XSSFCellStyle cellStyle = null)
        {
            AddRange(data, colLen, 1, cellStyle);
        }

        public void AddRangeMoveCol(string data, int colLen, int rowLen, XSSFCellStyle cellStyle = null)
        {
            AddRange(data, colLen, rowLen, cellStyle);
            colIndex += colLen;
        }

        public void InsertRangeMoveCol(string data, int colLen, int rowLen)
        {
            row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            ICell cell = row.GetCell(colIndex) ?? row.CreateCell(colIndex);
            cell.SetCellValue(creationHelper.CreateRichTextString(data));

            //Да не прави Range на една клетка, че има проблем с Wrap трябва да измислим и как да оправим Autofit-a  на Merge cells/rows
            if (colLen != 1 || rowLen != 1)
            {
                CellRangeAddress cra = new CellRangeAddress(rowIndex, rowIndex + rowLen - 1, colIndex, colIndex + colLen - 1);
                sheet.AddMergedRegion(cra);
            }
            colIndex += colLen;
        }

        public void SetColumnWidths(int[] colWidths)
        {
            for (int i = 0; i < colWidths.Length; i++)
            {
                sheet.SetColumnWidth(i, colWidths[i]);
            }
        }
        public void SetColumnWidth(int col, int colWidth)
        {
            sheet.SetColumnWidth(col, colWidth);
        }

        public void SetRowHeight(int rowIdx, int rowHeight)
        {
            row = sheet.GetRow(rowIdx) ?? sheet.CreateRow(rowIdx);
            row.Height = (short)rowHeight;
        }

        private static string GetValueFromProperty(PropertyInfo pInfo, object data)
        {
            var value = pInfo.GetValue(data);
            var propertyType = pInfo.PropertyType;
            var formatName = pInfo.GetCustomAttribute<DisplayFormatAttribute>()?.DataFormatString;
            var valueText = "";
            if (value != null)
            {
                if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                {
                    formatName = formatName != null ? formatName.Replace("{0:", "").Replace("}", "") : "";
                    valueText = string.IsNullOrEmpty(formatName) ? Convert.ToDateTime(value).ToString() : Convert.ToDateTime(value).ToString(formatName);
                }
                else
                {
                    valueText = value.ToString().Decode();
                }
            }
            return valueText;
        }

        public void AddList<TSource>(
            IList<TSource> dataList,
            int[] colWidths,
            ICollection<Expression<Func<TSource, object>>> propertyLambdaList,
            short titleColor, short oddColColor, short evenColColor,
            bool printHeader = true)
            where TSource : new()
        {

            SetColumnWidths(colWidths);
            if (printHeader)
            {
                AddRow();
                var styleTitle = CreateDefaultStyle();
                SetColor(styleTitle, titleColor);
                SetStyleBorderMedium(styleTitle);
                foreach (var propertyLambda in propertyLambdaList)
                {
                    var dataItem = new TSource();
                    PropertyInfo pInfo = GetPropertyInfo(dataItem, propertyLambda);
                    AddCell(pInfo.GetCustomAttribute<DisplayAttribute>()?.Name, styleTitle);
                }
            }

            var styleRowOdd = CreateDefaultStyle();
            SetStyleBorderThin(styleRowOdd);
            SetColor(styleRowOdd, oddColColor);

            var styleRowEven = CreateDefaultStyle();
            SetStyleBorderThin(styleRowEven);
            SetColor(styleRowEven, evenColColor);
            bool isOdd = true;
            foreach (var data in dataList)
            {
                AddRow();
                var styleRow = isOdd ? styleRowOdd : styleRowEven;
                foreach (var propertyLambda in propertyLambdaList)
                {
                    PropertyInfo pInfo = GetPropertyInfo(dataList[0], propertyLambda);
                    var value = pInfo.GetValue(data);
                    var valueText = GetValueFromProperty(pInfo, data);
                    AddCell(valueText, styleRow, getCellTypeIsNumeric(pInfo));
                }
                isOdd = !isOdd;
            }
        }
        private bool getCellTypeIsNumeric(PropertyInfo pInfo)
        {
            switch (pInfo.PropertyType.Name.ToLower())
            {
                case "integer":
                case "int32":
                case "int64":
                case "long":
                    return true;
                default:
                    return false;
            }
        }
        public void InsertList<TSource>(
           IList<TSource> dataList,
           ICollection<Expression<Func<TSource, object>>> propertyLambdaList)
           where TSource : new()
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                if (i > 0)
                    InsertRow(true);
                foreach (var propertyLambda in propertyLambdaList)
                {
                    PropertyInfo pInfo = GetPropertyInfo(dataList[0], propertyLambda);
                    var value = pInfo.GetValue(data);
                    var valueText = GetValueFromProperty(pInfo, data);
                    SetCellData(valueText);
                }
            }
        }
        public byte[] ToArray()
        {
            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                return stream.ToArray();
            }
        }

        private PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;

            if (member == null)
            {
                var expressionBody = propertyLambda.Body;
                if (expressionBody is UnaryExpression expression && expression.NodeType == ExpressionType.Convert)
                {
                    expressionBody = expression.Operand;
                }
                member = (MemberExpression)expressionBody;
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
        public void SetRepeatingRows(int startRow, int rowLen)
        {
            // workBook.SetRepeatingRowsAndColumns(sheet., startColumn, endColumn, startRow, endRow);
            CellRangeAddress cra = new CellRangeAddress(startRow, startRow + rowLen - 1, -1, -1);
            sheet.RepeatingRows = cra;
        }
        public void DeleteRowToEnd(int fromRow)
        {
            for (int row = fromRow; row <= sheet.LastRowNum; row++)
            {
                sheet.CreateRow(row);
            }
        }

        public void SetRowBreak()
        {
            sheet.SetRowBreak(rowIndex);
        }
        public void SetRowHeghtFromText(string data)
        {
            var datarows = data.Split(Environment.NewLine);
            if (datarows.Length > 1)
            {
                row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
                row.Height = (short)(row.Height * datarows.Length);
            }
        }

        private string Truncate(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (source.Length > 32000)
                {
                    source = source.Substring(0, 32000);
                }
            }

            return source;
        }
        public string GetValue(int row, int col)
        {
            return sheet.GetRow(row).GetCell(col).StringCellValue;
        }
        public string GetValue()
        {
            string result = string.Empty;
            if (sheet?.GetRow(rowIndex)?.GetCell(colIndex)?.CellType == null)
                return null;
            if (sheet.GetRow(rowIndex).GetCell(colIndex).CellType == CellType.Numeric)
            {
                result = sheet.GetRow(rowIndex).GetCell(colIndex).NumericCellValue.ToString();
            } else
            {
                result = sheet.GetRow(rowIndex).GetCell(colIndex).StringCellValue;
            }
            colIndex++;
            return result;
        }
    }
}
