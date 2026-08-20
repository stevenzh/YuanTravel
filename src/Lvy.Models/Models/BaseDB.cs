using PetaPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lvy.Models.BaseDB
{
    /// <summary>
    /// 业务日志
    /// </summary>
    [TableName("BizLog")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BizLogModel : BaseModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 消息发发送给谁
        /// </summary>
        public string SendTo { get; set; }

        /// <summary>
        /// 0:不发送微信 1:发送微信  2:已发送微信
        /// </summary>
        public int SendWeixin { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 关联编号
        /// </summary>
        public string JoinCode { get; set; }

        /// <summary>
        /// 事件说明
        /// 例如：换团、修改订单。。。
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// json数据
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// json数据 ，与data有关联的数据集合
        /// </summary>
        public string Data2 { get; set; }

        /// <summary>
        /// 关联链接
        /// </summary>
        public string LinkUrl { get; set; }

        /// <summary>
        /// 状态 0 初始 1 已读
        /// </summary>
        public int Status { get; set; }

        public string TeamID { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        public string OwnerCode { get; set; }
    }

    /// <summary>
    /// 番号取得
    /// </summary>
    public class SysSequenceModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// SeqType
        /// </summary>
        public string SeqType { get; set; }

        /// <summary>
        /// SeqNo
        /// </summary>
        public int SeqNo { get; set; }

        /// <summary>
        /// YEAR
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// MONTH
        /// </summary>
        public string Month { get; set; }
    }

    /// <summary>
    /// 航空公司信息
    /// </summary>
    [TableName("BaseAirlines")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BaseAirlineModel
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string ShortName { get; set; }

        public string FullName { get; set; }

        public string ContactName { get; set; }

        public string ContactPhone { get; set; }
        public int IsValid { get; set; }
    }

    /// <summary>
    /// 目的地
    /// </summary>
    [TableName("BaseDestination")]
    [PrimaryKey("Id")]
    public class BaseDestinationModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联编号
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        public string PinYin { get; set; }

        /// <summary>
        /// 简拼
        /// </summary>
        public string JPinYin { get; set; }

        /// <summary>
        /// 级别 级别 5:国家  10:省 15:市  20：景区目的地
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 状态     0 无效  1 有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 是否国内 0-境外 1-国内
        /// </summary>
        public int IsChina { get; set; }

        /// <summary>
        /// ModifiedBy
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 点击次数
        /// </summary>
        public int ClickCnt { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ParentStr { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// 行政区码（国标）
        /// </summary>
        public string RegionCode { get; set; }

        /// <summary>
        /// TreeId
        /// </summary>
        [PetaPoco.ResultColumn]
        public string LevelName { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    [TableName("BaseDictionary")]
    [PrimaryKey("Id")]
    public class BaseDictionaryModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 数据类型名称
        /// </summary>
        public string Name { get; set; }

        public string TableName { get; set; }
        public string FieldName { get; set; }
        public int IsValid { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    [TableName("BaseDictionaryDetail")]
    [PrimaryKey("Id")]
    public class BaseDictionaryDetailModel
    {
        public int Id { get; set; }
        public int DicId { get; set; }
        public string Name { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public int IsValid { get; set; }

        /// <summary>
        /// 扩展字段 团号前缀
        /// </summary>
        public string JPinYin { get; set; }
    }

    /// <summary>
    /// 类似同业线路月报
    /// </summary>
    [TableName("BaseFileRes")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BaseFileResModel : BaseModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 文件显示名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件路径（上传到文件服务器）
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 1：门票  2：团队游
        /// </summary>
        public int ResType { get; set; }

        /// <summary>
        /// 排序权重
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否有效 0：无效  1：有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// (修改人)
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string MediaType { get; set; }
    }

    /// <summary>
    /// 文章信息表
    /// </summary>
    [TableName("BaseArticles")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BaseArticleModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 类型 1.系统公告
        /// 2.国内短线公告
        /// 3.国内长线公告
        /// 4.境外游公告
        /// </summary>
        public int NoticeType { get; set; }

        /// <summary>
        /// 状态     0 无效  1 有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Contents { get; set; }

        /// <summary>
        /// 原始地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// 首图
        /// </summary>
        public string ImgUrl { get; set; }

        public string Tags { get; set; }
        public int BrowseCnt { get; set; }

        [ResultColumn]
        public List<BaseArticleCommentModel> Comments { get; set; }

        [ResultColumn]
        public string[] SelectedMutliTags { get; set; }
    }

    [TableName("BaseArticleComments")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BaseArticleCommentModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 文章ID
        /// </summary>
        public long ArticleId { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Contents { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreatedBy { get; set; }
    }

    [TableName("BaseArticleBrowses")]
    [PrimaryKey("Id")]
    public partial class BaseArticleBrowseModel
    {
        public long Id { get; set; }
        public long ArticleId { get; set; }
        public DateTime CreatedTime { get; set; }
        public string IPAdress { get; set; }
        public string RegionCode { get; set; }
        public string CityName { get; set; }
    }

    /// <summary>
    /// 系统日志
    /// </summary>
    [TableName("BaseTasks")]
    [PrimaryKey("TaskID")]
    [Serializable]
    public class BaseTaskModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long TaskID { get; set; }

        public long? ParentID { get; set; }
        public string TeamID { get; set; }

        /// <summary>
        /// 发起人
        /// </summary>
        public string Originator { get; set; }

        /// <summary>
        /// 流程ID
        /// </summary>
        public int WorkFlowID { get; set; }

        /// <summary>
        /// 申请内容
        /// </summary>
        public string Contents { get; set; }

        public string JsonData { get; set; }

        /// <summary>
        /// 状态 0:起始  10: 同意 20: 驳回  90：取消
        /// </summary>
        public int Status { get; set; }

        public string WorkmanTeam { get; set; }

        /// <summary>
        /// 受理人
        /// </summary>
        public string Workman { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime? OperateTime { get; set; }

        /// <summary>
        /// 发起时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        public string OwnerCode { get; set; }

        [ResultColumn]
        public List<BaseTaskModel> SubTasks { get; set; }
    }

    /// <summary>
    /// 景区
    /// </summary>
    [TableName("BasePlace")]
    [PrimaryKey("PlaceId")]
    [Serializable]
    public class BasePlaceModel : BaseModel
    {
        [Description("自增序号")]
        public int PlaceId { get; set; }

        [Description("景区编号")]
        public string PlaceCode { get; set; }

        [Description("景区名称")]
        public string PlaceName { get; set; }

        /// <summary>
        /// 简单描述
        /// </summary>
        public string SimpleDesc { get; set; }

        [Description("景区描述")]
        public string PlaceDesc { get; set; }

        [Description("开放时间")]
        public string OpenTime { get; set; }

        [Description("景区星级")]
        public int PlaceLevel { get; set; }

        [Description("修改人")]
        public string ModifiedBy { get; set; }

        [Description("修改时间")]
        public DateTime ModifiedTime { get; set; }

        [Description("全拼")]
        public string PinYin { get; set; }

        [Description("简拼")]
        public string JPinYin { get; set; }

        /// <summary>
        /// 景区状态 0无效 1有效
        /// </summary>
        [Description("景区状态")]
        public int IsValid { get; set; }

        [Description("是否免费")]
        public int IsFree { get; set; }

        /// <summary>
        /// 关联目的地ID
        /// </summary>
        public string DestinationStr { get; set; }

        public string OwnerCode { get; set; }

        [ResultColumn]
        public List<BasePlacePhotoModel> Photos { get; set; }

        /// <summary>
        /// 目的地名称
        /// </summary>
        [ResultColumn]
        public string DestinationName { get; set; }
    }

    /// <summary>
    /// 景区图片
    /// </summary>
    [TableName("BasePlacePhoto")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BasePlacePhotoModel : BaseModel
    {
        /// <summary>
        /// 自增列
        /// </summary>
        public int Id { get; set; }

        public string PlaceCode { get; set; }

        /// <summary>
        /// 文件显示名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 相对路径 （含文件名）
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 是否封面
        /// </summary>
        public int IsLogo { get; set; }

        /// <summary>
        /// 排序权重
        /// </summary>
        public int Sort { get; set; }

        public int IsValid { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// (修改人)
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }
    }

    /// <summary>
    /// 标签表
    /// </summary>
    [TableName("BaseTag")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BaseTagModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 标签类型 1：线路主题
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// 命中次数 用户输入tagname相同次数
        /// </summary>
        public int Hit { get; set; }

        /// <summary>
        /// 点击次数
        /// </summary>
        public int ClickCnt { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        public int IsValid { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }
    }

    /// <summary>
    /// 上车地点分组
    /// </summary>
    [TableName("BusPointGroup")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BusPointGroupModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 出发城市
        /// </summary>
        public string OutCity { get; set; }


        /// <summary>
        /// 是否有效    0:无效;1:有效;
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        [ResultColumn]
        public string OutCityName { get; set; }
    }

    /// <summary>
    /// 基础上车地点表
    /// </summary>
    [TableName("BaseBusPoint")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BaseBusPointModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 出发城市
        /// </summary>
        public string OutCity { get; set; }
        /// <summary>
        /// 发车地点
        /// </summary>
        public string BusPoint { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 发车时间
        /// </summary>
        public string JieSongTime { get; set; }

        /// <summary>
        /// 接送类型 1：只接不送 2：只送不接 3：接送
        /// </summary>
        public int JsType { get; set; }

        /// <summary>
        /// 状态     0 无效  1 有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 组别Id    Example:1|2|3
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }
        [ResultColumn]
        public string OutCityName { get; set; }
    }

    /// <summary>
    /// 领队信息表
    /// </summary>
    [TableName("BaseGuides")]
    [PrimaryKey("Id")]
    [Serializable]
    public class GuideModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 部门/组
        /// </summary>
        public string TeamID { get; set; }

        [PetaPoco.ResultColumn]
        public string TeamName { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string PinYin { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// 出生地
        /// </summary>
        public string BirthPlace { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string Card { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string Tel { get; set; }

        /// <summary>
        /// Eamil
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 住址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 是否有导游证
        /// </summary>
        public string TourKey { get; set; }

        /// <summary>
        /// 导游证号
        /// </summary>
        public string TourCard { get; set; }

        /// <summary>
        /// 年检时间
        /// </summary>
        public DateTime? CheckDate { get; set; }

        /// <summary>
        /// 是否有领队证
        /// </summary>
        public string LeadKey { get; set; }

        /// <summary>
        /// IC卡登记号
        /// </summary>
        public string ICCard { get; set; }

        /// <summary>
        /// 领队证号
        /// </summary>
        public string LeadCard { get; set; }

        /// <summary>
        /// 领队证有效期
        /// </summary>
        public DateTime? DateStart { get; set; }

        /// <summary>
        /// 领队证有效期
        /// </summary>
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// 护照种类
        /// </summary>
        public int Hzzl { get; set; }

        /// <summary>
        /// 护照号
        /// </summary>
        public string Hzno { get; set; }

        /// <summary>
        /// 护照签发地
        /// </summary>
        public string HzAddress { get; set; }

        /// <summary>
        /// 护照有效期
        /// </summary>
        public DateTime? HzDate { get; set; }

        /// <summary>
        /// 护照有效期
        /// </summary>
        public DateTime? HzEndDate { get; set; }

        /// <summary>
        /// 分类1
        /// </summary>
        public int WorkType1 { get; set; }

        /// <summary>
        /// 分类2
        /// </summary>
        public int WorkType2 { get; set; }

        /// <summary>
        /// 可带团类型
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 工作经历
        /// </summary>
        public string WorkRemark { get; set; }

        /// <summary>
        /// 语种
        /// </summary>
        [PetaPoco.ResultColumn]
        public string YuZhong { get; set; }

        /// <summary>
        /// 导游分类
        /// </summary>
        [PetaPoco.ResultColumn]
        public int GuideType { get; set; }

        /// <summary>
        /// 客户编号
        /// </summary>
        public string OwnerCode { get; set; }
    }

    /// <summary>
    /// 导游附件
    /// </summary>
    [TableName("BaseBrands")]
    [PrimaryKey("ID")]
    [Serializable]
    public class BrandModel
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TeamID { get; set; }
        public int IsValid { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string OwnerCode { get; set; }
    }

    /// <summary>
    /// 收款记录
    /// </summary>
    [TableName("VT_Proceeds")]
    [PrimaryKey("Id")]
    [Serializable]
    public class VTProceedsModel
    {
        /// <summary>
        /// 自增列
        /// </summary>
        public long Id { set; get; }

        /// <summary>
        /// 缴款单号
        /// </summary>
        public string ProceedsCode { set; get; }

        /// <summary>
        /// 业务员编码
        /// </summary>
        public string ChargerId { set; get; }

        /// <summary>
        ///  业务员姓名
        /// </summary>
        public string ChargerName { set; get; }

        /// <summary>
        /// 业务部门
        /// </summary>
        public string ChargerDept { set; get; }

        /// <summary>
        /// 交款客户
        /// </summary>
        public string ChargerHost { set; get; }

        /// <summary>
        /// 收款事由
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string MultiType { set; get; }

        public int? IsReceive { get; set; }

        /// <summary>
        /// 缴款金额
        /// </summary>
        public decimal? ReceiveSum { set; get; }

        /// <summary>
        /// 缴款金额大写
        /// </summary>
        [PetaPoco.Ignore]
        public string ReceiveSumChina { set; get; }

        /// <summary>
        /// 交款日期
        /// </summary>
        public DateTime? ProceedsDate { set; get; }

        /// <summary>
        /// 收款人编码
        /// </summary>
        public string CheckId { set; get; }

        /// <summary>
        /// 收款人姓名
        /// </summary>
        public string CheckName { set; get; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public string Remark { set; get; }

        /// <summary>
        ///
        /// </summary>
        public int IsValid { set; get; }
    }

    /// <summary>
    /// 相册
    /// </summary>
    [TableName("Photo_Album")]
    [PrimaryKey("PhotoAlbumId")]
    public partial class PhotoAlbumModel
    {
        /// <summary>
        /// 相册ID
        /// </summary>
        public long PhotoAlbumId { get; set; }

        /// <summary>
        /// 封面ID
        /// </summary>
        public long CoverPhotoId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 图片数量
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public long AreaId { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Seq { get; set; }

        /// <summary>
        /// 创建时间戳
        /// </summary>
        public DateTime CreateTs { get; set; }

        /// <summary>
        /// 更新时间戳
        /// </summary>
        public DateTime UpdateTs { get; set; }

        /// <summary>
        /// 图册名称
        /// </summary>
        public string AlbumName { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        [ResultColumn]
        public PhotoInfoModel ConverPhoto { get; set; }

        [ResultColumn]
        public DateTime? updateTime { get; set; }

        [ResultColumn]
        public string AreaName { get; set; }
    }

    /// <summary>
    /// 相册中的图片记录
    /// </summary>
    [TableName("Photo_Info")]
    [PrimaryKey("PhotoId")]
    public partial class PhotoInfoModel
    {
        public long PhotoId { get; set; }
        public string Caption { get; set; }
        public long AlbumId { get; set; }
        public string Url { get; set; }
        public int Seq { get; set; }
        public DateTime CreateTs { get; set; }
        public DateTime UpdateTs { get; set; }
        public string Operator { get; set; }
        public int Status { get; set; }

        //图片宽度
        [ResultColumn]
        public int PhotoWidth { get; set; }

        //图片高度
        [ResultColumn]
        public int PhotoHeight { get; set; }

        [ResultColumn]
        public string AreaStr { get; set; }

        [ResultColumn]
        public string AlbumName { get; set; }

        [ResultColumn]
        public int AreaId { get; set; }
    }

    /// <summary>
    /// 资源站点使用 yuanfile.sh-cct.cn
    /// </summary>
    [TableName("BaseFiles")]
    [PrimaryKey("Id")]
    [Serializable]
    public class BaseFileModel : BaseModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 文件显示名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件路径（上传到文件服务器）
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 是否有效 0：无效  1：有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// (修改人)
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string MediaType { get; set; }
    }
}