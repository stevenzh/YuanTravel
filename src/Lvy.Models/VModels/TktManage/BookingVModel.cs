using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.TicketDB;
using Lvy.Models.TourDB;
using Lvy.Visa.Models;
using System.Collections.Generic;

namespace Lvy.VModels.Ticket
{
    public class BookingVModel : BaseVModel
    {
        public BookingVModel()
        {
            this.PayInList = new List<TpOrderPayInModel>();
            this.TravellerList = new List<VisaApplicanterModel>();
            this.Order = new TpTourBalanceModel();
            this.FileList = new List<TourFileModel>();
            this.OrderedProducts = new List<TktProductModel>();
        }


        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductId { get; set; }

        #region 表单

        public TpTourBalanceModel Order { get; set; }

        public string[] OutDates { get; set; }
        public string[] ProductIds { get; set; }

        public List<KeyValueBean> ProIdDateBeans
        {
            get
            {
                List<KeyValueBean> kvs = new List<KeyValueBean>();
                KeyValueBean kv = null;
                if (OutDates != null)
                {
                    for (int i = 0; i < OutDates.Length; i++)
                    {
                        kv = new KeyValueBean();
                        kv.Value = OutDates[i].ToString();
                        kv.Key = ProductIds[i];
                        kvs.Add(kv);
                    }
                }
                return kvs;
            }
        }

        /// <summary>
        /// 门票人数
        /// </summary>
        public int[] PeopleNum { get; set; }

        public string[] PriceIds { get; set; }

        #endregion 表单

        /// <summary>
        /// 选择的产品
        /// </summary>
        public TktProductModel Product { get; set; }

        #region 编辑

        /// <summary>
        /// 已定的产品
        /// </summary>
        public List<TktProductModel> OrderedProducts { get; set; }

        /// <summary>
        /// 已定的产品当前价格集合
        /// </summary>
        public List<OrderPricesVModel> ProductsCurrentDatePrices { get; set; }

        public List<VisaApplicanterModel> TravellerList { get; set; }

        public List<TpOrderPayInModel> PayInList { get; set; }

        public List<TourFileModel> FileList { get; set; }

        public List<KeyValueBean> FileKeyList { get; set; }

        #endregion 编辑
    }

    public class OrderPricesVModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 预定数量
        /// </summary>
        public int PeopleNum { get; set; }

        /// <summary>
        /// 价格规则编号
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// 价格类型
        /// </summary>
        public string PriceType { get; set; }

        /// <summary>
        /// 市场价
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// 结算价
        /// </summary>
        public decimal SettlePrice { get; set; }

        /// <summary>
        /// 购票方式  1:固定签单，2:特殊签单，3:任务单，4:特殊任务单
        /// </summary>
        public int TktType { get; set; }

        /// <summary>
        /// 签单价|返利
        /// </summary>
        public decimal SysPrice { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public int IsValid { get; set; }

        /// <summary>
        /// 标准价
        /// </summary>
        public int IsStandard { get; set; }

        /// <summary>
        /// 价格政策说明
        /// </summary>
        public string PriceDesc { get; set; }

        /// <summary>
        /// 所属商户
        /// </summary>
        public string OwnerCode { get; set; }
    }
}