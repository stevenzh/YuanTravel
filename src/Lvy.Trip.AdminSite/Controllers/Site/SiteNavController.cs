using Lvy.Models.SiteDB;
using Lvy.Trip.Biz.Site;
using Lvy.VModels.Online;
using Lvy.Web.Common;
using Lvy.Web.Common.Mvc.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 网站菜单管理
    /// </summary>
    public class SiteNavController : BaseController
    {
        private SiteNavBiz _biz = new SiteNavBiz();
        private SearchProductBiz productBiz = new SearchProductBiz();

        public ActionResult Index(NavQModel qmodel)
        {
            qmodel.OwnerCode = UserInfo.OwnerCode;
            qmodel.PageList = _biz.NavPageList(qmodel);
            return View("~/Views/Site/SiteNav/Index.cshtml", qmodel);
        }

        public ActionResult PageList(NavQModel qmodel)
        {
            try
            {
                qmodel.OwnerCode = UserInfo.OwnerCode;
                qmodel.PageList = _biz.NavPageList(qmodel);
                //if (Request.IsAjaxRequest())
                //    return View("PageList", qmodel);
                return View("~/Views/Site/SiteNav/PageList.cshtml", qmodel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult EditNav(int id)
        {
            SiteNavModel vm = new SiteNavModel();
            if (id != default(int))
                vm = _biz.GetNavByID(id);
            ViewBag.OutCityEnum = DictionaryTools.GetEnumsBys(Enums.OutCityEnum).ToSelectListFor(a => a.Key, a => a.Value, vm.OutCity);

            return View("~/Views/Site/SiteNav/EditNav.cshtml", vm);
        }


        /// <summary>
        /// 推荐产品列表 简单设置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditList(int id)
        {
            NavItemVModel vm = new NavItemVModel();
            if (id != default(int))
            {
                vm.NavItem = _biz.GetNavItemByID(id);
                vm.NavList = _biz.GetNavList(id);
                var itemList = new List<SelectListItem>();

                if (vm.NavItem.ProductType == "1")  //旅游线路
                {
                    var list = productBiz.GetAllLine(OwnerCode);
                    foreach (var item in list)
                    {
                        var d = new SelectListItem() { Value = item.LineId, Text = item.LineName };
                        if (vm.NavList.Any(t => t.ProductId == item.LineId))
                        {
                            d.Selected = true;
                        }
                        itemList.Add(d);
                    }
                }
                else if (vm.NavItem.ProductType == "3")  // 签证办理
                {
                    var list = productBiz.GetAllVisa();
                    foreach (var item in list)
                    {
                        var d = new SelectListItem() { Value = item.LineId, Text = item.LineName };
                        if (vm.NavList.Any(t => t.ProductId == item.LineId))
                        {
                            d.Selected = true;
                        }
                        itemList.Add(d);
                    }
                }
                else if (vm.NavItem.ProductType == "4")  // 酒店
                {
                    var list = productBiz.GetAllHotel(OwnerCode);
                    foreach (var item in list)
                    {
                        var d = new SelectListItem() { Value = item.LineId, Text = item.LineName };
                        if (vm.NavList.Any(t => t.ProductId == item.LineId))
                        {
                            d.Selected = true;
                        }
                        itemList.Add(d);
                    }
                }
                else if (vm.NavItem.ProductType == "9")  // 其他产品
                {
                    var list = productBiz.GetAllTicket(OwnerCode);
                    foreach (var item in list)
                    {
                        var d = new SelectListItem() { Value = item.LineId, Text = item.LineName };
                        if (vm.NavList.Any(t => t.ProductId == item.LineId))
                        {
                            d.Selected = true;
                        }
                        itemList.Add(d);
                    }
                }

                ViewData["NavList"] = itemList;
            }

            return View("~/Views/Site/SiteNav/EditList.cshtml", vm);
        }

        public ActionResult SaveNavList(int ItemID, string[] ListStr)
        {
            _biz.SaveNavList(ItemID, ListStr);
            return Json(new { code = "" });
        }

        public ActionResult SaveNav(SiteNavModel model)
        {
            model.OwnerCode = UserInfo.OwnerCode;
            _biz.SaveNav(model);
            return Content("1");
        }

        public ActionResult Search()
        {
            ViewBag.NavList = _biz.GetAllNavs().ToSelectListFor(t => t.Code, t => t.Name);
            return View("~/Views/Site/SiteNav/Search.cshtml");
        }

        public ActionResult GetJsTreeData(string navCode)
        {
            string parentID = "0";
            string treeJson = "";
            var list = _biz.GetAllNavItems(navCode);
            if (list != null && list.Count() > 0)
            {
                treeJson = listToJson(list, "ParentId", parentID, "Id", "Name", "isParent").Substring(12);
            }
            else
            {
                treeJson = "[{ \"text\": \"无数据\"}]";
            }
            return Content(treeJson);
        }

        public static string listToJson(List<SiteNavItemModel> list, string pField, string pValue, string kField, string TextField, string isParent, int level = 0)
        {
            StringBuilder sb = new StringBuilder();
            var tempModels = list.Where(a => a.ParentID == pValue.ToInt()).ToList();

            if (tempModels == null || tempModels.Count() < 1)
                return "";
            sb.Append(",\"children\":[");
            foreach (var item in tempModels)
            {
                string pcv = item.ItemID.ToString();
                sb.Append("{");
                sb.AppendFormat("\"text\":\"{0}\",", item.Name);
                sb.Append("\"data\":{");
                sb.AppendFormat("\"id\":\"{0}\",\"code\":\"{1}\",\"level\":\"{2}\",\"isleaf\":\"{3}\",\"outcity\":\"{4}\",\"region\":\"{5}\",\"sortorder\":\"{6}\",\"linkurl\":\"{7}\",\"isvalid\":\"{8}\" ", item.ItemID, item.Code, item.Level, item.IsLeaf, item.OutCityName, item.Region, item.SortOrder, item.LinkUrl, (item.IsValid ? "有效" : "无效"));
                sb.Append("}");
                sb.Append(listToJson(list, pField, pcv, kField, TextField, isParent, 0).TrimEnd(','));
                sb.Append("},");
            }
            if (sb.ToString().EndsWith(","))
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.Append("]");

            return sb.ToString();
        }

        /// <summary>
        /// 修改节点 添加节点 添加子节点
        /// </summary>
        /// <param name="id">修改节点</param>
        /// <param name="navCode">菜单项 就是大类</param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public ActionResult EditNavItem(int id, string navCode, int pid)
        {
            SiteNavItemModel vm = new SiteNavItemModel();
            vm.NavCode = navCode;
            if (pid != default(int))
            {
                vm.ParentID = pid;
                vm.ParentNode = _biz.GetNavItemByID(pid);
                vm.ParentName = vm.ParentNode.Name;
            }
            if (id != default(int))
                vm = _biz.GetNavItemByID(id);

            ViewBag.OutCityEnum = DictionaryTools.GetEnumsBys(Enums.OutCityEnum).ToSelectListFor(a => a.Key, a => a.Value, vm.OutCity);
            ViewBag.ProductType = DictionaryTools.GetEnumsBys(Enums.ProductTypeEnum).ToSelectListFor(a => a.Key, a => a.Value, vm.ProductType);
            return View("~/Views/Site/SiteNav/EditNavItem.cshtml", vm);
        }

        public ActionResult SaveNavItem(SiteNavItemModel model)
        {
            _biz.SaveNavItem(model);
            return Content("1");
        }

        /// <summary>
        /// 设置有效无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetValidStateById(int id)
        {
            _biz.SetValidStateByDest(id);
            return Content("1");
        }
    }
}