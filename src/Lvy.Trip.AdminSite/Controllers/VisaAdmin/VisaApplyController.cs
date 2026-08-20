using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 团签证费用申请
    /// </summary>
    public class VisaApplyController : Controller
    {
        // GET: VisaApply
        public ActionResult Index()
        {
            return View();
        }

        // 在 TourCost 查出做过签证 成本的列表

        // Add  查询已成团的计划，列出名单 显示 签证是否申请过

        // 保存 到 TourCost 新的成本记录， 更新名单签证状态

        // 取消不好做 名单不好对应
    }
}