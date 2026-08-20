using Arch.Common;
using log4net;
using Lvy.Models.CrmDB;
using Lvy.Trip.Biz.Crm;
using Lvy.Web.Common.FileUpload;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobalContext = Lvy.Web.Common.GlobalContext;

namespace Lvy.Trip.AdminSite.Controllers
{
    /// <summary>
    /// 后台基础Controller
    /// </summary>
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
                || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                base.OnActionExecuting(filterContext);
            }
            else
            {
                // base.OnActionExecuting(filterContext);
                var ownerInfo = GlobalContext.Current.OwnerInfo;
                if (ownerInfo == null)
                {
                    var host = (SysPlatformModel)this.RouteData.Values["tenant"];
                    var customer = new CustomerBiz().GetById(host.CustomerCode);
                    if (customer != null)
                    {
                        host.CrmCustomer = customer;
                        GlobalContext.Current.OwnerCode = host.CustomerCode;
                        GlobalContext.Current.OwnerInfo = host;
                    }
                }

                var userInfo = GlobalContext.Current.UserInfo;
                if (userInfo == null)
                {
                    if (Request.Cookies["uid_cookie"] != null && !Request.Cookies["uid_cookie"].Value.IsNullOrEmpty())
                    {
                        string cookieValue = System.Text.Encoding.Default.GetString(Convert.FromBase64String(Request.Cookies["uid_cookie"].Value));
                        var arr = cookieValue.Split(':');
                        var loginName = arr[0];
                        var loginPwd = arr[1];
                        var oid = arr[2];

                        var tempModel = new AccountBiz().GetByLogin(oid, loginName, loginPwd);
                        if (tempModel != null)
                        {
                            GlobalContext.Current.UserInfo = tempModel;
                            GlobalContext.Current.FunctionList = new FunctionBiz().GetFunctionByAccountCode(tempModel);

                            return;
                        }
                    }

                    // 没有session的场合
                    string urls = "/User/Login";//?url=" + HttpContext.Request.Url;
                    filterContext.Result = new RedirectResult(urls);
                }
            }
        }

        public ActionResult SaveResult(string result, string successUrl = "", string failJS = "")
        {
            string script = "";
            switch (result)
            {
                case "1":
                    if (successUrl.IsNullOrEmpty())
                        script = "<script type=\"text/javascript\">alert('操作执行成功！');</script>";
                    else
                        script = "<script type=\"text/javascript\">window.location.href='" + successUrl + "';alert('操作执行成功！');</script>";
                    break;

                case "0":
                    script = "<script type=\"text/javascript\">alert('操作执行失败！');" + failJS + "</script>";
                    break;
            }

            return Content(script);
        }

        public ActionResult AlertResult(string msg)
        {
            return Content("<script type=\"text/javascript\">alert('{0}');history.back(0);</script>".With(msg));
        }

        /// <summary>
        /// 初始化页面数据
        /// </summary>
        protected virtual void InitPage()
        {
        }

        public CrmAccountModel UserInfo
        {
            get
            {
                return GlobalContext.Current.UserInfo;
            }
        }

        /// <summary>
        /// 所属客户
        /// </summary>
        public string OwnerCode
        {
            get { return GlobalContext.Current.OwnerCode; }
        }

        public const int OutputCacheDuration = 3600;

        /// <summary>
        /// 此处进行异常记录,记录到文本中
        /// 通过filterContext.Exception来获取这个异常
        /// </summary>
        /// <param name="filterContext"></param>
        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    // 执行基类中的OnException
        //    base.OnException(filterContext);
        //    if (filterContext.Exception != null)
        //    {
        //        _logger = LogManager.GetLogger(filterContext.Controller.GetType());

        //        string error = string.Format("类名：{0}  \r\n  错误信息：{1} \r\n  {2}",
        //                   filterContext.Controller.GetType(),
        //                   filterContext.Exception.Message,
        //                   filterContext.Exception.StackTrace);
        //        _logger.Error(error);

        //        Response.Redirect("/Base/Error");
        //    }
        //}

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult NoAuthorityAccess()
        {
            return View();
        }

        #region 上传的文件保存

        /// <summary>
        /// 保存文件到服务器
        /// </summary>
        /// <param name="wordUpload">文件流</param>
        /// <param name="virtualPath">路径</param>
        /// <param name="postfix">后缀组</param>
        /// <returns></returns>
        public string SaveFile(HttpPostedFileBase wordUpload, string virtualPath, string[] postfix)
        {
            var name = wordUpload.FileName.Split('.')[wordUpload.FileName.Split('.').Length - 1];
            if (!postfix.Contains(name.ToLower()))
            {
                //格式错误
                return "-1";
            }

            string filename = string.Format("{0:yyyyMMdd_HHmmss_}{1}", DateTime.Now, (new Random()).Next().ToString().Substring(0, 4)) + Path.GetExtension(wordUpload.FileName);
            UploadFileRequest request = new UploadFileRequest();
            request.FileName = filename;
            request.FileStream = Toolkit.Image.StreamToBytes(wordUpload.InputStream);

            // 所属客户code\文件类型
            request.VirtualPath = virtualPath;
            UploadServiceClient client = new UploadServiceClient();
            UploadFileResponse response = client.UploadFile(request);
            string fileUrl = AppSetting.Get("UploadFileRoot") + response.FilePath + response.FileName;

            return fileUrl;
        }

        #endregion 上传的文件保存

        #region 删除网站缓存

        public void RemoveWebCache(string type)
        {
            //string url = "domain/api/controller/method?parameter1=param";
            //using (var client = new HttpClient())
            //{
            //    HttpResponseMessage response = client.GetAsync(url).ConfigureAwait(false);
            //    if (response.IsSuccessStatusCode)
            //    {
            //        var jsonResponse = response.Content.ReadAsStringAsync().Result;
            //        bool data = JsonConvert.DeserializeObject<bool>(jsonResponse);
            //       // return data;
            //    }
            //}
        }

        public void RemoveWebCacheByKey(string key)
        {
            //CCT.Web.VisaManage.Site.WCF.Visaweb.VisawebServiceSoapClient _client = new CCT.Web.VisaManage.Site.WCF.Visaweb.VisawebServiceSoapClient();
            //_client.RemoveCacheByKey(key);
        }

        #endregion 删除网站缓存
    }
}