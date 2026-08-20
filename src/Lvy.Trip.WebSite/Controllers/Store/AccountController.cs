using Arch.Common;
using Common.Logging;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Trip.WebSite.Mvc.Attributes;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 账户管理
    /// </summary>
    [LvyAuth]
    public class AccountController : BaseController
    {
        private ILog _logger = LogManager.GetLogger(typeof(AccountController));
        private readonly AccountBiz _biz = new AccountBiz();

        /// <summary>
        /// 账户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult BaseInfo()
        {
            return View();
        }

        public ActionResult UpdateBaseInfo(CrmAccountModel model)
        {
            var entity = _biz.CheckContactEmail(OwnerCode, model.Email, model.Code);
            if (entity != null)
                return Json(new { Code = "2", Message = "邮箱重复" });
            int row = _biz.UpdateFromSite(model);
            if (row > 0)
                return Json(new { Code = "1", Message = "" });
            return Json(new { Code = "0", Message = "没有更新" });
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ModifyPwd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ModifyPwd(string accountCode, string pwd, string newPwd)
        {

            string _pwd = Toolkit.Security.ToEncrypt(pwd);
            var entity = _biz.GetById(accountCode);
            if (_pwd != entity.Pwd)
            {
                return Json(new { Code = "0", Message = "原始密码错误" });
            }
            else
            {
                int row = _biz.ResetPwd(accountCode, newPwd);
                if (row > 0)
                    return Json(new { Code = "1", Message = "修改成功" });
            }

            return Json(new { Code = "0", Message = "未知原因" });
        }
    }
}