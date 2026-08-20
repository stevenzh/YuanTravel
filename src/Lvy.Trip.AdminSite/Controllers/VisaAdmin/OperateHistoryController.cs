using Common.Logging;
using Lvy.Trip.AdminSite.Controllers;
using Lvy.Visa.Biz;
using Lvy.Visa.VModels;
using System;
using System.Web.Mvc;

namespace Lvy.Visa.AdminSite.Controllers
{
    public class OperateHistoryController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(OperateHistoryController));

        private ProductBiz _biz = new ProductBiz();


        /// <summary>
        /// 获取产品操作历史列表
        /// </summary>
        /// <param name="formValues"></param>
        /// <returns></returns>
        public ActionResult SearchInforOperateHistorys(YlInformationOperateHistoryQModel formValues)
        {
            try
            {
                var model = new YlInformationOperateHistoryQModel()
                {
                    InformationCode = formValues.InformationCode,
                    OperateHistoryModels = _biz.SearchInforOperateHistorys(formValues)
                };
                return View("~/Views/Visa/OperateHistory/SearchInforOperateHistorys.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return RedirectToAction("Error", "Error");
            }
        }

    }
}