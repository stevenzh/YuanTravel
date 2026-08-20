using System.Runtime.Serialization;
using System;
using System.Collections.Generic;

namespace Lvy.Visa.Models.API
{

    public class GetVisaAreaResponse
    {
        public List<VisaAreaData> AreaList { get; set; }
    }


    public class GetVisaResponse
    {
        public List<VisaInformationData> ProductList { get; set; }
    }


    public class VisaAreaData
    {
        /// <summary>
        /// 区域编码
        /// </summary>
        public int VisaAreaCode { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string VisaAreaName { get; set; }

        /// <summary>
        /// 产品列表
        /// </summary>
        public List<VisaInformationData> VisaProductList { get; set; }
    }


    public class VisaInformationData
    {
        /// <summary>
        /// 产品Id 主键
        /// </summary>
        public int InformationId { get; set; }
        /// <summary>
        /// 产品编码
        /// </summary>
        public string InformationCode { get; set; }
        /// <summary>
        /// 类型 （个签，团签）
        /// </summary>
        public int VType { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string InformationName { get; set; }
        /// <summary>
        ///  产品类型
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// 签证类型
        /// </summary>
        public int VisaType { get; set; }
        /// <summary>
        /// 签证类型名称
        /// </summary>
        public string VisaTypeName { get; set; }
        /// <summary>
        /// 面试类型
        /// </summary>
        public int InterviewType { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 是否有效
        /// </summary>
        public int IsValid { get; set; }
        /// <summary>
        /// 所属洲(1,亚洲;2,欧洲;3,非洲;4,美洲;5,大洋洲)
        /// </summary>
        public int Continent { get; set; }
        /// <summary>
        /// 签证地区
        /// </summary>
        public string ContinentName { get; set; }
        /// <summary>
        ///  签证国家或地区
        /// </summary>
        public string VisaCountry { get; set; }
        /// <summary>
        /// 签证国家或地区 ParentStr  Area_Info 表里面对应的ParentStr值
        /// </summary>
        public string VisaCountryParentStr { get; set; }
        /// <summary>
        /// 所属领区(1,北京领区;2,上海领区;3,广州领区;4,武汉领区;5，成
        /// </summary>
        public string VisaArea { get; set; }
        /// <summary>
        /// 销售价格
        /// </summary>
        public decimal SellPrice { get; set; }
        /// <summary>
        /// 成本价
        /// </summary>
        public decimal CostPrice { get; set; }
        /// <summary>
        /// 受理时间
        /// </summary>
        public string AcceptedTime { get; set; }
        /// <summary>
        /// 受理范围
        /// </summary>
        public string AcceptedRange { get; set; }
        /// <summary>
        /// 签证有效期
        /// </summary>
        public string VisaExpiryDate { get; set; }
        /// <summary>
        /// 停留天数
        /// </summary>
        public string StayDays { get; set; }
        /// <summary>
        /// 入境次数
        /// </summary>
        public string EnterCount { get; set; }
        /// <summary>
        /// 是否需要担保
        /// </summary>
        public int? IsDanBao { get; set; }
        /// <summary>
        /// 预定须知
        /// </summary>
        public string BookingNodes { get; set; }
        /// <summary>
        /// 特别提示
        /// </summary>
        public string WarmTips { get; set; }
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImgUrl { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 录入人
        /// </summary>
        public string CreateBy { get; set; }
        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime Createtime { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string PManageUser { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? PManageDate { get; set; }
        /// <summary>
        /// 上线人
        /// </summary>
        public string OnlineUser { get; set; }
        /// <summary>
        /// 上线人姓名
        /// </summary>
        public string OnlineUserName { get; set; }
        /// <summary>
        /// 上线时间
        /// </summary>
        public DateTime? OnlineDate { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateByName { get; set; }
        /// <summary>
        /// 审核人(产品经理)
        /// </summary>
        public string PManageUserName { get; set; }
        /// <summary>
        /// 居住/护照签发地范围
        /// </summary>
        public string LivePassportArea { get; set; }
        /// <summary>
        /// 签发地编码
        /// </summary>
        public string VisaIssuePlace { get; set; }
        /// <summary>
        /// 签发地名称
        /// </summary>
        public string VisaIssuePlaceName { get; set; }
        /// <summary>
        /// 付款时限
        /// </summary>
        public int PayTimeLimit { get; set; }
        /// <summary>
        /// 提前预定天数
        /// </summary>
        public int? AdvanceDays { get; set; }
        /// <summary>
        /// 是否可以加急办理  值
        /// </summary>
        public int? IsHurry { get; set; }
        /// <summary>
        /// 是否可以加急办理  名称  可以 不可以
        /// </summary>
        public string IsHurryName { get; set; }
        /// <summary>
        /// 签证国家值
        /// </summary>
        public string CountryProValue { get; set; }
    }
}
