using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models;
using Lvy.Models.SiteDB;

namespace Lvy.VModels.Site
{
    public class BannerVModel : BaseVModel
    {
        public BannerVModel()
        {
            this.BannerList = new List<SiteBannerModel>();
        }

        public string Name { get; set; }

        public string Type { get; set; }

        public List<SiteBannerModel> BannerList { get; set; }
    }
}
