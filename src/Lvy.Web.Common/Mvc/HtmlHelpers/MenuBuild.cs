using Lvy.Models.SiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Lvy.Web.Common.Mvc.HtmlHelpers
{
    public static partial class HtmlHelperEx
    {

        /// <summary>
        /// Bootstrap 结构菜单创建（AdminLTE）
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static MvcHtmlString BootMenuBuild(this HtmlHelper htmlHelper)
        {

            if (GlobalContext.Current.UserInfo == null)
            {
                HttpContext.Current.Response.Redirect("/User/Login?url=" + HttpContext.Current.Request.Url);
            }

            StringBuilder sb = new StringBuilder();
            var funcs = GlobalContext.Current.FunctionList;
            var moduleMenus = funcs.Where(a => a.FuncType == 1).OrderBy(a => a.Sort);
            var moduleCount = moduleMenus.Count();
            if (moduleCount > 0)
            {
                foreach (var moduleMenu in moduleMenus)
                {
                    var parentId = moduleMenu.Id;
                    var menus = funcs.Where(a => a.ParentId == parentId && a.FuncType == 2).OrderBy(a => a.Sort);
                    //如果没有子菜单的情况
                    if (menus.Count() > 0)
                    {
                        sb.Append("<li class=\"nav-item\">");
                        sb.Append("<a href=\"#\" class=\"nav-link\">");
                        if (moduleMenu.IconClass.IsNullOrEmpty())
                        {
                            sb.AppendFormat("<i class=\"nav-icon fas fa-folder\"></i><p>{0} <i class=\"right fas fa-angle-left\"></i></p>", moduleMenu.Name);
                        }
                        else
                        {
                            sb.AppendFormat("<i class=\"nav-icon {1}\"></i><p>{0} <i class=\"right fas fa-angle-left\"></i></p>", moduleMenu.Name, moduleMenu.IconClass);
                        }


                        sb.Append("</a>");
                        sb.Append("<ul class=\"nav nav-treeview\">");

                        foreach (var menu in menus)
                        {
                            // 与两个模块是上海新康辉独有 <外部收款><单团核算复制>
                            if ((menu.Id == 49 || menu.Id == 58) && GlobalContext.Current.OwnerCode != "1611000001")
                            {
                                continue;
                            }

                            if (menu.IconClass.IsNullOrEmpty())
                            {
                                sb.Append("<li class=\"nav-item\"><a href=\"{0}\" class=\"nav-link\"><i class=\"far fa-circle nav-icon\"></i> <p>{1}</p></a></li>".With(menu.URL, menu.Name));
                            }
                            else
                            {
                                sb.Append("<li class=\"nav-item\"><a href=\"{0}\" class=\"nav-link\"><i class=\"{1} nav-icon\"></i> <p>{2}</p></a></li>".With(menu.URL, menu.IconClass, menu.Name));
                            }
                        }
                        sb.Append("</ul>");
                        sb.Append("</li>");
                    }
                    else
                    {
                        sb.AppendFormat("<li class=\"nav-item\"><a href=\"#\" class=\"nav-link\"><i class=\"nav-icon far fa-circle \"></i> <p>{0}</p></a></li>", moduleMenu.Name);
                    }
                }
            }
            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString FrontMenuBuild(this HtmlHelper htmlHelper, List<SiteNavItemModel> funcs)
        {
            StringBuilder sb = new StringBuilder();
            var moduleMenus = funcs.Where(a => a.Level == 1).OrderBy(a => a.SortOrder);
            var moduleCount = moduleMenus.Count();
            if (moduleCount > 0)
            {
                foreach (var moduleMenu in moduleMenus)
                {
                    var parentId = moduleMenu.ItemID;
                    var menus = funcs.Where(a => a.ParentID == parentId && a.Level == 2).OrderBy(a => a.SortOrder);
                    //如果没有子菜单的情况
                    if (menus.Count() > 0)
                    {
                        sb.Append("<li class=\"hassubs\">");
                        sb.AppendFormat("<a href=\"{0}\">", moduleMenu.LinkUrl);
                        sb.AppendFormat("{0}<i class=\"fas fa-chevron-right\"></i>", moduleMenu.Name);
                        sb.Append("</a>");
                        sb.Append("<ul>");
                        foreach (var menu in menus)
                        {
                            sb.Append("<li><a href=\"{0}\">{1}<i class=\"fas fa-chevron-right\"></i></a></li>".With(menu.LinkUrl, menu.Name));
                        }
                        sb.Append("</ul>");
                        sb.Append("</li>");
                    }
                    else
                    {
                        sb.AppendFormat("<li><a href=\"{0}\">{1}<i class=\"fas fa-chevron-right\"></i></a></li>", moduleMenu.LinkUrl, moduleMenu.Name);
                    }
                }
            }
            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString HidenMenuBuild(this HtmlHelper htmlHelper, List<SiteNavItemModel> funcs)
        {
            StringBuilder sb = new StringBuilder();
            var moduleMenus = funcs.Where(a => a.Level == 1).OrderBy(a => a.SortOrder);
            var moduleCount = moduleMenus.Count();
            if (moduleCount > 0)
            {
                foreach (var moduleMenu in moduleMenus)
                {
                    var parentId = moduleMenu.ItemID;
                    var menus = funcs.Where(a => a.ParentID == parentId && a.Level == 2).OrderBy(a => a.SortOrder);
                    //如果没有子菜单的情况
                    if (menus.Count() > 0)
                    {
                        sb.Append("<li class=\"page_menu_item has-children\">");
                        sb.AppendFormat("<a href=\"{0}\">", moduleMenu.LinkUrl);
                        sb.AppendFormat("{0}<i class=\"fa fa-angle-down\"></i>", moduleMenu.Name);
                        sb.Append("</a>");
                        sb.Append("<ul class=\"page_menu_selection\">");
                        foreach (var menu in menus)
                        {
                            sb.Append("<li><a href=\"{0}\">{1}<i class=\"fa fa-angle-down\"></i></a></li>".With(menu.LinkUrl, menu.Name));
                        }
                        sb.Append("</ul>");
                        sb.Append("</li>");
                    }
                    else
                    {
                        sb.AppendFormat("<li class=\"page_menu_item\"><a href=\"{0}\">{1}<i class=\"fa fa-angle-down\"></i></a></li>", moduleMenu.LinkUrl, moduleMenu.Name);
                    }
                }
            }
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// 创建菜单 H+ UI
        /// </summary>
        /// <returns></returns>
        public static MvcHtmlString CreateMenu()
        {
            if (GlobalContext.Current.UserInfo == null)
            {
                HttpContext.Current.Response.Redirect("/User/Login");//?url=" + HttpContext.Current.Request.Url
            }
            StringBuilder sb = new StringBuilder();
            var funcs = GlobalContext.Current.FunctionList;
            var moduleMenus = funcs.Where(a => a.FuncType == 1).OrderBy(a => a.Sort);
            var moduleCount = moduleMenus.Count();
            if (moduleCount > 0)
            {
                foreach (var moduleMenu in moduleMenus)
                {
                    var parentId = moduleMenu.Id;
                    var menus = funcs.Where(a => a.ParentId == parentId && a.FuncType == 2).OrderBy(a => a.Sort);
                    //如果没有子菜单的情况
                    if (menus.Count() > 0)
                    {
                        sb.Append("<li>");
                        sb.Append(" <a  href=\"#\"> ");
                        if (moduleMenu.IconClass.IsNullOrEmpty())
                        {
                            sb.Append(" <i class=\"fa fa-folder\"></i> ");
                        }
                        else
                        {
                            sb.AppendFormat(" <i class=\"fa {0}\"></i> ", moduleMenu.IconClass);
                        }

                        sb.AppendFormat("<span class=\"nav-label\">{0}</span> <span class=\"fa arrow\"></span>", moduleMenu.Name);
                        sb.Append("</a> ");
                        sb.Append(" <ul class=\"nav nav-second-level\">  ");
                        foreach (var menu in menus)
                        {
                            if (menu.IconClass.IsNullOrEmpty())
                            {
                                sb.Append("<li> <a class=\"J_menuItem\" href=\"{0}\" ><i class=\"fa fa-circle-o\"></i>{1}</a></li>".With(menu.URL, menu.Name));
                            }
                            else
                            {
                                sb.Append("<li> <a class=\"J_menuItem\" href=\"{0}\" ><i class=\"fa {2}\"></i>{1}</a></li>".With(menu.URL, menu.Name, menu.IconClass));
                            }
                        }
                        sb.Append(" </ul>");
                        sb.Append("</li>");
                    }
                    else
                    {
                        // 没有子菜单就没有父菜单
                        //sb.AppendFormat("<li><a href=\"#\"><i class=\"fa fa-circle-o text-warning\"></i> {0}</a></li>", moduleMenu.Name);
                    }
                }
            }

            return MvcHtmlString.Create(sb.ToString());

        }

        /// <summary>
        /// 原始菜单创建
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static MvcHtmlString MenuBuild(this HtmlHelper htmlHelper)
        {

            if (GlobalContext.Current.UserInfo == null)
            {
                HttpContext.Current.Response.Redirect("/User/Login?url=" + HttpContext.Current.Request.Url);
            }

            StringBuilder sb = new StringBuilder();
            var funcs = GlobalContext.Current.FunctionList;
            var moduleMenus = funcs.Where(a => a.FuncType == 1).OrderBy(a => a.Sort);
            var moduleCount = moduleMenus.Count();
            if (moduleCount > 0)
            {
                foreach (var moduleMenu in moduleMenus)
                {
                    var parentId = moduleMenu.Id;
                    var menus = funcs.Where(a => a.ParentId == parentId && a.FuncType == 2).OrderBy(a => a.Sort);
                    //如果没有子菜单的情况
                    if (menus.Count() > 0)
                    {
                        sb.Append("<div class=\"sidebar_nav\">");
                        sb.Append("<h2>");
                        sb.AppendFormat("<span>{0}</span></h2>", moduleMenu.Name);
                        sb.Append("<ul>");
                        foreach (var menu in menus)
                        {
                            sb.Append("<li><a href='{0}'>{1}</a> </li>".With(menu.URL, menu.Name));
                        }
                    }
                    else
                    {
                        sb.Append("<div>");
                        sb.Append("<h2>");
                        sb.Append("<span></span></h2>");
                        sb.Append("<ul>");
                    }
                    sb.Append("</ul>");
                    sb.Append("</div>");
                }
            }
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}
