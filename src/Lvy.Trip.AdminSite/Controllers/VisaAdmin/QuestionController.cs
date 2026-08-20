using Common.Logging;
using Lvy.Trip.AdminSite.Controllers;
using Lvy.Trip.Biz.Base;
using Lvy.Visa.Biz;
using Lvy.Visa.Models;
using Lvy.Visa.VModels;
using System;
using System.Web.Mvc;

namespace Lvy.Visa.AdminSite.Controllers
{
    /// <summary>
    /// 签证常见问题
    /// </summary>
    public class QuestionController : BaseController
    {
        private QuestionBiz _biz = new QuestionBiz();
        private DestinationBiz _destBiz = new DestinationBiz();
        private ILog _logger = LogManager.GetLogger(typeof(QuestionController));

        // GET: Question
        public ActionResult Index(VisaCountryQuestionQModel qmodel)
        {
            qmodel.OwnerCode = UserInfo.OwnerCode;
            qmodel.QuetionList = _biz.PagSearchQuestionList(qmodel);
            if (Request.IsAjaxRequest())
                return View("~/Views/Visa/Question/PageList.cshtml", qmodel);
            return View("~/Views/Visa/Question/Index.cshtml", qmodel);
        }

        /// <summary>
        /// 添加，编辑问题初始化
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult GetQuestion(string id)
        {
            var result = new { code = 0, entity = new VisaCountryQuestionModel() };
            var quetion = _biz.SearchQuestionDetail(id);
            if (quetion != null)
            {
                result = new { code = 1, entity = quetion };
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存问题
        /// </summary>
        /// <param name="model"></param>
        public int SaveQuestion(VisaCountryQuestionModel model)
        {
            try
            {
                //验证
                if (ValidateCountryData(model.CountryCode, model.CountryName) != 1)
                {
                    return 0;
                }
                if (!string.IsNullOrEmpty(model.QuestionCode))
                {
                    model.ModifyBy = UserInfo.Code;
                    _biz.SaveQuestion(model);
                }
                else
                {
                    model.OwnerCode = UserInfo.OwnerCode;
                    model.CreateBy = UserInfo.Code;

                    _biz.AddQuestion(model);
                }

                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 验证国家是否匹配存在
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public int ValidateCountryData(string code, string name)
        {
            try
            {
                return _destBiz.ValidateCountryData(code, name);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                return -1;
            }
        }

        /// <summary>
        /// 删除问题
        /// </summary>
        /// <param name="model"></param>
        public void Deletequestion(VisaCountryQuestionModel model)
        {
            try
            {
                _biz.Deletequestion(model);
            }
            catch (Exception ex)
            {
                _logger.Error("", ex);
                throw ex;
            }
        }
    }
}