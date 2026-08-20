using Lvy.Trip.Biz.Finance;
using Lvy.VModels.Excel;
using Lvy.VModels.Finance;
using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.Finance
{
    /// <summary>
    /// 统计
    /// </summary>
    public partial class StatController : BaseController
    {
        /// <summary>
        /// 上下车点统计
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public ActionResult BusPointCount(BusPointCountVModel vModel)
        {
            if (vModel.BeginOutDate.IsNullOrEmpty()) vModel.BeginOutDate = DateTime.Now.ToDateFormat();
            if (vModel.EndOutDate.IsNullOrEmpty()) vModel.EndOutDate = DateTime.Now.ToDateFormat();
            vModel.OwnerCode = UserInfo.OwnerCode;
            vModel.BusPointCountList = new BusPointCountBiz().GetBusPointCountList(vModel) ?? new List<BusPointCountRow>() { };
            if (Request.IsAjaxRequest())
                return PartialView("BusPointCount/UCList", vModel);
            return View("BusPointCount/BusPointCount", vModel);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        //public ActionResult ExportBusPointCount(BusPointCountVModel vModel)
        //{
        //    var orderList = new BusPointCountBiz().GetBusPointCountList(vModel);
        //    if (null == orderList || orderList.Count == 0)
        //        return null;
        //    int rowIndex = 1;
        //    var datas = new List<BusPointCountExcelVModel>();
        //    var busPointIds = orderList.OrderBy(p => p.BusPointModel.JsTime).Select(p => p.BusPointModel.BusPointId).Distinct();
        //    foreach (var id in busPointIds)
        //    {
        //        var orders = orderList.Where(p => p.BusPointModel.BusPointId == id);
        //        var busPoint = orders.FirstOrDefault().BusPointModel;
        //        var jsType = string.Empty;
        //        switch (busPoint.JsType)
        //        {
        //            case 1:
        //                jsType = "接";
        //                break;

        //            case 2:
        //                jsType = "送";
        //                break;

        //            case 3:
        //                jsType = "接/送";
        //                break;

        //            default:
        //                break;
        //        }
        //        datas.Add(new BusPointCountExcelVModel
        //        {
        //            RowIndex = rowIndex++,
        //            BusPointName = busPoint.BusPoint,
        //            JieSongType = jsType,
        //            OrderCount = orders.Count(),
        //            PeopleCount = orders.Sum(p => p.PeopleCount),
        //            JiePrice = busPoint.JiePrice,
        //            SongPrice = busPoint.SongPrice,
        //            JieSongTime = busPoint.JsTime
        //        });
        //    }

        //    using (var ms = new MemoryStream())
        //    {
        //        var workBook = new HSSFWorkbook();
        //        // 新增試算表。
        //        var sheet1 = workBook.CreateSheet("上车点统计");

        //        Arch.Common.Toolkit.Npoi.SetTitle(sheet1, 7, string.Empty);
        //        Arch.Common.Toolkit.Npoi.SetTitleStyle(workBook, sheet1);

        //        Arch.Common.Toolkit.Npoi.SetTable(sheet1, datas);
        //        Arch.Common.Toolkit.Npoi.SetTableStyle(workBook, sheet1);

        //        var row = sheet1.CreateRow(sheet1.LastRowNum + 1);
        //        row.CreateCell(3).SetCellValue((double)datas.Sum(a => a.OrderCount));
        //        row.CreateCell(4).SetCellValue(datas.Sum(a => a.PeopleCount));

        //        // 自适应宽度
        //        Arch.Common.Toolkit.Npoi.AutoSetWidth(sheet1);

        //        workBook.Write(ms);
        //        return File(ms.GetBuffer(), "application/vnd.ms-excel", HttpUtility.UrlEncode("上车点统计.xls"));
        //    }
        //}
    }
}