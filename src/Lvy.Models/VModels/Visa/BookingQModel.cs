using Lvy.Visa.Models;
using Lvy.VModels;
using System;
using System.Collections.Generic;

namespace Lvy.Visa.VModels
{
    public class BookingQModel : BaseVModel
    {
        /// <summary>
        /// 产品编码
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 预订来源
        /// </summary>
        public int OrderSource { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// 成人人数
        /// </summary>
        public int AudltNum { get; set; }

        /// <summary>
        /// 儿童人数
        /// </summary>
        public int ChildNum { get; set; }

        /// <summary>
        /// 产品详细
        /// </summary>
        public VisaInformationModel ProductModel { get; set; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal TotAmount { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal SalePrice { get; set; }

        /// <summary>
        /// 产品总价
        /// </summary>
        public decimal TotProductAmount { get; set; }

        /// <summary>
        /// 客户留言
        /// </summary>
        public string CustMessage { get; set; }

        /// <summary>
        /// 总人数
        /// </summary>
        public int TotPeopleNum { get; set; }

        /// <summary>
        /// 申请人列表
        /// </summary>
        public List<VisaApplicanterModel> TripModels { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        public string LinkName { get; set; }

        /// <summary>
        /// 联系人手机
        /// </summary>
        public string LinkPhone { get; set; }

        /// <summary>
        /// 联系人邮箱
        /// </summary>
        public string LinkEmail { get; set; }

        /// <summary>
        /// 联系人电话区号
        /// </summary>
        public string LinkTelD { get; set; }

        /// <summary>
        /// 联系人电话号码
        /// </summary>
        public string LinkTel { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 订单信息
        /// </summary>
        public VisaOrderModel OrderModel { get; set; }

        /// <summary>
        /// 网银支付类型
        /// </summary>
        public string Alipay { get; set; }

        /// <summary>
        /// 获取申请人列表
        /// </summary>
        public string Liststr { get; set; }

        /// <summary>
        /// 是否要发票
        /// </summary>
        public int IsneedInvoice { get; set; }

        /// <summary>
        ///预计出发日期
        /// </summary>
        public DateTime? ReadyDate { get; set; }

        public DateTime OutDate { get; set; }
        public string LinkSex { get; set; }

        public string ContractType { get; set; }

        /// <summary>
        /// 寄送地址
        /// </summary>
        public string Adress { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string PostCode { get; set; }

        public string ClientIP { get; set; }
    }

    [Serializable]
    public class BookContact
    {
        public string Name { get; set; }
        public string Sex { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime DeliverDate { get; set; }
    }
}