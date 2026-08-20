using Lvy.VModels.Order;
using System;
using System.Collections.Generic;

namespace Lvy.VModels.Op
{
    /// <summary>
    /// 出境名单使用
    /// </summary>
    public class TourOutBandHeadVModel
    {
        public TourOutBandHeadVModel()
        {
            if (TpTravellerList == null)
            {
                TpTravellerList = new List<PrintTravellerVModel>();
            }
        }

        public OutBandHeadModel OutBandHeadModel { get; set; }

        public List<PrintTravellerVModel> TpTravellerList { get; set; }
    }

    /// <summary>
    /// 出境名单表
    /// </summary>
    public class OutBandHeadModel
    {
        /// <summary>
        /// 线路Id
        /// </summary>
        public int LineId { get; set; }

        /// <summary>
        /// 团Id
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// 团队名称
        /// </summary>
        public string TourNo { get; set; }

        /// <summary>
        /// 出发日期
        /// </summary>
        public string OutDate { get; set; }

        /// <summary>
        /// 入境日期
        /// </summary>
        public DateTime? EntryDate { get; set; }

        /// <summary>
        /// 线路名称
        /// </summary>
        public string LineName { get; set; }

        /// <summary>
        /// 女性人数
        /// </summary>
        public int WomenCount { get; set; }

        /// <summary>
        /// 男性人数
        /// </summary>
        public int ManCount { get; set; }

        public int LeaderCount { get; set; }

        /// <summary>
        /// 游客总人数
        /// </summary>
        public int TravellerCount { get; set; }

        /// <summary>
        /// 组团序号
        /// </summary>
        public string ZuTuanNo { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public string Years { get; set; }

        /// <summary>
        /// 组团社名称
        /// </summary>
        public string ZuTuanName { get; set; }

        /// <summary>
        /// 组团社联系人及电话
        /// </summary>
        public string ZuTuanContact { get; set; }

        /// <summary>
        /// 	接  待  社  名  称
        /// </summary>
        public string ReceptionName { get; set; }

        /// <summary>
        /// 接待社联络人员姓名及电话
        /// </summary>
        public string ReceptionContact { get; set; }

        /// <summary>
        /// 出境年份
        /// </summary>
        public string LeaveYear { get; set; }

        /// <summary>
        /// 出境月份
        /// </summary>
        public string LeaveMonth { get; set; }

        /// <summary>
        /// 出境日
        /// </summary>
        public string LeaveDay { get; set; }

        /// <summary>
        /// 入境年份
        /// </summary>
        public string EnterYear { get; set; }

        /// <summary>
        /// 入境月份
        /// </summary>
        public string EnterMonth { get; set; }

        /// <summary>
        /// 入境日
        /// </summary>
        public string EnterDay { get; set; }

        /// <summary>
        /// 是否取消出入境日期和口岸
        /// </summary>
        public bool IsContainsEnterDateAndPosition { get; set; }

        /// <summary>
        /// 表头中含领队
        /// </summary>
        public bool IsContainsLingDui { get; set; }

        /// <summary>
        /// 领队姓名
        /// </summary>
        public string GuideName { get; set; }

        /// <summary>
        /// 领队证号
        /// </summary>
        public string GuideNo { get; set; }

        /// <summary>
        /// 出境口岸
        /// </summary>
        public string PortOfExit { get; set; }

        public string PortOfEntry { get; set; }
    }

    public class OutBandTravellerModel
    {
        /// <summary>
        /// 中文姓名
        /// </summary>
        public string TravellerName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string TravellerSpell { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string TravellerSex { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public string TravellerBirthday { get; set; }

        /// <summary>
        /// 出生地
        /// </summary>
        public string Birthplace { get; set; }

        /// <summary>
        /// 护照号码
        /// </summary>
        public string CardNum { get; set; }

        /// <summary>
        /// 发证机关及日期。
        /// </summary>
        public string IssueAt { get; set; }

        public string IssueDate { get; set; }
    }
}