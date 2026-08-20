using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Visa.Models
{
    /// <summary>
    /// 签证分类
    /// </summary>
    [TableName("Visa_Category")]
    [PrimaryKey("CategoryId")]
    public partial class VisaCategoryModel
    {
        /// <summary>
        /// 分类id 主键
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 分类编码
        /// </summary>
        public string CategoryCode { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string InformationCode { get; set; }

        /// <summary>
        /// 是不是第一次添加分类
        /// </summary>
        [ResultColumn]
        public int IsFirst { get; set; }
    }

    /// <summary>
    /// 签证国家对应领区
    /// </summary>
    [TableName("Visa_Country_ConsularDistrict")]
    [PrimaryKey("Id")]
    public partial class VisaCountryConsularDistrictModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 领区编码
        /// </summary>
        public string ConsularDistrictCode { get; set; }

        /// <summary>
        /// 国家编码
        /// </summary>
        public string VisaCountryCode { get; set; }

        /// <summary>
        /// 国家编码parentstr
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// 领区key
        /// </summary>
        public string ConsularDistrictKey { get; set; }

        /// <summary>
        /// 受理范围
        /// </summary>
        public string AcceptRange { get; set; }

        /// <summary>
        /// 录入人
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string ModifyBy { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 签证国家
        /// </summary>
        [ResultColumn]
        public string CountryName { get; set; }

        /// <summary>
        /// 签证领馆
        /// </summary>
        [ResultColumn]
        public string VisaAreaValue { get; set; }
    }

    /// <summary>
    /// 签证国家
    /// </summary>
    [TableName("Visa_CountryInfo")]
    [PrimaryKey("Id")]
    public partial class VisaCountryInfoModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string VisaCountryCode { get; set; }

        /// <summary>
        /// 国家编码
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// 国旗图片路径
        /// </summary>
        public string CountryImgPath { get; set; }

        /// <summary>
        /// 领区详解
        /// </summary>
        public string ConsularDistrictNotes { get; set; }

        /// <summary>
        /// 录入人
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string ModifyBy { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        public string OwnerCode { get; set; }

        /// <summary>
        /// 国家名称
        /// </summary>
        [ResultColumn]
        public string CountryName { get; set; }
    }

    /// <summary>
    /// 签证常见问题
    /// </summary>
    [TableName("Visa_CountryQuestion")]
    [PrimaryKey("Id")]
    public partial class VisaCountryQuestionModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string QuestionCode { get; set; }

        /// <summary>
        /// 国家编码
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// 国家名称
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// 问题
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// 答案
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifyBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifyDate { get; set; }
        public string OwnerCode { get; set; }
    }

    /// <summary>
    /// 签证产品
    /// </summary>
    [TableName("Visa_Information")]
    [PrimaryKey("InformationId")]
    public partial class VisaInformationModel
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int InformationId { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string InformationCode { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// 类型 （1:个签，2:团签）
        /// </summary>
        public int VType { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string InformationName { get; set; }


        /// <summary>
        /// 签证类型 1-旅游签证 2-商务签证 3-探访亲友
        /// </summary>
        public int VisaType { get; set; }

        /// <summary>
        /// 签证类型  1-需要面试 2-不需要面试 3-抽签面试
        /// </summary>
        public int InterviewType { get; set; }

        /// <summary>
        /// 1录入中 2提交审核 3审核中 4审核不通过 5上线 6下线
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
        /// 签证国家或地区
        /// </summary>
        public string VisaCountry { get; set; }

        /// <summary>
        /// 签证国家或地区 ParentStr  Area_Info 表里面对应的ParentStr值
        /// </summary>
        public string VisaCountryParentStr { get; set; }

        /// <summary>
        ///  所属领区 1-北京领区;2,上海领区;3,广州领区;4,武汉领区;5-成都领区 6-沈阳领区 7-四川领区
        /// </summary>
        public string VisaArea { get; set; }

        /// <summary>
        /// 销售价格
        /// </summary>
        public decimal SellPrice { get; set; }

        /// <summary>
        /// 同业价格
        /// </summary>
        public decimal TradePrice { get; set; }

        /// <summary>
        /// 成本
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
        ///  入境次数
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
        /// 温馨提示
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
        /// 创建人编码
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateByName { get; set; }

        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime Createtime { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string PManageUser { get; set; }

        /// <summary>
        /// 审核人(产品经理)
        /// </summary>
        public string PManageUserName { get; set; }

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
        ///
        /// </summary>
        public string LivePassportArea { get; set; }

        /// <summary>
        /// 材料是否分类
        /// </summary>
        public int IsCategory { get; set; }

        /// <summary>
        /// 签证签发地
        /// </summary>
        public string VisaIssuePlace { get; set; }

        /// <summary>
        /// 付款时限
        /// </summary>
        public int PayTimeLimit { get; set; }

        /// <summary>
        /// 提前预定天数
        /// </summary>
        public int? AdvanceDays { get; set; }

        /// <summary>
        /// 是否可以加急办理 0 1
        /// </summary>
        public int? IsHurry { get; set; }
        public string OwnerCode { get; set; }



        /// <summary>
        /// 是否可以加急办理  名称 可以 不可以
        /// </summary>
        [ResultColumn]
        public string IsHurryName { get; set; }
        [ResultColumn]
        public string SupplierName { get; set; }

        [ResultColumn]
        public string VTypeValue { get; set; }

        [ResultColumn]
        public string StateValue { get; set; }

        [ResultColumn]
        public string VisaTypeValue { get; set; }

        [ResultColumn]
        public string ContinentValue { get; set; }

        [ResultColumn]
        public string VisaAreaValue { get; set; }

        [ResultColumn]
        public string VisaIssuePlaceName { get; set; }

        [ResultColumn]
        public string[] SelectPassportArea { get; set; }

        [ResultColumn]
        public string InterviewTypeValue { get; set; }

        [ResultColumn]
        public string TeamName { get; set; }
        /// <summary>
        /// 操作备注
        /// </summary>
        [ResultColumn]
        public string Remarks { get; set; }
    }

    /// <summary>
    /// 签证材料
    /// </summary>
    [TableName("Visa_Data")]
    [PrimaryKey("DataId")]
    public partial class VisaDataModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int DataId { get; set; }

        /// <summary>
        /// 签证材料编码
        /// </summary>
        public string DataCode { get; set; }

        /// <summary>
        /// 分类编码
        /// </summary>
        public string CategoryCode { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string DataName { get; set; }

        /// <summary>
        /// 签证说明
        /// </summary>
        public string DataExplain { get; set; }

        /// <summary>
        ///  是否必须
        /// </summary>
        public int IsNeed { get; set; }

        /// <summary>
        /// 是否模板
        /// </summary>
        public int IsTemplate { get; set; }

        /// <summary>
        ///  产品编码
        /// </summary>
        public string InformationCode { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Createtime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifyBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? Modifytime { get; set; }

        /// <summary>
        /// 是否源文件
        /// </summary>
        public int IsOriginal { get; set; }

        /// <summary>
        /// 材料数量
        /// </summary>
        public int DataCount { get; set; }

        /// <summary>
        /// 是否退还
        /// </summary>
        public int IsBack { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 材料附件列表
        /// </summary>
        [ResultColumn]
        public List<VisaDataFileModel> DataFilesList { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        [ResultColumn]
        public string CategoryName { get; set; }
    }

    /// <summary>
    /// 签证材料分类中间表
    /// </summary>
    [TableName("Visa_Data_Category")]
    [PrimaryKey("Data_CategoryId")]
    public partial class VisaDataCategoryModel
    {
        public int Data_CategoryId { get; set; }
        public string Data_CategoryCode { get; set; }
        public string DataCode { get; set; }
        public string CategoryCode { get; set; }
        public string CreateBy { get; set; }
        public DateTime Createtime { get; set; }
    }

    /// <summary>
    /// 签证材料附件表
    /// </summary>
    [TableName("Visa_DataFiles")]
    [PrimaryKey("FilesId")]
    public partial class VisaDataFileModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int FilesId { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string FilesCode { get; set; }

        /// <summary>
        /// 签证材料编码
        /// </summary>
        public string DataCode { get; set; }

        /// <summary>
        /// 附件地址
        /// </summary>
        public string FileUrl { get; set; }

        /// <summary>
        /// 附件说明
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Createtime { get; set; }

        public string InformationCode { get; set; }
    }

    /// <summary>
    /// 产品操作历史记录
    /// </summary>
    [TableName("Visa_Information_OperateHistory")]
    [PrimaryKey("HistoryId")]
    public partial class VisaInformationOperateHistoryModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int HistoryId { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string HistoryCode { get; set; }

        /// <summary>
        /// 操作人姓名
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 操作人编码
        /// </summary>
        public string OperatorLoginCode { get; set; }

        /// <summary>
        /// 操作人Ip
        /// </summary>
        public string OperatorIp { get; set; }

        /// <summary>
        /// 操作对象名称
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// 操作对象编码
        /// </summary>
        public string ObjectCode { get; set; }

        /// <summary>
        /// 操作内容
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperatorTime { get; set; }

        /// <summary>
        /// 签证产品状态
        /// </summary>
        public int VState { get; set; }

        [ResultColumn]
        public string VStateValue { get; set; }
    }


    /// <summary>
    /// 签证订单
    /// </summary>
    [TableName("Visa_Order")]
    [PrimaryKey("Id")]
    public partial class VisaOrderModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 订单号 对应总订单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 签证产品编码
        /// </summary>
        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        /// <summary>
        /// 操作状态 1-已预订 | 2-已确认 | 3-材料收取中 |4-材料已收齐| 5-已送签 | 6-材料已审核 | 7-签出
        /// </summary>
        public int TraceState { get; set; }

        /// <summary>
        ///销售价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 总人数 - 份数
        /// </summary>
        public int TotalNum { get; set; }

        /// <summary>
        /// 特殊要求
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 送签日期
        /// </summary>
        public DateTime? SendVisaDate { get; set; }

        /// <summary>
        /// 签出日期
        /// </summary>
        public DateTime? FinishVisaDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string Midifier { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? MidifyDate { get; set; }

        /// <summary>
        /// 操作人姓名
        /// </summary>
        public string OperateName { get; set; }

        /// <summary>
        /// 材料截止收取日期
        /// </summary>
        public DateTime? MaterialDeadline { get; set; }

        /// <summary>
        /// 面试日期
        /// </summary>
        public DateTime? InterviewDate { get; set; }

        /// <summary>
        /// 跟进日期
        /// </summary>
        public DateTime? FollowupDate { get; set; }

        [ResultColumn]
        public string TraceStatusName { get; set; }
        [ResultColumn]
        public string PManageName { get; set; }
    }

    /// <summary>
    /// 申请人 游客
    /// </summary>
    [TableName("Visa_Applicanter")]
    [PrimaryKey("Id")]
    public partial class VisaApplicanterModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 类型（成人，儿童）
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        public string Pinyin { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int? Sex { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 常用联系人id
        /// </summary>
        public int? LinkmanID { get; set; }

        /// <summary>
        /// 签证状态(1=等待处理、2=出签成功、3=出签失败)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 材料分类
        /// </summary>
        public string Categorycode { get; set; }

        public int IsValid { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? MidifiedTime { get; set; }

        public int CardType { get; set; }

        public string CardNo { get; set; }

        [ResultColumn]
        public string CategoryName { get; set; }

        [ResultColumn]
        public string VisaStateName { get; set; }

    }

    /// <summary>
    /// 订单操作历史记录
    /// </summary>
    [TableName("Visa_OperationHistory")]
    [PrimaryKey("Id")]
    public partial class VisaOperationHistoryModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public int? OperateType { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public string OperateId { get; set; }

        /// <summary>
        /// 操作人姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 操作人IP
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 操作人角色
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// 操作内容
        /// </summary>
        public string OperateContent { get; set; }

        public DateTime? OperateDate { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    //[TableName("Visa_Paperwork")]
    //[PrimaryKey("Id")]
    //public partial class VisaPaperworkModel
    //{
    //    /// <summary>
    //    /// 主键ID
    //    /// </summary>
    //    public int Id { get; set; }
    //    /// <summary>
    //    /// 申请人编码
    //    /// </summary>
    //    public string AppCode { get; set; }
    //    /// <summary>
    //    /// 证件类型
    //    /// </summary>
    //    public int? PaperworkType { get; set; }
    //    /// <summary>
    //    /// 证件号码
    //    /// </summary>
    //    public string PaperworkNo { get; set; }
    //    /// <summary>
    //    /// 签发地
    //    /// </summary>
    //    public string VisaPlace { get; set; }
    //    /// <summary>
    //    /// 创建时间
    //    /// </summary>
    //    public DateTime? creatDate { get; set; }
    //    /// <summary>
    //    /// 修改时间
    //    /// </summary>
    //    public DateTime? MidifyDate { get; set; }
    //}

}