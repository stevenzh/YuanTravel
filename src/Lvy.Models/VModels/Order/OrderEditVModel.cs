using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using System.Collections.Generic;
using System.Linq;

namespace Lvy.VModels.Order
{
    public class OrderEditVModel : BaseVModel
    {
        public OrderEditVModel()
        {
            Order = new TpOrderModel();
            TourPlan = new TpTourPlanModel();
            Travellers = new List<TpTravellerModel>();
            Travellers2 = new List<TpTravellerModel>();
            FileModel = new TpOrderFileModel();
        }

        /// <summary>
        /// 线路类型
        /// </summary>
        public int LineType { get; set; }

        /// <summary>
        /// 接价
        /// </summary>
        public decimal JiePrice { get; set; }

        /// <summary>
        /// 送价
        /// </summary>
        public decimal SongPrice { get; set; }

        /// <summary>
        ///游客Id
        /// </summary>
        public int TravellerId { get; set; }

        /// <summary>
        /// 取消产生费用
        /// </summary>
        public decimal CancelMoney { get; set; }

        /// <summary>
        /// 订单信息
        /// </summary>
        public TpOrderModel Order { get; set; }

        /// <summary>
        /// 临时上车点Id|JSType
        /// </summary>
        public string LineBusPointView { get; set; }

        /// <summary>
        /// 上车点json
        /// </summary>
        public string LineBusPoint { get; set; }

        /// <summary>
        /// 上车点Key
        /// </summary>
        public string LineBusPointId
        {
            get
            {
                if (Order.LineBusPointId != 0)
                {
                    var item = LineBusPoints.Where(a => a.Id == Order.LineBusPointId).FirstOrDefault();
                    return item.Id.ToString() + "|" + item.JsType.ToString();
                }
                else
                    return "";
            }
        }

        /// <summary>
        /// 上车点信息
        /// </summary>
        public List<TpLineBusPointModel> LineBusPoints { get; set; }

        public IEnumerable<KeyValueBean> LineBusPointBeans
        {
            get
            {
                var items = LineBusPoints.Select(a => new KeyValueBean()
                {
                    Key = a.Id.ToString() + "|" + a.JsType,
                    //Value = a.BusPoint + " | " + a.JsTime + "　|  接价：" + a.JiePrice + "　| 送价：" + a.SongPrice
                    Value = GetLineBusPointValue(a)
                });

                return items;
            }
        }

        //暂时不做修改
        public string GetLineBusPointValue(TpLineBusPointModel model)
        {
            //只接不送
            if (model.JsType == 1)
                return model.BusPoint + "(只接不送) | " + model.JsTime + " | 接价：" + model.JiePrice;
            //只送不接
            else if (model.JsType == 2)
                return model.BusPoint + "(只送不接) | " + model.JsTime + "　|  送价：" + model.JiePrice;
            //接送
            else
                return model.BusPoint + " | " + model.JsTime + "　|  接价：" + model.JiePrice + "　| 送价：" + model.SongPrice;
        }

        /// <summary>
        /// 开班计划
        /// </summary>
        public TpTourPlanModel TourPlan { get; set; }

        /// <summary>
        /// 游客信息
        /// </summary>
        public List<TpTravellerModel> Travellers { get; set; }

        /// <summary>
        /// 状态为：2(有效)的游客信息
        /// </summary>
        public List<TpTravellerModel> Travellers2 { get; set; }

        /// <summary>
        /// 状态为：1(已退团)、0(已取消)的游客信息
        /// </summary>
        public List<TpTravellerModel> Travellers10 { get; set; }

        /// <summary>
        /// 线路信息
        /// </summary>
        public TpLineModel LineModel { get; set; }

        /// <summary>
        /// 该团期对应的价格
        /// </summary>
        public List<TpPriceModel> Prices { get; set; }

        /// <summary>
        /// 缴款信息
        /// </summary>
        public List<TpOrderPayInModel> ListTourPayInModel { get; set; }

        /// <summary>
        /// 发票记录
        /// </summary>
        public List<TpInvoiceModel> ListTpInvoiceInfo { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<TpOrderFileModel> FileList { get; set; }

        public TpOrderFileModel FileModel { get; set; }

        /// <summary>
        /// 子订单
        /// </summary>
        public List<TpChildOrderModel> ChildOrderList { get; set; }

        /// <summary>
        /// 操作日志
        /// </summary>
        public List<BizLogModel> LogList { get; set; }

        /// <summary>
        /// 当前用户是否是OP
        /// </summary>
        public int IsOP { get; set; }

        /// <summary>
        /// 当前用户是否可以修改价格
        /// </summary>
        public bool IsEditPric { get; set; }
    }
}