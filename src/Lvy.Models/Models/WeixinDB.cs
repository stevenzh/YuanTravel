using System;
using System.Collections.Generic;
using PetaPoco;
using Lvy.Models.OrderDB;

namespace Lvy.Models.WeixinDB
{
    /// <summary>
    /// 微信客户
    /// </summary>
    [TableName("WxMembers")]
    [PrimaryKey("MemberID")]
    public partial class Member
    {
        public int MemberID { get; set; }
        /// <summary>
        /// 所属商户
        /// </summary>	
        public string OwnerCode { get; set; }
        public string SalesID { get; set; }
        public string Sales { get; set; }
        public string RealName { get; set; }
        public string PhoneNumber { get; set; }
        public string CustomerName { get; set; }
        /// <summary>
        /// 0、未绑定 1、绑定通过
        /// </summary>
        public int Binding { get; set; }
        [ResultColumn]
        public string BindingValue { get; set; }
        /// <summary>
        /// 绑定时提交的信息
        /// </summary>
        public string BindMessage { get; set; }
        /// <summary>
        /// 同业审核（1、通过 0、未通过）
        /// </summary>
        public int Approved { get; set; }
        /// <summary>
        /// 微信最后消息时间
        /// </summary>
        public Nullable<DateTime> LastMessageTime { get; set; }
        /// <summary>
        /// 转发头像
        /// </summary>
        public string LogoUrl { get; set; }
        /// <summary>
        /// 微信分享时是否显示联系方式
        /// </summary>
        public bool HideShared { get; set; }
        /// <summary>
        /// 微信同步时间
        /// </summary>
        public Nullable<DateTime> SyncDate { get; set; }
        /// <summary>
        /// 关联内部员工
        /// </summary>
        public string EmployeeID { get; set; }
        /// <summary>
        /// 二维码
        /// </summary>
        public Nullable<int> QrID { get; set; }


        public string OpenID { get; set; }
        public string NickName { get; set; }
        public int Sex { get; set; }
        public string Language { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string HeadImgUrl { get; set; }
        public System.DateTime SubscribeTime { get; set; }
        public string Subscribe { get; set; }
        [ResultColumn]
        public string SubscribeValue { get; set; }
        public Nullable<DateTime> UnsubscribeTime { get; set; }
        public int IsEmployee { get; set; }
        /// <summary>
        /// 是否有效会员  0否|1是
        /// </summary>
        public int IsValid { get; set; }


        [ResultColumn]
        public IList<MemberMessage> Messages { get; set; }
        [ResultColumn]
        public IList<MemberAddress> AddressList { get; set; }
        [ResultColumn]
        public List<TpOrderModel> OrderList { get; set; }
    }

    /// <summary>
    /// 微信客户位置
    /// </summary>
    [TableName("WxMemberLocations")]
    [PrimaryKey("LocationID")]
    public partial class MemberLocation
    {
        public int LocationID { get; set; }
        public int HostID { get; set; }
        public string OpenID { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public string Latitude { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public string Longitude { get; set; }
        /// <summary>
        /// 精度
        /// </summary>
        public string Precision { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    /// <summary>
    /// 微信客户消息
    /// </summary>
    [TableName("WxMemberMessages")]
    [PrimaryKey("MessageID")]
    public partial class MemberMessage
    {
        public int MessageID { get; set; }
        /// <summary>
        /// 所属商户
        /// </summary>	
        public string OwnerCode { get; set; }
        public string OpenID { get; set; }
        public string MsgType { get; set; }
        public string Content { get; set; }
        public string FileUrl { get; set; }
        /// <summary>
        /// 微信外发 0:客户发送到服务号, 1:外发给客户,2:自动模板外发
        /// </summary>
        public int InOut { get; set; }
        /// <summary>
        /// 是否回复
        /// </summary>
        public string IsCallBack { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// 微信用户二维码
    /// </summary>
    [TableName("WxMemberQRs")]
    [PrimaryKey("QrID")]
    public partial class MemberQR
    {
        public int QrID { get; set; }
        /// <summary>
        /// 所属商户
        /// </summary>	
        public string OwnerCode { get; set; }
        public int Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int SceneID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Ticket { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string URL { get; set; }
        public string EmployeeID { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    /// <summary>
    /// 微信客户地址
    /// </summary>
    [TableName("WxMemberAddresses")]
    [PrimaryKey("AddressID")]
    public partial class MemberAddress
    {
        public int AddressID { get; set; }
        public int MemberID { get; set; }
        public string ConsigneeAlias { get; set; }
        public string ConsigneeName { get; set; }

        public string ConsigneeMobile { get; set; }

        public string ConsigneeAddress { get; set; }
        public int IsDefault { get; set; }

        public Nullable<DateTime> CreatedDate { get; set; }
    }

    /// <summary>
    /// 自定义消息
    /// </summary>
    [TableName("WxMessages")]
    [PrimaryKey("MsgId")]
    public partial class Message
    {
        public int MsgID { get; set; }
        /// <summary>
        /// 所属商户
        /// </summary>	
        public string OwnerCode { get; set; }
        public string MsgType { get; set; }
        public string Event { get; set; }
        public string EventKey { get; set; }
        public string ReturnType { get; set; }
        public string Ask { get; set; }
        public string Content { get; set; }
        public string MusicUrl { get; set; }
        public string HQMusicUrl { get; set; }
        public string PicUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string FuncFlag { get; set; }
    }

    /// <summary>
    /// 自定义消息主题
    /// </summary>
    [TableName("WxMessageArticles")]
    [PrimaryKey("ArticleId")]
    public partial class MessageArticle
    {
        public int ArticleId { get; set; }
        public int MsgId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PicUrl { get; set; }
        public string Url { get; set; }
    }

    /// <summary>
    /// 微信卡券
    /// </summary>
    [TableName("WxCards")]
    [PrimaryKey("ID")]
    public partial class WeixinCard
    {
        public string ID { get; set; }
        /// <summary>
        /// 所属商户
        /// </summary>	
        public string OwnerCode { get; set; }
        public string CardType { get; set; }
        /// <summary>
        /// 代金券 起用金额  元
        /// </summary>
        public Nullable<decimal> LeastCost { get; set; }
        /// <summary>
        /// 代金券 减免金额  元
        /// </summary>
        public Nullable<decimal> ReduceCost { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }
        /// <summary>
        /// 券名，字数上限为9 个汉字。(建议涵盖卡券属性、服务及金额)
        /// 必填
        /// </summary>
        public string Title { get; set; }

        public string DateType { get; set; }
        /// <summary>
        /// 固定日期区间专用，表示起用时间。从1970 年1 月1 日00:00:00 至起用时间的秒数，最终需转换为字符串形态传入，下同。（单位为秒）
        /// 必填
        /// </summary>
        public Nullable<DateTime> BeginTimestamp { get; set; }
        /// <summary>
        /// 固定日期区间专用，表示结束时间。（单位为秒）
        /// 必填
        /// </summary>
        public Nullable<DateTime> EndTimestamp { get; set; }
    }

}
