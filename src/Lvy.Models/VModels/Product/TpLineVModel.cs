using Lvy.Models;
using Lvy.Models.ProductDB;

namespace Lvy.VModels.Product
{
    public class TpLineVModel : BaseVModel
    {
        public TpLineVModel()
        {
            this.IsImport = "0";
            this.LineList = new PagedList<TpLineModel>();
            this.FirstTime = true;
        }

        #region 查询条件

        public string CustomerCode { get; set; }

        /// <summary>
        /// 产品组
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 线路名称
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string ArriveDest { get; set; }

        /// <summary>
        /// 是否外部录入 null=全部 0=内部 1=外部
        /// </summary>
        public string IsImport { get; set; }

        /// <summary>
        /// 线路类型(key) 1.跟团|2.自由行|3.当地参团|4.自驾|5.游轮
        /// </summary>
        public string LineType { get; set; }
        /// <summary>
        /// 目的地范围  1.周边|2.国内|3.台港澳|4.出境
        /// </summary>
        public string LineScope { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 行程天数
        /// </summary>
        public string TravelDays { get; set; }

        /// <summary>
        /// 分组id
        /// </summary>
        public string CrmTeamId { get; set; }

        public bool FirstTime { get; set; }

        #endregion 查询条件

        /// <summary>
        /// 线路列表
        /// </summary>
        public PagedList<TpLineModel> LineList { get; set; }
    }
}