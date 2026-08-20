using Arch.Common;
using Lvy.Models.CrmDB;
using System;
using System.Collections.Generic;
using System.Web;

namespace Lvy.Web.Common
{
    /// <summary>
    ///  上下文传递中需要用的数据集合类
    /// </summary>
    public class GlobalContext
    {
        public GlobalContext()
        {
            OnlineUsers = new Dictionary<string, int>();
        }

        private static GlobalContext _current;

        public static GlobalContext Current
        {
            get
            {
                return _current ?? (_current = Activator.CreateInstance<GlobalContext>());
            }
        }

        /// <summary>
        /// 获取当前帐号的所有权限
        /// </summary>
        public IList<SysFunctionModel> FunctionList
        {
            get { return HttpRuntime.Cache.Get("Funcs_" + UserInfo.Code) as List<SysFunctionModel>; }
            set { HttpRuntime.Cache.Insert("Funcs_" + UserInfo.Code, value); }
        }

        /// <summary>
        ///  获取当前账户的信息
        /// </summary>
        public CrmAccountModel UserInfo
        {
            get
            {
                return HttpContext.Current.Session["AccountInfo"] as CrmAccountModel;
            }
            set
            {
                HttpContext.Current.Session["AccountInfo"] = value;
            }
        }

        /// <summary>
        /// 是否是系统管理员
        /// </summary>
        public bool IsSysAdmin
        {
            get { return UserInfo.AccountType == 2; }
        }

        /// <summary>
        ///  是否是所属商户的员工
        /// </summary>
        public bool IsOwnerUser
        {
            get { return UserInfo.CustomerCode == OwnerCode; }
        }

        /// <summary>
        /// 所属商户编号
        /// </summary>
        public string OwnerCode
        {
            get { return HttpContext.Current.Session["OwnerCode"] as string; }
            set { HttpContext.Current.Session["OwnerCode"] = value; }
        }

        /// <summary>
        ///  所属商户完整信息 同OwnerCode
        /// </summary>
        public SysPlatformModel OwnerInfo
        {
            get { return HttpContext.Current.Session["OwnerInfo"] as SysPlatformModel; }
            set { HttpContext.Current.Session["OwnerInfo"] = value; }
        }
        /// <summary>
        /// 客户Logo路径
        /// </summary>
        public string CustomerLogoPath
        {
            get
            {
                return AppSetting.Get("UploadFileRoot") + OwnerInfo.SiteLogoPath;
            }
        }

        /// <summary>
        /// 当前登录用户的所有角色
        /// </summary>
        public IList<SysRoleModel> LoginUserRoles
        {
            get { return HttpContext.Current.Session["userRoles"] as IList<SysRoleModel>; }
            set { HttpContext.Current.Session["userRoles"] = value; }
        }

        public IList<CrmTeamModel> LoginUserTeams
        {
            get { return HttpContext.Current.Session["userTeams"] as IList<CrmTeamModel>; }
            set { HttpContext.Current.Session["userTeams"] = value; }
        }


        /// <summary>
        ///  默认出发城市
        /// </summary>
        public string OutCity
        {
            get { return AppSetting.Get("OutCity"); }
        }

        /// <summary>
        /// 返回路径
        /// </summary>
        public string UrlReferrerSession
        {
            get
            {
                return HttpContext.Current.Session["UrlReferrerSession"] as string;
            }
            set { HttpContext.Current.Session["UrlReferrerSession"] = value; }
        }

        public string ValidateCode
        {
            get
            {
                return HttpContext.Current.Session["ValidateCode"] as string;
            }

            set
            {
                HttpContext.Current.Session["ValidateCode"] = value;
            }
        }

        public string WeixinQrCode
        {
            get
            {
                return HttpContext.Current.Session["WeixinQrCode"] as string;
            }

            set
            {
                HttpContext.Current.Session["WeixinQrCode"] = value;
            }
        }


        public string UploadFileRoot
        {
            get
            {
                return AppSetting.Get("UploadFileRoot");
            }
        }


        /// <summary>
        /// 在线用户信息
        /// </summary>
        public Dictionary<string, int> OnlineUsers { get; set; }

        /// <summary>
        /// 当前用户所在客户
        /// </summary>
        public CrmCustomerModel CustomerBy
        {
            get { return HttpContext.Current.Session["CustomerBy"] as CrmCustomerModel; }
            set { HttpContext.Current.Session["CustomerBy"] = value; }
        }
        /// <summary>
        /// 当前选择的城市
        /// </summary>
        public string CurrentCity
        {
            get { return HttpContext.Current.Session["CurrentCity"] as string; }
            set { HttpContext.Current.Session["CurrentCity"] = value; }
        }
        public string CurrentCityName
        {
            get { return HttpContext.Current.Session["CurrentCityName"] as string; }
            set { HttpContext.Current.Session["CurrentCityName"] = value; }
        }


    }
}