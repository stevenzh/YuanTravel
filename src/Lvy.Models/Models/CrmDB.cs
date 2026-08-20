using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Lvy.Models.CrmDB
{
    /// <summary>
    /// 账户表
    /// 包含系统用户、客户联系人、供应商联系人
    /// </summary>
    [TableName("CrmAccount")]
    [PrimaryKey("Code", AutoIncrement = false)]
    [Serializable]
    public class CrmAccountModel
    {
        /// <summary>
        /// 账户编号 非自增长
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 登陆名称
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 1.男  2.女  3.其他
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        public string QQ { get; set; }

        /// <summary>
        /// 1. 平台管理员
        /// 2. 系统管理员
        /// 3. 普通
        /// 9. 其它
        /// </summary>
        public int AccountType { get; set; }

        /// <summary>
        /// 工作职能  1、综合部 2、操作部 5、销售部 9、财务部
        /// </summary>
        public int DepartCode { get; set; }

        /// <summary>
        /// 对应客户编码
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 有效无效 0：无效 1：有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// ModifiedBy
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
        /// 联系人对应销售组
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// 联系人对应销售
        /// </summary>
        public string SalerCode { get; set; }

        /// <summary>
        /// 联系人状态  0：未审核 1：已审核 2:审核不通过
        /// </summary>
        public int SalerState { get; set; }

        /// <summary>
        /// 微信关联ID
        /// </summary>
        public string OpenID { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 头像路径
        /// </summary>
        public string ProfilePath { get; set; }

        [ResultColumn]
        public string SalerName { get; set; }

        /// <summary>
        /// 客户对象
        /// </summary>
        [ResultColumn]
        public CrmCustomerModel Customer { get; set; }

        /// <summary>
        /// 客户注册信息
        /// </summary>
        [ResultColumn]
        public CustomerRegistrationModel Registration { get; set; }

        /// <summary>
        /// 对应客户名称
        /// </summary>
        [ResultColumn]
        public string CustomerName { get; set; }

        /// <summary>
        /// 前台愿望单总数
        /// </summary>
        [ResultColumn]
        public int WishCount { get; set; }
        [ResultColumn]
        public bool IsOwnerUser
        {
            get { return CustomerCode == OwnerCode; }
        }
    }

    /// <summary>
    /// 账户消息表
    /// </summary>
    [TableName("CrmAccountMessages")]
    [PrimaryKey("ID", AutoIncrement = true)]
    [Serializable]
    public class CrmAccountMessagesModel
    {
        /// <summary>
        /// ID 自增长
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 账号编码
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 阅读状态 0 初始 1
        /// </summary>
        public string ReadStatus { get; set; }

        /// <summary>
        /// ModifiedBy
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }

    /// <summary>
    /// 愿望单 收藏夹
    /// </summary>
    public class CrmAccountWishModel
    {
        /// <summary>
        /// ID 自增长
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 账号编码
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 现有产品类型  线路：门票：签证
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// 管理产品  线路：门票：签证
        /// </summary>
        public string ProductCode { get; set; }

        public DateTime CreatedTime { get; set; }
    }

    /// <summary>
    /// 商户信息
    /// </summary>
    [TableName("CrmCustomer")]
    [PrimaryKey("Code", AutoIncrement = false)]
    [Serializable]
    public class CrmCustomerModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 商户名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 简称
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// 快捷码 提供快速查询功能
        /// </summary>
        public string FastCode { get; set; }

        /// <summary>
        /// 负责人
        /// </summary>
        public string Head { get; set; }

        /// <summary>
        /// 联系手机
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 公司电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 所属省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 所属城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 所属区县
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 信用额度
        /// </summary>
        public decimal CreditLine { get; set; }

        /// <summary>
        /// 结算方式 1.现结  2.月结  3.季结  4.年结
        /// </summary>
        public int PaymentType { get; set; }

        /// <summary>
        /// 销售code
        /// </summary>
        public string SalerCode { get; set; }

        /// <summary>
        /// LogoPath
        /// </summary>
        public string LogoPath { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 是否有效
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

        /// <summary>
        /// 父公司编号
        /// </summary>
        /// <remarks>
        /// 用于描述客户的父子关系
        /// </remarks>
        public string ParentCode { get; set; }

        /// <summary>
        /// 客户状态  0：未审核 1：已审核 2:审核不通过
        /// </summary>
        public int CustomerState { get; set; }

        /// <summary>
        /// 渠道（1：同业 2：平台 3：电商）
        /// </summary>
        public int ChannelType { get; set; }

        /// <summary>
        /// 是否 分销商
        /// </summary>
        public bool IsDistributors { get; set; }

        /// <summary>
        /// 是否门店
        /// </summary>
        public bool IsBranch { get; set; }

        /// <summary>
        /// 是否 系统拥有者
        /// </summary>
        public bool IsOwner { get; set; }

        /// <summary>
        /// 是否新客户 [三个月内审核通过的客户]
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// 是否 供应商
        /// </summary>
        public bool IsSupplier { get; set; }
        /// <summary>
        /// 是否组团社
        /// </summary>
        public bool IsGroupTour { get; set; }
        /// <summary>
        /// 产品入驻部门
        /// </summary>
        public string ImportTeam { get; set; }
        /// <summary>
        /// 经营许可  出境线路 国内线路  签证 门票 酒店
        /// </summary>
        public string BusinessPermit { get; set; }
        /// <summary>
        /// 账单是否显示折扣 false 不显示 true 显示 (不显示在账单 为后返， 显示账单为即使折让)
        /// </summary>
        public bool RebateInBill { get; set; }

        /// <summary>
        /// 作为分销商分属销售部门
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// 客户活跃状态  0：高度活跃 1活跃 2:普通，3：沉睡
        ///
        /// 根据下单频次设置 新建客户默认活跃，
        /// 近三个月都开单并且单量大于等于6为非常活跃，近三个月每月都有开单为活跃， 三个月有开单为普通 ，超过三个月没开单为沉睡
        /// </summary>
        public int ActiveState { get; set; }

        /// <summary>
        /// 客户近一个月未开单标记 0 有单  1：未开单
        /// </summary>
        public bool EmptyInMonth { get; set; }

        /// <summary>
        /// 最后一次领用时间
        /// </summary>
        public DateTime? ReceiveDate { get; set; }

        /// <summary>
        /// 创建时间，建议审核通过更新此日期
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 客户等级：0：普通，1：优良，2：优质
        ///
        /// 可以根据开单量，或交易金额判断
        /// </summary>
        public int? Rank { get; set; }

        /// <summary>
        /// 税号
        /// </summary>
        public string TaxNumber { get; set; }

        /// <summary>
        /// 银行账号信息
        /// [对公户名、开户银行、账号、开户行所在地]
        /// </summary>
        public string BankInfo { get; set; }

        /// <summary>
        /// 是否在公共区域
        /// </summary>
        public bool InPublic { get; set; }

        /// <summary>
        /// 是否有子结构
        /// </summary>
        public bool HasChild { get; set; }

        [PetaPoco.ResultColumn]
        public string ParentName { get; set; }

        /// <summary>
        /// 销售对象
        /// </summary>
        [PetaPoco.ResultColumn]
        public CrmAccountModel Saler { get; set; }

        /// <summary>
        /// 销售员
        /// </summary>
        [PetaPoco.ResultColumn]
        public string SalerName { get; set; }

        /// <summary>
        /// 联系人列表
        /// </summary>
        [PetaPoco.ResultColumn]
        public List<CrmAccountModel> ContactList { get; set; }
    }

    /// <summary>
    /// 组
    /// </summary>
    [TableName("CrmTeam")]
    [PrimaryKey("TeamID", AutoIncrement = false)]
    [Serializable]
    public class CrmTeamModel
    {
        public string TeamID { get; set; }
        public string TeamName { get; set; }

        /// <summary>
        /// 职能编码: 1、综合部 2、操作部 5、销售部 6、签证 7、挂靠 9、财务部
        /// 1-7 部门拥有全职业部门（销售、计调）
        /// </summary>
        public int DepartCode { get; set; }

        public string LeaderCode { get; set; }

        public string Remark { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }

        /// <summary>
        /// 状态     0 无效  1 有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 是否锁定产品名称 0不锁 1 锁定
        /// </summary>
        public int LockName { get; set; }

        /// <summary>
        /// 财务负责部门编码
        /// </summary>
        public string FinanceCode { get; set; }
    }

    /// <summary>
    /// 角色-功能关系表
    /// </summary>
    [TableName("TeamAccountMap")]
    [PrimaryKey("ID")]
    [Serializable]
    public class TeamAccountMapModel
    {
        /// <summary>
        /// 关系ID,自动增长
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 角色编码
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// 账号編码
        /// </summary>
        public string AccountCode { get; set; }
    }

    /// <summary>
    /// 客户注册提交信息
    /// </summary>
    [TableName("CustomerContract")]
    [PrimaryKey("ContractId")]
    [Serializable]
    public class CustomerRegistrationModel
    {
        public int ContractId { get; set; }
        public string CustomerCode { get; set; }

        /// <summary>
        /// 营业执照
        /// </summary>
        public string BusinessLicencePath { get; set; }

        /// <summary>
        /// 身份证
        /// </summary>
        public string IdCardPath { get; set; }
    }

    /// <summary>
    /// 模块功能表
    /// </summary>
    [TableName("SysFunction")]
    [PrimaryKey("Id")]
    [Serializable]
    public class SysFunctionModel
    {
        /// <summary>
        /// 模块功能ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 模块功能父编码
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 模块功能类型
        /// 1:模块
        /// 2:菜单
        /// 5:功能
        /// </summary>
        public int FuncType { get; set; }

        /// <summary>
        /// 模块功能名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 模块功能URL
        /// </summary>
        public string URL { get; set; }

        public string IconClass { get; set; }

        /// <summary>
        /// 模块功能描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 当前状态
        /// 1,有效
        /// 0,失效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 平台功能
        /// 1，平台级别
        /// 0，普通级别
        /// </summary>
        public int IsSuper { get; set; }

        /// <summary>
        /// CreatedBy
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// CreatedDate
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// ModifiedBy
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 所属模块
        /// </summary>
        [PetaPoco.ResultColumn]
        public string ModuleValue { get; set; }
    }

    /// <summary>
    /// 角色-功能关系表
    /// </summary>
    [TableName("SysPermissionMap")]
    [PrimaryKey("Id")]
    [Serializable]
    public class SysPermissionMapModel
    {
        /// <summary>
        /// 关系ID,自动增长
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 角色编码
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// 模块功能编码
        /// </summary>
        public int FunctionId { get; set; }
    }

    /// <summary>
    /// 平台
    /// </summary>
    [TableName("SysPlatform")]
    [PrimaryKey("Id")]
    [Serializable]
    public class SysPlatformModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 网址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 商户编号（用作关联）
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 图标地址
        /// </summary>
        public string IconPath { get; set; }

        /// <summary>
        /// C端LOGO
        /// </summary>
        public string SiteLogoPath { get; set; }
        /// <summary>
        /// 电子章地址
        /// </summary>
        public string ElecCertifyPath { get; set; }
        public bool IsValid { get; set; }
        public string Profile { get; set; }

        [Ignore]
        public List<KeyValueBean> ProfileModels { get; set; }

        [ResultColumn]
        public CrmCustomerModel CrmCustomer { get; set; }

        [ResultColumn]
        public string CacheKey { get; set; }
        [ResultColumn]
        public List<String> UrlList
        {
            get
            {
                return Url.Split(',').Where(t => t.IsNullOrEmpty() == false).ToList();
            }
        }
    }

    /// <summary>
    /// 角色
    /// </summary>
    [TableName("SysRole")]
    [PrimaryKey("Id")]
    [Serializable]
    public class SysRoleModel
    {
        /// <summary>
        /// 角色ID,自动增长
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 组描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// CreatedBy
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// CreatedDate
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// ModifiedBy
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
    }

    /// <summary>
    /// 用户角色表
    /// </summary>
    [TableName("SysUserRoleMap")]
    [PrimaryKey("Id")]
    [Serializable]
    public class SysUserRoleMapModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 账号編码
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 角色编号
        /// </summary>
        public string RoleId { get; set; }
    }

    /// <summary>
    /// 客户附件表
    /// </summary>
    [TableName("CustomerFiles")]
    [PrimaryKey("Id")]
    [Serializable]
    public class CustomerFileModel
    {
        /// <summary>
        /// 自增编号
        /// </summary>
        public int Id { set; get; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerCode { set; get; }

        /// <summary>
        /// 合同名称
        /// </summary>
        public string Subject { set; get; }

        /// <summary>
        /// 合同起始日期
        /// </summary>
        public DateTime? StratDate { set; get; }

        /// <summary>
        /// 合同结束日期
        /// </summary>
        public DateTime? EndDate { set; get; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { set; get; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { set; get; }

        /// <summary>
        /// 文件类型    image  document（doc or pdf）
        /// </summary>
        public string MediaType { get; set; }

        public string FilePath { set; get; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public int IsValid { get; set; }
    }

    /// <summary>
    /// 客户领用
    /// </summary>
    [TableName("CustomerHolds")]
    [PrimaryKey("Id")]
    [Serializable]
    public class CustomerHoldModel
    {
        /// <summary>
        /// 自增ID
        /// </summary>
        public int Id { set; get; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerCode { set; get; }

        /// <summary>
        /// 销售编码
        /// </summary>
        public string SalerCode { set; get; }

        /// <summary>
        /// 领用时间
        /// </summary>
        public DateTime HoldDate { set; get; }

        /// <summary>
        /// 记录创建时间
        /// </summary>
        public DateTime CreateDate { set; get; }
    }

    /// <summary>
    /// 客户折让协议规则
    /// </summary>
    [TableName("CustomerPolicys")]
    [PrimaryKey("Id")]
    [Serializable]
    public class CustomerPolicyModel
    {
        public long Id { get; set; }
        public string CustomerCode { get; set; }

        /// <summary>
        /// 产品类型 出境线路、国内线路、签证
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// 旅游产品设置 线路区域
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 1：固定金额  2：百分比
        /// </summary>
        public int RebateType { get; set; }

        /// <summary>
        /// 数值
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 百分比
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// 百分百最大限额
        /// </summary>
        public decimal MaxAmount { get; set; }

        [PetaPoco.ResultColumn]
        public string ProductTypeName { get; set; }

        [PetaPoco.ResultColumn]
        public string RegionName { get; set; }
    }

    /// <summary>
    /// 发票抬头
    /// </summary>
    public class BankInfoModel
    {
        /// <summary>
        /// 企业名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 企业地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 企业电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 开户银行
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// 银行账户
        /// </summary>
        public string BankAccount { get; set; }
    }
}