using Lvy.Models.TicketDB;
using Lvy.Trip.Biz.Ticket;
using Lvy.VModels;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers.TktAdmin
{
    /// <summary>
    /// 通用产品分类管理
    /// </summary>
    public class TktCategoryController : Controller
    {
        private readonly TktProductBiz _biz = new TktProductBiz();

        // GET: TktCategory
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetJsTreeData(string navCode)
        {
            string parentID = "0";
            string treeJson = "";
            List<TktCategoryModel> list = _biz.GetCategoryByType(navCode);
            if (list != null && list.Count() > 0)
            {
                treeJson = listToJson(list, parentID, "Id", "Name").Substring(12);
            }
            else
            {
                treeJson = "[{ \"text\": \"无数据\"}]";
            }
            return Content(treeJson);
        }

        public static string listToJson(List<TktCategoryModel> list, string pValue, string kField, string TextField)
        {
            StringBuilder sb = new StringBuilder();
            var tempModels = list.Where(a => a.ParentID == pValue.ToInt()).ToList();

            if (tempModels == null || tempModels.Count() < 1)
                return "";
            sb.Append(",\"children\":[");
            foreach (var item in tempModels)
            {
                string pcv = item.ID.ToString();
                sb.Append("{");
                sb.AppendFormat("\"text\":\"{0}\",", item.Name);
                sb.Append("\"data\":{");
                sb.AppendFormat("\"id\":\"{0}\",\"code\":\"{1}\",\"level\":\"{2}\",\"isleaf\":\"{3}\",\"sortorder\":\"{4}\",\"isvalid\":\"{5}\" ", item.ID, item.Code, item.Level, item.IsLeaf, item.SortOrder, (item.IsValid ? "有效" : "无效"));
                sb.Append("}");
                sb.Append(listToJson(list, pcv, kField, TextField).TrimEnd(','));
                sb.Append("},");
            }
            if (sb.ToString().EndsWith(","))
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.Append("]");

            return sb.ToString();
        }

        public ActionResult GetJsTreeData2(string productType)
        {
            string parentID = "0";
            string treeJson = "";
            List<TktCategoryModel> list = _biz.GetCategoryByType(productType);
            if (list != null && list.Count() > 0)
            {
                treeJson = listToJson2(list, parentID).Substring(8);
                return Content("{\"Code\":\"1\", \"data\":" + treeJson + "}");
            }
            else
            {
                return Content("{\"Code\":\"0\", \"Message\":\"列表为空\"}");
            }
        }

        public static string listToJson2(List<TktCategoryModel> list, string pValue)
        {
            StringBuilder sb = new StringBuilder();
            var tempModels = list.Where(a => a.ParentID == pValue.ToInt()).ToList();

            if (tempModels == null || tempModels.Count() < 1)
                return "";
            sb.Append(",\"subs\":[");  // 首个要掐掉
            foreach (var item in tempModels)
            {
                string pcv = item.ID.ToString();
                sb.Append("{");
                sb.AppendFormat("\"id\":\"{0}\",\"title\":\"{1}\" ", item.ID, item.Name);
                sb.Append(listToJson2(list, pcv).TrimEnd(','));
                sb.Append("},");
            }
            if (sb.ToString().EndsWith(","))
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.Append("]");

            return sb.ToString();
        }

        public ActionResult EditCategory(int id, string navCode, int pid)
        {
            TktCategoryModel vm = new TktCategoryModel();
            vm.ProductType = navCode;
            if (pid != default(int))  // 新增
            {
                vm.ParentID = pid;
                vm.ParentNode = _biz.GetCategoryByID(pid);
                vm.ParentName = vm.ParentNode.Name;
            }
            if (id != default(int))
                vm = _biz.GetCategoryByID(id);

            ViewBag.ProductType = DictionaryTools.GetEnumsBys(Enums.ProductAllTypeEnum).ToSelectListFor(a => a.Key, a => a.Value, vm.ProductType);
            return View(vm);
        }

        public ActionResult SaveNavItem(TktCategoryModel model)
        {
            _biz.SaveCategory(model);
            return Json(new CommonJsonResult { Code = "1", Message = "Success" });
        }
    }
}