using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Lvy.Trip.Weixin
{
    public class Configs
    {

        /// <summary>
        /// 默认缓存键的第一级命名空间，默认值：DefaultCache
        /// </summary>
        public static string DefaultCacheNamespace = "DefaultCache";

        public static string emailName
        {
            get { return ConfigurationManager.AppSettings["myEmailName"]; }
        }

        /// <summary>
        /// XXX旅游网
        /// </summary>
        public static string SiteTitle
        {
            get { return ConfigurationManager.AppSettings["SiteTitle"]; }
        }
        public static string OwnerCode
        {
            get { return ConfigurationManager.AppSettings["OwnerCode"]; }
        }

        /// <summary>
        /// XXX有限责任公司
        /// </summary>
        public static string CompanyName
        {
            get { return ConfigurationManager.AppSettings["CompanyName"]; }
        }
        public static string ServicePhone
        {
            get { return ConfigurationManager.AppSettings["ServicePhone"]; }
        }
        public static string Login
        {
            get { return ConfigurationManager.AppSettings["Login"]; }
        }

        /// <summary>
        /// 共享域
        /// </summary>
        public static string domain
        {
            get { return ConfigurationManager.AppSettings["RootDomain"]; }
        }


        #region CookieName
        public static string CookieName
        {
            get { return ConfigurationManager.AppSettings["PrefixKey"]; }
        }
        #endregion

        /// <summary>
        /// 缓存时间(秒)
        /// </summary>
        public static int cacheDateTime
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["cacheDateTime"]); }
        }


        /// <summary>
        /// xml路径
        /// </summary>
        public static string XmlDocumentPath
        {
            get { return ConfigurationManager.AppSettings["XmlDictPath"]; }
        }

        /// <summary>
        /// 前台网址
        /// </summary>
        public static string wwwUrl
        {
            get { return ConfigurationManager.AppSettings["wwwUrl"]; }
        }

        #region 发邮件

        public static string myEmailName
        {
            get { return ConfigurationManager.AppSettings["myEmailName"]; }
        }

        public static string myEmailPwd
        {
            get { return ConfigurationManager.AppSettings["myEmailPwd"]; }
        }

        public static string myEmailServer
        {
            get { return ConfigurationManager.AppSettings["myEmailServer"]; }
        }

        #endregion

        #region 是否开启短信发送
        public static string isMsgOpen
        {
            get { return ConfigurationManager.AppSettings["isMsgOpen"]; }
        }
        #endregion

        #region 主网地址

        public static string maidouManage
        {
            get { return ConfigurationManager.AppSettings["maidouManage"]; }
        }

        #endregion

        #region 签证管理系统后台网址
        public static string visaManageUrl
        {
            get { return ConfigurationManager.AppSettings["VisaManageUrl"]; }
        }
        #endregion

    }
}