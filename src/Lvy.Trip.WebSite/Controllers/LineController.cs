using Lvy.Trip.Biz.Booking;
using Lvy.Trip.Biz.Product;
using Lvy.VModels.Booking;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Lvy.Trip.WebSite.Controllers
{
    /// <summary>
    /// 线路详情
    /// </summary>
    public partial class LineController : BaseController
    {
        private readonly BookingBiz _bookingBiz = new BookingBiz();
        private readonly TpLineTourPlanBiz planBiz = new TpLineTourPlanBiz();

        /// <summary>
        /// 线路详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult Details(string lineId, int tourId = 0)
        {
            var vModel = _bookingBiz.GetLineRoute(lineId, tourId);

            MatchPlace(ref vModel);
            return View(vModel);
        }

        /// <summary>
        /// 匹配景点 加上链接
        /// </summary>
        /// <param name="vModel"></param>
        public void MatchPlace(ref RouteVModel vModel)
        {
            foreach (var route in vModel.TpLineRoutes)
            {
                if (!string.IsNullOrEmpty(route.Contents))
                {
                    route.Contents = MatchRegex(route.Contents);
                }
            }
        }

        public string MatchRegex(string contents)
        {
            string pattern = @"\【.*?\】"; //正则表达式字符串
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.Replace(contents, new MatchEvaluator(ReplaceTo));
        }

        private string ReplaceTo(Match match)
        {
            string f = "<a class=\"trigger\" name=\"trigger\" href=\"javascript:;\"  rel=\"\">{0}</a>";

            return string.Format(f, match);
        }

        /// <summary>
        /// WAP日历使用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetCalendar(string id)
        {
            var plans = planBiz.GetByLineId(id, true);
            var rr = (from ss in plans
                      select new
                      {
                          title = ss.Price.ToString("￥00") + "\n余位:" + ">9",
                          //title = ss.Price.ToString("￥00") + "\n余位:" + (ss.PAX3 > 9 ? ">9" : ss.PAX3.ToString()),
                          start = ss.OutDate.ToDateFormat(),
                          // backgroundColor = (ss.PAX3 > 0) ? "#66cc99" : "#FF6666"
                          backgroundColor = "#66cc99"
                      }).ToList();

            //{
            //  title: 'Click for Google',
            //  start: new Date(y, m, 28),
            //  end: new Date(y, m, 29),
            //  url: 'http://google.com/',
            //  backgroundColor: '#3c8dbc', //Primary (light-blue)
            //  borderColor: '#3c8dbc' //Primary (light-blue)
            //}
            return Json(rr, JsonRequestBehavior.AllowGet);
        }
    }
}