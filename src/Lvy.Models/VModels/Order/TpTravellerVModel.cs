using System;
using System.Collections.Generic;
using Lvy.Models;
using Lvy.Models.OrderDB;
namespace Lvy.VModels.Order
{
    /// <summary>
    /// 对应订单游客信息的View Model
    /// </summary>
    public class TpTravellerVModel : BaseVModel
    {
        public TpTravellerVModel()
        {
            if (TpTraveller == null)
                TpTraveller = new TpTravellerModel();
            if (PagedList == null)
                PagedList = new PagedList<TpTravellerModel>();
        }

   
        /// <summary>
        /// 订单游客信息
        /// </summary>
        public TpTravellerModel TpTraveller { get; set; }

        /// <summary>
        /// 订单游客信息列表
        /// </summary>
        public PagedList<TpTravellerModel> PagedList { get; set; }
    }
}
