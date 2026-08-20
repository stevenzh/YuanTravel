using Lvy.Models;
using Lvy.Models.HotelDB;
using Lvy.Models.ProductDB;
using Lvy.Models.SiteDB;
using Lvy.Models.TicketDB;
using Lvy.Visa.Models;
using System;
using System.Collections.Generic;

namespace Lvy.Visa.VModels
{
    [Serializable]
    public class HotModuleQModel
    {
        public IList<SiteNavItemModel> HotModuleList { get; set; }
    }

    [Serializable]
    public class HotProductQModel
    {
        public string SelProCodes { get; set; }
        public int ItemID { get; set; }
        public string CreatedBy { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        /// <summary>
        /// 推荐产品列表
        /// </summary>
        public IList<SiteNavListModel> HotProductList { get; set; }

        /// <summary>
        /// 模块
        /// </summary>
        public SiteNavItemModel ModuleModel { get; set; }

        /// <summary>
        /// 签证分页列表
        /// </summary>
        public PagedList<VisaInformationModel> ProductList { get; set; }

        /// <summary>
        /// 产品信息
        /// </summary>
        public VisaInformationModel ProductInfo { get; set; }

        /// <summary>
        /// 线路分页列表
        /// </summary>
        public PagedList<TpLineModel> LineList { get; set; }
        /// <summary>
        /// 门票分页列表
        /// </summary>
        public PagedList<TktProductModel> TicketList { get; set; }
        public PagedList<HotelModel> HotelPageList { get; set; }

    }
}