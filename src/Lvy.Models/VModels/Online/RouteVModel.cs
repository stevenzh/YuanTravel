using Arch.Common;
using Lvy.Models.ProductDB;
using System.Collections.Generic;

namespace Lvy.VModels.Booking
{
    public class RouteVModel : BaseVModel
    {
        private string _logoPath;

        /// <summary>
        /// LogoPath 路径
        /// </summary>
        public string LogoPath
        {
            get
            {
                return AppSetting.Get("UploadFileRoot") + _logoPath;
            }
            set { _logoPath = value; }
        }

        /// <summary>
        /// 出团计划信息
        /// </summary>
        public TpTourPlanModel TpTourPlanModel { get; set; }

        public List<TpTourPlanModel> PlanList { get; set; }

        /// <summary>
        /// 线路信息
        /// </summary>
        public TpLineModel LineModel { get; set; }

        /// <summary>
        /// 线路行程安排信息
        /// </summary>
        public List<TpLineRouteModel> TpLineRoutes { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<TpLineFileModel> FileList { get; set; }
    }
}