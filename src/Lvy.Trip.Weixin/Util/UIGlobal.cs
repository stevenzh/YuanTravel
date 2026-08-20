using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Lvy.Models.CrmDB;

namespace Lvy.Trip.Weixin
{
    public class UIGlobal
    {
        private static UIGlobal _current;

        public static UIGlobal Current
        {
            get
            {
                if (_current == null)
                    _current = Activator.CreateInstance<UIGlobal>();
                return _current;
            }
        }

        public string OwnerCode
        {
            get
            {
                var u = HttpContext.Current.Session["OwnerCode"] as string;
                if (string.IsNullOrEmpty(u))
                {
                    u = Convert.ToString(WebConfigurationManager.AppSettings["OwnerCode"]);
                }
                return u;
            }
            set { HttpContext.Current.Session["OwnerCode"] = value; }
        }

        public CrmAccountModel UserInfo
        {
            get
            {
                var u = HttpContext.Current.Session["userInfo"] as CrmAccountModel;
                if (u == null)
                {
                    u = new CrmAccountModel();
                    u.OwnerCode = Convert.ToString(WebConfigurationManager.AppSettings["OwnerCode"]);
                }
                return u;
            }
            set { HttpContext.Current.Session["userInfo"] = value; }
        }

        public static string WeixinSiteURL
        {
            get { return WebConfigurationManager.AppSettings["WeixinSiteUrl"]; }
        }


        ////////////////////////////////////////////////////////////////////////////////////
        private int _maxChatMessages;
        public int MaxChatMessages { get { return _maxChatMessages; } }

        private int _checkNewMessageTimeout;
        public int CheckNewMessageTimeout { get { return _checkNewMessageTimeout; } }

        public static UIGlobal Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Sync)
                    {
                        if (_instance == null)
                            _instance = new UIGlobal().Init();
                    }
                }

                return _instance;
            }
        }


        private static int ParseInt(int maxVal, int minVal, int defVal, string configKey)
        {
            string valueSt = ConfigurationManager.AppSettings[configKey];
            int value;
            if (!int.TryParse(valueSt, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return defVal;

            if (value < minVal || value > maxVal)
                return defVal;
            return value;
        }

        private UIGlobal Init()
        {
            _maxChatMessages = ParseInt(100, 1, 10, "maxChatMessages");
            _checkNewMessageTimeout = ParseInt(30000, 3000, 3000, "checkNewMessageTimeout");
            return this;
        }

        private static UIGlobal _instance;
        public static readonly object Sync = new object();

    }
}