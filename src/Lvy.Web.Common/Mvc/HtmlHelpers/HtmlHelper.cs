using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Lvy.Models;

namespace Lvy.Web.Common.Mvc.HtmlHelpers
{
    public static partial class HtmlHelperEx
    {
        #region ToSelectListFor
        /// <summary>
        ///  对继承所有IEnumerable<T> 的集合绑定到dropdownlist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="defaultValue"></param>
        /// <param name="defaultOption"></param>
        /// <returns></returns>
        public static List<SelectListItem> ToSelectListFor<T>(
          this IEnumerable<T> enumerable,
          Func<T, string> value,
          Func<T, string> text,
          string selectedValue = "",
          string defaultValue = "",
          string defaultOption = "-- 请选择 --")
        {
            var items = enumerable.Select(f => new SelectListItem()
            {
                Text = text(f),
                Value = value(f)
            }).ToList();
            items.Insert(0, new SelectListItem()
            {
                Text = defaultOption,
                Value = defaultValue
            });

            if (!selectedValue.IsNullOrEmpty())
            {
                foreach (var item in items)
                {
                    if (item.Value == selectedValue)
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }

            return items;
        }

        public static IEnumerable<SelectListItem> ToSelectListFor(this IEnumerable<KeyValueBean> enumerable,
            string selectedValue = "",
            string defaultValue = "",
         string defaultOption = "-- 请选择 --")
        {
            var items = enumerable.Select(f => new SelectListItem()
            {
                Text = f.Value,
                Value = f.Key
            }).ToList();
            items.Insert(0, new SelectListItem()
            {
                Text = defaultOption,
                Value = defaultValue
            });

            if (!selectedValue.IsNullOrEmpty())
            {
                foreach (var item in items)
                {
                    if (item.Value == selectedValue)
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// 不需要默认值
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> ToSelectListForNoDefualt(this IEnumerable<KeyValueBean> enumerable, string selectedValue = "")
        {
            var items = enumerable.Select(f => new SelectListItem()
            {
                Text = f.Value,
                Value = f.Key
            }).ToList();

            if (!selectedValue.IsNullOrEmpty())
            {
                foreach (var item in items)
                {
                    if (item.Value == selectedValue)
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }

            return items;
        }
        #endregion

        #region DropDownList

        public static MvcHtmlString DropDownList(this HtmlHelper htmlHelper, string name, IEnumerable<KeyValueBean> keyValueBeans, string defualtValue = null)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<select class=\"text150\" id=\"" + name + "\"  name=\"" + name + "\">");
            sb.Append("<option value=\"0\" >-- 请选择 --</option> ");
            foreach (var kv in keyValueBeans)
            {
                sb.Append("<option value=\"" + kv.Key + "\" data-help1=\"" + kv.Help1 + "\" ");

                if (defualtValue == kv.Key)
                    sb.Append(" selected=\"selected\"");

                sb.Append(">" + kv.Value);
                sb.Append("</option>");
            }

            sb.Append("</select>");

            return MvcHtmlString.Create(sb.ToString());
        }

        //public static MvcHtmlString DropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, dynamic viewData, object htmlAttributes)
        //{
        //    return htmlHelper.DropDownListFor(expression, (viewData as IEnumerable<SelectListItem>), null, htmlAttributes);
        //}


        public static MvcHtmlString DropDownListTemplateFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string labelName, object viewData, object htmlAttributes, bool isMust = false)
        {
            StringBuilder sb = new StringBuilder();
            if (isMust)
                sb.Append("<td width=\"1%\" align=\"center\" style=\"height:30px; color:#ff0000;\">*</td>");
            else
                sb.Append("<td width=\"1%\" ></td>");
            sb.Append("<td align=\"right\" width=\"10%\" >{0}：</td>".With(labelName));

            MvcHtmlString html = htmlHelper.DropDownListFor(expression, (viewData as IEnumerable<SelectListItem>), null, htmlAttributes);
            sb.Append("<td align=\"left\" width=\"20%\"> {0}</td>".With(html));
            sb.Append("<td></td>");
            return MvcHtmlString.Create(sb.ToString());
        }

        #endregion

        #region RadioGroup/CheckboxGroup

        public static MvcHtmlString RadioGroup(this HtmlHelper htmlHelper, string name, IEnumerable<KeyValueBean> keyValueBeans, string defualtValue = "")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kv in keyValueBeans)
            {
                sb.Append("<label style=\"margin-right:15px;\"><input type=\"radio\" name=\"" + name + "\" value=\"" + kv.Key + "\"");
                if (kv.Key == defualtValue)
                {
                    sb.Append(" checked=\"checked\"");
                }
                sb.Append(" />" + kv.Value + "</label>");
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString CheckboxGroup(this HtmlHelper htmlHelper, string name, IEnumerable<KeyValueBean> keyValueBeans, string[] defualtValues = null)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kv in keyValueBeans)
            {
                sb.Append("<label style=\"margin-right:15px;\"><input type=\"checkbox\" name=\"" + name + "\" value=\"" + kv.Key + "\"");


                if (defualtValues != null && defualtValues.Contains(kv.Key))
                    sb.Append(" checked=\"checked\"");

                sb.Append(" />" + kv.Value + "</label>");
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString CheckboxList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> values, string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var temp in values)
            {
                sb.Append("<span style=\"margin-right:15px;\"> <input type=\"checkbox\" name=\"" + name + "\" value=\"" + temp.Value + "\"");
                if (null != value && value.Split(',').Contains(temp.Value))
                {
                    sb.Append(" checked=\"checked\"");
                }
                sb.Append(" />" + temp.Text + "</span>");
            }

            return MvcHtmlString.Create(sb.ToString());
        }
        #endregion

        #region ActionLink

        public static MvcHtmlString EditActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues = null)
        {
            return htmlHelper.ActionLink("编辑", actionName, controllerName, routeValues, new { @class = "edit", @title = "编辑" });
        }
        public static MvcHtmlString Update(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues = null)
        {
            return htmlHelper.ActionLink("删除", actionName, controllerName, routeValues, new { @class = "edit", @title = "删除", @onclick = "return confirm('确认需要删除吗？')" });
        }
        public static MvcHtmlString DetailActionLink(this HtmlHelper htmlHelper, string actionName, object routeValues)
        {
            return htmlHelper.ActionLink("详细", actionName, routeValues, new { @class = "search" });
        }
        public static MvcHtmlString download(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues = null)
        {
            return htmlHelper.ActionLink("下载", actionName, controllerName, routeValues, new { @class = "edit", @title = "下载" });
        }

        public static MvcHtmlString VaildActionLink(this HtmlHelper htmlHelper, int isValid, string actionName, object routeValues, bool isAlert = true, object htmlAttributes = null)
        {
            MvcHtmlString returnValue = null;
            if (isValid == 0)
            {
                if (isAlert)
                    htmlAttributes = new { @onclick = "return confirm('确认需要设置有效吗？')", @style = "color:Gray; cursor:pointer;" };
                returnValue = htmlHelper.ActionLink("无效", actionName, routeValues, htmlAttributes);
                //returnValue = MvcHtmlString.Create(@"<span style=' color:Gray; cursor:pointer;'>无效</span>");
                // returnValue = MvcHtmlString.Create(@"<span style=' color:Gray;'>无效</span>");
            }
            else
            {
                if (isAlert)
                    htmlAttributes = new { @onclick = "return confirm('确认需要设置无效吗？')" };

                returnValue = htmlHelper.ActionLink("有效", actionName, routeValues, htmlAttributes);
            }

            return returnValue;
        }

        public static MvcHtmlString DeleteActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName, int isValid, object routeValues, bool isAlert = true)
        {
            object htmlAttributes = null;
            MvcHtmlString html = null;
            if (isValid == 1)
            {
                if (isAlert)
                    htmlAttributes = new { @title = "删除", @class = "delete", @onclick = "return confirm('确认需要删除吗？')" };

                html = htmlHelper.ActionLink("删除", actionName, controllerName, routeValues, htmlAttributes);
            }
            else
            {
                if (isAlert)
                    htmlAttributes = new { @title = "恢复", @onclick = "return confirm('确认需要恢复吗？')" };
                html = htmlHelper.ActionLink("恢复", actionName, controllerName, routeValues, htmlAttributes);
            }

            return html;
        }


        public static MvcHtmlString IconDeleteActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName, int isValid,
            object routeValues, object htmlAttributes = null, bool isAlert = true)
        {
            TagBuilder spanBuilder = new TagBuilder("i");
            if (isValid == 1)
            {
                spanBuilder.AddCssClass("fa fa-trash");
            }
            else
            {
                spanBuilder.AddCssClass("fa fa-recycle");
            }
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);

            TagBuilder anchorBuilder = new TagBuilder("a");
            anchorBuilder.Attributes.Add("href", urlHelper.Action(actionName, controllerName, routeValues));
            if (isValid == 1)
            {
                anchorBuilder.Attributes.Add("title", "删除");
                anchorBuilder.Attributes.Add("onclick", "return confirm('确认需要删除吗？')");
            }
            else
            {
                anchorBuilder.Attributes.Add("title", "恢复");
                anchorBuilder.Attributes.Add("onclick", "return confirm('确认需要恢复吗？')");
            }
           
            anchorBuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            anchorBuilder.InnerHtml = spanBuilder.ToString();

            return MvcHtmlString.Create(anchorBuilder.ToString());
        }

        public static MvcHtmlString IconActionLink(this HtmlHelper helper, string title, string actionName, string controllerName,
            string iconClass, object routeValues, object htmlAttributes = null)
        {
            TagBuilder spanBuilder = new TagBuilder("i");
            //spanBuilder.InnerHtml = linkText;
            spanBuilder.AddCssClass(iconClass);
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);

            TagBuilder anchorBuilder = new TagBuilder("a");
            anchorBuilder.Attributes.Add("href", urlHelper.Action(actionName, controllerName, routeValues));
            anchorBuilder.Attributes.Add("title", title);
            anchorBuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            anchorBuilder.InnerHtml = spanBuilder.ToString();

            return MvcHtmlString.Create(anchorBuilder.ToString());
        }

        #endregion


        #region css js文件引用

        public static MvcHtmlString ScriptInclude(this HtmlHelper html, string jsFile)
        {
            string jsPath = jsFile.Contains("~") ? jsFile : "~/Scripts/" + jsFile;
            string url = "../.." + html.ResolveUrl(jsPath);
            string template = "<script type=\"text/javascript\" src=\"{0}\" ></script>\n".With(url);
            return MvcHtmlString.Create(template);
        }

        public static MvcHtmlString Stylesheet(this HtmlHelper html, string cssFile)
        {
            string cssPath = cssFile.Contains("~") ? cssFile : "~/Content/" + cssFile;
            string url = "../.." + html.ResolveUrl(cssPath);
            string template = "<link type=\"text/css\" rel=\"stylesheet\" href=\"{0}\" />\n".With(url);
            return MvcHtmlString.Create(template);
        }

        #endregion

        #region Images

        public static MvcHtmlString Image(this HtmlHelper html, string imageVPath)
        {
            string cssPath = imageVPath.Contains("~") ? imageVPath : "~/Images/" + imageVPath;
            string url = "../.." + html.ResolveUrl(cssPath);
            string template = "<img src=\"{0}\" />".With(url);
            return MvcHtmlString.Create(template);
        }
        public static MvcHtmlString Image(this HtmlHelper html, string id, string imageVPath, string alt = "")
        {
            string cssPath = imageVPath.Contains("~") ? imageVPath : "~/Images/" + imageVPath;
            string url = "../.." + html.ResolveUrl(cssPath);
            string template = "<img  id=\"{0}\" src=\"{1}\"  alt=\"{2}\"  />".With(id, url, alt);
            return MvcHtmlString.Create(template);
        }
        #endregion

        #region Common

        public static string ResolveUrl(this HtmlHelper html, string relativeUrl)
        {
            if (relativeUrl == null)
                return null;

            if (!relativeUrl.StartsWith("~"))
                return relativeUrl;

            var basePath = html.ViewContext.HttpContext.Request.ApplicationPath;
            string url = basePath + relativeUrl.Substring(1);
            return url.Replace("//", "/");
        }

        /// <summary>
        /// 去除HTML标记
        /// </summary>
        /// <param name="html"> </param>
        /// <param name="Htmlstring"> </param>
        /// <returns>已经去除后的文字</returns>
        public static MvcHtmlString NoHTML(this HtmlHelper html, string Htmlstring)
        {
            Htmlstring.ToNoHTML();
            return MvcHtmlString.Create(Htmlstring);
        }

        #endregion

        #region Validation
        //<script type="text/javascript">
        //    $(document).ready(function () {
        //        $("#form1").validationEngine('attach');
        //    })
        //</script>

        /// <summary>
        /// 在页面输出一个input标签并验证用户的输入
        /// </summary>
        /// <typeparam name="TModel">模型的类型。</typeparam>
        /// <typeparam name="TProperty">值的类型。</typeparam>
        /// <param name="htmlHelper">此方法扩展的 HTML 帮助程序实例。</param>
        /// <param name="expression"> 一个表达式，标识包含要呈现的属性的对象。</param>
        /// <param name="className">此标签使用到的样式，多个请用空格隔开。</param>
        /// <param name="rules">自定义验证类型，可同时定义多个验证类型</param>
        /// <returns></returns>
        public static MvcHtmlString TextBoxValidationFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string className, params AllRules[] rules)
        {
            string validation = rules.Aggregate("validate[", (current, rule) => current + GetValidationString(rule) + ",");
            validation = validation.Substring(0, validation.Length - 1) + "]";
            object att = string.IsNullOrEmpty(className) ? new { @class = validation } : new { @class = validation + " " + className };
            return htmlHelper.TextBoxFor(expression, att);
        }


        private static string GetValidationString(AllRules rules)
        {
            string validation = string.Empty;
            switch (rules)
            {
                case AllRules.Required:
                    validation += "required";
                    break;
                case AllRules.Date:
                    validation += "custom[date],future[NOW]";
                    break;
                case AllRules.Url:
                    validation += "custom[url]";
                    break;
                case AllRules.Phone:
                    validation += "custom[phone]";
                    break;
                case AllRules.Email:
                    validation += "custom[email]";
                    break;
                case AllRules.Number:
                    validation += "custom[number]";
                    break;
                case AllRules.Integer:
                    validation += "custom[integer],";
                    break;
                case AllRules.MobilePhone:
                    validation += "custom[mobilephone],";
                    break;
                case AllRules.IdentityCard:
                    validation += "custom[identitycard],";
                    break;
            }
            return validation;
        }

        /// <summary>
        /// 自定义验证类型
        /// </summary>
        public enum AllRules
        {
            Required,
            Phone,
            Email,
            Integer,
            Number,
            Date,
            Url,
            MobilePhone,
            IdentityCard
        }
        #endregion

        #region TextBoxDropDown

        /// <summary>
        /// TextBox型的下拉框 
        /// </summary>
        /// <typeparam name="TModel">页面的Model</typeparam>
        /// <typeparam name="TProperty">页面中的属性</typeparam>
        /// <param name="htmlHelper">表示支持在强类型视图中呈现 HTML 控件。</param>
        /// <param name="expression">一个表达式，标识包含要呈现的属性的对象。</param>
        /// <param name="controllerName">异步提交的Controller名称</param>
        /// <param name="actionName">异步提交的Action名称</param>
        /// <param name="dictionaryName">编辑时传递所需的TextBox中的文字</param>
        /// <returns>返回不应再次进行编码的 HTML 编码的字符串。</returns>
        public static MvcHtmlString TextBoxDropDown<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression,
            string controllerName, string actionName, KeyValueBean dictionaryName)
        {
            string id = expression.ToString().Split('.').Where(str => str.IndexOf("=>") == -1).Aggregate("", (current, str) => current + str);
            var html = new StringBuilder(string.Format("<input type='text' class='text150 text_arrows' onclick=\"popDropDownList(this,'{0}','{1}','{2}')\" id='{0}' readonly='readonly' value='{3}' />", id, controllerName, actionName, dictionaryName != null ? dictionaryName.Value : ""));
            html.Append(htmlHelper.HiddenFor(expression, new { id = "h" + id }).ToString());
            html.Append("<div style='width: 150px; max-height:150px;overflow:auto; background-color: #e9ecf2; z-index: 200; display: none;position: absolute;' id='div" + id + "'></div>");
            return MvcHtmlString.Create(html.ToString());
        }

        /// <summary>
        /// TextBox型的下拉框 
        /// </summary>
        /// <typeparam name="TModel">页面的Model</typeparam>
        /// <typeparam name="TProperty">页面中的属性</typeparam>
        /// <param name="htmlHelper">表示支持在强类型视图中呈现 HTML 控件。</param>
        /// <param name="expression">一个表达式，标识包含要呈现的属性的对象。</param>
        /// <param name="controllerName">异步提交的Controller名称</param>
        /// <param name="actionName">异步提交的Action名称</param>
        /// <param name="nodeName">读取的节点名称</param>
        /// <param name="dictionaryName">编辑时传递所需的TextBox中的文字</param>
        /// <returns>返回不应再次进行编码的 HTML 编码的字符串。</returns>
        public static MvcHtmlString TextBoxDropDown<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression,
            string controllerName, string actionName, string nodeName, KeyValueBean dictionaryName)
        {
            string id = expression.ToString().Split('.').Where(str => str.IndexOf("=>") == -1).Aggregate("", (current, str) => current + str);
            var html =
                new StringBuilder(
                    string.Format(
                        "<input type='text' class='text150 text_arrows' onclick=\"popDropDownListEx(this,'{0}','{1}','{2}','{4}')\" id='{0}' readonly='readonly' value='{3}' />",
                        id, controllerName, actionName, dictionaryName != null ? dictionaryName.Value : "", nodeName));
            html.Append(htmlHelper.HiddenFor(expression, new { id = "h" + id }).ToString());
            html.Append("<div style='width: 150px; max-height:150px;overflow:auto; background-color: #e9ecf2; z-index: 200; display: none;position: absolute;' id='div" + id + "'></div>");
            return MvcHtmlString.Create(html.ToString());
        }
        #endregion

        #region TextBoxKeyUpPopDiv
        /// <summary>
        /// 在TextBox中输入后提交至服务器获取数据
        /// </summary>
        /// <typeparam name="TModel">页面的Model</typeparam>
        /// <typeparam name="TProperty">页面中的属性</typeparam>
        /// <param name="htmlHelper">表示支持在强类型视图中呈现 HTML 控件。</param>
        /// <param name="expression">一个表达式，标识包含要呈现的属性的对象。</param>
        /// <param name="controllerName">异步提交的Controller名称</param>
        /// <param name="actionName">异步提交的Action名称</param>
        /// <param name="className">class样式名称</param>
        /// <param name="dictionaryName">编辑时传递所需的TextBox中的文字</param>
        /// <returns>返回不应再次进行编码的 HTML 编码的字符串。</returns>
        public static MvcHtmlString TextBoxKeyUpPopDiv<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression,
            string controllerName, string actionName, string className, KeyValueBean dictionaryName)
        {
            string id = expression.ToString().Split('.').Where(str => str.IndexOf("=>") == -1).Aggregate("", (current, str) => current + str);
            var html =
                new StringBuilder(
                    string.Format(
                        "<input type='text' class='{3}' onkeyup=\"popkeyup(this,'{0}','{1}','{2}')\" onclick=\"popdiv(this,'{0}','{1}','{2}')\" id='{0}' value='{4}' />",
                        id, controllerName, actionName, className, dictionaryName != null ? dictionaryName.Value : ""));
            html.Append(htmlHelper.HiddenFor(expression, new { id = "h" + id }).ToString());
            html.Append("<div style='width: 150px; background-color: #e9ecf2; z-index: 200; display: none;position: absolute;' id='div" + id + "'></div>");
            return MvcHtmlString.Create(html.ToString());
        }
        #endregion


        public static MvcHtmlString BusSeat(this HtmlHelper htmlHelper, string name, List<BusSeatModel> seatList, int seatNumPerRow, int seatNumBeforeBlank)
        {
            var sb = new StringBuilder();
            //if (!seatDetail.IsNullOrEmpty())
            //{
            //    var serializer = new JavaScriptSerializer();
            //    var seatList = serializer.Deserialize<List<BusSeatModel>>(seatDetail);
            if (seatList.Count > 0)
            {
                int lastRowCount = seatList.Count % seatNumPerRow;
                //最后一排之前的总行数
                int fullRowCount = 0;
                if (lastRowCount == 0)
                {
                    fullRowCount = seatList.Count / seatNumPerRow - 1;
                    lastRowCount = seatNumPerRow;
                }
                else
                {
                    if (lastRowCount == 1)
                    {
                        lastRowCount += seatNumPerRow;
                        fullRowCount = seatList.Count / seatNumPerRow - 1;
                    }
                    else
                    {
                        fullRowCount = seatList.Count / seatNumPerRow;
                    }
                }

                //= lastRowCount > 0 ?  : (seatList.Count / seatNumPerRow - 1);
                //每行起始位置在座位列表中所占的索引
                for (int i = 0; i < fullRowCount; i++)
                {
                    int indexOfStartRow = (i * seatNumPerRow);
                    sb.Append("<tr>");
                    for (int j = 0; j < seatNumPerRow; j++)
                    {
                        var seat = seatList[indexOfStartRow + j];
                        //未占
                        if (seat.State == 1)
                        {
                            sb.Append("<td name=" + name + " class=\"gray\">" + seat.No + "<input type=\"hidden\" name=[" + i + "]." + name + " value=\"" + seatList[indexOfStartRow + j].State + "\"/></td>");
                        }
                        //已占
                        else if (seat.State == 2)
                        {
                            sb.Append("<td name=" + name + " class=\"red\">" + seat.No + "</td>");
                        }
                        //锁定
                        else
                        {
                            sb.Append("<td name=" + name + " class=\"blue\">" + seat.No + "</td>");
                        }
                        //过道
                        if (j == (seatNumBeforeBlank - 1))
                        {
                            sb.Append("<td name=" + name + " class=\"noborder\">&nbsp;</td>");
                        }
                    }
                    sb.Append("</tr>");
                }
                if (lastRowCount > 0)
                {
                    int indexOfStartLastRow = fullRowCount * seatNumPerRow;
                    sb.Append("<tr>");
                    for (int i = 0; i < lastRowCount; i++)
                    {
                        var seat = seatList[indexOfStartLastRow + i];
                        //未占
                        if (seat.State == 1)
                        {
                            sb.Append("<td name=" + name + " class=\"gray\">" + seat.No + "</td>");
                        }
                        //已占
                        else if (seat.State == 2)
                        {
                            sb.Append("<td name=" + name + " class=\"red\">" + seat.No + "</td>");
                        }
                        //锁定
                        else
                        {
                            sb.Append("<td name=" + name + " class=\"blue\">" + seat.No + "</td>");
                        }
                    }
                    sb.Append("</tr>");
                }
            }
            //}
            return MvcHtmlString.Create(sb.ToString());
        }


    }
}
