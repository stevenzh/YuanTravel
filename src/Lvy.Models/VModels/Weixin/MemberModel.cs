using Lvy.Models.WeixinDB;
using System;

namespace Lvy.VModels.Weixin
{
//    public partial class MemberModel
//    {
//        public int MemberID { get; set; }
//        public string OwnerCode { get; set; }

//        #region 微信客户信息
//        public string OpenID { get; set; }
//        public string NickName { get; set; }
//        public Nullable<int> Sex { get; set; }
//        public string Language { get; set; }
//        public string City { get; set; }
//        public string Province { get; set; }
//        public string Country { get; set; }
//        public string HeadImgUrl { get; set; }
//        public System.DateTime SubscribeTime { get; set; }
//        public string Subscribe { get; set; }
//        public string SubscribeValue { get; set; }
//        #endregion

//        /// <summary>
//        /// 0:默认 1:客户申请 2:绑定通过审核 3:申请未通过
//        /// </summary>
//        [Display(Name = "淘宝绑定")]
//        public string Binding { get; set; }
//        public string BindingValue { get; set; }
//        public int? SalesID { get; set; }
//        [Display(Name = "对应销售")]
//        public string Sales { get; set; }
//        public Nullable<System.DateTime> UnsubscribeTime { get; set; }
//        [Display(Name = "真实姓名")]
//        public string RealName { get; set; }
//        [Display(Name = "手机号")]
//        public string PhoneNumber { get; set; }
//        [Display(Name = "审核")]
//        public int Approved { get; set; }
//        /// <summary>
//        /// 客户最后一次发送的消息
//        /// </summary>
//        public Nullable<DateTime> LastMessageTime { get; set; }
//        public string EmployeeID { get; set; }

//        public int? CustomerID { get; set; }
//        public string CustomerName { get; set; }
//        public string LogoUrl { get; set; }
//        public bool HideShared { get; set; }
//        public int? QrID { get; set; }
//        /// <summary>
//        /// 绑定时提交的信息
//        /// </summary>
//        [Display(Name = "客户提交绑定信息")]
//        public string BindMessage { get; set; }
//        /// <summary>
//        /// 微信消息
//        /// </summary>
//        public IList<MemberMessage> Messages { get; set; }
//        /// <summary>
//        /// 微信消息
//        /// </summary>
//        //public PagedList<MemberMessage> MessagePageList { get; set; }

//        public IList<MemberAddress> AddressList { get; set; }

//        public IList<MemberCardModel> WxCardList { get; set; }
//        /// <summary>
//        /// 仅用于发送微信通知
//        /// </summary>
//        public string SalesOpenID { get; set; }
//        public int CartProductNum { get; set; }
//        //public IList<OrderModel> OrderList { get; set; }
//    }


    public partial class MemberAdressModel
    {
        public int AddressID { get; set; }
        public int? HostID { get; set; }
        public int MemberID { get; set; }
        public string ConsigneeAlias { get; set; }
        public string ConsigneeName { get; set; }

        public string ConsigneeMobile { get; set; }

        public string ConsigneeAddress { get; set; }
        public int IsDefault { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
    public partial class MemberCardModel
    {
        public string code { get; set; }
        //public string can_consume { get; set; }
        public string user_card_status { get; set; }
        public WeixinCard cardInfo { get; set; }
    }
}
