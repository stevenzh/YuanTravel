using Lvy.Models;
using Lvy.Models.ProductDB;
using Lvy.VModels.Product;
using System.Collections.Generic;

namespace Lvy.VModels.Op
{
    public class TourVModel : BaseVModel
    {
        /// <summary>
        /// 查询团单信息构造函数
        /// </summary>
        public TourVModel()
        {
            if (Condition == null)
                Condition = new TourSearchCondition();
            if (TourList == null)
                TourList = new PagedList<TourInfoVModel>();
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        public TourSearchCondition Condition { get; set; }

        /// <summary>
        /// 团订单信息列表
        /// </summary>
        public PagedList<TourInfoVModel> TourList { get; set; }

        /// <summary>
        /// 所选线路类型
        /// </summary>
        public string SelectedLineTypeIds { get; set; }

        /// <summary>
        /// 出发日期
        /// </summary>
        public string OutDate { get; set; }

        /// <summary>
        ///单团核算团号
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 团核算成本附件列表
        /// </summary>
        public List<TpTourFileModel> TourFileList { get; set; }
    }

    /// <summary>
    /// 查询条件
    /// </summary>
    public class TourSearchCondition
    {
        /// <summary>
        /// 团号
        /// </summary>
        public string TourNo { get; set; }

        /// <summary>
        /// 线路名称+标注
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// 出发日期 起
        /// </summary>
        public string OutDateRange { get; set; }

        /// <summary>
        /// 是否成团
        /// </summary>
        public string IsTourOk { get; set; }

        /// <summary>
        /// 推荐方式
        /// </summary>
        public string RecommendType { get; set; }

        /// <summary>
        /// 团队性质
        /// </summary>
        public int TourType { get; set; }

        /// <summary>
        /// 团期状态 0 未审核 1 已审核
        /// </summary>
        public string TourAuditState { get; set; }

        /// <summary>
        /// 分组id
        /// </summary>
        public string CrmTeamId { get; set; }
    }
}