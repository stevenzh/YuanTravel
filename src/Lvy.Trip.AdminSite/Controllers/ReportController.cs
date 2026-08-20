using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using NPOI.HSSF.UserModel;
using Lvy.Models;
using Lvy.Trip.Biz;
using Lvy.Web.Common;
using Lvy.Trip.AdminSite.Mvc.Attributes;
using Lvy.Web.Common.Mvc.HtmlHelpers;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    ///  报表模块
    /// </summary>
    public class ReportController : BaseController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [LvyAuth]
        public ActionResult Reports()
        {
            var kvs = new List<KeyValueBean>();

            int Month = DateTime.Now.Month;
            for (int i = 0; i < 12; i++)
            {
                var dt = DateTime.Now.AddMonths(-i);
                kvs.Add(new KeyValueBean()
                {
                    Key = "{0}-{1}".With(dt.Year, dt.Month),
                    Value = "{0}年{1}月".With(dt.Year, dt.Month)
                });
            }

            ViewBag.MonthBeans = kvs.ToSelectListFor();
            return View();
        }

        /// <summary>
        /// 账龄分析
        /// </summary>
        /// <returns></returns>
        public ActionResult ZhangLing(string month)
        {
            var datas = ReportsBiz.GetZhangLing(month, OwnerCode);

            using (var ms = new MemoryStream())
            {
                string fileName = "{0} 账龄分析".With(month);
                var workBook = new HSSFWorkbook();
                // 新增試算表。
                var sheet1 = workBook.CreateSheet(fileName);

                Arch.Common.Toolkit.Npoi.SetTitle(sheet1, 9, fileName);
                Arch.Common.Toolkit.Npoi.SetTitleStyle(workBook, sheet1, true);

                Arch.Common.Toolkit.Npoi.SetTable(sheet1, datas);

                for (int i = 2; i <= sheet1.LastRowNum; i++)
                {
                    sheet1.GetRow(i).CreateCell(9).SetCellFormula(datas[0].Sum1.With(i + 1));
                }

                Arch.Common.Toolkit.Npoi.SetTableStyle(workBook, sheet1);

                var row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                int y = 1;
                for (int i = 66; i <= 74; i++)
                {
                    System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                    byte[] byteArray = new byte[] { (byte)i };
                    string strCharacter = asciiEncoding.GetString(byteArray);

                    row.CreateCell(y).SetCellFormula("SUM({0}3:{0}{1})".With(strCharacter, sheet1.LastRowNum));
                    y++;
                }


                // 自适应宽度
                Arch.Common.Toolkit.Npoi.AutoSetWidth(sheet1, -1);

                sheet1.SetColumnWidth(0, 40 * 256);

                workBook.Write(ms);
                return File(ms.GetBuffer(), "application/vnd.ms-excel", HttpUtility.UrlEncode("账龄分析.xls"));
            }
        }

        /// <summary>
        ///  平台操作费明细
        /// </summary>
        /// <returns></returns>
        public ActionResult ShoukeInfo1(string sDate, string eDate)
        {
            var datas = ReportsBiz.GetShoukeInfo1(sDate, eDate, OwnerCode);

            using (var ms = new MemoryStream())
            {
                string fileName = "{0} - {1}平台操作费明细".With(sDate, eDate);
                var workBook = new HSSFWorkbook();
                // 新增試算表。
                var sheet1 = workBook.CreateSheet(fileName);

                Arch.Common.Toolkit.Npoi.SetTitle(sheet1, 6, fileName);
                Arch.Common.Toolkit.Npoi.SetTitleStyle(workBook, sheet1, true);
                Arch.Common.Toolkit.Npoi.SetTable(sheet1, datas);
                Arch.Common.Toolkit.Npoi.SetTableStyle(workBook, sheet1);


                var row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                int y = 1;
                for (int i = 66; i <= 71; i++)
                {
                    System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                    byte[] byteArray = new byte[] { (byte)i };
                    string strCharacter = asciiEncoding.GetString(byteArray);

                    row.CreateCell(y).SetCellFormula("SUM({0}3:{0}{1})".With(strCharacter, sheet1.LastRowNum));
                    y++;
                }


                // 自适应宽度
                Arch.Common.Toolkit.Npoi.AutoSetWidth(sheet1, -1);

                sheet1.SetColumnWidth(0, 40 * 256);

                workBook.Write(ms);
                return File(ms.GetBuffer(), "application/vnd.ms-excel", HttpUtility.UrlEncode("平台操作费明细.xls"));
            }
        }

        /// <summary>
        ///  供应商 收客情况
        /// </summary>
        /// <returns></returns>
        public ActionResult ShoukeInfo2(string sDate, string eDate, string customerCode)
        {
            var datas = ReportsBiz.GetShoukeInfo2(sDate, eDate, customerCode, OwnerCode);

            using (var ms = new MemoryStream())
            {
                string cusName = DictionaryTools.GetCachedCustomer(customerCode).Name;
                string fileName = "{0}-{1} {2}供应商收客情况表".With(sDate, eDate, cusName);
                var workBook = new HSSFWorkbook();
                // 新增試算表。
                var sheet1 = workBook.CreateSheet(fileName);

                Arch.Common.Toolkit.Npoi.SetTitle(sheet1, 7, fileName);
                Arch.Common.Toolkit.Npoi.SetTitleStyle(workBook, sheet1, true);
                Arch.Common.Toolkit.Npoi.SetTable(sheet1, datas);
                Arch.Common.Toolkit.Npoi.SetTableStyle(workBook, sheet1);

                var row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                int y = 1;
                for (int i = 66; i <= 72; i++)
                {
                    System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                    byte[] byteArray = new byte[] { (byte)i };
                    string strCharacter = asciiEncoding.GetString(byteArray);

                    row.CreateCell(y).SetCellFormula("SUM({0}3:{0}{1})".With(strCharacter, sheet1.LastRowNum));
                    y++;
                }


                // 自适应宽度
                Arch.Common.Toolkit.Npoi.AutoSetWidth(sheet1, -1);

                sheet1.SetColumnWidth(0, 40 * 256);


                workBook.Write(ms);
                return File(ms.GetBuffer(), "application/vnd.ms-excel", HttpUtility.UrlEncode("供应商收客情况表.xls"));
            }
        }


        /// <summary>
        /// 分销商收客情况
        /// </summary>
        /// <returns></returns>
        public ActionResult ShoukeInfo3(string sDate, string eDate)
        {
            var datas = ReportsBiz.GetShoukeInfo2(sDate, eDate, "", OwnerCode);

            using (var ms = new MemoryStream())
            {

                string fileName = "{0}-{1}分销商收客情况表".With(sDate, eDate);
                var workBook = new HSSFWorkbook();
                // 新增試算表。
                var sheet1 = workBook.CreateSheet(fileName);

                Arch.Common.Toolkit.Npoi.SetTitle(sheet1, 7, fileName);
                Arch.Common.Toolkit.Npoi.SetTitleStyle(workBook, sheet1, true);
                Arch.Common.Toolkit.Npoi.SetTable(sheet1, datas);
                Arch.Common.Toolkit.Npoi.SetTableStyle(workBook, sheet1);

                var row = sheet1.CreateRow(sheet1.LastRowNum + 1);
                int y = 1;
                for (int i = 66; i <= 72; i++)
                {
                    System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                    byte[] byteArray = new byte[] { (byte)i };
                    string strCharacter = asciiEncoding.GetString(byteArray);

                    row.CreateCell(y).SetCellFormula("SUM({0}3:{0}{1})".With(strCharacter, sheet1.LastRowNum));
                    y++;
                }


                // 自适应宽度
                Arch.Common.Toolkit.Npoi.AutoSetWidth(sheet1, -1);

                sheet1.SetColumnWidth(0, 40 * 256);

                workBook.Write(ms);
                return File(ms.GetBuffer(), "application/vnd.ms-excel", HttpUtility.UrlEncode("分销商收客情况表.xls"));
            }
        }


        /// <summary>
        ///  每日订单量
        /// </summary>
        /// <param name="sDate1"></param>
        /// <param name="eDate1"></param>
        /// <returns></returns>
        public ActionResult OrderReportByDate(string sDate1, string eDate1)
        {
            var datas = ReportsBiz.GetOrderReportByDate(sDate1, eDate1, UserInfo);

            using (var ms = new MemoryStream())
            {

                string fileName = "{0}-{1}日收客量".With(sDate1, eDate1);
                var workBook = new HSSFWorkbook();
                // 新增試算表。
                var sheet1 = workBook.CreateSheet(fileName);

                Arch.Common.Toolkit.Npoi.SetTitle(sheet1, 6, fileName);
                Arch.Common.Toolkit.Npoi.SetTitleStyle(workBook, sheet1, true);
                Arch.Common.Toolkit.Npoi.SetTable(sheet1, datas);
                Arch.Common.Toolkit.Npoi.SetTableStyle(workBook, sheet1);

                // 自适应宽度
                Arch.Common.Toolkit.Npoi.AutoSetWidth(sheet1, -1);


                workBook.Write(ms);
                return File(ms.GetBuffer(), "application/vnd.ms-excel",
                            HttpUtility.UrlEncode("日收客量.xls"));
            }

        }

    }
}
