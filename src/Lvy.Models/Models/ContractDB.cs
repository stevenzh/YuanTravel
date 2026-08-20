using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaPoco;

namespace Lvy.Models
{

    [TableName("ContractInfo")]
    [PrimaryKey("id")]
    [Serializable]
    public class ContractInfo
    {

        public ContractInfo()
        {
            this.disputeResolution = 1;
            this.solutionA = "1";
            this.solutionB = "1";
            this.agreeToBuy = true;
            this.signingPassType = 0;
            this.deductStandardA = "abc,def,ghi|jkl,mno,pqr";
            this.deductStandardB = "abc,def,ghi|jkl,mno,pqr";
        }

        public int id { get; set; }

        /// <summary>
        /// 合同编号
        /// </summary>
        public string contractNumber { get; set; }

        /// <summary>
        /// 模版ID
        /// </summary>
        public string templateId { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string orderCode { get; set; }

        /// <summary>
        /// 是否多人签署
        /// </summary>
        public bool isMultiSign { get; set; }

        /// <summary>
        /// 其他约定内容
        /// </summary>
        public string additionContent { get; set; }

        /// <summary>
        /// 经办人姓名
        /// </summary>
        public string transactorName { get; set; }

        /// <summary>
        /// 经办人电话
        /// </summary>
        public string transactorPhone { get; set; }
        /// <summary>
        /// 旅行社名称
        /// </summary>
        public string agencyName { get; set; }
        /// <summary>
        /// 经营许可证号
        /// </summary>
        public string licenseCode { get; set; }

        /// <summary>
        /// 旅行社地址
        /// </summary>
        public string agencyAddress { get; set; }

        /// <summary>
        /// 旅行社统一社会信用代码/工商注册号
        /// </summary>
        public string creditCode { get; set; }
        /// <summary>
        /// 旅行社签人联系姓名
        /// </summary>
        public string contactName { get; set; }
        /// <summary>
        /// 旅行社签人联系电话
        /// </summary>
        public string contactPhone { get; set; }
        /// <summary>
        /// 旅行社签约客户电话
        /// </summary>
        public string servicePhone { get; set; }
        /// <summary>
        /// 旅行社签约人邮箱
        /// </summary>
        public string contactEmail { get; set; }
        /// <summary>
        /// 旅游产品名称
        /// </summary>
        public string routeName { get; set; }
        /// <summary>
        /// 团号
        /// </summary>
        public string groupId { get; set; }
        /// <summary>
        /// 组团方式
        /// </summary>
        public int groupMode { get; set; }
        /// <summary>
        /// 出发日期
        /// </summary>
        public string startDate { get; set; }
        /// <summary>
        /// 出发地点
        /// </summary>
        public string departureCity { get; set; }
        /// <summary>
        /// 途径地点（经停地点）
        /// </summary>
        public string passCity { get; set; }
        /// <summary>
        /// 目的地
        /// </summary>
        public string arrivalCity { get; set; }
        /// <summary>
        /// 结束日期
        /// </summary>
        public string endDate { get; set; }
        /// <summary>
        /// 返回地点
        /// </summary>
        public string endCity { get; set; }
        /// <summary>
        /// 景点名称和浏览时间
        /// </summary>
        public string attraction { get; set; }
        /// <summary>
        /// 往返交通
        /// </summary>
        public string longTransport { get; set; }
        /// <summary>
        /// 标准（往返交通）
        /// </summary>
        public string longStandard { get; set; }
        /// <summary>
        /// 浏览交通
        /// </summary>
        public string localTransport { get; set; }
        /// <summary>
        /// 标准（浏览交通）
        /// </summary>
        public string localStandard { get; set; }
        /// <summary>
        /// 旅游者自由活动时间
        /// </summary>
        public string freeTime { get; set; }
        /// <summary>
        /// 自由活动次数
        /// </summary>
        public string freeNumber { get; set; }
        /// <summary>
        /// 用餐次数
        /// </summary>
        public string mealNumber { get; set; }
        /// <summary>
        /// 用餐标准
        /// </summary>
        public string mealStandard { get; set; }
        /// <summary>
        /// 导游服务
        /// </summary>
        public string tourGuideService { get; set; }
        /// <summary>
        /// 行程单
        /// </summary>
        public string travelItinerary { get; set; }
        /// <summary>
        /// 旅游费用
        /// </summary>
        public decimal? totalCost { get; set; }
        /// <summary>
        /// 导游服务费（元）/每人
        /// </summary>
        public decimal? guideServiceCost { get; set; }
        /// <summary>
        /// 导游服务费总费用（元）
        /// </summary>
        public decimal? totalGuideServiceCost { get; set; }
        /// <summary>
        /// 费用缴纳期限描述
        /// </summary>
        public string deadlineDescription { get; set; }
        /// <summary>
        /// 交纳期限
        /// </summary>
        public string deadline { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public string payMethod { get; set; }
        /// <summary>
        /// 其他支付方式
        /// </summary>
        public string otherPaymethod { get; set; }
        /// <summary>
        /// 争议的解决方式
        /// 1：提交仲裁委员会仲裁，2：向人民法院提起诉讼
        /// </summary>
        public int disputeResolution { get; set; }
        /// <summary>
        /// 仲裁委员会名称
        /// </summary>
        public string tribunalName { get; set; }
        /// <summary>
        /// 其他解决办法
        /// 1: 委托其他社，2: 延期出团，3: 改签其他路线，4: 解除合同。
        /// </summary>
        public int otherResolution { get; set; }

        /// <summary>
        ///成团人数约定
        /// </summary>
        public string groupAgreementResolution { get; set; }

        /// <summary>
        /// 最低成团人数
        /// </summary>
        public string leastCustomerNumbe { get; set; }
        /// <summary>
        /// 出团通知后违约金间隔(补偿期)
        /// </summary>
        public string compensationPeriod { get; set; }
        /// <summary>
        /// 严重违约
        /// </summary>
        public string seriousCompensateRatio { get; set; }
        /// <summary>
        /// 逾期每日费用
        /// </summary>
        public string overduePenaltyDayRatio { get; set; }
        /// <summary>
        /// 其他违约责任
        /// </summary>
        public string otherLiability { get; set; }
        /// <summary>
        /// 解决方式（甲方违约）
        /// </summary>
        public string solutionA { get; set; }
        /// <summary>
        /// 有约定逾期日（甲方违约）
        /// </summary>
        public string overdueDaysA { get; set; }
        /// <summary>
        /// 无约定逾期日（甲方违约）
        /// </summary>
        public string overdueDaysA2 { get; set; }
        /// <summary>
        /// 退还费用日期（甲方违约）
        /// </summary>
        public string timeLimitA { get; set; }
        /// <summary>
        /// 退还费用日期（甲方违约）
        /// </summary>
        public string timeLimitA2 { get; set; }
        /// <summary>
        /// 费用扣除标准 用|符号分隔项，用逗号分隔值
        /// </summary>
        public string deductStandardA { get; set; }
        /// <summary>
        /// 公式数组（费用扣除计算）用|符号分隔
        /// </summary>
        public string formulaA { get; set; }
        /// <summary>
        /// 解决方式（乙方违约）
        /// </summary>
        public string solutionB { get; set; }
        /// <summary>
        /// 有约定逾期日（乙方违约）
        /// </summary>
        public string overdueDaysB { get; set; }
        /// <summary>
        /// 无约定逾期日（乙方违约）
        /// </summary>
        public string overdueDaysB2 { get; set; }
        /// <summary>
        /// 退还费用日期（乙方违约）
        /// </summary>
        public string timeLimitB { get; set; }
        /// <summary>
        /// 退还费用日期（乙方违约）
        /// </summary>
        public string timeLimitB2 { get; set; }
        /// <summary>
        /// 费用扣除标准（乙方违约）用|符号分隔项，用逗号分隔值
        /// </summary>
        public string deductStandardB { get; set; }
        /// <summary>
        /// 委托社名称
        /// </summary>
        public string entrustAgency { get; set; }
        /// <summary>
        /// 是否购买保险
        /// </summary>
        public bool agreeToBuy { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string productName { get; set; }
        /// <summary>
        /// 保险公司名称 
        /// </summary>
        public string company { get; set; }
        /// <summary>
        /// 保险费
        /// </summary>
        public decimal? coverage { get; set; }
        /// <summary>
        /// 保险金额
        /// </summary>
        public decimal? premium { get; set; }
        /// <summary>
        /// 联系人/游客代表
        /// </summary>
        public string signingName { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string signingPhone { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        public int? signingPassType { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        public string signingPassNo { get; set; }
        /// <summary>
        /// 游客类别
        /// </summary>
        public int signingTouristType { get; set; }
        /// <summary>
        /// 联系人地址
        /// </summary>
        public string signingAddress { get; set; }
        /// <summary>
        /// 联系人邮箱
        /// </summary>
        public string signingEmail { get; set; }
        /// <summary>
        /// 联系人参团
        /// </summary>
        public bool signingIsJoin { get; set; }
        /// <summary>
        /// 联系人性别
        /// </summary>
        public string signingSex { get; set; }

        /// <summary>
        /// 签署地点
        /// </summary>
        public string signingPlace { get; set; }

        /// <summary>
        /// 传真
        /// </summary>
        public string signingFax { get; set; }

        /// <summary>
        /// 签约方式
        /// </summary>
        public int signingMode { get; set; }

        /// <summary>
        /// 是否含购
        /// </summary>
        public bool hasShopping { get; set; }

        /// <summary>
        /// 含购物景点名称
        /// </summary>
        public string shoppingViewSpot { get; set; }

        /// <summary>
        /// 地接社名称
        /// </summary>
        public string localAgencyName { get; set; }

        /// <summary>
        /// 地接社地址
        /// </summary>
        public string localAgencyAddress { get; set; }
        /// <summary>
        /// 地接社联系人
        /// </summary>
        public string localAgencyContact { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        public string localAgencyPhone { get; set; }
        /// <summary>
        /// 补充协议标题
        /// </summary>
        public string supptitle { get; set; }
        /// <summary>
        /// 补充协议
        /// </summary>
        public string suppContent { get; set; }
        /// <summary>
        /// 签署链接
        /// </summary>
        public string signingURL { get; set; }
        /// <summary>
        /// 二维码链接
        /// </summary>
        public string qrCodeURL { get; set; }
        /// <summary>
        /// 合同状态
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string createBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? modifyTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string modifyBy { get; set; }

        /// <summary>
        /// 其他社名称
        /// </summary>
        public string otherAgency { get; set; }

        public string fileURL { get; set; }

        public string viewURL { get; set; }

        /// <summary>
        /// 状态更新接收消息
        /// </summary>
        public string receiveMsg { get; set; }

    }

    [TableName("ContractTourist")]
    [PrimaryKey("id")]
    [Serializable]
    public class ContractTourist
    {
        public int id { get; set; }

        /// <summary>
        /// 合同ID
        /// </summary>
        public int contractId { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        /// 证件类型
        /// </summary>
        public int passType { get; set; }

        /// <summary>
        /// 证件号
        /// </summary>
        public string passNo { get; set; }

        /// <summary>
        /// 游客性别
        /// </summary>
        public string gender { get; set; }

        /// <summary>
        /// 健康状况
        /// </summary>
        public string health { get; set; }

        /// <summary>
        /// 是否签署人
        /// </summary>
        public bool isSign { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// 民族
        /// </summary>
        public string nation { get; set; }

        /// <summary>
        /// 国籍
        /// </summary>
        public string nationality { get; set; }

        /// <summary>
        /// 是否儿童
        /// </summary>
        public bool isChildren { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public int age { get; set; }

    }

    [TableName("ContractShopping")]
    [PrimaryKey("id")]
    [Serializable]
    public class ContractShopping
    {
        public int id { get; set; }

        /// <summary>
        /// 合同ID
        /// </summary>
        public int contractId { get; set; }
        /// <summary>
        /// 购物时间
        /// </summary>
        public DateTime date { get; set; }

        /// <summary>
        /// 购物地点
        /// </summary>
        public string place { get; set; }

        /// <summary>
        /// 购物场所
        /// </summary>
        public string shoppingPlace { get; set; }

        /// <summary>
        /// 商品信息
        /// </summary>
        public string goods { get; set; }

        /// <summary>
        /// 最长停留时间
        /// </summary>
        public string stayDuration { get; set; }
        /// <summary>
        /// 其他说明
        /// </summary>
        public string memo { get; set; }
    }

    [TableName("ContractPayItem")]
    [PrimaryKey("id")]
    [Serializable]
    public class ContractPayItem
    {
        public int id { get; set; }

        /// <summary>
        /// 合同ID
        /// </summary>
        public int contractId { get; set; }

        /// <summary>
        /// 具体时间
        /// </summary>
        public DateTime date { get; set; }
        /// <summary>
        /// 服务地点
        /// </summary>
        public string place { get; set; }
        /// <summary>
        /// 项目名称和内容
        /// </summary>
        public string item { get; set; }
        /// <summary>
        /// 费用(元)
        /// </summary>
        public decimal fee { get; set; }
        /// <summary>
        /// 项目时长
        /// </summary>
        public string stayDuration { get; set; }

        /// <summary>
        /// 其他说明
        /// </summary>
        public string memo { get; set; }
    }

    [TableName("ContractFiles")]
    [PrimaryKey("id")]
    public class ContractFiles
    {
        public int id { get; set; }

        /// <summary>
        /// 合同ID
        /// </summary>
        public int contractId { get; set; }

        /// <summary>
        /// 附件名称
        /// </summary>
        public string fileName { get; set; }

        /// <summary>
        /// 下载地址
        /// </summary>
        public string filePath { get; set; }

        public DateTime createTime { get; set; }
    }

    [TableName("ContractAdditions")]
    [PrimaryKey("id")]
    public class ContractAdditions
    {
        public int id { get; set; }

        public string title { get; set; }

        public string content { get; set; }

        public string createBy { get; set; }

        public DateTime createTime { get; set; }
    }
}
