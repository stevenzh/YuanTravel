using System.Collections.Generic;
using Lvy.Models;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Product
{
    /// <summary>
    /// 对应编辑线路上车点的View Model
    /// </summary>
    public class SelectBusPointVModel : BaseVModel
    {
        #region 查询条件
        public string LineId { get; set; }
        /// <summary>
        /// 上车点
        /// </summary>
        public string BusPointName { get; set; }
        /// <summary>
        /// 组别Id
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 组别项 {GroupId:GroupName}
        /// </summary>
        public List<KeyValueBean> GroupItems { get; set; }

        #endregion

        /// <summary>
        /// 所属线路Id
        /// </summary>
        public TpLineModel Line { get; set; }

        /// <summary>
        /// 列表信息
        /// </summary>
        public IList<BusPointItemVModel> BusPointList { get; set; }
    }

    public class BusPointItemVModel
    {
        /// <summary>
        /// 是否已选择
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        /// 是否接
        /// </summary>
        public bool IsJie { get; set; }

        /// <summary>
        /// 是否送
        /// </summary>
        public bool IsSong { get; set; }

        /// <summary>
        /// 上车点信息
        /// </summary>
        public TpLineBusPointModel BusPointModel { get; set; }

        /// <summary>
        /// 组别Id    Example:1|2|3
        /// </summary>
        public string GroupId { get; set; }
    }
}
