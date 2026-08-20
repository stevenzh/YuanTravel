using Lvy.Models;
using Lvy.Models.OrderDB;
using Lvy.Models.ProductDB;
using Lvy.VModels.Excel;
using System.Collections.Generic;

namespace Lvy.VModels.Op
{
    public class EditTouristsVModel : BaseVModel
    {
        public EditTouristsVModel()
        {
            if (Line == null)
            {
                Line = new TpLineModel();
            }
            if (Tour == null)
            {
                Tour = new TpTourPlanModel();
            }
            if (TpTourFileList == null)
            {
                TpTourFileList = new List<TpTourFileModel>();
            }
        }

        /// <summary>
        /// 线路
        /// </summary>
        public TpLineModel Line { get; set; }

        /// <summary>
        /// 团计划
        /// </summary>
        public TpTourPlanModel Tour { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public QuotaModel Quota { get; set; }

        public GuestModels Visitor { get; set; }

        public List<KeyValueBean> DestinationList { get; set; }

        /// <summary>
        /// 游客信息
        /// </summary>
        public List<TpTravellerModel> Tourists { get; set; }

        /// <summary>
        /// 订单信息列表
        /// </summary>
        public List<TpOrderModel> TpOrderList { get; set; }

        /// <summary>
        /// 团附件信息
        /// </summary>
        public List<TpTourFileModel> TpTourFileList { get; set; }
    }
}