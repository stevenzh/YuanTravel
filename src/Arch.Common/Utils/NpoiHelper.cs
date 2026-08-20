using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;

namespace Arch.Common.Utils
{
    /// <summary>
    /// npoi 帮助类
    /// </summary>
    public class NpoiHelper
    {

        #region 针对接口版本的函数。目前不适合我们npoi版本
        //private ISheet _sheet;
        //public ISheet WorkSheet
        //{
        //    get { return _sheet; }
        //    set { _sheet = value; }
        //}

        //private HSSFWorkbook _workbook;
        //public HSSFWorkbook Workbook
        //{
        //    get { return _workbook; }
        //    set { _workbook = value; }
        //}

        //private int _currentrow;
        ///// <summary>
        ///// 当前行
        ///// </summary>
        //public int CurrentRow
        //{
        //    get { return _currentrow; }
        //    set { _currentrow = value; }
        //}
        //private int _currentcol;
        ///// <summary>
        ///// 当前列
        ///// </summary>
        //public int CurrentCol
        //{
        //    get { return _currentcol; }
        //    set { _currentcol = value; }
        //}
        //private HSSFFormulaEvaluator _evaluator;

        ///// <summary>
        ///// 列数
        ///// </summary>
        //public int ColsCount { get; set; }


        //public ExcelHelper(HSSFWorkbook workbook, ISheet sheet, int colsCount)
        //{
        //    _workbook = workbook;
        //    _sheet = sheet;
        //    ColsCount = colsCount;
        //}

        //public ExcelHelper(HSSFWorkbook workbook, ISheet sheet)
        //{
        //    _workbook = workbook;
        //    _sheet = sheet;
        //}
        ///// 设置sheet的名称
        ///// </summary>
        ///// <param name="sheetName"></param>
        //public void SetSheetName(string sheetName)
        //{
        //    _workbook.SetSheetName(_workbook.GetSheetIndex(_sheet), sheetName);
        //}

        ///// <summary>
        ///// 隐藏列
        ///// </summary>
        ///// <param name="col"></param>
        //public void HideColumn(int col)
        //{
        //    _sheet.SetColumnHidden(col - 1, true);
        //}

        ///// <summary>
        ///// sheet克隆
        ///// </summary>
        ///// <param name="sheetName"></param>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public ExcelHelper CloneSheet(string sheetName, ExcelHelper source)
        //{
        //    ISheet sheet = source.Workbook.CloneSheet(source.Workbook.GetSheetIndex(source.WorkSheet));

        //    source.Workbook.SetSheetName(source.Workbook.GetSheetIndex(sheet), sheetName);

        //    return new ExcelHelper(source.Workbook, sheet, source.ColsCount);
        //}

        ///// <summary>
        ///// 创建 用于自定义样式
        ///// </summary>
        ///// <returns></returns>
        //public IDataFormat CreateDataFormat()
        //{
        //    return _workbook.CreateDataFormat();
        //}

        ///// <summary>
        ///// 创建单元格样式
        ///// </summary>
        ///// <returns></returns>
        //public ICellStyle CreateCellStyle()
        //{
        //    return _workbook.CreateCellStyle();
        //}

        ///// <summary>
        ///// 创建字体样式
        ///// </summary>
        ///// <returns></returns>
        //public IFont CreateFont()
        //{
        //    return _workbook.CreateFont();
        //}

        ///// <summary>
        ///// 克隆单元格样式
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public ICellStyle CloneCellStyle(ICellStyle source)
        //{
        //    ICellStyle cellstyle = CreateCellStyle();
        //    cellstyle.CloneStyleFrom(source);
        //    return cellstyle;
        //}

        //public ICellStyle GetCellStyle(int row, int col)
        //{
        //    int iRow = CurrentRow;
        //    int iCol = CurrentCol;
        //    ICellStyle cellStyle = GetCell(row, col).CellStyle;
        //    CurrentRow = iRow;
        //    CurrentCol = iCol;
        //    return cellStyle;
        //}

        //public void SetCellStyle(ICellStyle fromCellStyle)
        //{
        //    SetCellStyle(CurrentRow, CurrentCol, fromCellStyle);
        //}

        //public void SetCellStyle(int fromRow, int fromCol, int toRow, int toCol)
        //{
        //    SetCellStyle(toRow, toCol, GetCellStyle(fromRow, fromCol));
        //}

        //public void SetCellStyle(int toRow, int toCol, ICellStyle fromCellStyle)
        //{
        //    GetCell(toRow, toCol).CellStyle = fromCellStyle;
        //}

        ////private void InitCellstyle()
        ////{
        ////    //百分比样式
        ////    PercentCellStyle = _workbook.CreateCellStyle();
        ////    PercentCellStyle.BorderBottom = CellBorderType.THIN;
        ////    PercentCellStyle.BorderLeft = CellBorderType.THIN;
        ////    PercentCellStyle.BorderRight = CellBorderType.THIN;
        ////    PercentCellStyle.BorderTop = CellBorderType.THIN;
        ////    PercentCellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");
        ////    PercentCellStyle.Alignment = HorizontalAlignment.CENTER;
        ////    //小数样式
        ////    DecimalCellStyle = _workbook.CreateCellStyle();
        ////    DecimalCellStyle.BorderBottom = CellBorderType.THIN;
        ////    DecimalCellStyle.BorderLeft = CellBorderType.THIN;
        ////    DecimalCellStyle.BorderRight = CellBorderType.THIN;
        ////    DecimalCellStyle.BorderTop = CellBorderType.THIN;
        ////    DecimalCellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00");
        ////    DecimalCellStyle.Alignment = HorizontalAlignment.CENTER;
        ////    //标题样式（居中对齐，自动换行）
        ////    HeaderCellStyle = _workbook.CreateCellStyle();
        ////    HeaderCellStyle.BorderBottom = CellBorderType.THIN;
        ////    HeaderCellStyle.BorderLeft = CellBorderType.THIN;
        ////    HeaderCellStyle.BorderRight = CellBorderType.THIN;
        ////    HeaderCellStyle.BorderTop = CellBorderType.THIN;
        ////    HeaderCellStyle.Alignment = HorizontalAlignment.CENTER;
        ////    HeaderCellStyle.VerticalAlignment = VerticalAlignment.CENTER;
        ////    HeaderCellStyle.WrapText = true;
        ////    //文本样式
        ////    TextCellStyle = _workbook.CreateCellStyle();
        ////    TextCellStyle.BorderBottom = CellBorderType.THIN;
        ////    TextCellStyle.BorderLeft = CellBorderType.THIN;
        ////    TextCellStyle.BorderRight = CellBorderType.THIN;
        ////    TextCellStyle.BorderTop = CellBorderType.THIN;
        ////    TextCellStyle.Alignment = HorizontalAlignment.LEFT;
        ////}

        //public void SetCellStyle(int row, string col, HSSFCellStyle cellstyle)
        //{
        //    SetCellStyle(row, col.ToInt(), cellstyle);
        //}

        ///// <summary>
        ///// 设置单元格样式
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="col"></param>
        ///// <param name="cellstyle"></param>
        //public void SetCellStyle(int row, int col, HSSFCellStyle cellstyle)
        //{
        //    GetCell(row, col).CellStyle = cellstyle;
        //}

        ///// <summary>
        ///// 取得单元格
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="col"></param>
        ///// <returns></returns>
        //public ICell GetCell(int row, int col)
        //{
        //    CurrentRow = row;
        //    CurrentCol = col;
        //    IRow rowObj = _sheet.GetRow(row - 1);
        //    if (col > ColsCount)
        //    {
        //        ColsCount = col;
        //    }
        //    if (rowObj == null)
        //    {
        //        CreateRow(row);
        //        rowObj = _sheet.GetRow(row - 1);
        //    }
        //    ICell cellObj = rowObj.GetCell(col - 1);
        //    if (cellObj == null)
        //    {
        //        rowObj.CreateCell(col - 1);
        //        cellObj = rowObj.GetCell(col - 1);
        //    }

        //    return cellObj;
        //}

        ///// <summary>
        ///// 取得单元格
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="colName">列的字母名称</param>
        ///// <returns></returns>
        //public ICell GetCell(int row, string colName)
        //{
        //    return GetCell(row, colName.ToInt());
        //}

        //#region 设置单元格内容
        ///// <summary>
        ///// 设置单元格内容
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="colName"></param>
        //public void SetCellText(int row, string colName, object value)
        //{
        //    int col = colName.ToInt();
        //    SetCellText(row, col, value);
        //}

        ///// <summary>
        ///// 设置单元格内容
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="col"></param>
        ///// <param name="value"></param>
        //public void SetCellText(int row, int col, object value)
        //{
        //    if (value == null) return;
        //    Type t = value.GetType();
        //    ICell cell = GetCell(row, col);
        //    if (t == typeof(decimal) || t == typeof(double)
        //        || t == typeof(Int32) || t == typeof(Int64))
        //    {
        //        cell.SetCellValue((double)(value));
        //    }
        //    else if (t == typeof(DateTime))
        //    {
        //        cell.SetCellValue(Convert.ToDateTime(value));
        //    }
        //    else
        //    {
        //        //cell.SetCellValue(Utils.NvStr(value));
        //        cell.SetCellValue(Convert.ToString(value));
        //    }
        //}

        ///// <summary>
        ///// 设置单元格内容及样式
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="col"></param>
        ///// <param name="value"></param>
        ///// <param name="cellStyle"></param>
        //public void SetCellTextStyle(int row, int col, object value, HSSFCellStyle cellStyle)
        //{
        //    SetCellText(row, col, value);
        //    SetCellStyle(row, col, cellStyle);
        //}

        ///// <summary>
        ///// 设置单元格公式
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="colName"></param>
        ///// <param name="formula"></param>
        //public void SetCellFormula(int row, string colName, string formula)
        //{
        //    int col = colName.ToInt();
        //    SetCellFormula(row, col, formula);
        //}

        //public void SetCellFormula(int row, int col, string formula)
        //{
        //    GetCell(row, col).CellFormula = formula;
        //}

        ///// <summary>
        ///// 设置单元格公式及样式
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="col"></param>
        ///// <param name="value"></param>
        ///// <param name="cellStyle"></param>
        //public void SetCellFormulaStyle(int row, int col, string formula, HSSFCellStyle cellStyle)
        //{
        //    SetCellFormula(row, col, formula);
        //    SetCellStyle(row, col, cellStyle);
        //}

        //#endregion

        //#region 取得单元格内容
        ///// <summary>
        ///// 取得单元格内容
        ///// </summary>
        ///// <param name="row">行号，从1开始</param>
        ///// <param name="colName">列序号(A,B,C...)</param>
        ///// <returns></returns>
        //public string GetCellText(int row, string colName)
        //{
        //    int col = colName.ToInt();
        //    return GetCellText(row, col);
        //}

        ///// <summary>
        ///// 取得单元格内容
        ///// </summary>
        ///// <param name="sheet"></param>
        ///// <param name="row">行数，从1开始</param>
        ///// <param name="col">列数，从1开始</param>
        ///// <returns></returns>
        //public string GetCellText(int row, int col)
        //{
        //    //用不惯POI啊。GetRow会返回null
        //    IRow hssfrow = _sheet.GetRow(row - 1);
        //    if (hssfrow == null)
        //        return "";

        //    ICell cell = hssfrow.GetCell(col - 1);
        //    if (cell == null)
        //        return "";

        //    if (cell.CellType == CellType.NUMERIC)
        //    {
        //        if (DateUtil.IsCellDateFormatted(cell))
        //        {
        //            //日期型
        //            return cell.DateCellValue.ToString("yyyy-MM-dd");
        //        }
        //        else
        //            //数字型
        //            return cell.NumericCellValue.ToString();
        //    }
        //    else if (cell.CellType == CellType.STRING)
        //    {
        //        return cell.StringCellValue.Trim();
        //    }
        //    else if (cell.CellType == CellType.BOOLEAN)
        //    {
        //        return cell.BooleanCellValue == true ? "true" : "false";
        //    }
        //    else if (cell.CellType == CellType.FORMULA)
        //    {
        //        return GetCalcValue(row, col).ToString();
        //    }

        //    return "";
        //}

        ///// <summary>
        ///// 获取公式计算后的值
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="col"></param>
        ///// <returns></returns>
        ///// <remarks>适用场景：用npoi生成Excel时，获取单元格经公式计算后的值时使用</remarks>
        //public decimal GetCalcValue(int row, int col)
        //{
        //    if (_evaluator == null)
        //        if (_workbook != null)
        //            _evaluator = new HSSFFormulaEvaluator(_workbook);

        //    if (_evaluator == null)
        //        return 0;

        //    ICell cell = GetCell(row, col);
        //    return (decimal)(_evaluator.Evaluate(cell).NumberValue);
        //}



        //#endregion

        ///// <summary>
        ///// 合并单元格
        ///// </summary>
        ///// <param name="startRow">起始行</param>
        ///// <param name="startCol">起始列</param>
        ///// <param name="endRow">结束行</param>
        ///// <param name="endCol">结束列</param>
        //public void MergedRegion(int startRow, int startCol, int endRow, int endCol)
        //{
        //    _sheet.AddMergedRegion(new CellRangeAddress(startRow - 1, endRow - 1, startCol - 1, endCol - 1));
        //}

        ///// <summary>
        ///// 添加新行
        ///// </summary>
        ///// <returns></returns>
        //public IRow CreateRow()
        //{
        //    return CreateRow(++CurrentRow);
        //}

        ///// <summary>
        ///// 添加新行
        ///// </summary>
        ///// <param name="row"></param>
        ///// <returns></returns>
        //public IRow CreateRow(int row)
        //{

        //    return CreateRow(row, null);
        //}

        ///// <summary>
        ///// 在当前行插入一行（把当前行移到下一行）
        ///// </summary>
        ///// <returns></returns>
        //public void ShiftRow()
        //{
        //    _sheet.ShiftRows(CurrentRow - 1, _sheet.LastRowNum, 1);
        //    IRow row = _sheet.GetRow(CurrentRow - 1);

        //    for (int i = 0; i < ColsCount; i++)
        //    {
        //        row.CreateCell(i);
        //    }
        //}

        ///// <summary>
        ///// 在当前行插入一行,并拷贝指定行的样式到新行上
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="copyRowStyleFrom"></param>
        ///// <returns></returns>
        //public IRow ShiftRow(int row, int copyRowStyleFrom)
        //{
        //    _sheet.ShiftRows(row, _sheet.LastRowNum, 1);
        //    IRow newrow = _sheet.GetRow(row);
        //    IRow hssfSourceRow = _sheet.GetRow(copyRowStyleFrom - 1);
        //    for (int i = 0; i < hssfSourceRow.Cells.Count; i++)
        //    {
        //        newrow.CreateCell(i);
        //    }


        //    for (int i = 1; i < hssfSourceRow.Cells.Count; i++)
        //    {
        //        newrow.GetCell(i).CellStyle = hssfSourceRow.GetCell(i).CellStyle;
        //    }
        //    return newrow;
        //}

        ///// <summary>
        ///// 添加新行，并拷贝指定行的样式到新行上
        ///// </summary>
        ///// <param name="row"></param>
        ///// <param name="copyRowStyleFrom"></param>
        ///// <returns></returns>
        //public IRow CreateRow(int row, int copyRowStyleFrom)
        //{
        //    IRow hssfRow = CreateRow(row);
        //    CurrentRow = row;
        //    IRow hssfSourceRow = _sheet.GetRow(copyRowStyleFrom - 1);
        //    for (int i = 1; i < ColsCount; i++)
        //    {
        //        hssfRow.GetCell(i).CellStyle = hssfSourceRow.GetCell(i).CellStyle;
        //    }

        //    return hssfRow;
        //}

        //public IRow CreateRow(int row, ICellStyle cellstyle)
        //{
        //    IRow hssfRow = _sheet.CreateRow(row - 1);
        //    CurrentRow = row;
        //    for (int i = 0; i < ColsCount; i++)
        //    {
        //        hssfRow.CreateCell(i);
        //        if (cellstyle != null)
        //        {
        //            GetCell(row, i + 1).CellStyle = cellstyle;
        //        }
        //    }

        //    return hssfRow;
        //}

        ///// <summary>
        ///// 设置列宽
        ///// </summary>
        ///// <param name="col"></param>
        ///// <param name="width"></param>
        ///// <remarks>poi设置后的列宽要比给定的width要小0.71，所以适当放大</remarks>
        //public void SetColumnWidth(string col, int width)
        //{
        //    SetColumnWidth(col.ToInt(), width);
        //}
        ///// <summary>
        ///// 设置列宽
        ///// </summary>
        ///// <param name="col"></param>
        ///// <param name="width"></param>
        //public void SetColumnWidth(int col, int width)
        //{
        //    _sheet.SetColumnWidth(col - 1, width * 256);
        //}

        ///// <summary>
        ///// 取得某一列多个行的合计公式
        ///// </summary>
        ///// <param name="rowList">参与合计的行的集合</param>
        ///// <returns></returns>
        //public static string GetTotalFormula(List<int> rowList)
        //{
        //    string ret = "";
        //    for (int i = 0; i < rowList.Count; i++)
        //    {
        //        if (ret.Length > 0)
        //        {
        //            ret += "+";
        //        }
        //        //{0}是列的占位符
        //        ret += "{0}" + rowList[i].ToString();
        //    }

        //    return ret;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="title"></param>
        ///// <param name="titleColNum">行号从1开始</param>
        ///// <returns></returns>
        //public string GetMergeCellText(string title, int titleColNum)
        //{
        //    bool searchEnd = false;
        //    int mergeCellStartRow = 0;
        //    int mergeCellEndRow = 0;
        //    for (int i = _sheet.FirstRowNum; i <= _sheet.LastRowNum; i++)
        //    {
        //        string val = GetCellText(i, titleColNum);

        //        if (searchEnd)
        //        {
        //            if (!string.IsNullOrEmpty(val) && !val.Equals(title))
        //            {
        //                mergeCellEndRow = i - 1;
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            if (val.Equals(title))
        //            {
        //                mergeCellStartRow = i + 1;
        //                searchEnd = true;
        //                continue;
        //            }
        //        }

        //    }
        //    return GetCellTextMultiRow(mergeCellStartRow, mergeCellEndRow, titleColNum + 1);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="title"></param>
        ///// <param name="titleColNum">行号从1开始</param>
        ///// <param name="maxLength"></param>
        ///// <returns></returns>
        //public string GetMergeCellText(string title, int titleColNum, int maxLength)
        //{

        //    return GetSubString(GetMergeCellText(title, titleColNum), maxLength);
        //}

        ///// <summary>
        ///// 读取单列一排的数据
        ///// </summary>
        ///// <param name="fromRow"></param>
        ///// <param name="toRow"></param>
        ///// <param name="colNum"></param>
        ///// <returns></returns>
        //public string GetCellTextMultiRow(int fromRow, int toRow, int colNum)
        //{
        //    string result = "";
        //    for (int i = fromRow; i <= toRow; i++)
        //    {
        //        result += GetCellText(i, colNum);
        //    }
        //    return result;
        //}

        //public string GetCellText(string title, int titleColNum)
        //{
        //    for (int i = _sheet.FirstRowNum; i <= _sheet.LastRowNum; i++)
        //    {
        //        if (GetCellText(i, titleColNum).Equals(title))
        //        {
        //            return GetCellText(i, titleColNum + 1);
        //        }

        //    }

        //    return "";
        //}

        //public string GetCellText(string title, int titleColNum, int maxLength)
        //{
        //    return GetSubString(GetCellText(title, titleColNum), maxLength);
        //}


        //public string GetCellText(int row, int col, int maxLength)
        //{
        //    return GetSubString(GetCellText(row, col), maxLength);
        //}


        //public static string GetSubString(string str, int length)
        //{
        //    string temp = str;
        //    int j = 0;
        //    int k = 0;
        //    for (int i = 0; i < temp.Length; i++)
        //    {
        //        if (Regex.IsMatch(temp.Substring(i, 1), @"[\u4e00-\u9fa5]+"))
        //        {
        //            j += 2;
        //        }
        //        else
        //        {
        //            j += 1;
        //        }
        //        if (j <= length)
        //        {
        //            k += 1;
        //        }
        //        if (j >= length)
        //        {
        //            return temp.Substring(0, k);
        //        }
        //    }
        //    return temp;
        //}
        #endregion


        #region styletemplate

        public ICellStyle TitleStyle(HSSFWorkbook workBook, bool isCenter)
        {
            var cellStyle0 = workBook.CreateCellStyle();
            var cFont0 = workBook.CreateFont();
            cFont0.FontName = "微软雅黑"; //字体名称
            cFont0.IsBold = true;  //字体加粗
            cFont0.FontHeightInPoints = 10; //字号
            cellStyle0.VerticalAlignment = VerticalAlignment.Center; //垂直居中
            if (isCenter)
            {
                cellStyle0.Alignment = HorizontalAlignment.Center;  //水平居中
            }
            else
            {
                cellStyle0.Alignment = HorizontalAlignment.Left; //水平居左
            }

            cellStyle0.SetFont(cFont0);
            cellStyle0.WrapText = true;
            return cellStyle0;
        }

        private ICellStyle HeadStyle(HSSFWorkbook workBook)
        {
            var cellStyle0 = workBook.CreateCellStyle();
            var cFont0 = workBook.CreateFont();
            cFont0.FontName = "微软雅黑"; //字体名称
            cFont0.IsBold = true; //字体加粗
            cFont0.FontHeightInPoints = 10; //字号
            cellStyle0.VerticalAlignment = VerticalAlignment.Center; //垂直居中
            cellStyle0.Alignment = HorizontalAlignment.Left; //水平居左
            cellStyle0.BorderTop = BorderStyle.Thin; //上边框
            cellStyle0.BorderBottom = BorderStyle.Thin; //下边框
            cellStyle0.BorderLeft = BorderStyle.Thin; //左边框
            cellStyle0.BorderRight = BorderStyle.Thin; //右边框
            cellStyle0.SetFont(cFont0);
            return cellStyle0;
        }

        private ICellStyle ContentsStyle(HSSFWorkbook workBook)
        {
            var cellStyle = workBook.CreateCellStyle();
            var cFont = workBook.CreateFont();
            cFont.FontName = "微软雅黑"; //字体名称
            cFont.FontHeightInPoints = 10; //字号
            cellStyle.VerticalAlignment = VerticalAlignment.Center; //垂直居中
            //cellStyle.Alignment = HorizontalAlignment.Left; //水平居左
            cellStyle.BorderTop = BorderStyle.Thin; //上边框
            cellStyle.BorderBottom = BorderStyle.Thin; //下边框
            cellStyle.BorderLeft = BorderStyle.Thin; //左边框
            cellStyle.BorderRight = BorderStyle.Thin; //右边框
            cellStyle.SetFont(cFont);
            cellStyle.WrapText = true;
            return cellStyle;
        }

        #endregion

        /// <summary>
        /// 合并单元格
        /// </summary>
        public void SetMergedRegion(ISheet sheet, int rowNum, int rowTo, int colForm, int colTo)
        {

            for (int i = colForm; i <= colTo; i++)
            {
                sheet.GetRow(rowNum).CreateCell(i);
            }
            //合并单元格
            sheet.AddMergedRegion(new CellRangeAddress(rowNum, rowTo, colForm, colTo));
        }

        /// <summary>
        /// 设置标题内容
        /// </summary>
        public void SetTitle(ISheet sheet, int colNum, string contents)
        {
            var row = sheet.CreateRow(0);
            //sheet1.SetColumnWidth(0, 550);
            SetMergedRegion(sheet, sheet.LastRowNum, sheet.LastRowNum, 0, colNum);
            row.GetCell(0).SetCellValue(contents);

        }

        /// <summary>
        /// 设置标题样式到当前sheet
        /// </summary>
        /// <param name="workBook"></param>
        /// <param name="sheet"> </param>
        /// <param name="isCenter"> </param>
        /// <param name="titleHeight">title的高度 </param>
        public void SetTitleStyle(HSSFWorkbook workBook, ISheet sheet, bool isCenter = false, int titleHeight = 30)
        {

            var cellStyle0 = TitleStyle(workBook, isCenter);
            //title 样式
            sheet.GetRow(0).GetCell(0).CellStyle = cellStyle0;
            sheet.GetRow(0).HeightInPoints = titleHeight;
        }

        /// <summary>
        /// 设置表样式到当前sheet
        /// </summary>
        /// <param name="workBook"></param>
        /// <param name="sheet"></param>

        /// <param name="titleHeight">title的高度 </param>
        public void SetTableStyle(HSSFWorkbook workBook, ISheet sheet, int titleHeight = 20)
        {
            int colNum = sheet.GetRow(1).LastCellNum - 1;
            // head
            var cellStyle0 = HeadStyle(workBook);
            // table head
            for (int i = 0; i <= colNum; i++)
            {
                sheet.GetRow(1).GetCell(i).CellStyle = cellStyle0;
                sheet.GetRow(1).HeightInPoints = titleHeight;
            }

            //---------------------------------------
            //设置表正文样式到当前sheet
            var cellStyle = ContentsStyle(workBook);

            for (int y = 2; y <= sheet.LastRowNum; y++)
            {

                for (int i = 0; i <= colNum; i++)
                {
                    //单元格式设置
                    sheet.GetRow(y).GetCell(i).CellStyle = cellStyle;

                }
            }

        }


        /// <summary>
        /// 设置表foot样式到当前sheet
        /// </summary>
        /// <param name="workBook"></param>
        /// <param name="sheet"></param>
        /// <param name="colNum"></param>
        public void SetFootStyle(HSSFWorkbook workBook, HSSFSheet sheet, int colNum)
        {

        }

        /// <summary>
        ///  自动设置宽度
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="fixLen">最大宽度  =-1的场合 无限制</param>
        public void AutoSetWidth(ISheet sheet, int maxLen = 30)
        {
            int colNum = sheet.GetRow(1).LastCellNum - 1;
            for (int i = 1; i <= colNum; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            //获取当前列的宽度，然后对比本列的长度，取最大值  
            for (int columnNum = 1; columnNum <= colNum; columnNum++)
            {
                int columnWidth = sheet.GetColumnWidth(columnNum) / 256;
                for (int rowNum = 1; rowNum <= sheet.LastRowNum; rowNum++)
                {
                    IRow currentRow;
                    //当前行未被使用过  
                    if (sheet.GetRow(rowNum) == null)
                    {
                        currentRow = sheet.CreateRow(rowNum);
                    }
                    else
                    {
                        currentRow = sheet.GetRow(rowNum);
                    }

                    if (currentRow.GetCell(columnNum) != null)
                    {
                        ICell currentCell = currentRow.GetCell(columnNum);
                        int length = Encoding.Default.GetBytes(currentCell.ToString()).Length + 2;
                        if (columnWidth < length)
                        {
                            if (maxLen < 0)
                            {
                                columnWidth = length;
                            }
                            else
                            {
                                columnWidth = length > maxLen ? maxLen : length;
                            }


                        }
                    }
                    // sheet.GetRow(rowNum).HeightInPoints = 20;
                }
                sheet.SetColumnWidth(columnNum, columnWidth * 256);

            }
        }


        public void SetTable<T>(ISheet sheet, List<T> lists)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            DescriptionAttribute descriptionAttribute = null;

            // SET HEAD
            var row = sheet.CreateRow(1);
            int index = 0;
            foreach (var property in properties)
            {
                descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(property, typeof(DescriptionAttribute));
                row.CreateCell(index).SetCellValue(descriptionAttribute.Description);
                index = index + 1;
            }


            // SET LIST
            for (int i = 0; i < lists.Count; i++)
            {
                row = sheet.CreateRow(i + 2);
                for (int y = 0; y < properties.Count(); y++)
                {
                    var cell = row.CreateCell(y);
                    SetCellValue(cell, properties[y], lists[i]);
                }
            }
        }


        private void SetCellValue<T>(ICell cell, PropertyInfo property, T obj)
        {
            Type type = typeof(T);
            var value = GetValue(property, obj);
            switch (value.GetType().FullName)
            {
                case "System.String"://字符串类型   
                    cell.SetCellValue(value.ToString());
                    break;
                case "System.DateTime"://日期类型   
                    cell.SetCellValue(value.ToString());
                    //cell.CellStyle = dateStyle;//格式化显示   
                    break;
                case "System.Boolean"://布尔型   
                    //bool boolV = false;
                    //bool.TryParse(drValue, out boolV);
                    //newCell.SetCellValue(boolV);
                    break;
                case "System.Int16"://整型   
                case "System.Int32":
                case "System.Int64":
                case "System.Byte":
                    cell.SetCellValue((int)value);
                    break;
                case "System.Decimal"://浮点型   
                case "System.Double":
                    cell.SetCellValue(Convert.ToDouble(value));
                    break;
                case "System.DBNull"://空值处理   
                    cell.SetCellValue("");
                    break;
                default:
                    cell.SetCellValue("");
                    break;
            }
        }

        private object GetValue<T>(PropertyInfo property, T obj)
        {
            if (property.GetValue(obj, null) == null)
            {
                return "";
            }
            else
            {
                return property.GetValue(obj, null);
            }
        }

    }
}
