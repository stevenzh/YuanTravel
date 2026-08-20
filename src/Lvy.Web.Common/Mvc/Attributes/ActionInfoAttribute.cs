using System;
using System.Linq;
using System.Web.Mvc;

namespace Lvy.Web.Common.Mvc.Attributes
{
    /// <summary>
    ///  权限判断模块
    ///  
    /// 使用说明：
    /// 父菜单需要通过系统添加进DB。然后将父菜单的ID保存到ModuleType里
    /// </summary>
    public class ActionInfoAttribute //: AuthorizeAttribute
    {
        public ActionInfoAttribute()
        {
            IsValid = true;
            IsMenu = false;
        }

        public string Description { get; set; }

        public bool IsValid { get; set; }

        public bool IsMenu { get; set; }

        /// <summary>
        ///  菜单说明
        /// </summary>
        public string MenuDescription { get; set; }
        /// <summary>
        ///  上级模块
        /// </summary>
        public string ParentModule { get; set; }


        /// <summary>
        /// Premission filter
        /// </summary>
        /// <param name="filterContext"></param>
        //        public override void OnAuthorization(AuthorizationContext filterContext)
        //        {
        ////#if DEBUG
        ////            if (GlobalContext.Current.FunctionsByUser == null)
        ////                return;
        ////#else
        ////            if (GlobalContext.Current.FunctionsByUser == null)
        ////                throw new NullReferenceException("FunctionsByUser is null!!");
        ////#endif

        ////            if (GlobalContext.Current.UserInfo.IsAdmin)
        ////                return;

        ////            var funcs = GlobalContext.Current.FunctionsByUser
        ////                                .Where(a => a.Url != null && filterContext.HttpContext.Request.Path.Contains(a.Url));
        ////            if (funcs.Count() < 1)
        ////            {
        ////                // 没有权限
        ////                string path = "http://erp.gogotrips.com/User/ForbidAuth/";
        ////                // redirect  forbid Premission

        ////                filterContext.HttpContext.Response.Redirect(path);

        ////                filterContext.HttpContext.Response.End();
        ////            }


        //        }


    }

    public class ModuleType
    {
        /// <summary>
        /// 会员管理
        /// </summary>
        public const string Member = "6CDEF2E1-9689-4BD9-BAA0-B140C330A290";
        /// <summary>
        /// 基础设置
        /// </summary>
        public const string BaseInfo = "c1f9e39b-b928-420b-958e-32cae369ee76";

        public const string DestGuides = "9c4158e9-ff98-403f-b94b-73ae6288b0c6";

        #region menpiao
        public const string Menpiao_Agent = "808a27cb-37f8-4068-8fcb-4b22aba9d6ae";
        public const string Menpiao_Supplier = "5136e831-5908-4f84-95ba-9aa17e899fea";
        public const string Menpiao_Product = "8e028883-94b1-41d5-bfa7-1489fea38613";
        #endregion


        #region  酒店系统菜单 hotelmanager.gogotrips.com

        /// <summary>
        /// 呼叫中心
        /// </summary>
        public const string Hotel_CallCenter = "92196ba5-6010-4b5b-8d8d-3602e2c753e9";

        /// <summary>
        /// 酒店订单
        /// </summary>
        public const string Hotel_HotelOrder = "b9501c8c-d55d-4fa1-9f87-5f4dcb9a22a1";

        /// <summary>
        /// 财务管理
        /// </summary>
        public const string Hotel_FinanceManager = "e2c3ad31-da27-475c-bfb7-fbd78c0d07bc";

        /// <summary>
        /// 酒店管理
        /// </summary>
        public const string Hotel_HotelManager = "6c80d85a-9e7f-4d86-a5e2-53e4240985cd";

        #endregion

    }



}
