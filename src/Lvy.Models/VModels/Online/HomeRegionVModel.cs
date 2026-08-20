using System.Collections.Generic;

namespace Lvy.VModels.Online
{
    /// <summary>
    /// 外网首页 产品列表 Model
    /// </summary>
    public class HomePageRegionVModel
    {
        public HomePageRegionVModel()
        {
            RegionList = new List<HomeRegionVModel>();
        }

        /// <summary>
        ///  产品列表
        /// </summary>
        public List<HomeRegionVModel> RegionList { get; set; }
    }

    public class HomeRegionVModel : BaseVModel
    {
        public HomeRegionVModel()
        {
            PlanList = new List<HotTourVModel>();
        }

        public string Name { get; set; }
        public string OutCity { get; set; }

        public List<HotTourVModel> PlanList { get; set; }
    }
}