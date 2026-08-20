using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lvy.APIVModels.Res
{
    public class GetProductsResponse : BaseResponse
    {
        public List<TpLineModel> Products { get; set; }
    }

    public class TpLineModel
    {
        /// <summary>
        /// 线路编号
        /// </summary>
        public int LineId { get; set; }

        /// <summary>
        /// 线路名称
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// 行程天数
        /// </summary>
        public int TravelDays { get; set; }

        /// <summary>
        /// 提前预定天数
        /// </summary>
        public int MoveUpDays { get; set; }

        /// <summary>
        /// 出发地
        /// </summary>
        public string DepartDest { get; set; }

        [JsonIgnore]
        public string ArriveDestId { get; set; }

        /// <summary>
        /// 到达地
        /// </summary>
        public string ArriveDest { get; set; }

        /// <summary>
        /// 线路类型
        /// </summary>
        public int LineType { get; set; }

        /// <summary>
        /// 交通类型 1:汽车 2：火车 3：飞机 4：轮船 5：自驾  9：其他
        /// </summary>
        public int TrafficType { get; set; }

        /// <summary>
        /// 线路特色
        /// </summary>
        public string LineSpecial { get; set; }

        /// <summary>
        /// 费用包含
        /// </summary>
        public string PriceContain { get; set; }

        /// <summary>
        /// 费用不包含
        /// </summary>
        public string PriceNoContain { get; set; }

        /// <summary>
        /// 预定须知
        /// </summary>
        public string BookingNodes { get; set; }

        /// <summary>
        /// 操作说明
        /// </summary>
        public string OpDesc { get; set; }

        /// <summary>
        /// 出行提示
        /// </summary>
        public string FootNotes { get; set; }

        /// <summary>
        /// 购物描述
        /// </summary>
        public string Shopping { get; set; }

        /// <summary>
        /// 应急电话 【线路管理员的电话】
        /// </summary>
        public string YingJiPhone { get; set; }

        public List<TpTourModel> Tours { get; set; }

        public List<TpLineRouteModel> Routes { get; set; }
    }

    public class TpTourModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public int LineId { get; set; }

        /// <summary>
        /// 团名称
        /// </summary>
        public string TourName { get; set; }

        /// <summary>
        /// 出团日期
        /// </summary>
        public int OutDate { get; set; }

        /// <summary>
        /// 标准价  成人价
        /// </summary>
        public decimal Price { get; set; }

        public PriceInfo PriceInfo { get; set; }

        /// <summary>
        /// 报名截止日期
        /// </summary>
        public int BookingLastDays { get; set; }

        /// <summary>
        /// 推荐方式 0 常规团  1 特价团
        /// </summary>
        public int TuiJianType { get; set; }

        /// <summary>
        /// 状态 0：无效 1：有效 2：下线 3:上线
        /// </summary>
        public int TourState { get; set; }

        /// <summary>
        /// 最小成团人数
        /// </summary>
        public int MixedNum { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// 团状态
        /// </summary>
        [JsonIgnore]
        public int Source { get; set; }

        /// <summary>
        /// 线路状态
        /// </summary>
        [JsonIgnore]
        public int LineState { get; set; }

        #region TpQuota

        /// <summary>
        /// 计划名额
        /// </summary>
        public int PlanQuota { get; set; }

        /// <summary>
        /// 可用名额
        /// </summary>
        public int UseQuota { get; set; }

        #endregion TpQuota
    }

    public class TpLineRouteModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int RouteId { get; set; }

        /// <summary>
        /// 线路编号
        /// </summary>
        public int LineId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 第几天
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// 住宿
        /// </summary>
        public string Hotel { get; set; }

        /// <summary>
        /// 餐饮
        /// </summary>
        public string Catering { get; set; }

        /// <summary>
        /// 行程内容
        /// </summary>
        public string Contents { get; set; }
    }
}